using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FunctionRunner
{
    class FunctionBaseService(FunctionInfo funcInfo, ILogger<FunctionBaseService>? logger) : BackgroundService
    {
        protected readonly FunctionInfo _funcInfo = funcInfo;
        protected readonly ILogger<FunctionBaseService>? _logger = logger;
        protected readonly FunctionBinding _binding = funcInfo.Function.Bindings[0];

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public virtual void PrintFunctionInfo(bool unsupported = false)
        {
            var suffix = unsupported ? $" {ConsoleColor.Red}(Unsupported){ConsoleColor.Default}" : string.Empty;
            suffix += _funcInfo.IsDisabled() ? $" {ConsoleColor.Yellow}(Disabled){ConsoleColor.Default}" : string.Empty;
            Console.WriteLine($"{ConsoleColor.Blue}{_funcInfo.Name}:{ConsoleColor.Default} {_funcInfo.Function.Bindings[0].Type}{suffix}");
            Console.WriteLine($"  File:       {_funcInfo.Function.ScriptFile}");
            Console.WriteLine($"  Entry:      {_funcInfo.Function.EntryPoint}");
        }

        protected object?[] PrepareParameters(params object[]? arg)
        {
            var list = new List<object>(arg ?? [])
            {
                _logger!
            };

            var len = _funcInfo.Parameters.Length;
            var parameters = new object?[len];

            for (int i = 0; i < len; ++i)
            {
                var type = _funcInfo.Parameters[i].ParameterType;

                foreach (var obj in list)
                {
                    if (type.IsAssignableFrom(obj.GetType()))
                    {
                        parameters[i] = obj;
                        break;
                    }
                }
            }

            return parameters;
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
            [FunctionAction.Disabled] = " is disabled",
        };

        protected void PrintStatus(FunctionAction action)
        {
            var message = messages.TryGetValue(action, out var result) ? result : string.Empty;

            Console.WriteLine($"[{ConsoleColor.Yellow}{_funcInfo.Name}{ConsoleColor.Default}{message} at {DateTime.UtcNow:u}]");
        }
    }
}