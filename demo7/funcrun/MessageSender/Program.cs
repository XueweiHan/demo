using Azure.Messaging.ServiceBus;
using Azure.Identity;

if (args.Length < 2)
{
    Console.WriteLine("Usage: <exe> <sbus-namespace> <queue> {count}");
    return;
}

var sbNamespace = args[0];
var queueName = args[1];
var countStr = args.Length == 3 ? args[2] : "1";
int.TryParse(countStr, out var numOfMessages);
// number of messages to be sent to the queue

Console.WriteLine($"Namespace: {sbNamespace}");
Console.WriteLine($"Queue:     {queueName}");
Console.WriteLine($"Count:     {numOfMessages}");

// name of your Service Bus queue
// the client that owns the connection and can be used to create senders and receivers
ServiceBusClient client;

// the sender used to publish messages to the queue
ServiceBusSender sender;

// The Service Bus client types are safe to cache and use as a singleton for the lifetime
// of the application, which is best practice when messages are being published or read
// regularly.
//
// Set the transport type to AmqpWebSockets so that the ServiceBusClient uses the port 443. 
// If you use the default AmqpTcp, ensure that ports 5671 and 5672 are open.
var clientOptions = new ServiceBusClientOptions
{
    TransportType = ServiceBusTransportType.AmqpWebSockets
};
//TODO: Replace the "<NAMESPACE-NAME>" and "<QUEUE-NAME>" placeholders.
client = new ServiceBusClient(
    $"{sbNamespace}.servicebus.windows.net",
    new DefaultAzureCredential(),
    clientOptions);
sender = client.CreateSender(queueName);

// create a batch 
using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

for (int i = 1; i <= numOfMessages; i++)
{
    // try adding a message to the batch
    if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i} {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}")))
    {
        // if it is too large for the batch
        throw new Exception($"The message {i} is too large to fit in the batch.");
    }
}

try
{
    // Use the producer client to send the batch of messages to the Service Bus queue
    await sender.SendMessagesAsync(messageBatch);
    Console.WriteLine($"A batch of {numOfMessages} messages has been published to the queue.");
}
finally
{
    // Calling DisposeAsync on client types is required to ensure that network
    // resources and other unmanaged objects are properly cleaned up.
    await sender.DisposeAsync();
    await client.DisposeAsync();
}

//Console.WriteLine("Press any key to end the application");
//Console.ReadKey();
