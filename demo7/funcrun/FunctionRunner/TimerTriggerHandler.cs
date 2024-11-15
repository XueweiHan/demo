using Microsoft.Extensions.Logging;
using NCrontab;

namespace FunctionRunner
{
    internal class TimerTriggerHandler
    {
        FunctionInfo _funcInfo;
        ILogger _logger;
        string? _name;

        public TimerTriggerHandler(FunctionInfo funcInfo, ILogger logger)
        {
            _funcInfo = funcInfo;
            _logger = logger;
            _name = Path.GetFileName(Path.GetDirectoryName(_funcInfo.JsonFilePath));

            foreach (var binding in funcInfo.Function.Bindings)
            {
                TimerStart(binding);
            }
        }

        void TimerStart(FunctionBinding binding)
        {
            Console.WriteLine($"{ConsoleColor.Yellow}{_name}:{ConsoleColor.Default} {binding.Type}");
            Console.WriteLine($"  File:       {_funcInfo.Function.ScriptFile}");
            Console.WriteLine($"  Entry:      {_funcInfo.Function.EntryPoint}");
            Console.WriteLine($"  Schedule:   {binding.Schedule}");

            _ = Run(binding.Schedule, () =>
            {
                Console.WriteLine($"[{ConsoleColor.Cyan}{_name}{ConsoleColor.Default} at {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}]");
                _funcInfo.Method.Invoke(_funcInfo.Instance, new object[] { null, _logger });
            });
        }

        static async Task Run(string cronExpression, Action action)
        {
            var schedule = CrontabSchedule.Parse(cronExpression, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
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
                            await Task.Delay(timeSpan);
                        }
                        break;
                    }
                    else
                    {
                        await Task.Delay(nextCheckTimeSpan);
                    }
                }
              
                action();

                while (schedule.GetNextOccurrence(DateTime.UtcNow) == next)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                }
            }
        }
    }
}
