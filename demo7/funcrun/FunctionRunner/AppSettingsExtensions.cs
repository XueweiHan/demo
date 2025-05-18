using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;

namespace FunctionRunner
{
    static class AppSettingsExtensions
    {
        public static async Task ExecuteAsync(this AppSettings appSettings, ILoggerFactory loggerFactory)
        {
            if (appSettings.PrintConfigJson)
            {
                loggerFactory.CreateLogger("").LogInformation(JsonHelper.GetJson(appSettings));
            }

            if (!string.IsNullOrWhiteSpace(appSettings.ConfigFile))
            {
                var json = File.ReadAllText(appSettings.ConfigFile);
                var config = JsonHelper.Deserialize<Config>(json);
                if (appSettings.PrintConfigJson)
                {
                    loggerFactory.CreateLogger("").LogInformation(JsonHelper.GetJson(config));
                }

                await ExecuteConfigAsync(config!, loggerFactory);
            }

            if (appSettings.ConfigJson != null)
            {
                await ExecuteConfigAsync(appSettings.ConfigJson, loggerFactory);
            }
        }

        static async Task ExecuteConfigAsync(Config config, ILoggerFactory loggerFactory)
        {
            await LoadKeyvaultAsync(config!.Keyvaults, loggerFactory.CreateLogger("T.Cyan2.Cyan-1"));
            CopyFiles(config!.CopyFiles, loggerFactory.CreateLogger("T.Cyan3.Cyan-1"));
        }

        static async Task LoadKeyvaultAsync(Keyvault[]? keyvaults, ILogger logger)
        {
            foreach (var kv in keyvaults ?? [])
            {
                var url = $"https://{kv.Name}.vault.azure.net/";
                var client = new SecretClient(new Uri(url), new DefaultAzureCredential());

                foreach (var secret in kv.Secrets ?? [])
                {
                    await LoadSecretsAsync(client, secret, logger);
                }
            }
        }

        static async Task LoadSecretsAsync(SecretClient client, Secret secret, ILogger logger)
        {
            var resp = await client.GetSecretAsync(secret.Name);

            if (!string.IsNullOrEmpty(secret.EnvVar))
            {
                Environment.SetEnvironmentVariable(secret.EnvVar, resp.Value.Value);
                logger.LogInformation($"Set secret {secret.Name} to environment variable {secret.EnvVar}");
            }

            if (!string.IsNullOrEmpty(secret.FilePath))
            {
                await File.WriteAllTextAsync(secret.FilePath, resp.Value.Value);
                logger.LogInformation($"Write secret {secret.Name} to file {secret.FilePath}");
            }
        }

        static void CopyFiles(CopyFile[]? copyFiles, ILogger logger)
        {
            foreach (var cp in copyFiles ?? [])
            {
                File.Copy(cp.From, cp.To, overwrite: true);
                logger.LogInformation($"Copy file from {cp.From} to {cp.To}");
            }
        }
    }
}