using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace FunctionRunner
{
    internal class FunctionServiceBusTriggerService(FunctionInfo funcInfo, ILogger<FunctionBaseService> logger)
        : FunctionBaseService(funcInfo, logger)
    {
        public override void PrintFunctionInfo(bool u)
        {
            base.PrintFunctionInfo(u);
            Console.WriteLine($"  QueueName:  {_binding.QueueName}");
            Console.WriteLine($"  Connection: {_binding.Connection}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            PrintStatus(FunctionAction.Start);

            var fullyQualifiedNamespace = Environment.GetEnvironmentVariable(_binding.Connection + "__fullyQualifiedNamespace") ?? string.Empty;

            var client = new ServiceBusClient(
                fullyQualifiedNamespace,
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
            var parameters = new List<object?>();
            foreach (var p in _funcInfo.Parameters)
            {
                object? obj = null;
                if (!base.FillParameter(p.ParameterType.FullName, ref obj, args.CancellationToken)
                    && p.ParameterType.FullName == "System.String")
                {
                    // TODO: do we need to check the attribute on the parameter?
                    parameters.Add(body);
                    continue;
                }

                parameters.Add(obj);
            }

            if (args.CancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException("Operation was cancelled", args.CancellationToken);
            }

            await _funcInfo.InvokeAsync(parameters.ToArray());

            // complete the message. message is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message, args.CancellationToken);
        }

        Task ErrorHandlerAsync(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}