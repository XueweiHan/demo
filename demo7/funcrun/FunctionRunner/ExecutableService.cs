using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FunctionRunner;

internal class ExecutableService(string fileName, string arguments, ILoggerFactory loggerFactory) : BackgroundService
{
    readonly string _fileName = fileName;
    readonly string _arguments = arguments;
    readonly ILoggerFactory _loggerFactory = loggerFactory;
    readonly ILogger _logger = loggerFactory.CreateLogger("T.Cyan0");
    readonly ILogger _elogger = loggerFactory.CreateLogger("T");
    Process? _process;

    public override void Dispose()
    {
        _process?.Dispose();
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var name = $"ExecutableService ({_fileName})";
        _logger.LogInformation($"{name} is starting");

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

            var tcs = new TaskCompletionSource<int>();

            _process.OutputDataReceived += (s, e) => _elogger.LogInformation(e.Data);

            _process.ErrorDataReceived += (s, e) => _elogger.LogError(e.Data);

            _process.Exited += (s, e) => tcs.TrySetResult(_process.ExitCode);

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
                        _logger.LogWarning($"{name} is killed");
                    }
                }
                catch
                {
                    _logger.LogError($"{name} is failed");
                }
            }
            else
            {
                var exitCode = await tcs.Task;
                _logger.LogError($"{name} exited with code {exitCode}");
            }
        }
        catch (Exception ex)
        {
            _loggerFactory.CreateLogger("T.Cyan0.Red4").LogError(ex, $"{name} encountered an exception");
        }
        finally
        {
            _logger.LogInformation($"{name} is stopped");
        }
    }
}