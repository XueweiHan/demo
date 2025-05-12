using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace FunctionRunner
{
    class FunctionServiceBusTriggerService(FunctionInfo funcInfo, ILogger<FunctionBaseService> logger)
        : FunctionBaseService(funcInfo, logger)
    {
        readonly string _fullyQualifiedNamespace = funcInfo.FullyQualifiedNamespace();

        public override void PrintFunctionInfo(bool u)
        {
            base.PrintFunctionInfo(u);
            Console.WriteLine($"  Connection: {_fullyQualifiedNamespace}");
            Console.WriteLine($"  Queue:      {_binding.QueueName}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_funcInfo.IsDisabled())
            {
                PrintStatus(FunctionAction.Disabled);
                return;
            }

            PrintStatus(FunctionAction.Start);

            await using var client = new ServiceBusClient(
                _fullyQualifiedNamespace,
                new DefaultAzureCredential(),
                new ServiceBusClientOptions()
                {
                    TransportType = ServiceBusTransportType.AmqpWebSockets
                });

            await using var processor = client.CreateProcessor(_binding.QueueName);

            // add handler to process messages
            processor.ProcessMessageAsync += MessageHandlerAsync;

            // add handler to process any errors
            processor.ProcessErrorAsync += ErrorHandlerAsync;

            try
            {
                // start processing 
                await processor.StartProcessingAsync(stoppingToken);

                // wait for cancellation
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            finally
            {
                // stop processing
                await processor.StopProcessingAsync();
                PrintStatus(FunctionAction.Stop);
            }
        }

        async Task MessageHandlerAsync(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();

            var parameters = PrepareParameters(body, args.CancellationToken);

            var success = await _funcInfo.InvokeAsync(parameters);

            if (success)
            {
                // complete the message. message is deleted from the queue. 
                await args.CompleteMessageAsync(args.Message, args.CancellationToken);
            }
            else
            {
                await args.AbandonMessageAsync(args.Message, null, args.CancellationToken);
            }
        }

        Task ErrorHandlerAsync(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}