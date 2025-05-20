// <copyright file="HTTPService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using System.Net;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FunctionRunner;

/// <summary>
/// Provides an HTTP service that listens for incoming requests and responds with a simple message.
/// </summary>
class HTTPService(AppSettings appSettings, ILoggerFactory loggerFactory) : BackgroundService
{
    /// <summary>
    /// The HTTP listener instance.
    /// </summary>
    readonly HttpListener httpListener = new();

    /// <summary>
    /// The port on which the HTTP service listens.
    /// </summary>
    readonly int port = appSettings.FunctionRunnerHttpPort;

    /// <summary>
    /// The logger instance for this service.
    /// </summary>
    readonly ILogger logger = loggerFactory.CreateLogger("T.Cyan0");

    /// <inheritdoc/>
    public override void Dispose()
    {
        httpListener.Close();
        ((IDisposable)httpListener).Dispose();
        base.Dispose();
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (appSettings.FunctionRunnerHttpPort <= 0)
        {
            return; // No HTTP service to start
        }

        var name = $"HTTPService";
        logger.LogInformation($"{name} is listening on port {port}");

        httpListener.Prefixes.Add($"http://localhost:{port}/");

        try
        {
            httpListener.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                var context = await httpListener.GetContextAsync(); // Wait for an incoming request
                _ = HandleRequestAsync(context, stoppingToken); // Process the request asynchronously
            }
        }
        catch (HttpListenerException) when (stoppingToken.IsCancellationRequested) { }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{name} encountered an error");
        }
        finally
        {
            logger.LogInformation($"{name} is stopped");
        }
    }

    /// <summary>
    /// Handles an incoming HTTP request asynchronously.
    /// </summary>
    /// <param name="context">The HTTP listener context.</param>
    /// <param name="stoppingToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    async Task HandleRequestAsync(HttpListenerContext context, CancellationToken stoppingToken)
    {
        try
        {
            var request = context.Request;
            using var response = context.Response;

            Console.WriteLine($"Received {request.HttpMethod} request for {request.Url}");

            var responseString = $"Hello from HTTPService!";
            var buffer = Encoding.UTF8.GetBytes(responseString);

            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length, stoppingToken);
            response.OutputStream.Close();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error handling request");
        }
    }

    /// <inheritdoc/>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        httpListener.Stop(); // Stop the listener immediately
        return base.StopAsync(cancellationToken);
    }
}
