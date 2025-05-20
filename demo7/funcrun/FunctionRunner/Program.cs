// <copyright file="Program.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

[assembly: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

namespace FunctionRunner;

/// <summary>
/// The main entry point for the FunctionRunner application.
/// </summary>
class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    static async Task Main(string[] args)
    {
        var appSettings = new AppSettings();
        using var loggerFactory = LoggerFactory.Create(AnsiConsoleFormatter.LoggingBuilder);

        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(config =>
            {
                config.AddJsonFile("appSettings.json", optional: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((ctx, services) =>
            {
                ctx.Configuration.Bind(appSettings);
                ctx.Configuration.GetSection("CONFIG_FILE").Bind(appSettings.ConfigFile);
                appSettings.ConfigJson = JsonHelper.GetEnvJson<Config>("CONFIG_JSON") ?? ctx.Configuration.GetSection("CONFIG_JSON").Get<Config>();

                appSettings.ExecuteAsync(loggerFactory).Wait();

                services.AddSingleton(loggerFactory);
                services.AddSingleton(appSettings);
                services.AddHostedService<HTTPService>();
                services.AddHostedService<HeartBeatService>();
                AddExecutableServices(services, appSettings.ConfigJson!.Executables);

                if (!appSettings.DisableFunctionRunner && !string.IsNullOrEmpty(appSettings.AzureWebJobsScriptRoot))
                {
                    var functionInfos = FunctionInfo.Load(appSettings.AzureWebJobsScriptRoot);

                    foreach (var funcInfo in functionInfos)
                    {
                        AddFunctionServices(services, funcInfo, loggerFactory);
                    }
                }
            })
            .ConfigureHostOptions(options =>
            {
                options.ShutdownTimeout = TimeSpan.FromSeconds(appSettings.ShutdownTimeoutInSeconds);
            })
            .Build();

        await host.RunAsync();

        loggerFactory.CreateLogger("T").LogInformation("Exit");
    }

    static readonly Dictionary<string, Func<FunctionInfo, ILoggerFactory, FunctionBaseService>> serviceMap = new()
    {
        ["serviceBusTrigger"] = (funcInfo, loggerFactory) => new FunctionServiceBusTriggerService(funcInfo, loggerFactory),
        ["timerTrigger"] = (funcInfo, loggerFactory) => new FunctionTimerTriggerService(funcInfo, loggerFactory)
    };

    /// <summary>
    /// Adds function services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="funcInfo">The function information.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    static void AddFunctionServices(IServiceCollection services, FunctionInfo funcInfo, ILoggerFactory loggerFactory)
    {
        var type = funcInfo.Function.Bindings[0].Type;
        if (serviceMap.TryGetValue(type, out var serviceFactory))
        {
            services.AddSingleton<IHostedService>(sp =>
            {
                var service = serviceFactory(funcInfo, sp.GetRequiredService<ILoggerFactory>());
                service.PrintFunctionInfo();
                return service;
            });
        }
        else
        {
            new FunctionBaseService(funcInfo, loggerFactory).PrintFunctionInfo(true);
        }
    }

    /// <summary>
    /// Adds executable services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="executables">The array of executables.</param>
    static void AddExecutableServices(IServiceCollection services, Executable[] executables)
    {
        foreach (var executable in executables)
        {
            services.AddSingleton<IHostedService>(sp =>
                new ExecutableService(executable.Exec, executable.Args, sp.GetRequiredService<ILoggerFactory>()));
        }
    }
}
