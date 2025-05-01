using Microsoft.Extensions.Logging;

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

        protected bool FillParameter(string? typeName, ref object? obj)
        {
            switch (typeName)
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

        internal bool Disabled()
        {
            var disabled = Environment.GetEnvironmentVariable($"AzureWebJobs.{_funcInfo.Name}.Disabled");
            return bool.TryParse(disabled, out var isDisabled) ? isDisabled: false;
        }
    }
}
