using Microsoft.Extensions.Hosting;

static class FunctionRunnerHostExtensions
{
    static Action<IHost>? FunctionRunnerRun;

    public static void Run(this IHost host)
    {
        (FunctionRunnerRun ?? HostingAbstractionsHostExtensions.Run)(host);
    }
}
