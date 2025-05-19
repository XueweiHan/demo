// <copyright file="JsonHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using System.Text.Json;
using System.Text.RegularExpressions;

namespace FunctionRunner;

/// <summary>
/// Provides helper methods for JSON serialization and deserialization.
/// </summary>
static class JsonHelper
{
    /// <summary>
    /// Gets the <see cref="JsonSerializerOptions"/> used for deserialization.
    /// </summary>
    static readonly JsonSerializerOptions DeserializeOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Deserializes the specified JSON string to an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="json">The JSON string.</param>
    /// <returns>The deserialized object, or <c>default</c> if the input is null or empty.</returns>
    public static T? Deserialize<T>(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return default;
        }

        json = Regex.Replace(json, @"//.*?$", string.Empty, RegexOptions.Multiline);
        return JsonSerializer.Deserialize<T>(json, DeserializeOptions);
    }

    /// <summary>
    /// Gets and deserializes a JSON string from an environment variable.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="envName">The name of the environment variable.</param>
    /// <returns>The deserialized object, or <c>default</c> if the environment variable is not set or empty.</returns>
    public static T? GetEnvJson<T>(string envName)
    {
        return Deserialize<T>(Environment.GetEnvironmentVariable(envName));
    }

    /// <summary>
    /// Serializes the specified object to a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>The JSON string representation of the object.</returns>
    public static string GetJson<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }
}
