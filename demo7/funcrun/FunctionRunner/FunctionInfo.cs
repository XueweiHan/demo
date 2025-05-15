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

    class FunctionInfo(FunctionDefinition function, Type type, MethodInfo method, IServiceProvider serviceProvider, string name)
    {
        public FunctionDefinition Function { get; } = function;
        public ParameterInfo[] Parameters { get; } = method.GetParameters();
        public string Name { get; } = name;
        public IServiceProvider ServiceProvider { get; } = serviceProvider;

        readonly Type _type = type;
        readonly MethodInfo _method = method;
        readonly TimeSpan _timeout = GetFunctionTimeout(method);

        public async Task<bool> InvokeAsync(object?[] parameters)
        {
            bool success = false;

            var name = $"{ConsoleColor.Cyan}{Name}{ConsoleColor.Default}";

            Console.WriteLine($"{ConsoleStyle.TimeStamp}{name} is triggered");

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
                        Console.WriteLine($"{ConsoleStyle.TimeStamp}{name} {ConsoleBackgroundColor.Red}timed out{ConsoleBackgroundColor.Default}");
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine($"{ConsoleStyle.TimeStamp}{name} is cancelled");
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
                var exception = $"{ex}".Replace(Environment.NewLine, "\t");
                Console.Error.WriteLine($"{ConsoleStyle.TimeStamp}{name} encountered an {ConsoleBackgroundColor.Red}exception{ConsoleBackgroundColor.Default} {exception}");
            }

            return success;
        }

        async Task InvokeAsyncCore(object?[] parameters)
        {
            var instance = ServiceProvider.GetService(_type);

            dynamic? result = _method.Invoke(instance, parameters);

            if (result is Task task)
            {
                await task;
            }
        }

        public static List<FunctionInfo> Load(string root, Action<ILoggingBuilder> loggingBuilder)
        {
            root = Path.GetFullPath(root);
            Directory.SetCurrentDirectory(root);
            var funcInfos = new List<FunctionInfo>();

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

                var assembly = Assembly.LoadFrom(dllPath);
                var targetType = assembly.GetType(typeName)!;
                var method = targetType.GetMethod(methodName)!;

                funcInfos.Add(new FunctionInfo(
                    function: function,
                    type: targetType,
                    method: method,
                    serviceProvider: assembly.ServiceProviderBuild(functionRoot, targetType, loggingBuilder),
                    name: Path.GetFileName(Path.GetDirectoryName(file))!));
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