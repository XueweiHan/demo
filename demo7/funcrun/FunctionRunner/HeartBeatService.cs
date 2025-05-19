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
    readonly ILoggerFactory loggerFactoryField = loggerFactory;

    /// <inheritdoc/>
    public override void Dispose()
    {
        loggerFactoryField.Dispose();
        base.Dispose();
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var name = $"HeartBeatService";
        loggerFactoryField.CreateLogger("T.Cyan0").LogInformation($"{name} is starting");

        try
        {
            var logger = loggerFactoryField.CreateLogger("T.Green3");
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation($"pod heartbeat from {Environment.MachineName}");
                await Task.Delay(heartbeatLogInterval, stoppingToken);
            }
        }
        finally
        {
            loggerFactoryField.CreateLogger("T.Cyan0").LogInformation($"{name} is stopped");
        }
    }
}
