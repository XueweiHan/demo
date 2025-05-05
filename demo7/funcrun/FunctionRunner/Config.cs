using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Text.Json;

namespace FunctionRunner
{
    public class Keyvault
    {
        public required string Name { get; set; }
        public Secret[]? Secrets { get; set; }
        // public VaultObject[]? Certificates { get; set; }
        // public VaultObject[]? Keys { get; set; }
    }

    public class Secret
    {
        public required string Name { get; set; }
        public string? StorePath { get; set; }
        public string? EnvVarName { get; set; }
    }

    public class CopyFile
    {
        public required string From { get; set; }
        public required string To { get; set; }
    }

    public class Config
    {
        public Keyvault[]? Keyvaults { get; set; }
        public CopyFile[]? CopyFiles { get; set; }

        internal static async Task LoadAsync()
        {
            var json = Environment.GetEnvironmentVariable("CONFIG_JSON");
            if (!string.IsNullOrEmpty(json))
            {
                await ConfigLoadHelper.LoadAsync(json);
            }

            var path = Environment.GetEnvironmentVariable("CONFIG_PATH");
            if (!string.IsNullOrEmpty(path))
            {
                json = File.ReadAllText(path);
                await ConfigLoadHelper.LoadAsync(json);
            }
        }
    }

    internal static class ConfigLoadHelper
    {
        public static async Task LoadAsync(string configJson)
        {
            var config = JsonSerializer.Deserialize<Config>(configJson);
            Console.WriteLine(JsonSerializer.Serialize(config, new JsonSerializerOptions() { WriteIndented = true }));

            await LoadKeyvaultAsync(config!.Keyvaults);
            CopyFiles(config!.CopyFiles);
        }

        static async Task LoadKeyvaultAsync(Keyvault[]? keyvaults)
        {
            foreach (var kv in keyvaults ?? [])
            {
                var url = $"https://{kv.Name}.vault.azure.net/";
                var client = new SecretClient(new Uri(url), new DefaultAzureCredential());

                foreach (var secret in kv.Secrets ?? [])
                {
                    await LoadSecretsAsync(client, secret);
                }
            }
        }

        static async Task LoadSecretsAsync(SecretClient client, Secret secret)
        {
            var resp = await client.GetSecretAsync(secret.Name);

            if (!string.IsNullOrEmpty(secret.EnvVarName))
            {
                Environment.SetEnvironmentVariable(secret.EnvVarName, resp.Value.Value);
            }

            if (!string.IsNullOrEmpty(secret.StorePath))
            {
                await File.WriteAllTextAsync(secret.StorePath, resp.Value.Value);
            }
        }

        static void CopyFiles(CopyFile[]? copyFiles)
        {
            foreach (var cp in copyFiles ?? [])
            {
                File.Copy(cp.From, cp.To, overwrite: true);
            }
        }
    }
}