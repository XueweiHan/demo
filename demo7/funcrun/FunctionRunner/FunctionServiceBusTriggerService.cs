// <copyright file="FunctionServiceBusTriggerService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FunctionRunner;

/// <summary>
/// Service for running a function triggered by Azure Service Bus messages.
/// </summary>
class FunctionServiceBusTriggerService : FunctionBaseService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionServiceBusTriggerService"/> class.
    /// </summary>
    /// <param name="funcInfo">The function information.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public FunctionServiceBusTriggerService(FunctionInfo funcInfo, ILoggerFactory loggerFactory)
        : base(funcInfo, loggerFactory)
    {
    }

    /// <summary>
    /// Gets the fully qualified namespace from configuration.
    /// </summary>
    private string? FullyQualifiedNamespace => funcInfo.ServiceProvider
        .GetRequiredService<IConfiguration>()
        .GetValue<string>($"{binding.Connection}:fullyQualifiedNamespace");

    /// <inheritdoc/>
    public override void PrintFunctionInfo(bool u)
    {
        base.PrintFunctionInfo();
        elogger.LogInformation($"  Connection: {FullyQualifiedNamespace}");
        elogger.LogInformation($"  Queue:      {binding.QueueName}");
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await base.ExecuteAsync(stoppingToken);
        if (IsDisabled)
        {
            return;
        }

        PrintStatus(FunctionAction.Start);

        await using var client = new ServiceBusClient(
            FullyQualifiedNamespace,
            new DefaultAzureCredential(),
            new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            });

        await using var processor = client.CreateProcessor(binding.QueueName);

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

    /// <summary>
    /// Handles incoming Service Bus messages.
    /// </summary>
    /// <param name="args">The message event arguments.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task MessageHandlerAsync(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();

        var parameters = PrepareParameters(body, args.CancellationToken);

        var success = await funcInfo.InvokeAsync(parameters, loggerFactory);

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

    /// <summary>
    /// Handles errors that occur during message processing.
    /// </summary>
    /// <param name="args">The error event arguments.</param>
    /// <returns>A completed task.</returns>
    private Task ErrorHandlerAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception.ToString());
        return Task.CompletedTask;
    }
}
