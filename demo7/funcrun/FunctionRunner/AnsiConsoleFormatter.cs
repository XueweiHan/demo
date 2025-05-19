// <copyright file="AnsiConsoleFormatter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace FunctionRunner;

/// <summary>
/// Provides a custom console formatter for logging with ANSI color support.
/// </summary>
class AnsiConsoleFormatter() : ConsoleFormatter(FormatterName)
{
    /// <inheritdoc/>
    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        var logLevel = logEntry.LogLevel;
        var category = logEntry.Category;
        var message = logEntry.Formatter.Invoke(logEntry.State, logEntry.Exception);
        var exception = logEntry.Exception == null ? string.Empty : $" {logEntry.Exception}".Replace(Environment.NewLine, "  ");

        if (message == null)
        {
            return;
        }

        var words = message.Split(' ');
        int wordCount = words.Length;

        var sections = category.Split('.', StringSplitOptions.RemoveEmptyEntries);

        var colorMapping = new string?[wordCount];

        var showTimestamp = false;
        var showLogLevel = false;
        var colorFormat = true;

        foreach (var section in sections)
        {
            if (section == "T")
            {
                showTimestamp = true;
                continue;
            }

            if (section == "L")
            {
                showLogLevel = true;
                continue;
            }

            var match = Regex.Match(section, @"^(?<color>[A-Za-z]+)(?<index>-?\d+)$");
            if (!match.Success)
            {
                colorFormat = false;
                break;
            }

            var colorName = match.Groups["color"].Value;
            var indexStr = match.Groups["index"].Value;

            if (!int.TryParse(indexStr, out int index))
            {
                colorFormat = false;
                break;
            }

            index = index >= 0 ? index : wordCount + index;
            if (index < 0 || index >= wordCount)
            {
                continue;
            }

            colorMapping[index] = colorName;
        }

        if (!colorFormat)
        {
            textWriter.WriteLine($"{AnsiColors["Gray"]}[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]{Reset} {message}{exception}");
        }
        else
        {
            var result = new StringBuilder();
            if (showTimestamp)
            {
                result.Append($"{AnsiColors["Gray"]}[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]{Reset} ");
            }
            if (showLogLevel)
            {
                result.Append($"{GetLogLevelString(logLevel)} ");
            }

            for (int i = 0; i < wordCount; i++)
            {
                if (i > 0)
                {
                    result.Append(' ');
                }

                if (colorMapping[i] != null)
                {
                    var colorName = colorMapping[i] !;
                    var colorCode = AnsiColors.TryGetValue(colorName, out var code) ? code : string.Empty;

                    result.Append(colorCode).Append(words[i]).Append(Reset);
                }
                else
                {
                    result.Append(words[i]);
                }
            }

            result.Append(exception);

            textWriter.WriteLine(result.ToString());
        }
    }

    /// <summary>
    /// Gets the log level string with ANSI color codes.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <returns>The formatted log level string.</returns>
    static string GetLogLevelString(LogLevel logLevel)
    {
        var color = logLevel switch
        {
            LogLevel.Trace => AnsiColors["Gray"],
            LogLevel.Debug => AnsiColors["Cyan"],
            LogLevel.Information => AnsiColors["Green"],
            LogLevel.Warning => AnsiColors["Yellow"],
            LogLevel.Error => AnsiColors["Red"],
            LogLevel.Critical => AnsiColors["Magenta"],
            _ => Reset,
        };

        var level = logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => "none"
        };

        return $"{color}{level}{Reset}";
    }

    /// <summary>
    /// ANSI color codes mapping.
    /// </summary>
    static readonly Dictionary<string, string> AnsiColors = new()
    {
        ["Black"] = "\x1B[30m",
        ["Blue"] = "\x1B[34m",
        ["Magenta"] = "\x1B[35m",
        ["Cyan"] = "\x1B[36m",
        ["Gray"] = "\x1B[90m",
        ["Green"] = "\x1B[32m",
        ["Red"] = "\x1B[31m",
        ["White"] = "\x1B[37m",
        ["Yellow"] = "\x1B[93m",

        ["BkRed"] = "\x1B[41m",
    };

    /// <summary>
    /// ANSI reset code.
    /// </summary>
    static readonly string Reset = "\x1B[0m";

    /// <summary>
    /// Formatter name.
    /// </summary>
    static readonly string FormatterName = "AnsiConsole";

    /// <summary>
    /// Configures the logging builder to use the AnsiConsoleFormatter.
    /// </summary>
    /// <param name="loggingBuilder">The logging builder.</param>
    public static void LoggingBuilder(ILoggingBuilder loggingBuilder)
    {
        loggingBuilder
            .SetMinimumLevel(LogLevel.Information)
            .AddConsoleFormatter<AnsiConsoleFormatter, ConsoleFormatterOptions>()
            .AddConsole(options =>
            {
                options.FormatterName = FormatterName;
            });
    }
}
