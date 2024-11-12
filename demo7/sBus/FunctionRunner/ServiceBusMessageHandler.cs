using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace FunctionRunner
{
    internal class ServiceBusMessageHandler
    {
        FunctionInfo _funcInfo;
        ILogger _logger;

        public ServiceBusMessageHandler(FunctionInfo funcInfo, ILogger logger)
        {
            _funcInfo = funcInfo;
            _logger = logger;

            var tasks = new List<Task>();
            foreach (var binding in funcInfo.Function.Bindings)
            {
                tasks.Add(ServiceBusWatcher(binding));
            }
            Task.WhenAll(tasks).Wait();
        }

        // handle received messages
        Task MessageHandlerAsync(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();

            Console.WriteLine($"{ConsoleColor.Cyan}[Received Message at {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}]{ConsoleColor.Default}");

            _ = Task.Run(async () =>
            {
                _funcInfo.Method.Invoke(_funcInfo.Instance, new object[] { body, _logger });
                // complete the message. message is deleted from the queue. 
                await args.CompleteMessageAsync(args.Message);
            });

            return Task.CompletedTask;
        }

        async Task MessageHandler(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            var name = Path.GetFileName(Path.GetDirectoryName(_funcInfo.JsonFilePath));

            Console.WriteLine($"{ConsoleColor.Cyan}[Received Message at {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}]{ConsoleColor.Default}");

            _funcInfo.Method.Invoke(_funcInfo.Instance, new object[] { body, _logger });

            // complete the message. message is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message);
        }

        // handle any errors when receiving messages
        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        async Task ServiceBusWatcher(FunctionBinding binding)
        {
            var fullyQualifiedNamespace = Environment.GetEnvironmentVariable(binding.Connection + "__fullyQualifiedNamespace");

            Console.WriteLine($"function.json: {Path.GetDirectoryName(_funcInfo.JsonFilePath)}");
            Console.WriteLine($"  File:        {_funcInfo.Function.ScriptFile}");
            Console.WriteLine($"  Entry:       {_funcInfo.Function.EntryPoint}");
            Console.WriteLine($"  Type:        {binding.Type}");
            Console.WriteLine($"  Connection:  {fullyQualifiedNamespace}");
            Console.WriteLine($"  Queue:       {binding.QueueName}");

            var client = new ServiceBusClient(
                fullyQualifiedNamespace,
                new DefaultAzureCredential(),
                new ServiceBusClientOptions()
                {
                    TransportType = ServiceBusTransportType.AmqpWebSockets
                });

            var processor = client.CreateProcessor(binding.QueueName, new ServiceBusProcessorOptions());

            // add handler to process messages
            processor.ProcessMessageAsync += MessageHandler;

            // add handler to process any errors
            processor.ProcessErrorAsync += ErrorHandler;

            // start processing 
            await processor.StartProcessingAsync();
        }
    }
}
