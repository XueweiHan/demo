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

        public static T? Deserialize<T>(string json)
        {
            json = Regex.Replace(json, @"//.*?$", "", RegexOptions.Multiline);
            return JsonSerializer.Deserialize<T>(json, DeserializeOptions);
        }
    }
}