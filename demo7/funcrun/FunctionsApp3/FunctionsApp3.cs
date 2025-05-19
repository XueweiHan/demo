using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionsApp3;

public class FunctionX
{
    private readonly ILogger _logger;

    public FunctionX(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<FunctionX>();
    }

    [Function("FunctionX")]
    public void Run([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);
    }
}

public class FunctionY
{
    private readonly ILogger<FunctionY> _logger;

    public FunctionY(ILogger<FunctionY> logger)
    {
        _logger = logger;
    }

    [Function(nameof(FunctionY))]
    public async Task Run(
        [ServiceBusTrigger("queue1", Connection = "MyServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        // Complete the message
        await messageActions.CompleteMessageAsync(message);
    }
}