using Microsoft.Extensions.Logging;
using System.Reflection;

namespace FunctionRunner
{
    internal class BaseTriggerHandler
    {
        protected FunctionInfo _funcInfo;
        protected ILogger _logger;
        protected CancellationToken _cancellationToken;
        protected FunctionBinding _binding;

        public BaseTriggerHandler(FunctionInfo funcInfo, ILogger logger, CancellationToken cancellationToken)
        {
            _funcInfo = funcInfo;
            _logger = logger;
            _cancellationToken = cancellationToken;
            _binding = _funcInfo.Function.Bindings[0];
        }

        public virtual void PrintFunctionInfo()
        {
            Console.WriteLine($"{ConsoleColor.Yellow}{_funcInfo.Name}:{ConsoleColor.Default} {_funcInfo.Function.Bindings[0].Type}");
            Console.WriteLine($"  File:       {_funcInfo.Function.ScriptFile}");
            Console.WriteLine($"  Entry:      {_funcInfo.Function.EntryPoint}");
            Console.WriteLine($"  Disabled:   {_funcInfo.Function.Disabled}");
        }

        public virtual Task RunAsync()
        {
            return Task.CompletedTask;
        }

        protected bool FillParameter(ParameterInfo p, ref object? obj)
        {
            switch (p.ParameterType.FullName)
            {
                case "Microsoft.Extensions.Logging.ILogger":
                    obj = _logger;
                    return true;
                case "System.Threading.CancellationToken":
                    obj = _cancellationToken;
                    return true;
            }

            return false;
        }

        internal void UpdateDisabled()
        {
            var disabled = Environment.GetEnvironmentVariable($"AzureWebJobs.{_funcInfo.Name}.Disabled");
            if (bool.TryParse(disabled, out var isDisabled))
            {
                _funcInfo.Function.Disabled = isDisabled;
            }
        }
    }
}
