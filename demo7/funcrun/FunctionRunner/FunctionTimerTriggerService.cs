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
        binding.Schedule = ResolveBindingExpressions(binding.Schedule);

        base.PrintFunctionInfo();
        elogger.LogInformation($"  Schedule:   {binding.Schedule}");
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (IsDisabled) { return; }

        try
        {
            PrintStatus(FunctionAction.Start);

            var parameters = PrepareParameters(stoppingToken);

            if (binding.RunOnStartup)
            {
                await funcInfo.InvokeAsync(parameters, loggerFactory);
            }

            var schedule = CrontabSchedule.Parse(binding.Schedule, new CrontabSchedule.ParseOptions { IncludingSeconds = true });

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var next = schedule.GetNextOccurrence(now);
                var timeSpan = next - now;

                await Task.Delay(timeSpan, stoppingToken);

                await funcInfo.InvokeAsync(parameters, loggerFactory);

                while (schedule.GetNextOccurrence(DateTime.UtcNow) == next && !stoppingToken.IsCancellationRequested)
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
