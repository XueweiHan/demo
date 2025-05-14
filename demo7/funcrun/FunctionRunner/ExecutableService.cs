using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FunctionRunner
{
    internal class ExecutableService(string fileName, string arguments, ILogger<ExecutableService> logger) : BackgroundService
    {
        private readonly string _fileName = fileName;
        private readonly string _arguments = arguments;
        private readonly ILogger<ExecutableService> _logger = logger;
        private Process? _process;

        public override void Dispose()
        {
            _process?.Dispose();
            base.Dispose();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var name = $"{ConsoleColor.Cyan}ExecutableService{ConsoleColor.Default} ({_fileName})";
            Console.WriteLine($"{ConsoleStyle.TimeStamp}{name} is starting");
            try
            {
                _process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _fileName,
                        Arguments = _arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    },
                    EnableRaisingEvents = true
                };

                var tcs = new TaskCompletionSource();

                _process.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);

                _process.ErrorDataReceived += (s, e) => Console.Error.WriteLine(e.Data);

                _process.Exited += (s, e) =>
                {
                    Console.WriteLine($"{_fileName} exited with code {_process.ExitCode}");
                    tcs.TrySetResult();
                };

                _process.Start();
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();

                // Wait for either the process to exit or cancellation
                var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, stoppingToken));

                if (completedTask != tcs.Task)
                {
                    try
                    {
                        if (!_process.HasExited)
                        {
                            _process.Kill(entireProcessTree: true);
                            Console.WriteLine($"{ConsoleStyle.TimeStamp}{name} is killed");
                        }
                    }
                    catch
                    {
                        Console.WriteLine($"{ConsoleStyle.TimeStamp}{name} killing failed");
                    }
                }
            }
            catch (Exception ex)
            {
                var exception = $"{ex}".Replace(Environment.NewLine, "\t");
                Console.Error.WriteLine($"{ConsoleStyle.TimeStamp}{name} encountered an {ConsoleBackgroundColor.Red}exception{ConsoleBackgroundColor.Default} {exception}");
            }
            finally
            {
                Console.WriteLine($"{ConsoleStyle.TimeStamp}{name} is stopped");
            }
        }
    }
}