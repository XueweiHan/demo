using Microsoft.Extensions.Hosting;

namespace FunctionRunner
{
    class HeartBeatService(AppSettings appSettings) : BackgroundService
    {
        readonly TimeSpan _heartbeatLogInterval = TimeSpan.FromSeconds(appSettings.HeartbeatLogIntervalInSeconds);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var name = $"{ConsoleColor.Yellow}HeartBeatService{ConsoleColor.Default}";
            Console.WriteLine($"[{name} is starting at {DateTime.UtcNow:u}]");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    Console.WriteLine($"{DateTime.UtcNow:u} pod heartbeat {Environment.MachineName}");
                    await Task.Delay(_heartbeatLogInterval, stoppingToken);
                }
            }
            finally
            {
                Console.WriteLine($"[{name} is stopped at {DateTime.UtcNow:u}]");
            }
        }
    }
}