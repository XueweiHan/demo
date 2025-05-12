using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

//<PublishSingleFile>true</PublishSingleFile>
//<SelfContained>true</SelfContained>

namespace FunctionRunner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var appSettings = new AppSettings();
            ILoggerFactory? loggerFactory = null;

            using var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config =>
                {
                    config.AddJsonFile("appSettings.json", optional: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((ctx, services) =>
                {
                    ctx.Configuration.Bind(appSettings);
                    services.AddSingleton(appSettings);
                    appSettings.ExecuteAsync(ctx.Configuration).Wait();

                    if (!appSettings.DisableFunctionRunner && !string.IsNullOrEmpty(appSettings.AzureWebJobsScriptRoot))
                    {
                        var (functionInfos, instanceProviders) = FunctionInfo.Load(appSettings.AzureWebJobsScriptRoot);
                        loggerFactory = instanceProviders.Select(ip => ip.GetLoggerFactory()).FirstOrDefault(lf => lf != null);

                        foreach (var funcInfo in functionInfos)
                        {
                            AddFunctionServices(services, funcInfo);
                        }
                    }

                    if (appSettings.FunctionRunnerHttpPort > 0)
                    {
                        services.AddHostedService<HTTPService>();
                    }

                    if (appSettings.HeartbeatLogIntervalInSeconds > 0)
                    {
                        services.AddHostedService<HeartBeatService>();
                    }

                    AddExecutableServices(services, appSettings.ConfigJson?.Executables);
                })
                .ConfigureLogging(loggingBuilder =>
                {
                    if (loggerFactory != null)
                    {
                        loggingBuilder.ClearProviders();
                        loggingBuilder.AddProvider(new LoggerFactoryAdapter(loggerFactory));
                    }

                })
                .ConfigureHostOptions(options =>
                {
                    options.ShutdownTimeout = TimeSpan.FromSeconds(appSettings.ShutdownTimeoutInSeconds);
                })
                .Build();

            await host.RunAsync();

            Console.WriteLine("Exit.");
        }

        static readonly Dictionary<string, Func<FunctionInfo, ILogger, FunctionBaseService>> serviceMap = new()
        {
            ["serviceBusTrigger"] = (funcInfo, logger) => new FunctionServiceBusTriggerService(funcInfo, logger),
            ["timerTrigger"] = (funcInfo, logger) => new FunctionTimerTriggerService(funcInfo, logger)
        };

        static void AddFunctionServices(IServiceCollection services, FunctionInfo funcInfo)
        {
            var type = funcInfo.Function.Bindings[0].Type;
            if (serviceMap.TryGetValue(type, out var serviceFactory))
            {
                services.AddSingleton<IHostedService>(sp =>
                {
                    var service = serviceFactory(funcInfo, sp.GetRequiredService<ILoggerFactory>().CreateLogger(funcInfo.Function.EntryPoint));
                    //var service = serviceFactory(funcInfo, sp.GetRequiredService<ILogger<FunctionBaseService>>());
                    service.PrintFunctionInfo();
                    return service;
                });
            }
            else
            {
                new FunctionBaseService(funcInfo, null).PrintFunctionInfo(true);
            }
        }

        static void AddExecutableServices(IServiceCollection services, Executable[]? executables)
        {
            foreach (var executable in executables ?? [])
            {
                services.AddSingleton<IHostedService>(sp => new ExecutableService(executable.Exec, executable.Args,
                    sp.GetRequiredService<ILogger<ExecutableService>>()));
            }
        }
    }
}