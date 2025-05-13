using Microsoft.Extensions.Logging;
using System.Reflection;

namespace FunctionRunner
{
    class FunctionBinding
    {
        public required string Type { get; set; }
        public string? Connection { get; set; }
        public string? QueueName { get; set; }
        public string? Schedule { get; set; }
        public bool RunOnStartup { get; set; }
    }

    class FunctionDefinition
    {
        public required string EntryPoint { get; set; }
        public required string ScriptFile { get; set; }
        public required FunctionBinding[] Bindings { get; set; }
    }

    class FunctionInfo(FunctionDefinition function, FunctionInstanceProvider instanceProvider, Type type, MethodInfo method, ParameterInfo[] parameters, string name, TimeSpan timeout)
    {
        public FunctionDefinition Function { get; } = function;
        public ParameterInfo[] Parameters { get; } = parameters;
        public string Name { get; } = name;
        public FunctionInstanceProvider InstanceProvider { get; } = instanceProvider;

        readonly Type _type = type;
        readonly MethodInfo _method = method;
        readonly TimeSpan _timeout = timeout;

        public async Task<bool> InvokeAsync(object?[] parameters)
        {
            var t = typeof(Task);

            bool success = false;

            var name = $"{ConsoleColor.Cyan}{Name}{ConsoleColor.Default}";

            Console.WriteLine($"[{name} is triggered at {DateTime.UtcNow:u}]");

            try
            {
                int cancelTokenIndex = Array.FindIndex(parameters, p => p is CancellationToken);

                if (_timeout != TimeSpan.Zero &&
                    cancelTokenIndex > -1 &&
                    parameters[cancelTokenIndex] is CancellationToken originalToken)
                {
                    using var timeoutCts = new CancellationTokenSource(_timeout);
                    using var joinedCts = CancellationTokenSource.CreateLinkedTokenSource(originalToken, timeoutCts.Token);

                    parameters[cancelTokenIndex] = joinedCts.Token;

                    try
                    {
                        await InvokeAsyncCore(parameters);
                        success = true;
                    }
                    catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
                    {
                        Console.WriteLine($"[{name} {ConsoleBackgroundColor.Red}timed out{ConsoleBackgroundColor.Default} at {DateTime.UtcNow:u}]");
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine($"[{name} is cancelled at {DateTime.UtcNow:u}]");
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
                Console.Error.WriteLine($"[{name} encountered an {ConsoleBackgroundColor.Red}exception{ConsoleBackgroundColor.Default} at {DateTime.UtcNow:u}] {ex.GetType()} : {ex.StackTrace}");
            }

            return success;
        }

        async Task InvokeAsyncCore(object?[] parameters)
        {
            var instance = this.InstanceProvider.Create(_type);

            dynamic? result = _method.Invoke(instance, parameters);

            if (result is Task task)
            {
                await task;
            }
        }

        public static List<FunctionInfo> Load(string root)
        {
            root = Path.GetFullPath(root);
            Directory.SetCurrentDirectory(root);
            var funcInfos = new List<FunctionInfo>();

            var pathToInstanceProvider = new Dictionary<string, FunctionInstanceProvider>();

            var functionJsonFiles = Directory.GetFiles(root, "function.json", SearchOption.AllDirectories);
            foreach (var file in functionJsonFiles)
            {
                var functionJson = File.ReadAllText(file);
                var function = JsonHelper.Deserialize<FunctionDefinition>(functionJson);
                if (function == null) { continue; }

                var dllPath = Path.GetFullPath(Path.Combine(root, Path.GetFileName(function.ScriptFile)));
                var typeName = Path.GetFileNameWithoutExtension(function.EntryPoint);
                var methodName = Path.GetExtension(function.EntryPoint).TrimStart('.');

                var assembly = Assembly.LoadFrom(dllPath);
                var targetType = assembly.GetType(typeName)!;
                var method = targetType.GetMethod(methodName)!;

                if (!pathToInstanceProvider.TryGetValue(dllPath, out var instanceProvider))
                {
                    instanceProvider = new FunctionInstanceProvider(assembly, root);
                    pathToInstanceProvider[dllPath] = instanceProvider;
                }

                funcInfos.Add(new FunctionInfo(
                    function: function,
                    instanceProvider: instanceProvider,
                    type: targetType,
                    method: method,
                    parameters: method.GetParameters().ToArray()!,
                    name: Path.GetFileName(Path.GetDirectoryName(file))!,
                    timeout: GetFunctionTimeout(method)));
            }

            return funcInfos;
        }

        static TimeSpan GetFunctionTimeout(MethodInfo method)
        {
            var timeoutAttribute = method.GetCustomAttributes()
                .FirstOrDefault(a => a.GetType().FullName == "Microsoft.Azure.WebJobs.TimeoutAttribute");
            return (TimeSpan)(timeoutAttribute?.GetType().GetProperty("Timeout")?.GetValue(timeoutAttribute) ?? TimeSpan.Zero);
        }
    }
}