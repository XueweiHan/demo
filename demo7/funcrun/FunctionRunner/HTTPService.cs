using Microsoft.Extensions.Hosting;
using System.Net;

namespace FunctionRunner
{
    internal class HTTPService : BackgroundService
    {
        readonly HttpListener _httpListener;
        readonly int _port = 8080;

        public HTTPService()
        {
            var portStr = Environment.GetEnvironmentVariable("FuctionRunnerHttpPort");
            _port = int.TryParse(portStr, out var port) ? port : 8080;

            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add($"http://localhost:{_port}/");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _httpListener.Start();
            Console.WriteLine($"[{ConsoleColor.Cyan}HTTPService{ConsoleColor.Default} is listening on http://localhost:{_port}/ at {DateTime.UtcNow:u}]");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var context = await _httpListener.GetContextAsync(); // Wait for an incoming request
                    _ = HandleRequestAsync(context, stoppingToken); // Process the request asynchronously
                }
            }
            catch (HttpListenerException) when (stoppingToken.IsCancellationRequested) { }
            finally
            {
                Console.WriteLine($"[{ConsoleColor.Cyan}HTTPService{ConsoleColor.Default} is stopped at {DateTime.UtcNow:u}]");
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
