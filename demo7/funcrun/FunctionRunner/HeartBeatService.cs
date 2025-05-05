using Microsoft.Extensions.Hosting;

namespace FunctionRunner
{
    class HeartBeatService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var name = $"{ConsoleColor.Yellow}HeartBeatService{ConsoleColor.Default}";
            Console.WriteLine($"[{name} is starting at {DateTime.UtcNow:u}]");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    Console.WriteLine($"{DateTime.UtcNow:u} pod heartbeat {Environment.MachineName}");
                    await Task.Delay(60 * 1000, stoppingToken);
                }
            }
            finally
            {
                Console.WriteLine($"[{name} is stopped at {DateTime.UtcNow:u}]");
            }
        }
    }
}