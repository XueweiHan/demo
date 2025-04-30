using Newtonsoft.Json;
using System.Reflection;

namespace FunctionRunner
{
    public class FunctionBinding
    {
        public required string Type { get; set; }
        public string? Connection { get; set; }
        public string? QueueName { get; set; }
        public string? Schedule { get; set; }
        public bool RunOnStartup { get; set; }
    }

    public class FunctionDefinition
    {
        public required bool Disabled { get; set; }
        public required string EntryPoint { get; set; }
        public required string ScriptFile { get; set; }
        public required FunctionBinding[] Bindings { get; set; }
    }

    public class FunctionInfo
    {
        public required FunctionDefinition Function { get; set; }
        public required dynamic Instance { get; set; }
        public required MethodInfo Method { get; set; }
        public required ParameterInfo[] Parameters { get; set; }
        public required string Name { get; set; }

        public static List<FunctionInfo> Load(string root)
        {
            var funcInfos = new List<FunctionInfo>();

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
                        funcInfos.Add(new FunctionInfo
                        {
                            Function = function,
                            Method = method!,
                            Instance = instance!,
                            Parameters = method!.GetParameters().ToArray()!,
                            Name = Path.GetFileName(Path.GetDirectoryName(file))!
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
