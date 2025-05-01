using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FunctionRunner
{
    internal class FunctionBaseService : BackgroundService
    {
        readonly ILogger<FunctionBaseService>? _logger;

        protected FunctionInfo _funcInfo;
        protected FunctionBinding _binding;

        public FunctionBaseService(FunctionInfo funcInfo, ILogger<FunctionBaseService>? logger)
        {
            _funcInfo = funcInfo;
            _logger = logger;
            _binding = _funcInfo.Function.Bindings[0];
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public virtual void PrintFunctionInfo(bool unsupported = false)
        {
            var suffix = unsupported ? $" {ConsoleColor.Red}(Unsupported){ConsoleColor.Default}" : string.Empty;
            Console.WriteLine($"{ConsoleColor.Blue}{_funcInfo.Name}:{ConsoleColor.Default} {_funcInfo.Function.Bindings[0].Type}{suffix}");
            Console.WriteLine($"  File:       {_funcInfo.Function.ScriptFile}");
            Console.WriteLine($"  Entry:      {_funcInfo.Function.EntryPoint}");
        }

        protected bool FillParameter(string? typeName, ref object? obj, CancellationToken stoppingToken)
        {
            switch (typeName)
            {
                case "Microsoft.Extensions.Logging.ILogger":
                    obj = _logger;
                    return true;
                case "System.Threading.CancellationToken":
                    obj = stoppingToken;
                    return true;
            }

            return false;
        }

        protected bool Disabled()
        {
            var disabled = Environment.GetEnvironmentVariable($"AzureWebJobs.{_funcInfo.Name}.Disabled");
            return bool.TryParse(disabled, out var isDisabled) && isDisabled;
        }

        protected enum FunctionAction
        {
            Start,
            Stop,
            Disabled
        }

        static readonly Dictionary<FunctionAction, string> messages = new()
        {
            [FunctionAction.Start] = " is starting",
            [FunctionAction.Stop] = " is stopped",
            [FunctionAction.Disabled] = " is disabled"
        };

        protected void PrintStatus(FunctionAction action)
        {

            var message = messages.TryGetValue(action, out var result) ? result : string.Empty;

            Console.WriteLine($"[{ConsoleColor.Cyan}{_funcInfo.Name}{ConsoleColor.Default}{message} at {DateTime.UtcNow:u}]");
        }
    }
}
