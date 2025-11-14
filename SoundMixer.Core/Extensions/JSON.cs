using System.Text.Json;
using System.Text.Json.Serialization;

namespace SoundMixer.Core.Extensions
{
    public static class JSON
    {
        private static JsonSerializerOptions defaultSerializerSettings = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() },
            IncludeFields = true
        };

        public static string Serialize(this object o)
        {
            return JsonSerializer.Serialize(o, defaultSerializerSettings);
        }

        public static T? Deserialize<T>(this string json)
        {
            return JsonSerializer.Deserialize<T>(json, defaultSerializerSettings);
        }

        public static bool TryDeserialize<T>(this string json, out T? o)
        {
            try
            {
                o = JsonSerializer.Deserialize<T>(json, defaultSerializerSettings);
            }
            catch
            {
                o = default;
                return false;
            }
            return true;
        }
    }
}