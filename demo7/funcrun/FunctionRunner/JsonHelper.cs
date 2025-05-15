using System.Text.Json;
using System.Text.RegularExpressions;

namespace FunctionRunner
{
    static class JsonHelper
    {
        static readonly JsonSerializerOptions DeserializeOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static T? Deserialize<T>(string? json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            json = Regex.Replace(json, @"//.*?$", "", RegexOptions.Multiline);
            return JsonSerializer.Deserialize<T>(json, DeserializeOptions);
        }

        public static T? GetEnvJson<T>(string envName)
        {
            return Deserialize<T>(Environment.GetEnvironmentVariable(envName));
        }

        public static string GetJson<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }
    }
}