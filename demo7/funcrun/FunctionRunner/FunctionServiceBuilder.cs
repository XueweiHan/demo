using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace FunctionRunner
{
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
                    EnvironmentName = "Development",
                    ApplicationRootPath = root,
                };
                var functionRunnerBuilder = new FunctionRunnerBuilder(
                    services, new ConfigurationBuilder(), new FunctionRunnerHostBuilderContext(webJobsBuilderContext));

                startup.ConfigureAppConfiguration((IFunctionsConfigurationBuilder)functionRunnerBuilder);

                configuration = functionRunnerBuilder.ConfigurationBuilder.Build();
                webJobsBuilderContext.Configuration = configuration;

                startup.Configure(functionRunnerBuilder);
            }

            services.AddSingleton<IConfiguration>(configuration ?? new ConfigurationBuilder().AddEnvironmentVariables().Build());

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
                        options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss.fff] ";
                        options.SingleLine = true;
                    });
                });
            }

            services.AddTransient(type);

            return services.BuildServiceProvider();
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
}
