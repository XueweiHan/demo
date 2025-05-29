using IniParser;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

[assembly: FunctionsStartup(typeof(FunctionsApp.Startup))]

namespace FunctionsApp;

class Startup : FunctionsStartup
{
    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        //using var loggerFactory = LoggerFactory.Create(builder => { });
        //var logger = loggerFactory.CreateLogger(nameof(Startup));
        //var env = Environment.GetEnvironmentVariable("environment");
        //logger.LogInformation("++++++++++++++++++++++++++++++++++ Building configuration, env={environment}", env);

        builder.ConfigurationBuilder
            .SetBasePath(builder.GetContext().ApplicationRootPath)
            .AddJsonFile("settings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("settings.prod.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        //base.ConfigureAppConfiguration(builder);
    }

    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddOptions<Settings>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                builder.GetContext().Configuration.Bind(settings);
            });

        // Bind the Settings class to the configuration
        //var settings = new Settings();
        //var ctx = builder.GetContext();
        //ctx.Configuration.Bind(settings);

        ////Register Settings as a singleton
        //builder.Services.AddSingleton(settings);

        // Register the IHello service
        builder.Services.AddSingleton<IHello, Hello>();

        // Register the logger
        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.SetMinimumLevel(LogLevel.Information);
            loggingBuilder.AddFilter("FunctionsApp", LogLevel.Debug);
            //loggingBuilder.AddConsole();
        });
    }
}

public class Settings
{
    public string Name { get; set; } = "Default";
    public int Count { get; set; } = 1;
    public string Function1_CronSchedule { get; set; } = "*/2 * * * * *";
    public Queue Queue { get; set; } = new Queue();
    public Timer Timer { get; set; } = new Timer();
}

public class Queue
{
    public string Name { get; set; }
}

public class Timer
{
    public string Schedule { get; set; }
}

interface IHello
{
    string SayHello(string name);
}

class Hello : IHello
{
    public string SayHello(string name)
    {
        return $"Hello from {name}";
    }
}

class Function1
{
    readonly IHello _hello;
    readonly ILogger<Function1> _logger;
    readonly Settings _settings;

    public Function1(IHello hello, ILogger<Function1> logger, IOptions<Settings> settings)
    {
        _hello = hello;
        _logger = logger;
        _settings = settings.Value;
    }

    [FunctionName("Function1_timer")]
    public void Run([TimerTrigger("%Function1_CronSchedule%", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
    {
        _logger.LogWarning($"{typeof(Function1)}");
        log.LogInformation($"================= Logger From function parameter");
        _logger.LogInformation($"================= Logger From class ID {_hello.SayHello(_settings.Name)}");

        log.LogDebug(">>>>>>>>>>>>> debug1 <<<<<<<<<<<<<<<");
        _logger.LogDebug(">>>>>>>>>>>>> debug2 <<<<<<<<<<<<<<<");
        //throw new InvalidOperationException("Test exception");
    }
}

class Function2
{
    readonly ILogger<Function2> _logger;

    public Function2(ILogger<Function2> logger)
    {
        _logger = logger;
    }

    [FunctionName("Function2_timer")]
    [Timeout("00:00:01")]
    public async Task Run([TimerTrigger("%Timer:Schedule%")] TimerInfo myTimer, CancellationToken cancel)
    {
        _logger.LogWarning($"{typeof(Function2)}");
        _logger.LogInformation($"{typeof(Function2).Name} Start --------------");

        test3rdPartyLib();

        await Task.Delay(TimeSpan.FromSeconds(2), cancel);
        _logger.LogInformation($"{typeof(Function2).Name} End --------------");
    }

    void test3rdPartyLib()
    {
        string iniContent = @"
            [General]
            AppName=MyApp
            Version=1.0
            [User]
            Name=John Doe
            ";

        var parser = new IniDataParser();
        IniData data = parser.Parse(iniContent);

        string appName = data["General"]["AppName"];
        string userName = data["User"]["Name"];

        _logger.LogWarning($"App: {appName}");
        _logger.LogWarning($"User: {userName}");
    }
}

class Function3
{
    [FunctionName("Function3_sb_queue")]
    public Task Run([ServiceBusTrigger("%Queue:Name%", Connection = "MyServiceBusConnection")] string myQueueItem, ILogger log, CancellationToken cancel)
    {
        log.LogWarning($"{typeof(Function3)}");
        log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
        return Task.CompletedTask;
    }
}
