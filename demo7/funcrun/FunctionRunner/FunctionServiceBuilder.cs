using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace FunctionRunner;

static class FunctionServiceProviderBuilderExtensions
{
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

class FunctionRunnerBuilder(IServiceCollection services, IConfigurationBuilder configurationBuilder, FunctionsHostBuilderContext context)
    : IFunctionsHostBuilder, IFunctionsConfigurationBuilder, IOptions<FunctionsHostBuilderContext>
{
    public IServiceCollection Services { get; } = services;
    public IConfigurationBuilder ConfigurationBuilder { get; } = configurationBuilder;
    public FunctionsHostBuilderContext Value { get; } = context;
}

class FunctionRunnerHostBuilderContext(WebJobsBuilderContext webJobsBuilderContext) : FunctionsHostBuilderContext(webJobsBuilderContext)
{
}

class ServiceProviderProxy(IServiceProvider source) : IServiceProvider
{
    readonly IServiceProvider _source = source;

    public object? GetService(Type serviceType)
    {
        var instance = _source.GetService(serviceType);

        if (instance == null)
        {
            var args = serviceType.GetConstructors().First()!.GetParameters();
            var services = new ServiceCollection();

            var configuration = _source.GetService<IConfiguration>();

            services
                .AddTransient(serviceType)
                .AddSingleton<IConfiguration>(configuration ?? new ConfigurationBuilder().AddEnvironmentVariables().Build())
                .AddLogging(loggingBuilder => loggingBuilder.Build(services));

            foreach (var arg in args)
            {
                var argType = arg.ParameterType;
                if (argType == typeof(ILoggerFactory)) continue;

                services.AddSingleton(argType, _source.GetService(argType)!);
            }

            using var serviceProvider = services.BuildServiceProvider();
            instance = serviceProvider.GetService(serviceType);
        }

        return instance;
    }
}
