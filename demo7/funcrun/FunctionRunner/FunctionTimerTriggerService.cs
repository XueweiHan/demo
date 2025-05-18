using Microsoft.Extensions.Logging;
using NCrontab;

namespace FunctionRunner
{
    class FunctionTimerTriggerService(FunctionInfo funcInfo, ILoggerFactory loggerFactory)
        : FunctionBaseService(funcInfo, loggerFactory)
    {
        public override void PrintFunctionInfo(bool u)
        {
            base.PrintFunctionInfo();
            _elogger.LogInformation($"  Schedule:   {_binding.Schedule}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await base.ExecuteAsync(stoppingToken);
            if (_isDisabled)
            {
                return;
            }

            try
            {
                PrintStatus(FunctionAction.Start);

                var parameters = PrepareParameters(stoppingToken);

                if (_binding.RunOnStartup)
                {
                    await _funcInfo.InvokeAsync(parameters, _loggerFactory);
                }

                var schedule = CrontabSchedule.Parse(_binding.Schedule, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
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

                    await _funcInfo.InvokeAsync(parameters, _loggerFactory);

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
}