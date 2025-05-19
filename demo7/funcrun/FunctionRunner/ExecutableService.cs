// <copyright file="ExecutableService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FunctionRunner;

/// <summary>
/// Runs an external executable as a background service.
/// </summary>
internal class ExecutableService(string fileName, string arguments, ILoggerFactory loggerFactory) : BackgroundService
{
    readonly string fileName = fileName;
    readonly string arguments = arguments;
    readonly ILoggerFactory loggerFactory = loggerFactory;
    readonly ILogger logger = loggerFactory.CreateLogger("T.Cyan0");
    readonly ILogger elogger = loggerFactory.CreateLogger("T");
    Process? process;

    /// <inheritdoc/>
    public override void Dispose()
    {
        process?.Dispose();
        process = null;
        loggerFactory.Dispose();
        base.Dispose();
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var name = $"ExecutableService ({fileName})";
        logger.LogInformation($"{name} is starting");

        try
        {
            process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            var tcs = new TaskCompletionSource<int>();

            process.OutputDataReceived += (s, e) => elogger.LogInformation(e.Data);

            process.ErrorDataReceived += (s, e) => elogger.LogError(e.Data);

            process.Exited += (s, e) => tcs.TrySetResult(process.ExitCode);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Wait for either the process to exit or cancellation
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, stoppingToken));

            if (completedTask != tcs.Task)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                        logger.LogWarning($"{name} is killed");
                    }
                }
                catch
                {
                    logger.LogError($"{name} is failed");
                }
            }
            else
            {
                var exitCode = await tcs.Task;
                logger.LogError($"{name} exited with code {exitCode}");
            }
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("T.Cyan0.Red4").LogError(ex, $"{name} encountered an exception");
        }
        finally
        {
            logger.LogInformation($"{name} is stopped");
        }
    }
}
