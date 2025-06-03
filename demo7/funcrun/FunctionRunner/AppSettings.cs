// <copyright file="AppSettings.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FunctionRunner;

/// <summary>
/// Represents the application settings for the Function Runner.
/// </summary>
class AppSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether the Function Runner is disabled.
    /// </summary>
    public bool DisableFunctionRunner { get; set; } = false;

    /// <summary>
    /// Gets or sets the root directory for Azure WebJobs scripts.
    /// </summary>
    public string? AzureWebJobsScriptRoot { get; set; }

    /// <summary>
    /// Gets or sets the HTTP port for the Function Runner.
    /// </summary>
    public int FunctionRunnerHttpPort { get; set; } = 8080;

    /// <summary>
    /// Gets or sets the shutdown timeout in seconds.
    /// </summary>
    public int ShutdownTimeoutInSeconds { get; set; } = 20;

    /// <summary>
    /// Gets or sets the interval for heartbeat log in seconds.
    /// </summary>
    public int HeartbeatLogIntervalInSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets a value indicating whether to print the configuration JSON.
    /// </summary>
    public bool PrintConfigJson { get; set; } = true;

    /// <summary>
    /// Gets or sets the configuration file path.
    /// </summary>
    [JsonPropertyName("CONFIG_FILE")]
    public string? ConfigFile { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the configuration JSON object.
    /// </summary>
    [JsonPropertyName("CONFIG_JSON")]
    public Config? ConfigJson { get; set; } = new Config();
}

/// <summary>
/// Represents the configuration settings.
/// </summary>
class Config
{
    /// <summary>
    /// Gets or sets the key vaults configuration.
    /// </summary>
    public Keyvault[] Keyvaults { get; set; } = Array.Empty<Keyvault>();

    /// <summary>
    /// Gets or sets the copy files configuration.
    /// </summary>
    public CopyFile[] CopyFiles { get; set; } = Array.Empty<CopyFile>();

    /// <summary>
    /// Gets or sets the executables configuration.
    /// </summary>
    public Executable[] Executables { get; set; } = Array.Empty<Executable>();
}

/// <summary>
/// Represents a key vault configuration.
/// </summary>
class Keyvault
{
    /// <summary>
    /// Gets or sets the name of the key vault.
    /// </summary>
    required public string Name { get; set; }

    /// <summary>
    /// Gets or sets the secrets in the key vault.
    /// </summary>
    required public Secret[] Secrets { get; set; } = Array.Empty<Secret>();
}

/// <summary>
/// Represents a secret in a key vault.
/// </summary>
class Secret
{
    /// <summary>
    /// Gets or sets the name of the secret.
    /// </summary>
    required public string Name { get; set; }

    /// <summary>
    /// Gets or sets the file path for the secret.
    /// </summary>
    public string? File { get; set; }

    /// <summary>
    /// Gets or sets the environment variable for the secret.
    /// </summary>
    public string? Env { get; set; }
}

/// <summary>
/// Represents a file copy operation.
/// </summary>
class CopyFile
{
    /// <summary>
    /// Gets or sets the source file path.
    /// </summary>
    required public string From { get; set; }

    /// <summary>
    /// Gets or sets the destination file path.
    /// </summary>
    required public string To { get; set; }
}

/// <summary>
/// Represents an executable configuration.
/// </summary>
class Executable
{
    /// <summary>
    /// Gets or sets the executable path.
    /// </summary>
    required public string Exec { get; set; }

    /// <summary>
    /// Gets or sets the arguments for the executable.
    /// </summary>
    public string Args { get; set; } = string.Empty;
}
