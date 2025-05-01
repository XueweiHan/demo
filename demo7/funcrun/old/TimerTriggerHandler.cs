using Microsoft.Extensions.Logging;
using NCrontab;

namespace FunctionRunner
{
    internal class TimerTriggerHandler(FunctionInfo funcInfo, ILogger logger, CancellationToken cancellationToken) : BaseTriggerHandler(funcInfo, logger, cancellationToken)
    {
        public override void PrintFunctionInfo()
        {
            base.PrintFunctionInfo();
            Console.WriteLine($"  Schedule:   {_binding.Schedule}");
        }

        public override async Task RunAsync()
        {
            var parameters = new List<object?>();
            foreach (var p in _funcInfo.Parameters)
            {
                object? obj = null;
                FillParameter(p.ParameterType.FullName, ref obj);
                parameters.Add(obj);
            }

            void run()
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException("Operation was cancelled", _cancellationToken);
                }

                Console.WriteLine($"[{ConsoleColor.Cyan}{_funcInfo.Name}{ConsoleColor.Default} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}]");
                _funcInfo.Invoke(parameters.ToArray());
            }

            if (_binding.RunOnStartup)
            {
                run();
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
                            await Task.Delay(timeSpan, _cancellationToken);
                        }
                        break;
                    }
                    else
                    {
                        await Task.Delay(nextCheckTimeSpan, _cancellationToken);
                    }
                }

                run();

                while (schedule.GetNextOccurrence(DateTime.UtcNow) == next)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), _cancellationToken);
                }
            }
        }
    }
}
