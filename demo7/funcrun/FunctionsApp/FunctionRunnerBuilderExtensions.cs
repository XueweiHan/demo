using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FunctionsApp
{
    static class FunctionRunnerBuilderExtensions
    {
        public static FunctionsHostBuilderContext GetContext(this IFunctionsHostBuilder builder)
        {
            return builder is IOptions<FunctionsHostBuilderContext> funcBuilder ? funcBuilder.Value : FunctionsBuilderExtensions.GetContext(builder);
        }
        public static FunctionsHostBuilderContext GetContext(this IFunctionsConfigurationBuilder builder)
        {
            return builder is IOptions<FunctionsHostBuilderContext> funcBuilder ? funcBuilder.Value : FunctionsBuilderExtensions.GetContext(builder);
        }
    }
}
