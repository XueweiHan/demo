using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FunctionRunner
{
    class FunctionBaseService(FunctionInfo funcInfo, ILogger? logger) : BackgroundService
    {
        protected readonly FunctionInfo _funcInfo = funcInfo;
        protected readonly ILogger? _logger = logger;
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
            var services = new ServiceCollection();
            foreach (var a in arg ?? [])
            {
                services.AddSingleton(a.GetType(), a);
            }
            services.AddSingleton<ILogger>(_logger!);

            using var serviceProvider = services.BuildServiceProvider();

            return _funcInfo.Parameters
                .Select(p => serviceProvider.GetService(p.ParameterType))
                .ToArray();
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