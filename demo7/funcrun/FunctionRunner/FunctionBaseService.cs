using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace FunctionRunner
{
    class FunctionBaseService(FunctionInfo funcInfo, ILogger? logger) : BackgroundService
    {
        protected readonly FunctionInfo _funcInfo = funcInfo;
        protected readonly ILogger? _logger = logger;
        protected readonly FunctionBinding _binding = funcInfo.Function.Bindings[0];

        protected bool _isDisabled => _funcInfo.Builder.Provider
                                        .GetRequiredService<IConfiguration>()
                                        .GetValue<bool>($"AzureWebJobs.{_funcInfo.Name}.Disabled");

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public virtual void PrintFunctionInfo(bool unsupported = false)
        {
            _binding.Schedule = ResolveBindingExpressions(_binding.Schedule);
            _binding.QueueName = ResolveBindingExpressions(_binding.QueueName);

            var suffix = unsupported ? $" {ConsoleColor.Red}(Unsupported){ConsoleColor.Default}" : string.Empty;
            suffix += _isDisabled ? $" {ConsoleColor.Yellow}(Disabled){ConsoleColor.Default}" : string.Empty;
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
        }

        static readonly Dictionary<FunctionAction, string> messages = new()
        {
            [FunctionAction.Start] = "is starting",
            [FunctionAction.Stop] = "is stopped",
        };

        protected void PrintStatus(FunctionAction action)
        {
            var message = messages.TryGetValue(action, out var result) ? result : string.Empty;

            Console.WriteLine($"{ConsoleStyle.TimeStamp}{ConsoleColor.Cyan}{_funcInfo.Name}{ConsoleColor.Default} {message}");
        }

        static readonly Regex _expressionPattern = new("%([^%]+)%");

        string? ResolveBindingExpressions(string? expression)
        {
            if (expression != null)
            {
                expression = _expressionPattern.Replace(expression,
                    match => _funcInfo.Builder.Provider
                                .GetRequiredService<IConfiguration>()
                                .GetValue<string>(match.Groups[1].Value, match.Value));
            }

            return expression;
        }
    }
}