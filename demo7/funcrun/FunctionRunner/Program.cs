using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;

namespace FunctionRunner
{
    public class FunctionBinding
    {
        public required string Type { get; set; }
        public string Connection { get; set; }
        public string QueueName { get; set; }
        public string Schedule { get; set; }
    }

    public class FunctionDefination
    {
        public required string EntryPoint { get; set; }
        public required string ScriptFile { get; set; }
        public required FunctionBinding[] Bindings { get; set; }
    }

    public class FunctionInfo
    {
        public required FunctionDefination Function { get; set; }
        public required dynamic Instance { get; set; }
        public required MethodInfo Method { get; set; }
        public required ParameterInfo[] Parameters { get; set; }
        public required string JsonFilePath { get; set; }
    }

    internal class Program
    {
        static ILogger logger = new ConsoleLogger();

        static void Main(string[] args)
        {
            var root = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
            if (root == null)
            {
                Console.WriteLine("AzureWebJobsScriptRoot is not set.");
                return;
            }

            var functionInfos = LoadFunctions(root);

            foreach (var info in functionInfos)
            {
                var type = info.Function.Bindings[0].Type;
                if (type == "serviceBusTrigger")
                {
                    new ServiceBusMessageHandler(info, logger);
                }
                else if (type == "timerTrigger")
                {
                    new TimerTriggerHandler(info, logger);
                }
            }

            // var port = Environment.GetEnvironmentVariable("FuctionRunnerHttpPort");
            // int.TryParse(port, out var portNumber);
            // var httpHandler = new HttpHandler(portNumber);
            // httpHandler.Start();

            Console.WriteLine($"{Environment.NewLine}Press Ctrl-C to exit.{Environment.NewLine}");
            Thread.Sleep(Timeout.Infinite);
        }

        static List<FunctionInfo> LoadFunctions(string root)
        {
            var funcInfos = new List<FunctionInfo>();

            var functionJsonFiles = Directory.GetFiles(root, "function.json", SearchOption.AllDirectories);
            foreach (var file in functionJsonFiles)
            {
                var functionJson = File.ReadAllText(file);
                var function = JsonConvert.DeserializeObject<FunctionDefination>(functionJson);
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
                        funcInfos.Add(new FunctionInfo
                        {
                            Function = function,
                            Method = method!,
                            Instance = instance!,
                            Parameters = method!.GetParameters().ToArray()!,
                            JsonFilePath = file,
                        });
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
