// <copyright file="FunctionTimerTriggerService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using NCrontab;

namespace FunctionRunner;

/// <summary>
/// Service for running a function on a timer trigger using a cron schedule.
/// </summary>
class FunctionTimerTriggerService(FunctionInfo funcInfo, ILoggerFactory loggerFactory)
    : FunctionBaseService(funcInfo, loggerFactory)
{
    /// <inheritdoc/>
    public override void PrintFunctionInfo(bool u)
    {
        base.PrintFunctionInfo();
        elogger.LogInformation($"  Schedule:   {binding.Schedule}");
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await base.ExecuteAsync(stoppingToken);
        if (IsDisabled)
        {
            return;
        }

        try
        {
            PrintStatus(FunctionAction.Start);

            var parameters = PrepareParameters(stoppingToken);

            if (binding.RunOnStartup)
            {
                await funcInfo.InvokeAsync(parameters, loggerFactory);
            }

            var schedule = CrontabSchedule.Parse(binding.Schedule, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
            var nextCheckTimeSpan = TimeSpan.FromDays(1);

            for (; ; )
            {
                DateTime next;
                for (; ; )
                {
                    var now = DateTime.UtcNow;
                    next = schedule.GetNextOccurrence(now);
                    var timeSpan = next - now;
                    if (timeSpan < nextCheckTimeSpan)
                    {
                        if (timeSpan > TimeSpan.Zero)
                        {
                            await Task.Delay(timeSpan, stoppingToken);
                        }
                        break;
                    }
                    else
                    {
                        await Task.Delay(nextCheckTimeSpan, stoppingToken);
                    }
                }

                await funcInfo.InvokeAsync(parameters, loggerFactory);

                while (schedule.GetNextOccurrence(DateTime.UtcNow) == next)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
                }
            }
        }
        finally
        {
            PrintStatus(FunctionAction.Stop);
        }
    }
}
