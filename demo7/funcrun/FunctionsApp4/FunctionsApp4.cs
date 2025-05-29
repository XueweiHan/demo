using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data;

namespace FunctionsApp4;

public class Program
{
    public static Task Main(string[] args)
    {
        var host = new HostBuilder()
           .ConfigureFunctionsWebApplication()
           .ConfigureAppConfiguration((context, config) =>
           {
               var appRootPath = context.HostingEnvironment.ContentRootPath;
               config.AddJsonFile(Path.Combine(appRootPath, "appsettings.json"), optional: false, reloadOnChange: false);
               config.AddEnvironmentVariables();
           })
           .ConfigureServices((context, services) =>
           {
               var configuration = context.Configuration;

               // Store all settings in environment variables
               foreach (var kvp in configuration.AsEnumerable())
               {
                   if (!string.IsNullOrEmpty(kvp.Value))
                   {
                       Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
                   }
               }

               services
                    .AddLogging(builder =>
                        {
                            builder.AddFilter(string.Empty, LogLevel.Information);
                        })
                   .AddApplicationInsightsTelemetryWorkerService()
                   .ConfigureFunctionsApplicationInsights();
           })
           .Build();

        host.Run();

        return Task.CompletedTask;
    }
}

public class FunctionsApp4_1
{
    private readonly ILogger _logger;

    public FunctionsApp4_1(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<FunctionsApp4_1>();
    }

    [Function("FunctionsApp4_1")]
    public void Run([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);
    }
}