using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FunctionRunner
{
    internal class FunctionRunnerService(ILogger<FunctionRunnerService> logger) : BackgroundService
    {
        readonly ILogger<FunctionRunnerService> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var functionInfos = FunctionInfo.Load();

                var handlers = new List<BaseTriggerHandler>();
                foreach (var info in functionInfos)
                {
                    BaseTriggerHandler? handler = null;

                    var type = info.Function.Bindings[0].Type;
                    if (type == "serviceBusTrigger")
                    {
                        handler = new ServiceBusMessageHandler(info, _logger, stoppingToken);
                    }
                    else if (type == "timerTrigger")
                    {
                        handler = new TimerTriggerHandler(info, _logger, stoppingToken);
                    }
                    else
                    {
                        handler = new BaseTriggerHandler(info, _logger, stoppingToken);
                        Console.WriteLine($"Unsupported trigger type: {type}");
                    }

                    handler.Disabled();
                    handler.PrintFunctionInfo();

                    if (!info.Function.Disabled)
                    {
                        handlers.Add(handler);
                    }
                }

                var tasks = handlers.Select(h => h.RunAsync());

                await Task.WhenAll(tasks);
            }
            finally
            {
                Console.WriteLine("FunctionRunnerService is stopped.");
            }
        }
    }
}