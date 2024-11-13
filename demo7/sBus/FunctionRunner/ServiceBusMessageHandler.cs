using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace FunctionRunner
{
    internal class ServiceBusMessageHandler
    {
        FunctionInfo _funcInfo;
        ILogger _logger;
        string? _name;

        public ServiceBusMessageHandler(FunctionInfo funcInfo, ILogger logger)
        {
            _funcInfo = funcInfo;
            _logger = logger;
            _name = Path.GetFileName(Path.GetDirectoryName(_funcInfo.JsonFilePath));

            foreach (var binding in funcInfo.Function.Bindings)
            {
                ServiceBusWatcher(binding);
            }
        }

        // handle received messages
        Task MessageHandlerAsync(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"[{ConsoleColor.Cyan}{_name}{ConsoleColor.Default} message received at {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}]");

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

            Console.WriteLine($"[{ConsoleColor.Cyan}{_name}{ConsoleColor.Default} message received at {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}]");

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

        void ServiceBusWatcher(FunctionBinding binding)
        {
            var fullyQualifiedNamespace = Environment.GetEnvironmentVariable(binding.Connection + "__fullyQualifiedNamespace");

            Console.WriteLine($"{ConsoleColor.Cyan}Function: {_name}{ConsoleColor.Default}");
            Console.WriteLine($"  File:       {_funcInfo.Function.ScriptFile}");
            Console.WriteLine($"  Entry:      {_funcInfo.Function.EntryPoint}");
            Console.WriteLine($"  Type:       {binding.Type}");
            Console.WriteLine($"  Connection: {fullyQualifiedNamespace}");
            Console.WriteLine($"  Queue:      {binding.QueueName}");

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
            processor.StartProcessingAsync().Wait();
        }
    }
}
