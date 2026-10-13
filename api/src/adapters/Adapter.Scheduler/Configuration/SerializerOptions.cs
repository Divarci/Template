using System.Text.Json;
using System.Text.Json.Serialization;

namespace Adapter.Scheduler.Configuration;

internal static class SerializerOptions
{
    public static readonly JsonSerializerOptions Instance = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };
}
