using System.Text.Json.Serialization;

namespace FunctionRunner;

class AppSettings
{
    public bool DisableFunctionRunner { get; set; } = false;

    public string? AzureWebJobsScriptRoot { get; set; }

    public int FunctionRunnerHttpPort { get; set; } = 8080;

    public int ShutdownTimeoutInSeconds { get; set; } = 20;

    public int HeartbeatLogIntervalInSeconds { get; set; } = 60;

    public bool PrintConfigJson { get; set; } = true;

    [JsonPropertyName("CONFIG_FILE")]
    public string? ConfigFile { get; set; } = string.Empty;

    [JsonPropertyName("CONFIG_JSON")]
    public Config? ConfigJson { get; set; } = new Config();
}

class Config
{
    public Keyvault[]? Keyvaults { get; set; }
    public CopyFile[]? CopyFiles { get; set; }
    public Executable[]? Executables { get; set; }
}

class Keyvault
{
    public required string Name { get; set; }
    public Secret[]? Secrets { get; set; }
}

class Secret
{
    public required string Name { get; set; }
    public string? FilePath { get; set; }
    public string? EnvVar { get; set; }
}

class CopyFile
{
    public required string From { get; set; }
    public required string To { get; set; }
}

class Executable
{
    public required string Exec { get; set; }
    public string Args { get; set; } = string.Empty;
}