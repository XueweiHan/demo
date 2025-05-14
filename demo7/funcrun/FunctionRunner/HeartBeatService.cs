using Microsoft.Extensions.Hosting;

namespace FunctionRunner
{
    class HeartBeatService(AppSettings appSettings) : BackgroundService
    {
        readonly TimeSpan _heartbeatLogInterval = TimeSpan.FromSeconds(appSettings.HeartbeatLogIntervalInSeconds);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var name = $"{ConsoleColor.Cyan}HeartBeatService{ConsoleColor.Default}";
            Console.WriteLine($"{ConsoleStyle.TimeStamp}{name} is starting");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    Console.WriteLine($"{ConsoleStyle.TimeStamp}pod heartbeat from {Environment.MachineName}");
                    await Task.Delay(_heartbeatLogInterval, stoppingToken);
                }
            }
            finally
            {
                Console.WriteLine($"{ConsoleStyle.TimeStamp}{name} is stopped");
            }
        }
    }
}