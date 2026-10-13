using System.Text.Json;

namespace Infra.Caching;

internal static class SerializerOptions
{
    public static readonly JsonSerializerOptions Instance = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };
}
