using System.Text.Json.Serialization;

namespace FunctionRunner
{
    class AppSettings
    {
        public string? AzureWebJobsScriptRoot { get; set; }

        public int FunctionRunnerHttpPort { get; set; } = 8080;

        public int ShutdownTimeoutInSeconds { get; set; } = 20;

        public bool PrintConfigJson { get; set; } = false;

        public int HeartbeatLogIntervalInSeconds { get; set; } = 60;

        [JsonPropertyName("CONFIG_FILE")]
        public string? ConfigFile { get; set; }

        [JsonPropertyName("CONFIG_JSON")]
        public Config? ConfigJson { get; set; }
    }

    class Config
    {
        public Keyvault[]? Keyvaults { get; set; }
        public CopyFile[]? CopyFiles { get; set; }
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
}