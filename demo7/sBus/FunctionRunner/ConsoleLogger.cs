using Microsoft.Extensions.Logging;

namespace FunctionRunner
{
    internal class ConsoleLogger : ILogger
    {
        Dictionary<LogLevel, string> logLevelToString { get; set; } = new()
        {
            [LogLevel.Information] = "info",
            [LogLevel.Warning] = "warn",
            [LogLevel.Error] = "fail",
            [LogLevel.Critical] = "crit",
        };

        Dictionary<LogLevel, string> logLevelToColor { get; set; } = new()
        {
            [LogLevel.Information] = ConsoleColor.Green,
            [LogLevel.Warning] = ConsoleColor.Yellow,
            [LogLevel.Error] = ConsoleColor.Black + ConsoleBackgroundColor.Red,
            [LogLevel.Critical] = ConsoleColor.White + ConsoleBackgroundColor.Red,
        };

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

        public bool IsEnabled(LogLevel logLevel) => logLevelToString.ContainsKey(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var COLOR = logLevelToColor.GetValueOrDefault(logLevel, "");
            var NORMAL = ConsoleColor.Default + ConsoleBackgroundColor.Default;

            var levelStr = logLevelToString.GetValueOrDefault(logLevel, "none");
            Console.WriteLine($"{COLOR}{levelStr,-4}{NORMAL}: {formatter(state, exception)}");
        }
    }
}
