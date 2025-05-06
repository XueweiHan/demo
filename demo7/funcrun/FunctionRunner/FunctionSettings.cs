namespace FunctionRunner
{
    static class FunctionSettings
    {
        public static bool IsDisabled(this FunctionInfo funcInfo)
        {
            var disabled = Environment.GetEnvironmentVariable($"AzureWebJobs.{funcInfo.Name}.Disabled");
            return bool.TryParse(disabled, out var isDisabled) && isDisabled;
        }

        public static string FullyQualifiedNamespace(this FunctionInfo funcInfo)
        {
            return Environment.GetEnvironmentVariable($"{funcInfo.Function.Bindings[0].Connection}__fullyQualifiedNamespace") ?? string.Empty;
        }
    }
}