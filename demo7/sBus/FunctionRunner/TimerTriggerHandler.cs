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
            Console.WriteLine($"{ConsoleColor.Cyan}Function: {_name}{ConsoleColor.Default}");
            Console.WriteLine($"  File:       {_funcInfo.Function.ScriptFile}");
            Console.WriteLine($"  Entry:      {_funcInfo.Function.EntryPoint}");
            Console.WriteLine($"  Type:       {binding.Type}");
            Console.WriteLine($"  Schedule:   {binding.Schedule}");

            var _ = Run(binding.Schedule, () =>
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
                for (; ; )
                {
                    var now = DateTime.Now;
                    var next = schedule.GetNextOccurrence(now);
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
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}
