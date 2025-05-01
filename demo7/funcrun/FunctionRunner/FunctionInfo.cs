using Newtonsoft.Json;
using System.Reflection;

namespace FunctionRunner
{
    internal class FunctionBinding
    {
        public required string Type { get; set; }
        public string? Connection { get; set; }
        public string? QueueName { get; set; }
        public string? Schedule { get; set; }
        public bool RunOnStartup { get; set; }
    }

    internal class FunctionDefinition
    {
        //public required bool Disabled { get; set; }
        public required string EntryPoint { get; set; }
        public required string ScriptFile { get; set; }
        public required FunctionBinding[] Bindings { get; set; }
    }

    internal class FunctionInfo(FunctionDefinition function, dynamic instance, MethodInfo method, ParameterInfo[] parameters, string name, TimeSpan timeout)
    {
        public FunctionDefinition Function { get; set; } = function;
        public ParameterInfo[] Parameters { get; set; } = parameters;
        public string Name { get; set; } = name;

        readonly dynamic _instance = instance;
        readonly MethodInfo _method = method;
        readonly TimeSpan _timeout = timeout;

        public async Task InvokeAsync(object?[] parameters)
        {
            for (int i = 0; i < parameters.Length; ++i)
            {
                if (parameters[i] is CancellationToken originalToken)
                {
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(originalToken);
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(_timeout, originalToken);
                        Console.WriteLine($"[{ConsoleColor.Cyan}{Name}{ConsoleColor.Default} time out at {DateTime.UtcNow:u}]");
                        cts.Cancel();
                        cts.Dispose();
                    });

                    parameters[i] = cts.Token;
                }
            }

            Console.WriteLine($"[{ConsoleColor.Cyan}{Name}{ConsoleColor.Default} is triggered at {DateTime.UtcNow:u}]");

            try
            {
                dynamic? result = _method.Invoke(_instance, parameters);

                if (result is Task task)
                {
                    await task;
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                Console.WriteLine($"[{ConsoleColor.Cyan}{Name} {ConsoleColor.Red}exception{ConsoleColor.Default} at {DateTime.UtcNow:u}] {ex.GetType()} : {ex.StackTrace}");
            }
        }

        public static List<FunctionInfo> Load()
        {
            var funcInfos = new List<FunctionInfo>();

            var root = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
            if (root == null)
            {
                Console.WriteLine("AzureWebJobsScriptRoot is not set.");
                return funcInfos;
            }

            var functionJsonFiles = Directory.GetFiles(root, "function.json", SearchOption.AllDirectories);
            foreach (var file in functionJsonFiles)
            {
                var functionJson = File.ReadAllText(file);
                var function = JsonConvert.DeserializeObject<FunctionDefinition>(functionJson);
                if (function == null) { continue; }

                var dllPath = Path.GetFullPath(Path.Combine(root, Path.GetFileName(function.ScriptFile)));
                var typeName = Path.GetFileNameWithoutExtension(function.EntryPoint);
                var methodName = Path.GetExtension(function.EntryPoint).TrimStart('.');

                var dll = LoadFunction(dllPath);
                var type = dll.GetType(typeName);
                if (type != null)
                {
                    dynamic? instance = Activator.CreateInstance(type);
                    var method = type.GetMethod(methodName);
                    if (method != null && instance != null)
                    {
                        var timeoutAttribute = method?.GetCustomAttributes()
                            .FirstOrDefault(a => a.GetType().FullName == "Microsoft.Azure.WebJobs.TimeoutAttribute");
                        var timeout = (TimeSpan)(timeoutAttribute?.GetType().GetProperty("Timeout")?.GetValue(timeoutAttribute) ?? TimeSpan.Zero);

                        funcInfos.Add(new FunctionInfo(
                            function!,
                            instance!,
                            method!,
                            method!.GetParameters().ToArray()!,
                            Path.GetFileName(Path.GetDirectoryName(file))!,
                            timeout));
                    }
                }
            }

            return funcInfos;
        }

        static Assembly LoadFunction(string functionBinaryLocation)
        {
            var loadContext = new FunctionLoadContext(functionBinaryLocation);
            return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(functionBinaryLocation)));
        }
    }
}
