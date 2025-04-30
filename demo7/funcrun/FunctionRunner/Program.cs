using Microsoft.Extensions.Logging;

namespace FunctionRunner
{
    internal class Program
    {
        static ILogger logger = new ConsoleLogger();

        static async Task Main(string[] args)
        {
            var root = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
            if (root == null)
            {
                Console.WriteLine("AzureWebJobsScriptRoot is not set.");
                return;
            }

            var functionInfos = FunctionInfo.Load(root);

            using var cts = new CancellationTokenSource();

            var tasks = new List<Task>();

            foreach (var info in functionInfos)
            {
                BaseTriggerHandler? handler = null;

                var type = info.Function.Bindings[0].Type;
                if (type == "serviceBusTrigger")
                {
                    handler = new ServiceBusMessageHandler(info, logger, cts.Token);
                }
                else if (type == "timerTrigger")
                {
                    handler = new TimerTriggerHandler(info, logger, cts.Token);
                }
                else
                {
                    handler = new BaseTriggerHandler(info, logger, cts.Token);
                    Console.WriteLine($"Unsupported trigger type: {type}");
                }

                handler.UpdateDisabled();
                handler.PrintFunctionInfo();

                if (!info.Function.Disabled)
                {
                    tasks.Add(handler.RunAsync());
                }
            }

            // var port = Environment.GetEnvironmentVariable("FuctionRunnerHttpPort");
            // int.TryParse(port, out var portNumber);
            // var httpHandler = new HttpHandler(portNumber);
            // httpHandler.Start();

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true; // Prevent the process from terminating.
                Console.WriteLine($"{Environment.NewLine}Cancelling...{Environment.NewLine}");
                cts.Cancel();
            };

            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                if (!cts.IsCancellationRequested)
                {
                    Console.WriteLine($"{Environment.NewLine}Exiting...{Environment.NewLine}");
                    cts.Cancel(false);
                }
            };

            Console.WriteLine($"{Environment.NewLine}Press Ctrl-C to exit.{Environment.NewLine}");

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Cancelled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exit Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"Exit.");
            }
        }
    }
}
