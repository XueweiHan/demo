// <copyright file="HeartBeatService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FunctionRunner;

/// <summary>
/// Background service that logs a heartbeat message at a configurable interval.
/// </summary>
class HeartBeatService(AppSettings appSettings, ILoggerFactory loggerFactory) : BackgroundService
{
    readonly TimeSpan heartbeatLogInterval = TimeSpan.FromSeconds(appSettings.HeartbeatLogIntervalInSeconds);

    #pragma warning disable CA2213 // Disposable fields should be disposed
    readonly ILoggerFactory loggerFactory = loggerFactory;  // loggerFactory is a singleton, so it doesn't need to be disposed
    #pragma warning restore CA2213

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (appSettings.HeartbeatLogIntervalInSeconds <= 0) { return; }

        var name = $"HeartBeatService";
        loggerFactory.CreateLogger("T.Cyan0").LogInformation($"{name} is starting");

        try
        {
            var logger = loggerFactory.CreateLogger("T.Green3");
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation($"pod heartbeat from {Environment.MachineName}");
                await Task.Delay(heartbeatLogInterval, stoppingToken);
            }
        }
        finally
        {
            loggerFactory.CreateLogger("T.Cyan0").LogInformation($"{name} is stopped");
        }
    }
}
