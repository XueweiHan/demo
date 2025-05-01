using Microsoft.Extensions.Logging;
using NCrontab;

namespace FunctionRunner
{
    internal class FunctionTimerTriggerService(FunctionInfo funcInfo, ILogger<FunctionBaseService> logger)
        : FunctionBaseService(funcInfo, logger)
    {
        public override void PrintFunctionInfo(bool u)
        {
            base.PrintFunctionInfo();
            Console.WriteLine($"  Schedule:   {_binding.Schedule}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                PrintStatus(FunctionAction.Start);
                var parameters = new List<object?>();
                foreach (var p in _funcInfo.Parameters)
                {
                    object? obj = null;
                    FillParameter(p.ParameterType.FullName, ref obj, stoppingToken);
                    parameters.Add(obj);
                }

                async Task runAsync()
                {
                    if (Disabled())
                    {
                        PrintStatus(FunctionAction.Disabled);
                        return;
                    }

                    if (stoppingToken.IsCancellationRequested)
                    {
                        throw new OperationCanceledException("Operation was cancelled", stoppingToken);
                    }

                    await _funcInfo.InvokeAsync(parameters.ToArray());
                }

                if (_binding.RunOnStartup)
                {
                    await runAsync();
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

                    await runAsync();

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
