using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Library.Helpers;

public static class DomainEventSerializerOptions
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
