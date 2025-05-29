// <copyright file="FunctionServiceProviderBuilderInProcess.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using System.Reflection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace FunctionRunner;

/// <summary>
/// Extension methods for building a service provider for Azure Functions.
/// </summary>
static class FunctionServiceProviderBuilderInProcess
{
    /// <summary>
    /// Builds a service provider for the specified function type and assembly.
    /// </summary>
    /// <param name="assembly">The assembly containing the function.</param>
    /// <param name="root">The application root path.</param>
    /// <param name="type">The function type to register.</param>
    /// <returns>An <see cref="IServiceProvider"/> instance.</returns>
    public static IServiceProvider ServiceProviderBuild(this Assembly assembly, string root, Type type)
    {
        var services = new ServiceCollection();
        IConfiguration? configuration = null;

        var startupAttr = assembly.GetCustomAttributes().FirstOrDefault(a => a.GetType().Name == "FunctionsStartupAttribute");
        if (startupAttr != null)
        {
            // Set up the HostBuilderContext
            var webJobsBuilderContext = new WebJobsBuilderContext()
            {
                ApplicationRootPath = root,
                EnvironmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production",
            };

            var property = assembly.GetTypes()
                    .First(t => t.IsClass && t.IsAbstract && t.IsSealed && t.Name == "FunctionRunnerBuilderExtensions")!
                    .GetProperty("FunctionRunnerHostBuilderContext", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!;
            property.SetValue(null, new FunctionRunnerHostBuilderContext(webJobsBuilderContext));

            // Create the FunctionRunnerBuilder
            var functionRunnerBuilder = new FunctionRunnerBuilder(services, new ConfigurationBuilder());

            // Create the startup instance
            var startupType = startupAttr.GetType().GetProperty("WebJobsStartupType")!.GetValue(startupAttr) as Type;
            dynamic startup = Activator.CreateInstance(startupType!)!;

            startup.ConfigureAppConfiguration((IFunctionsConfigurationBuilder)functionRunnerBuilder);

            configuration = functionRunnerBuilder.ConfigurationBuilder.Build();
            webJobsBuilderContext.Configuration = configuration;

            startup.Configure(functionRunnerBuilder);
        }

        return services
            .AddTransient(type)
            .AddSingleton<IConfiguration>(configuration ?? new ConfigurationBuilder().AddEnvironmentVariables().Build())
            .AddLogging(loggingBuilder => loggingBuilder.Build(services))
            .BuildServiceProvider();
    }

    /// <summary>
    /// Configures logging for the function app.
    /// </summary>
    /// <param name="loggingBuilder">The logging builder.</param>
    /// <param name="services">The service collection.</param>
    public static void Build(this ILoggingBuilder loggingBuilder, IServiceCollection services)
    {
        if (!services.Any(sp => sp.ServiceType == typeof(ConsoleFormatter)))
        {
            loggingBuilder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = false;
                options.UseUtcTimestamp = true;
                options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss.fff] ";
                options.SingleLine = true;
            });
        }
    }

    class FunctionRunnerBuilder(IServiceCollection services, IConfigurationBuilder configurationBuilder)
        : IFunctionsHostBuilder, IFunctionsConfigurationBuilder
    {
        public IServiceCollection Services { get; } = services;

        public IConfigurationBuilder ConfigurationBuilder { get; } = configurationBuilder;
    }

    class FunctionRunnerHostBuilderContext(WebJobsBuilderContext webJobsBuilderContext) : FunctionsHostBuilderContext(webJobsBuilderContext)
    {
    }
}
