using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infra.Persistence;

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
