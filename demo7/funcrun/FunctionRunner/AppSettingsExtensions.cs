using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace FunctionRunner
{
    static class AppSettingsExtensions
    {
        public static async Task ExecuteAsync(this AppSettings appSettings, IConfiguration cfg)
        {
            appSettings.ConfigJson = JsonHelper.GetEnvJson<Config>("CONFIG_JSON") ?? cfg.GetSection("CONFIG_JSON").Get<Config>();
            appSettings.ConfigFile = cfg.GetValue<string>("CONFIG_FILE");

            if (appSettings.PrintConfigJson)
            {
                Console.WriteLine(JsonHelper.GetJson(appSettings));
            }

            if (!string.IsNullOrWhiteSpace(appSettings.ConfigFile))
            {
                var json = File.ReadAllText(appSettings.ConfigFile);
                var config = JsonHelper.Deserialize<Config>(json);
                if (appSettings.PrintConfigJson)
                {
                    Console.WriteLine(JsonHelper.GetJson(config));
                }

                await ExecuteConfigAsync(config!);
            }

            if (appSettings.ConfigJson != null)
            {
                await ExecuteConfigAsync(appSettings.ConfigJson);
            }
        }

        static async Task ExecuteConfigAsync(Config config)
        {
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
                Console.WriteLine($"{ConsoleStyle.TimeStamp}Set secret {ConsoleColor.Cyan}{secret.Name}{ConsoleColor.Default} to environment variable {ConsoleColor.Cyan}{secret.EnvVar}{ConsoleColor.Default}");
            }

            if (!string.IsNullOrEmpty(secret.FilePath))
            {
                await File.WriteAllTextAsync(secret.FilePath, resp.Value.Value);
                Console.WriteLine($"{ConsoleStyle.TimeStamp}Write secret {ConsoleColor.Cyan}{secret.Name}{ConsoleColor.Default} to file {ConsoleColor.Cyan}{secret.FilePath}{ConsoleColor.Default}");
            }
        }

        static void CopyFiles(CopyFile[]? copyFiles)
        {
            foreach (var cp in copyFiles ?? [])
            {
                File.Copy(cp.From, cp.To, overwrite: true);
                Console.WriteLine($"{ConsoleStyle.TimeStamp}Copy file from {ConsoleColor.Cyan}{cp.From}{ConsoleColor.Default} to {ConsoleColor.Cyan}{cp.To}{ConsoleColor.Default}");
            }
        }
    }
}