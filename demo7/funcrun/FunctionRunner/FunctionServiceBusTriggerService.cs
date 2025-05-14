using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FunctionRunner
{
    class FunctionServiceBusTriggerService(FunctionInfo funcInfo, ILogger logger)
        : FunctionBaseService(funcInfo, logger)
    {
        string? _fullyQualifiedNamespace => _funcInfo.Builder.Provider
                                                .GetRequiredService<IConfiguration>()
                                                .GetValue<string>($"{_binding.Connection}:fullyQualifiedNamespace");

        public override void PrintFunctionInfo(bool u)
        {
            base.PrintFunctionInfo();
            Console.WriteLine($"  Connection: {_fullyQualifiedNamespace}");
            Console.WriteLine($"  Queue:      {_binding.QueueName}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await base.ExecuteAsync(stoppingToken);
            if (_isDisabled)
            {
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