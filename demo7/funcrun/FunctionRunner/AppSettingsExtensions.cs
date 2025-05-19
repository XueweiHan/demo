// <copyright file="AppSettingsExtensions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;

namespace FunctionRunner;

/// <summary>
/// Extension methods for <see cref="AppSettings"/>.
/// </summary>
static class AppSettingsExtensions
{
    /// <summary>
    /// Executes the application settings logic.
    /// </summary>
    /// <param name="appSettings">The application settings.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task ExecuteAsync(this AppSettings appSettings, ILoggerFactory loggerFactory)
    {
        if (appSettings.PrintConfigJson)
        {
            loggerFactory.CreateLogger(string.Empty).LogInformation(JsonHelper.GetJson(appSettings));
        }

        if (!string.IsNullOrWhiteSpace(appSettings.ConfigFile))
        {
            var json = File.ReadAllText(appSettings.ConfigFile);
            var config = JsonHelper.Deserialize<Config>(json);
            if (appSettings.PrintConfigJson)
            {
                loggerFactory.CreateLogger(string.Empty).LogInformation(JsonHelper.GetJson(config));
            }

            await ExecuteConfigAsync(config!, loggerFactory);
        }

        if (appSettings.ConfigJson != null)
        {
            await ExecuteConfigAsync(appSettings.ConfigJson, loggerFactory);
        }
    }

    /// <summary>
    /// Executes the configuration logic.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    static async Task ExecuteConfigAsync(Config config, ILoggerFactory loggerFactory)
    {
        await LoadKeyvaultAsync(config!.Keyvaults, loggerFactory.CreateLogger("T.Cyan2.Cyan-1"));
        CopyFiles(config!.CopyFiles, loggerFactory.CreateLogger("T.Cyan3.Cyan-1"));
    }

    /// <summary>
    /// Loads secrets from Azure Key Vaults.
    /// </summary>
    /// <param name="keyvaults">The key vaults.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    static async Task LoadKeyvaultAsync(Keyvault[] keyvaults, ILogger logger)
    {
        foreach (var kv in keyvaults)
        {
            var url = $"https://{kv.Name}.vault.azure.net/";
            var client = new SecretClient(new Uri(url), new DefaultAzureCredential());

            foreach (var secret in kv.Secrets ?? Array.Empty<Secret>())
            {
                await LoadSecretsAsync(client, secret, logger);
            }
        }
    }

    /// <summary>
    /// Loads a secret from Azure Key Vault.
    /// </summary>
    /// <param name="client">The secret client.</param>
    /// <param name="secret">The secret.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

    /// <summary>
    /// Copies files as specified in the configuration.
    /// </summary>
    /// <param name="copyFiles">The copy file operations.</param>
    /// <param name="logger">The logger.</param>
    static void CopyFiles(CopyFile[] copyFiles, ILogger logger)
    {
        foreach (var cp in copyFiles ?? Array.Empty<CopyFile>())
        {
            File.Copy(cp.From, cp.To, overwrite: true);
            logger.LogInformation($"Copy file from {cp.From} to {cp.To}");
        }
    }
}
