using Microsoft.Extensions.Logging;
using NCrontab;

namespace FunctionRunner
{
    class FunctionTimerTriggerService(FunctionInfo funcInfo, ILogger logger)
        : FunctionBaseService(funcInfo, logger)
    {
        public override void PrintFunctionInfo(bool u)
        {
            base.PrintFunctionInfo();
            Console.WriteLine($"  Schedule:   {_binding.Schedule}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_funcInfo.IsDisabled())
            {
                PrintStatus(FunctionAction.Disabled);
                return;
            }

            try
            {
                PrintStatus(FunctionAction.Start);

                var parameters = PrepareParameters(stoppingToken);

                if (_binding.RunOnStartup)
                {
                    await _funcInfo.InvokeAsync(parameters);
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

                    await _funcInfo.InvokeAsync(parameters);

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