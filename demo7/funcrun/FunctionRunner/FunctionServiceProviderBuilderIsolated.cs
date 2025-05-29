// <copyright file="FunctionServiceProviderBuilderIsolated.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;

namespace FunctionRunner;

/// <summary>
/// Extension methods for building a service provider for Azure Functions.
/// </summary>
static class FunctionServiceProviderBuilderIsolated
{
    /// <summary>
    /// Builds a service provider for isolated worker Azure Functions.
    /// </summary>
    /// <param name="assembly">The assembly containing the function.</param>
    /// <param name="root">The application root path.</param>
    /// <param name="type">The function type to register.</param>
    /// <returns>An <see cref="IServiceProvider"/> instance.</returns>
    public static IServiceProvider ServiceProviderIsolatedBuild(this Assembly assembly, string root, Type type)
    {
        var hostBuilder = new FunctionRunnerHostBuilder();

        // Set the hostBuilder on the assembly's FunctionRunnerHostExtensions.FunctionRunnerHostBuilder property
        var property = assembly.GetTypes()
                            .First(t => t.IsClass && t.IsAbstract && t.IsSealed && t.Name == "FunctionRunnerHostExtensions")!
                            .GetProperty("FunctionRunnerHostBuilder", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!;
        property.SetValue(null, hostBuilder);

        // Invoke the entry point of the assembly (Main method)
        var entryPoint = assembly.EntryPoint!;
        var parameters = entryPoint.GetParameters();
        object[] args = parameters.Length == 0 ? Array.Empty<object>() : new object[] { Array.Empty<string>() };

        var result = entryPoint.Invoke(null, args);
        if (result is Task task)
        {
            task.GetAwaiter().GetResult();
        }

        var services = new ServiceCollection();

        var context = new HostBuilderContext(new Dictionary<object, object>())
        {
            HostingEnvironment = new HostingEnvironment
            {
                ApplicationName = assembly.GetName().Name!,
                ContentRootPath = root,
                EnvironmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"
            },
        };

        var configBuilder = new ConfigurationBuilder();

        // Call the ConfigureAppConfiguration method if it was set
        hostBuilder.ConfigureAppConfigurationDelegate?.Invoke(context, configBuilder);

        context.Configuration = configBuilder.Build();

        // Call the ConfigureServices method if it was set
        hostBuilder.ConfigureServicesDelegate?.Invoke(context, services);

        return services
            .AddTransient(type)
            .AddSingleton<IConfiguration>(context.Configuration ?? new ConfigurationBuilder().AddEnvironmentVariables().Build())
            .AddLogging(loggingBuilder => loggingBuilder.Build(services))
            .BuildServiceProvider();
    }

    class FunctionRunnerHostBuilder : HostBuilder
    {
        public Action<HostBuilderContext, IConfigurationBuilder>? ConfigureAppConfigurationDelegate { get; private set; }

        public Action<HostBuilderContext, IServiceCollection>? ConfigureServicesDelegate { get; private set; }

        public new IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            ConfigureAppConfigurationDelegate = configureDelegate;
            return this;
        }

        public new IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            ConfigureServicesDelegate = configureDelegate;
            return this;
        }
    }
}
