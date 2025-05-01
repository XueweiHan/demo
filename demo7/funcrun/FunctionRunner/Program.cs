using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FunctionRunner
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var functionInfos = FunctionInfo.Load();

            using var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<HeartBeatService>();
                    services.AddHostedService<HTTPService>();

                    foreach (var info in functionInfos)
                    {
                        AddFunctionService(services, info);
                    }

                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                        builder.SetMinimumLevel(LogLevel.Information);
                    });
                })
                .ConfigureHostOptions(options =>
                {
                    options.ShutdownTimeout = TimeSpan.FromSeconds(20);
                })
                .Build();

            await host.RunAsync();

            Console.WriteLine("Exit.");
        }

        static readonly Dictionary<string, Func<FunctionInfo, ILogger<FunctionBaseService>, IHostedService>> serviceMap = new()
        {
            ["serviceBusTrigger"] = (funcInfo, logger) => new FunctionServiceBusTriggerService(funcInfo, logger),
            ["timerTrigger"] = (funcInfo, logger) => new FunctionTimerTriggerService(funcInfo, logger)
        };

        static void AddFunctionService(IServiceCollection services, FunctionInfo info)
        {
            var type = info.Function.Bindings[0].Type;
            if (serviceMap.TryGetValue(type, out var serviceFactory))
            {
                services.AddSingleton(sp =>
                {
                    var service = serviceFactory(info, sp.GetRequiredService<ILogger<FunctionBaseService>>());
                    ((FunctionBaseService)service).PrintFunctionInfo();
                    return service;
                });
            }
            else
            {
                new FunctionBaseService(info, null).PrintFunctionInfo(true);
            }
        }
    }
}
