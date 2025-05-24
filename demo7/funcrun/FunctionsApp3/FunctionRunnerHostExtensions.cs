using Microsoft.Extensions.Hosting;

static class FunctionRunnerHostExtensions
{
    public static void Run(this IHost host)
    {
        (FunctionRunnerRun ?? HostingAbstractionsHostExtensions.Run)(host);
    }

    #pragma warning disable CS0649 // Field is never assigned to
    static Action<IHost>? FunctionRunnerRun;
}
