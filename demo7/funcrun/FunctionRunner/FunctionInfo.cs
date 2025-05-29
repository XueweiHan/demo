// <copyright file="FunctionInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using System.Reflection;
using Microsoft.Extensions.Logging;

namespace FunctionRunner;

/// <summary>
/// Represents a function binding configuration.
/// </summary>
class FunctionBinding
{
    /// <summary>
    /// Gets or sets the type of the binding.
    /// </summary>
    required public string Type { get; set; }

    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    public string? Connection { get; set; }

    /// <summary>
    /// Gets or sets the queue name.
    /// </summary>
    public string? QueueName { get; set; }

    /// <summary>
    /// Gets or sets the schedule.
    /// </summary>
    public string? Schedule { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the function should run on startup.
    /// </summary>
    public bool RunOnStartup { get; set; }
}

/// <summary>
/// Represents a function definition.
/// </summary>
class FunctionDefinition
{
    /// <summary>
    /// Gets or sets the function name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the entry point.
    /// </summary>
    required public string EntryPoint { get; set; }

    /// <summary>
    /// Gets or sets the script file.
    /// </summary>
    required public string ScriptFile { get; set; }

    /// <summary>
    /// Gets or sets the function bindings.
    /// </summary>
    required public FunctionBinding[] Bindings { get; set; }
}

/// <summary>
/// Provides information and invocation logic for a function.
/// </summary>
class FunctionInfo(FunctionDefinition function, Type type, MethodInfo method, IServiceProvider serviceProvider, string name)
{
    /// <summary>
    /// Gets the function definition.
    /// </summary>
    public FunctionDefinition Function { get; } = function;

    /// <summary>
    /// Gets the method parameters.
    /// </summary>
    public ParameterInfo[] Parameters { get; } = method.GetParameters();

    /// <summary>
    /// Gets the function name.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the service provider.
    /// </summary>
    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    readonly Type type = type;
    readonly MethodInfo method = method;
    readonly TimeSpan timeout = GetFunctionTimeout(method);

    /// <summary>
    /// Invokes the function asynchronously.
    /// </summary>
    /// <param name="parameters">The parameters to pass to the function.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <returns>True if the function executed successfully; otherwise, false.</returns>
    public async Task<bool> InvokeAsync(object?[] parameters, ILoggerFactory loggerFactory)
    {
        bool success = false;

        loggerFactory.CreateLogger("T.Cyan0").LogInformation($"{Name} is triggered");

        try
        {
            int cancelTokenIndex = Array.FindIndex(parameters, p => p is CancellationToken);

            if (timeout != TimeSpan.Zero &&
                cancelTokenIndex > -1 &&
                parameters[cancelTokenIndex] is CancellationToken originalToken)
            {
                using var timeoutCts = new CancellationTokenSource(timeout);
                using var joinedCts = CancellationTokenSource.CreateLinkedTokenSource(originalToken, timeoutCts.Token);

                parameters[cancelTokenIndex] = joinedCts.Token;

                try
                {
                    await InvokeAsyncCore(parameters);
                    success = true;
                }
                catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
                {
                    loggerFactory.CreateLogger("T.Cyan0.Red1.Red2").LogWarning($"{Name} timed out");
                }
                catch (OperationCanceledException)
                {
                    loggerFactory.CreateLogger("T.Cyan0").LogWarning($"{Name} is cancelled");
                }
                finally
                {
                    parameters[cancelTokenIndex] = originalToken;
                }
            }
            else
            {
                await InvokeAsyncCore(parameters);
                success = true;
            }
        }
        catch (StackOverflowException) { throw; }
        catch (OutOfMemoryException) { throw; }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("T.Cyan0.Red3").LogError(ex, $"{Name} encountered an exception");
        }

        return success;
    }

    async Task InvokeAsyncCore(object?[] parameters)
    {
        var instance = ServiceProvider.GetService(type);

        var result = method.Invoke(instance, parameters);

        if (result is Task task)
        {
            await task;
        }
    }

    /// <summary>
    /// Loads all function infos from the specified root directory.
    /// </summary>
    /// <param name="root">The root directory.</param>
    /// <param name="loggerFactory"> The logger factory.</param>
    /// <returns>A list of <see cref="FunctionInfo"/> objects.</returns>
    public static List<FunctionInfo> Load(string root, ILoggerFactory loggerFactory)
    {
        root = Path.GetFullPath(root);
        Directory.SetCurrentDirectory(root);
        var funcInfos = new List<FunctionInfo>();

        LoadInProcessFunctions(root, funcInfos, loggerFactory);

        LoadIsolatedFunctions(root, funcInfos, loggerFactory);

        return funcInfos;
    }

    static void LoadInProcessFunctions(string root, List<FunctionInfo> funcInfos, ILoggerFactory loggerFactory)
    {
        var functionJsonFiles = Directory.GetFiles(root, "function.json", SearchOption.AllDirectories);
        foreach (var file in functionJsonFiles)
        {
            var functionJson = File.ReadAllText(file);
            var function = JsonHelper.Deserialize<FunctionDefinition>(functionJson);
            if (function == null) { continue; }

            var functionRoot = Path.GetFullPath(Path.GetDirectoryName(Path.GetDirectoryName(file)!)!);
            function.ScriptFile = Path.GetRelativePath(root, Path.Combine(functionRoot, Path.GetFileName(function.ScriptFile)));

            var dllPath = function.ScriptFile;
            var typeName = Path.GetFileNameWithoutExtension(function.EntryPoint);
            var methodName = Path.GetExtension(function.EntryPoint).TrimStart('.');

            var assembly = IsolatedLoadContext.LoadAssembly(dllPath, loggerFactory);
            var targetType = assembly.GetType(typeName)!;
            var method = targetType.GetMethod(methodName)!;

            funcInfos.Add(new FunctionInfo(
                function: function,
                type: targetType,
                method: method,
                serviceProvider: assembly.ServiceProviderBuild(functionRoot, targetType),
                name: Path.GetFileName(Path.GetDirectoryName(file))!));
        }
    }

    static void LoadIsolatedFunctions(string root, List<FunctionInfo> funcInfos, ILoggerFactory loggerFactory)
    {
        var functionJsonFiles = Directory.GetFiles(root, "functions.metadata", SearchOption.AllDirectories);
        foreach (var file in functionJsonFiles)
        {
            var functionJson = File.ReadAllText(file);
            var functions = JsonHelper.Deserialize<FunctionDefinition[]>(functionJson);
            foreach (var function in functions ?? Array.Empty<FunctionDefinition>())
            {
                var functionRoot = Path.GetFullPath(Path.GetDirectoryName(file)!);
                function.ScriptFile = Path.GetRelativePath(root, Path.Combine(functionRoot, Path.GetFileName(function.ScriptFile)));

                var dllPath = function.ScriptFile;
                var typeName = Path.GetFileNameWithoutExtension(function.EntryPoint);
                var methodName = Path.GetExtension(function.EntryPoint).TrimStart('.');
                
                var assembly = IsolatedLoadContext.LoadAssembly(dllPath, loggerFactory);
                var targetType = assembly.GetType(typeName)!;
                var method = targetType.GetMethod(methodName)!;

                funcInfos.Add(new FunctionInfo(
                    function: function,
                    type: targetType,
                    method: method,
                    serviceProvider: assembly.ServiceProviderIsolatedBuild(functionRoot, targetType),
                    name: function.Name!));
            }
        }
    }

    static TimeSpan GetFunctionTimeout(MethodInfo method)
    {
        var timeoutAttribute = method.GetCustomAttributes()
            .FirstOrDefault(a => a.GetType().FullName == "Microsoft.Azure.WebJobs.TimeoutAttribute");
        return (TimeSpan)(timeoutAttribute?.GetType().GetProperty("Timeout")?.GetValue(timeoutAttribute) ?? TimeSpan.Zero);
    }
}
