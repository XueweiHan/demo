using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FunctionRunner;

class HeartBeatService(AppSettings appSettings, ILoggerFactory loggerFactory) : BackgroundService
{
    readonly TimeSpan _heartbeatLogInterval = TimeSpan.FromSeconds(appSettings.HeartbeatLogIntervalInSeconds);
    readonly ILoggerFactory _loggerFactory = loggerFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var name = $"HeartBeatService";
        _loggerFactory.CreateLogger("T.Cyan0").LogInformation($"{name} is starting");

        try
        {
            var logger = _loggerFactory.CreateLogger("T.Green3");
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation($"pod heartbeat from {Environment.MachineName}");
                await Task.Delay(_heartbeatLogInterval, stoppingToken);
            }
        }
        finally
        {
            _loggerFactory.CreateLogger("T.Cyan0").LogInformation($"{name} is stopped");
        }
    }
}