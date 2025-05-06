using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FunctionRunner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            AppSettings? appSettings = null;

            using var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config =>
                {
                    config.AddJsonFile("appSettings.json", optional: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((ctx, services) =>
                {
                    appSettings = new AppSettings()
                    {
                        ConfigJson = JsonHelper.GetEnvJson<Config>("CONFIG_JSON") ?? ctx.Configuration.GetSection("CONFIG_JSON").Get<Config>(),
                        ConfigFile = ctx.Configuration.GetValue<string>("CONFIG_FILE"),
                    };
                    ctx.Configuration.Bind(appSettings);
                    services.AddSingleton(appSettings);
                    appSettings.ExecuteAsync().Wait();

                    var functionInfos = FunctionInfo.Load(appSettings.AzureWebJobsScriptRoot!);
                    foreach (var funcInfo in functionInfos)
                    {
                        AddFunctionService(services, funcInfo);
                    }

                    if (appSettings.FunctionRunnerHttpPort > 0)
                    {
                        services.AddHostedService<HTTPService>();
                    }

                    if (appSettings.HeartbeatLogIntervalInSeconds > 0)
                    {
                        services.AddHostedService<HeartBeatService>();
                    }

                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                        builder.SetMinimumLevel(LogLevel.Information);
                    });
                })
                .ConfigureHostOptions(options =>
                {
                    options.ShutdownTimeout = TimeSpan.FromSeconds(appSettings!.ShutdownTimeoutInSeconds);
                })
                .Build();

            await host.RunAsync();

            Console.WriteLine("Exit.");
        }

        static readonly Dictionary<string, Func<FunctionInfo, ILogger<FunctionBaseService>, FunctionBaseService>> serviceMap = new()
        {
            ["serviceBusTrigger"] = (funcInfo, logger) => new FunctionServiceBusTriggerService(funcInfo, logger),
            ["timerTrigger"] = (funcInfo, logger) => new FunctionTimerTriggerService(funcInfo, logger)
        };

        static void AddFunctionService(IServiceCollection services, FunctionInfo funcInfo)
        {
            var type = funcInfo.Function.Bindings[0].Type;
            if (serviceMap.TryGetValue(type, out var serviceFactory))
            {
                services.AddSingleton(sp =>
                {
                    var service = serviceFactory(funcInfo, sp.GetRequiredService<ILogger<FunctionBaseService>>());
                    service.PrintFunctionInfo();
                    return (IHostedService)service;
                });
            }
            else
            {
                new FunctionBaseService(funcInfo, null).PrintFunctionInfo(true);
            }
        }
    }
}