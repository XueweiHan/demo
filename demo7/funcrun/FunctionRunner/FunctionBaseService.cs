using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace FunctionRunner;

class FunctionBaseService(FunctionInfo funcInfo, ILoggerFactory loggerFactory) : BackgroundService
{
    protected readonly FunctionInfo _funcInfo = funcInfo;
    protected readonly ILoggerFactory _loggerFactory = loggerFactory;
    protected readonly ILogger _elogger = loggerFactory.CreateLogger("");
    protected readonly ILogger _logger = loggerFactory.CreateLogger("T.Cyan0");
    protected readonly FunctionBinding _binding = funcInfo.Function.Bindings[0];

    protected bool _isDisabled => _funcInfo.ServiceProvider
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

        var suffix = unsupported ? $" (Unsupported)" :
                     _isDisabled ? $" (Disabled)" :
                                   string.Empty;

        var category = unsupported ? "Cyan0.Red-1" : "Cyan0.Magenta2";

        _loggerFactory.CreateLogger(category).LogInformation($"{_funcInfo.Name}: {_funcInfo.Function.Bindings[0].Type}{suffix}");
        _elogger.LogInformation($"  File:       {_funcInfo.Function.ScriptFile}");
        _elogger.LogInformation($"  Entry:      {_funcInfo.Function.EntryPoint}");
    }

    protected object?[] PrepareParameters(params object[]? arg)
    {
        var services = new ServiceCollection();
        foreach (var a in arg ?? [])
        {
            services.AddSingleton(a.GetType(), a);
        }
        services
            .AddLogging(loggingBuilder => loggingBuilder.Build(services))
            .AddSingleton<ILogger>(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger(_funcInfo.Function.EntryPoint));

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

        _logger.LogInformation($"{_funcInfo.Name} {message}");
    }

    static readonly Regex _expressionPattern = new("%([^%]+)%");

    string? ResolveBindingExpressions(string? expression)
    {
        if (expression != null)
        {
            expression = _expressionPattern.Replace(expression,
                match => _funcInfo.ServiceProvider
                            .GetRequiredService<IConfiguration>()
                            .GetValue<string>(match.Groups[1].Value, match.Value));
        }

        return expression;
    }
}