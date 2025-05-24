// <copyright file="FunctionServiceBusTriggerService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using Azure.Core;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FunctionRunner;

/// <summary>
/// Service for running a function triggered by Azure Service Bus messages.
/// </summary>
class FunctionServiceBusTriggerService(FunctionInfo funcInfo, ILoggerFactory loggerFactory)
    : FunctionBaseService(funcInfo, loggerFactory)
{
    readonly IConfiguration configuration = funcInfo.ServiceProvider.GetRequiredService<IConfiguration>();

    string? ConnectionString => configuration.GetValue<string>($"{binding.Connection}");

    string? FullyQualifiedNamespace => configuration.GetValue<string>($"{binding.Connection}:fullyQualifiedNamespace");

    string? ClientId => configuration.GetValue<string>($"{binding.Connection}:clientId")
                     ?? configuration.GetValue<string>($"AZURE_CLIENT_ID");

    string? TenantId => configuration.GetValue<string>($"{binding.Connection}:tenantId")
                     ?? configuration.GetValue<string>($"AZURE_TENANT_ID");

    /// <inheritdoc/>
    public override void PrintFunctionInfo(bool u)
    {
        binding.QueueName = ResolveBindingExpressions(binding.QueueName);

        base.PrintFunctionInfo();
        elogger.LogInformation($"  Connection: {FullyQualifiedNamespace}");
        elogger.LogInformation($"  Queue:      {binding.QueueName}");
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (IsDisabled) { return; }

        PrintStatus(FunctionAction.Start);

        TokenCredential credential = string.IsNullOrEmpty(ClientId)
                                    ? new DefaultAzureCredential()
                                    : new WorkloadIdentityCredential(new WorkloadIdentityCredentialOptions
                                    {
                                        ClientId = ClientId,
                                        TenantId = TenantId,
                                    });

        await using var client = !string.IsNullOrEmpty(ConnectionString)
                                ? new ServiceBusClient(
                                    ConnectionString,
                                    new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets })
                                : new ServiceBusClient(
                                    FullyQualifiedNamespace,
                                    credential,
                                    new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets });

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
    async Task MessageHandlerAsync(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();

        var actionCalled = false;
        var action = new FunctionRunnerServiceBusMessageActions(args, () => actionCalled = true);

        var parameters = PrepareParameters(body, args.CancellationToken, args.Message,
                            new Tuple<Type, object>(typeof(ServiceBusMessageActions), action));

        var success = await funcInfo.InvokeAsync(parameters, loggerFactory);

        if (!actionCalled)
        {
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
    }

    /// <summary>
    /// Handles errors that occur during message processing.
    /// </summary>
    /// <param name="args">The error event arguments.</param>
    /// <returns>A completed task.</returns>
    Task ErrorHandlerAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception, $"Service Bus error in entity '{args.EntityPath}' (namespace: '{args.FullyQualifiedNamespace}')");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Provides actions for processing Service Bus messages.
    /// </summary>
    /// <param name="args">The message event arguments.</param>
    /// <param name="actionCalled">Callback to indicate an action was called.</param>
    class FunctionRunnerServiceBusMessageActions(ProcessMessageEventArgs args, Action actionCalled) : ServiceBusMessageActions
    {
        readonly ProcessMessageEventArgs args = args;
        readonly Action actionCalled = actionCalled;

        /// <inheritdoc/>
        /// <summary>
        /// Completes the specified message, removing it from the queue.
        /// </summary>
        /// <param name="message">The message to complete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override Task CompleteMessageAsync(
            ServiceBusReceivedMessage message,
            CancellationToken cancellationToken = default)
        {
            actionCalled();
            return args.CompleteMessageAsync(message, cancellationToken);
        }

        /// <inheritdoc/>
        /// <summary>
        /// Abandons the specified message, making it available for reprocessing.
        /// </summary>
        /// <param name="message">The message to abandon.</param>
        /// <param name="propertiesToModify">Properties to modify on the message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override Task AbandonMessageAsync(
            ServiceBusReceivedMessage message,
            IDictionary<string, object>? propertiesToModify = default,
            CancellationToken cancellationToken = default)
        {
            actionCalled();
            return args.AbandonMessageAsync(message, propertiesToModify, cancellationToken);
        }

        /// <inheritdoc/>
        /// <summary>
        /// Dead-letters the specified message.
        /// </summary>
        /// <param name="message">The message to dead-letter.</param>
        /// <param name="propertiesToModify">Properties to modify on the message.</param>
        /// <param name="deadLetterReason">The reason for dead-lettering.</param>
        /// <param name="deadLetterErrorDescription">Error description for dead-lettering.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override Task DeadLetterMessageAsync(
            ServiceBusReceivedMessage message,
            Dictionary<string, object>? propertiesToModify = default,
            string? deadLetterReason = default,
            string? deadLetterErrorDescription = default,
            CancellationToken cancellationToken = default)
        {
            actionCalled();
            return args.DeadLetterMessageAsync(message, propertiesToModify, deadLetterReason, deadLetterErrorDescription, cancellationToken);
        }

        /// <inheritdoc/>
        /// <summary>
        /// Defers the specified message.
        /// </summary>
        /// <param name="message">The message to defer.</param>
        /// <param name="propertiesToModify">Properties to modify on the message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override Task DeferMessageAsync(
            ServiceBusReceivedMessage message,
            IDictionary<string, object>? propertiesToModify = default,
            CancellationToken cancellationToken = default)
        {
            actionCalled();
            return args.DeferMessageAsync(message, propertiesToModify, cancellationToken);
        }

        /// <inheritdoc/>
        /// <summary>
        /// Renews the lock on the specified message.
        /// </summary>
        /// <param name="message">The message to renew the lock for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override Task RenewMessageLockAsync(
            ServiceBusReceivedMessage message,
            CancellationToken cancellationToken = default)
        {
            actionCalled();
            return args.RenewMessageLockAsync(message, cancellationToken);
        }
    }
}
