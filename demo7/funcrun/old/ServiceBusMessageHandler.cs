using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace FunctionRunner
{
    internal class ServiceBusMessageHandler : BaseTriggerHandler
    {
        readonly string _fullyQualifiedNamespace;
        public ServiceBusMessageHandler(FunctionInfo funcInfo, ILogger logger, CancellationToken cancellationToken)
            : base(funcInfo, logger, cancellationToken)
        {
            _fullyQualifiedNamespace = Environment.GetEnvironmentVariable(_binding.Connection + "__fullyQualifiedNamespace") ?? string.Empty;
        }

        public override void PrintFunctionInfo()
        {
            base.PrintFunctionInfo();
            Console.WriteLine($"  Connection: {_fullyQualifiedNamespace}");
            Console.WriteLine($"  Queue:      {_binding.QueueName}");
        }

        public override async Task RunAsync()
        {
            var client = new ServiceBusClient(
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
                await processor.StartProcessingAsync(_cancellationToken);

                // wait for cancellation
                await Task.Delay(Timeout.Infinite, _cancellationToken);
            }
            finally
            {
                // stop processing
                await processor.StopProcessingAsync();
            }
        }

        // handle received messages
        async Task MessageHandlerAsync(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            var parameters = new List<object?>();
            foreach (var p in _funcInfo.Parameters)
            {
                object? obj = null;
                if (!FillParameter(p.ParameterType.FullName, ref obj) && p.ParameterType.FullName == "System.String")
                {
                    // TODO: do we need to check the attribute on the parameter?
                    parameters.Add(body);
                    continue;
                }

                parameters.Add(obj);
            }

            if (_cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException("Operation was cancelled", _cancellationToken);
            }

            Console.WriteLine($"[{ConsoleColor.Cyan}{_funcInfo.Name}{ConsoleColor.Default} message received at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}]");
            _funcInfo.Invoke(parameters.ToArray());

            // complete the message. message is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message, _cancellationToken);
        }

        // handle any errors when receiving messages
        Task ErrorHandlerAsync(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}
