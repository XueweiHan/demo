using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(FunctionApp.Startup))]

namespace FunctionApp
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Bind the Settings class to the configuration
            var settings = new Settings();
            var ctx = builder.GetContext();
            ctx.Configuration.Bind(settings);

            // Register Settings as a singleton
            builder.Services.AddSingleton(settings);

            // Register the IHello service
            builder.Services.AddSingleton<IHello, Hello>();

            // Register the logger
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddFilter("*", LogLevel.Debug);
            });
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var ctx = builder.GetContext();

            builder.ConfigurationBuilder
                .SetBasePath(ctx.ApplicationRootPath)
                .AddJsonFile("settings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("settings.prod.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            base.ConfigureAppConfiguration(builder);
        }
    }

    public class Settings
    {
        public string Name { get; set; } = "Default";
        public int Count { get; set; } = 1;
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

    class Function1
    {
        readonly IHello _hello;
        readonly ILogger<Function1> _logger;
        readonly Settings _settings;

        public Function1(IHello hello, ILogger<Function1> logger, Settings settings)
        {
            _hello = hello;
            _logger = logger;
            _settings = settings;
        }

        [FunctionName("Function1_timer")]
        public void Run([TimerTrigger("*/2 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"<<<<C# Timer trigger function executed at: {DateTime.Now}>>>>>");
            _logger.LogInformation($">>>>>>>>> {_hello.SayHello(_settings.Name)} <<<<<<<");

            log.LogDebug("debug1");
            _logger.LogDebug("debug2");
        }
    }

    class Function2
    {
        readonly ILogger<Function2> _logger;
        public Function2(IHello hello, ILogger<Function2> logger, Settings settings)
        {
            _logger = logger;
        }
        [FunctionName("Function2_timer")]
        [Timeout("00:00:01")]
        public async Task Run([TimerTrigger("*/3 * * * * *")] TimerInfo myTimer, CancellationToken cancel)
        {
            _logger.LogInformation($"{typeof(Function2).Name}: Start at {DateTime.Now:u}");
            await Task.Delay(TimeSpan.FromSeconds(2), cancel);
            _logger.LogInformation($"{typeof(Function2).Name}: End at {DateTime.Now:u}");
        }
    }

    public class Function3
    {
        [FunctionName("Function3_sb_queue")]
        public Task Run([ServiceBusTrigger("queue1", Connection = "MyServiceBusConnection")] string myQueueItem, ILogger log, CancellationToken cancell)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            return Task.CompletedTask;
        }
    }
}
