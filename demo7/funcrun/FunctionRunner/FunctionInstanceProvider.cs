using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace FunctionRunner
{
    class FunctionInstanceProvider : IDisposable
    {
        ServiceProvider _serviceProvider;

        public FunctionInstanceProvider(Assembly assembly, string root)
        {
            var services = new ServiceCollection();

            var startupAttr = assembly.GetCustomAttributes().FirstOrDefault(a => a.GetType().Name == "FunctionsStartupAttribute");
            if (startupAttr != null)
            {
                var startupType = startupAttr.GetType().GetProperty("WebJobsStartupType")!.GetValue(startupAttr) as Type;
                dynamic startup = Activator.CreateInstance(startupType!)!;

                var webJobsBuilderContext = new WebJobsBuilderContext()
                {
                    EnvironmentName = "Development",
                    ApplicationRootPath = root,
                };
                var functionRunnerBuilder = new FunctionRunnerBuilder(
                    services, new ConfigurationBuilder(), new FunctionRunnerHostBuilderContext(webJobsBuilderContext));

                startup.ConfigureAppConfiguration((IFunctionsConfigurationBuilder)functionRunnerBuilder);

                webJobsBuilderContext.Configuration = functionRunnerBuilder.ConfigurationBuilder.Build();

                startup.Configure(functionRunnerBuilder);
            }

            if (!services.Any(s => s.ServiceType == typeof(ILoggerFactory)))
            {
                services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.SetMinimumLevel(LogLevel.Information);
                    loggingBuilder.AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = false;
                        options.UseUtcTimestamp = true;
                        options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss.fff]";
                        options.SingleLine = true;
                    });
                });
            }

            _serviceProvider = services.BuildServiceProvider();
        }

        public ILoggerFactory GetLoggerFactory()
        {
            return _serviceProvider.GetService<ILoggerFactory>()!;
        }

        public object Create(Type type)
        {
            var ctor = type.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First()!;
            var parameters = ctor.GetParameters()
                .Select(p => _serviceProvider.GetService(p.ParameterType))// ?? throw new InvalidOperationException($"Unable to resolve {p.ParameterType}"))
                .ToArray();
            return ctor.Invoke(parameters)!;
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }
    }

    class FunctionRunnerBuilder(IServiceCollection services, IConfigurationBuilder configurationBuilder, FunctionsHostBuilderContext context) : IFunctionsHostBuilder, IFunctionsConfigurationBuilder, IOptions<FunctionsHostBuilderContext>
    {
        public IServiceCollection Services { get; } = services;
        public IConfigurationBuilder ConfigurationBuilder { get; } = configurationBuilder;
        public FunctionsHostBuilderContext Value { get; } = context;
    }

    class FunctionRunnerHostBuilderContext(WebJobsBuilderContext webJobsBuilderContext) : FunctionsHostBuilderContext(webJobsBuilderContext)
    {
    }
}
