using Microsoft.Extensions.Hosting;

namespace FunctionRunner
{
    internal class HeartBeatService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Console.WriteLine($"[{ConsoleColor.Cyan}HeartBeatService{ConsoleColor.Default} is starting at {DateTime.UtcNow:u}]");
                while (!stoppingToken.IsCancellationRequested)
                {
                    Console.WriteLine($"{DateTime.UtcNow:u} pod heartbeat {Environment.MachineName}");
                    await Task.Delay(60 * 1000, stoppingToken);
                }
            }
            finally
            {
                Console.WriteLine($"[{ConsoleColor.Cyan}HeartBeatService{ConsoleColor.Default} is stopped at {DateTime.UtcNow:u}]");
            }
        }
    }
}
