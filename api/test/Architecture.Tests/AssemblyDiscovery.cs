using System.Text.RegularExpressions;

namespace Architecture.Tests;

internal static class AssemblyDiscovery
{
    private static readonly Regex ApiAssemblyNamePattern = new(@"^(Core|App|Infra|Adapter)\.[A-Za-z0-9]+$");

    public static string[] ApiAssemblyNames() =>
        [.. TrustedPlatformAssemblyPaths()
            .Select(path => Path.GetFileNameWithoutExtension(path))
            .Where(name => ApiAssemblyNamePattern.IsMatch(name))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    public static string[] SharedRuntimeAssemblyNames()
    {
        string runtimeDirectory = Path.GetFullPath(Path.GetDirectoryName(typeof(object).Assembly.Location)!);
        StringComparison pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        return
        [
            .. TrustedPlatformAssemblyPaths()
                .Where(path => string.Equals(
                    Path.GetFullPath(Path.GetDirectoryName(path)!),
                    runtimeDirectory,
                    pathComparison))
                .Select(path => Path.GetFileNameWithoutExtension(path))
                .Distinct(StringComparer.Ordinal),
        ];
    }

    private static string[] TrustedPlatformAssemblyPaths() =>
        ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
}
