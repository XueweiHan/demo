// <copyright file="FunctionBaseService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FunctionRunner;

/// <summary>
/// Base service for function execution and management.
/// </summary>
class FunctionBaseService(FunctionInfo funcInfo, ILoggerFactory loggerFactory) : BackgroundService
{
    /// <summary>
    /// Gets the function information.
    /// </summary>
    protected readonly FunctionInfo funcInfo = funcInfo;

    /// <summary>
    /// Gets the logger factory.
    /// </summary>
    protected readonly ILoggerFactory loggerFactory = loggerFactory;

    /// <summary>
    /// Gets the logger for general logging.
    /// </summary>
    protected readonly ILogger elogger = loggerFactory.CreateLogger(string.Empty);

    /// <summary>
    /// Gets the logger for function-specific logging.
    /// </summary>
    protected readonly ILogger logger = loggerFactory.CreateLogger("T.Cyan0");

    /// <summary>
    /// Gets the function binding.
    /// </summary>
    protected readonly FunctionBinding binding = funcInfo.Function.Bindings[0];

    /// <summary>
    /// Gets a value indicating whether the function is disabled.
    /// </summary>
    protected bool IsDisabled => funcInfo.ServiceProvider
                                    .GetRequiredService<IConfiguration>()
                                    .GetValue<bool>($"AzureWebJobs.{funcInfo.Name}.Disabled");

    /// <inheritdoc/>
    public override void Dispose()
    {
        loggerFactory.Dispose();
        base.Dispose();
    }

    /// <inheritdoc/>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Prints function information to the logger.
    /// </summary>
    /// <param name="unsupported">Indicates if the function is unsupported.</param>
    public virtual void PrintFunctionInfo(bool unsupported = false)
    {
        binding.Schedule = ResolveBindingExpressions(binding.Schedule);
        binding.QueueName = ResolveBindingExpressions(binding.QueueName);

        var suffix = unsupported ? $" (Unsupported)" :
                     IsDisabled ? $" (Disabled)" :
                                   string.Empty;

        var category = unsupported ? "Cyan0.Red-1" : "Cyan0.Magenta2";

        loggerFactory.CreateLogger(category).LogInformation($"{funcInfo.Name}: {funcInfo.Function.Bindings[0].Type}{suffix}");
        elogger.LogInformation($"  File:       {funcInfo.Function.ScriptFile}");
        elogger.LogInformation($"  Entry:      {funcInfo.Function.EntryPoint}");
    }

    /// <summary>
    /// Prepares parameters for function invocation.
    /// </summary>
    /// <param name="arg">Arguments to inject.</param>
    /// <returns>Array of prepared parameters.</returns>
    protected object?[] PrepareParameters(params object[] arg)
    {
        var services = new ServiceCollection();
        foreach (var a in arg)
        {
            services.AddSingleton(a.GetType(), a);
        }
        services
            .AddLogging(loggingBuilder => loggingBuilder.Build(services))
            .AddSingleton<ILogger>(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger(funcInfo.Function.EntryPoint));

        using var serviceProvider = services.BuildServiceProvider();

        return funcInfo.Parameters
            .Select(p => serviceProvider.GetService(p.ParameterType))
            .ToArray();
    }

    /// <summary>
    /// Function action enumeration.
    /// </summary>
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

    /// <summary>
    /// Prints the status of the function.
    /// </summary>
    /// <param name="action">The action performed.</param>
    protected void PrintStatus(FunctionAction action)
    {
        var message = messages.TryGetValue(action, out var result) ? result : string.Empty;

        logger.LogInformation($"{funcInfo.Name} {message}");
    }

    static readonly Regex expressionPattern = new("%([^%]+)%");

    /// <summary>
    /// Resolves binding expressions in the given string.
    /// </summary>
    /// <param name="expression">The expression to resolve.</param>
    /// <returns>The resolved expression.</returns>
    string? ResolveBindingExpressions(string? expression)
    {
        if (expression != null)
        {
            expression = expressionPattern.Replace(
                expression,
                match => funcInfo.ServiceProvider
                            .GetRequiredService<IConfiguration>()
                            .GetValue<string>(match.Groups[1].Value, match.Value));
        }

        return expression;
    }
}
