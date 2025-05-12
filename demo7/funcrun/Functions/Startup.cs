using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;


[assembly: FunctionsStartup(typeof(Functions.Startup))]

namespace Functions
{
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Bind the Settings class to the configuration
            var configuration = builder.GetContext().Configuration;
            var settings = new Settings();
            configuration.Bind(settings);

            // Register Settings as a singleton
            builder.Services.AddSingleton(settings);

            // Register the IHello service
            builder.Services.AddSingleton<IHello, Hello>();

            // Register the logger
            builder.Services.AddLogging(builder =>
            {
                builder.AddConsole(); // Enables logging to the console
                builder.SetMinimumLevel(LogLevel.Information); // Sets the minimum log level to Information
            });
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            builder.ConfigurationBuilder
                .AddJsonFile("settings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("settings.prod.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            base.ConfigureAppConfiguration(builder);
        }
    }

    interface IHello
    {
        string SayHello(string name);
    }

    class Hello : IHello
    {
        public string SayHello(string name)
        {
            return $"================ Hello from {name} ==================";
        }
    }

    class MyFunction
    {
        readonly IHello _hello;
        readonly ILogger<MyFunction> _logger;
        readonly Settings _settings;

        public MyFunction(IHello hello, ILogger<MyFunction> logger, Settings settings)
        {
            _hello = hello;
            _logger = logger;
            _settings = settings;
        }

        [FunctionName("TimerTrigger_Hello")]
        [Timeout("00:00:02")]
        public async Task RunAsync([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            _logger.LogInformation(_hello.SayHello("TimerTrigger_Hello"));
            _logger.LogWarning($">>>>>>>>>>>>{_settings.Name} {_settings.Count} {_settings.Items}: {DateTime.Now}<<<<<<<<<<<<<<<<<");

            Console.WriteLine(_hello.SayHello("TimerTrigger_Hello"));
            Console.WriteLine($"<<<<<<<<<<<<<<<{_settings.Name} {_settings.Count} {_settings.Items}: {DateTime.Now}>>>>>>>>>>>>>>>>>>>");
        }
    }

    class Settings
    {
        public string Name { get; set; } = "World";
        public int Count { get; set; } = 1;
        public string[] Items { get; set; } = Array.Empty<string>();
    }
}