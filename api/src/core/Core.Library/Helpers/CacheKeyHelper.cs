namespace Core.Library.Helpers;

public static class CacheKeyHelper
{
    public static string Collection(string entityName, string methodName, params object[] parameters)
        => $"collection_{entityName}:{methodName}_{string.Join("_", parameters)}".ToLower();

    public static string Single(string entityName, params (string Label, object Value)[] segments)
        => $"{entityName}:{string.Join(":", segments.Select(s => $"{s.Label}({s.Value})"))}".ToLower();
}
