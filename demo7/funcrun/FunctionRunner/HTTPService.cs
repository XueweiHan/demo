using Microsoft.Extensions.Hosting;
using System.Net;

namespace FunctionRunner
{
    class HTTPService(AppSettings appSettings) : BackgroundService
    {
        readonly HttpListener _httpListener = new();
        readonly int _port = appSettings.FunctionRunnerHttpPort;

        public override void Dispose()
        {
            _httpListener.Close();
            base.Dispose();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var name = $"{ConsoleColor.Cyan}HTTPService{ConsoleColor.Default}";
            Console.WriteLine($"{ConsoleStyle.TimeStamp}{name} is listening on http://localhost:{_port}/");
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
                Console.WriteLine($"{ConsoleStyle.TimeStamp}{name} encountered an error: {ex}");
            }
            finally
            {
                Console.WriteLine($"{ConsoleStyle.TimeStamp}{name} is stopped");
            }
        }

        async Task HandleRequestAsync(HttpListenerContext context, CancellationToken stoppingToken)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;

                Console.WriteLine($"Received {request.HttpMethod} request for {request.Url}");

                var responseString = "Hello from HTTPService!";
                var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length, stoppingToken);
                response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling request: {ex.Message}");
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _httpListener.Stop(); // Stop the listener immediately
            return base.StopAsync(cancellationToken);
        }
    }
}