// <copyright file="FunctionServiceBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using System.Reflection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace FunctionRunner;

/// <summary>
/// Extension methods for building a service provider for Azure Functions.
/// </summary>
static class FunctionServiceProviderBuilderExtensions
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
            var startupType = startupAttr.GetType().GetProperty("WebJobsStartupType")!.GetValue(startupAttr) as Type;
            dynamic startup = Activator.CreateInstance(startupType!)!;

            var webJobsBuilderContext = new WebJobsBuilderContext()
            {
                ApplicationRootPath = root,
            };
            var functionRunnerBuilder = new FunctionRunnerBuilder(
                services, new ConfigurationBuilder(), new FunctionRunnerHostBuilderContext(webJobsBuilderContext));

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
    /// Builds a service provider for isolated worker Azure Functions.
    /// </summary>
    /// <param name="assembly">The assembly containing the function.</param>
    /// <param name="root">The application root path.</param>
    /// <param name="type">The function type to register.</param>
    /// <returns>An <see cref="IServiceProvider"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the required static field is not found.</exception>
    public static IServiceProvider ServiceProviderIsolatedBuild(this Assembly assembly, string root, Type type)
    {
        var services = new ServiceCollection();

        var field = assembly.GetTypes()
                            .FirstOrDefault(t =>
                                t.IsClass &&
                                t.IsAbstract && t.IsSealed && // static class in C#
                                t.Name == "FunctionRunnerHostExtensions")?
                            .GetField("FunctionRunnerRun", BindingFlags.NonPublic | BindingFlags.Static);

        if (field == null)
        {
            throw new InvalidOperationException("FunctionRunnerHostExtensions.FunctionRunnerRun is not exist");
        }

        IServiceProvider? provider = null;

        field.SetValue(null, (Action<IHost>)(host => provider = host.Services));

        var entryPoint = assembly.EntryPoint!;
        var parameters = entryPoint.GetParameters();
        object?[] args = parameters.Length == 0 ? [] : [Array.Empty<string>()];

        var result = entryPoint.Invoke(null, args);
        if (result is Task task)
        {
            task.GetAwaiter().GetResult();
        }

        return new ServiceProviderProxy(provider!);
    }

    /// <summary>
    /// Configures logging for the function app.
    /// </summary>
    /// <param name="loggingBuilder">The logging builder.</param>
    /// <param name="services">The service collection.</param>
    public static void Build(this ILoggingBuilder loggingBuilder, IServiceCollection services)
    {
        var customizedConsoleFormatter = services.Any(sp => sp.ServiceType == typeof(ConsoleFormatter));
        if (!customizedConsoleFormatter)
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
}

/// <summary>
/// Builder for function runner services and configuration.
/// Implements <see cref="IFunctionsHostBuilder"/>, <see cref="IFunctionsConfigurationBuilder"/>, and <see cref="IOptions{FunctionsHostBuilderContext}"/>.
/// </summary>
/// <param name="services">The service collection.</param>
/// <param name="configurationBuilder">The configuration builder.</param>
/// <param name="context">The host builder context.</param>
class FunctionRunnerBuilder(IServiceCollection services, IConfigurationBuilder configurationBuilder, FunctionsHostBuilderContext context)
    : IFunctionsHostBuilder, IFunctionsConfigurationBuilder, IOptions<FunctionsHostBuilderContext>
{
    /// <inheritdoc/>
    public IServiceCollection Services { get; } = services;

    /// <inheritdoc/>
    public IConfigurationBuilder ConfigurationBuilder { get; } = configurationBuilder;

    /// <inheritdoc/>
    public FunctionsHostBuilderContext Value { get; } = context;
}

/// <summary>
/// Host builder context for function runner.
/// Inherits from <see cref="FunctionsHostBuilderContext"/>.
/// </summary>
/// <param name="webJobsBuilderContext">The WebJobs builder context.</param>
class FunctionRunnerHostBuilderContext(WebJobsBuilderContext webJobsBuilderContext) : FunctionsHostBuilderContext(webJobsBuilderContext)
{
}

/// <summary>
/// Proxy for <see cref="IServiceProvider"/> that attempts to resolve services dynamically if not found in the main provider.
/// </summary>
/// <param name="source">The source service provider.</param>
class ServiceProviderProxy(IServiceProvider source) : IServiceProvider
{
    readonly IServiceProvider _source = source;

    /// <summary>
    /// Gets the service object of the specified type.
    /// If not found, attempts to construct it using a new service collection.
    /// </summary>
    /// <param name="serviceType">The type of service to retrieve.</param>
    /// <returns>The service object, or null if not found.</returns>
    public object? GetService(Type serviceType)
    {
        var instance = _source.GetService(serviceType);

        if (instance == null)
        {
            var services = new ServiceCollection();

            var args = serviceType.GetConstructors().First()!.GetParameters();
            foreach (var arg in args)
            {
                var argType = arg.ParameterType;
                if (argType.IsAssignableTo(typeof(ILoggerFactory)) ||
                    argType.IsAssignableTo(typeof(ILogger)))
                {
                    continue;
                }

                services.AddSingleton(argType, _source.GetService(argType)!);
            }

            var configuration = _source.GetService<IConfiguration>();

            using var serviceProvider = services
                .AddTransient(serviceType)
                .AddSingleton<IConfiguration>(configuration ?? new ConfigurationBuilder().AddEnvironmentVariables().Build())
                .AddLogging(loggingBuilder => loggingBuilder.Build(services))
                .BuildServiceProvider();

            instance = serviceProvider.GetService(serviceType);
        }

        return instance;
    }
}
