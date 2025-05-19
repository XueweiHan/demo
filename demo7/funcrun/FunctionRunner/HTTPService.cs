using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;

namespace FunctionRunner;

class HTTPService(AppSettings appSettings, ILoggerFactory loggerFactory) : BackgroundService
{
    readonly HttpListener _httpListener = new();
    readonly int _port = appSettings.FunctionRunnerHttpPort;
    readonly ILogger _logger = loggerFactory.CreateLogger("T.Cyan0");

    public override void Dispose()
    {
        _httpListener.Close();
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var name = $"HTTPService";
        _logger.LogInformation($"{name} is listening on port {_port}");

        _httpListener.Prefixes.Add($"http://localhost:{_port}/");

        try
        {
            _httpListener.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                var context = await _httpListener.GetContextAsync(); // Wait for an incoming request
                _ = HandleRequestAsync(context, stoppingToken); // Process the request asynchronously
            }
        }
        catch (HttpListenerException) when (stoppingToken.IsCancellationRequested) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{name} encountered an error");
        }
        finally
        {
            _logger.LogInformation($"{name} is stopped");
        }
    }

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
            _logger.LogError(ex, $"Error handling request");
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _httpListener.Stop(); // Stop the listener immediately
        return base.StopAsync(cancellationToken);
    }
}