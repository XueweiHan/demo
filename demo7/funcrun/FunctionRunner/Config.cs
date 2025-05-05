using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace FunctionRunner
{
    class Keyvault
    {
        public required string Name { get; set; }
        public Secret[]? Secrets { get; set; }
        // public VaultObject[]? Certificates { get; set; }
        // public VaultObject[]? Keys { get; set; }
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

    class Config
    {
        public Keyvault[]? Keyvaults { get; set; }
        public CopyFile[]? CopyFiles { get; set; }

        public static async Task LoadAsync()
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

    static class ConfigLoadHelper
    {
        public static async Task LoadAsync(string configJson)
        {
            var config = JsonHelper.Deserialize<Config>(configJson);
            //Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true }));

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

            if (!string.IsNullOrEmpty(secret.EnvVar))
            {
                Environment.SetEnvironmentVariable(secret.EnvVar, resp.Value.Value);
            }

            if (!string.IsNullOrEmpty(secret.FilePath))
            {
                await File.WriteAllTextAsync(secret.FilePath, resp.Value.Value);
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