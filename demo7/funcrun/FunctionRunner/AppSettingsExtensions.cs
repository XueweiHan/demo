// <copyright file="AppSettingsExtensions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using System.Security.Cryptography.X509Certificates;
using System.Text;
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
        var content = resp.Value.Value;
        if (IsPfx(content))
        {
            content = ConvertPfxToPem(content);
        }

        if (!string.IsNullOrEmpty(secret.Env))
        {
            Environment.SetEnvironmentVariable(secret.Env, content);
            logger.LogInformation($"Set secret {secret.Name} to environment variable {secret.Env}");
        }

        if (!string.IsNullOrEmpty(secret.File))
        {
            await File.WriteAllTextAsync(secret.File, content);
            logger.LogInformation($"Write secret {secret.Name} to file {secret.File}");
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

    /// <summary>
    /// Checks if the given value is a PFX certificate string.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    static bool IsPfx(string value)
    {
        // Try to base64 decode and load as X509Certificate2
        try
        {
            var raw = Convert.FromBase64String(value);
            using var cert = new X509Certificate2(raw);
            return cert.HasPrivateKey;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Converts a PFX byte array to PEM format.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    static string ConvertPfxToPem(string content)
    {
        var pfxBytes = Convert.FromBase64String(content);
        var collection = new X509Certificate2Collection();
        collection.Import(pfxBytes, (string?)null, X509KeyStorageFlags.Exportable);

        var builder = new StringBuilder();

        // Export private key for the first certificate with a private key
        var leaf = collection.Cast<X509Certificate2>().FirstOrDefault(c => c.HasPrivateKey);
        if (leaf != null)
        {
            var privateKey = leaf.GetRSAPrivateKey();
            if (privateKey != null)
            {
                var pkcs8 = privateKey.ExportPkcs8PrivateKey();
                var pkcs8Base64 = Convert.ToBase64String(pkcs8, Base64FormattingOptions.InsertLineBreaks);
                builder.AppendLine("-----BEGIN PRIVATE KEY-----");
                builder.AppendLine(pkcs8Base64);
                builder.AppendLine("-----END PRIVATE KEY-----");
            }
        }

        // Export all certificates in the chain
        foreach (var cert in collection)
        {
            var certBase64 = Convert.ToBase64String(cert.RawData, Base64FormattingOptions.InsertLineBreaks);
            builder.AppendLine("-----BEGIN CERTIFICATE-----");
            builder.AppendLine(certBase64);
            builder.AppendLine("-----END CERTIFICATE-----");
        }

        return builder.ToString();
    }
}
