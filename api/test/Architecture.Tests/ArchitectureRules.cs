using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using System.Reflection;
using System.Text.RegularExpressions;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ReflectionAssembly = System.Reflection.Assembly;

namespace Architecture.Tests;

internal static class ArchitectureRules
{
    public static readonly string PlatformOrApiAssemblyPattern =
        $@"^({string.Join("|", AllowedAssemblyNames().Select(Regex.Escape))}|Microsoft\.Extensions\.[^,]+\.Abstractions)(,|$)";

    public static IArchRule AssemblyMustNotDependOn(
        ReflectionAssembly assembly,
        params ReflectionAssembly[] forbidden) =>
        Types()
            .That()
            .ResideInAssembly(assembly)
            .Should()
            .NotDependOnAny(ApiArchitecture.TypesIn(forbidden));

    public static IArchRule ThirdPartyConfinedTo(
        ReflectionAssembly owner,
        string thirdPartyNamespacePattern) =>
        ApiArchitecture.ApiTypesExcept(owner)
            .Should()
            .NotDependOnAny(
                Types(true)
                    .That()
                    .ResideInNamespaceMatching(thirdPartyNamespacePattern));

    public static IArchRule EachMustNotDependOnPeers(
        IEnumerable<Class> subjects,
        Func<Class, IEnumerable<IType>> forbiddenPeers) =>
        subjects
            .Select(subject => (IArchRule)Classes()
                .That()
                .Are(subject)
                .Should()
                .NotDependOnAny(forbiddenPeers(subject)))
            .Aggregate((combined, next) => combined.And(next));

    public static bool IsAwaitable(IType returnType) =>
        returnType.FullName.StartsWith("System.Threading.Tasks.Task", StringComparison.Ordinal)
        || returnType.FullName.StartsWith("System.Threading.Tasks.ValueTask", StringComparison.Ordinal);

    public static bool ImplementsThirdPartyInterfaceMember(MethodMember method)
    {
        Type? declaringType = ApiArchitecture.All
            .Select(assembly => assembly.GetType(method.DeclaringType.FullName))
            .FirstOrDefault(type => type is not null);

        if (declaringType is null || declaringType.IsInterface)
            return false;

        MethodInfo[] resolved =
        [
            .. declaringType
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(candidate => Signature(candidate) == method.Name),
        ];

        if (resolved.Length != 1)
            return false;

        return declaringType
            .GetInterfaces()
            .Where(implemented => !ApiArchitecture.All.Contains(implemented.Assembly))
            .SelectMany(implemented => declaringType.GetInterfaceMap(implemented).TargetMethods)
            .Contains(resolved[0]);
    }

    private static IEnumerable<string> AllowedAssemblyNames() =>
        AssemblyDiscovery.SharedRuntimeAssemblyNames()
            .Concat(ApiArchitecture.All.Select(assembly => assembly.GetName().Name!));

    private static string Signature(MethodInfo method) =>
        $"{method.Name}({string.Join(",", method.GetParameters().Select(parameter => TypeName(parameter.ParameterType)))})";

    private static string TypeName(Type type)
    {
        if (type.IsByRef || type.IsPointer)
            return TypeName(type.GetElementType()!);

        if (type.IsArray)
            return $"{TypeName(type.GetElementType()!)}[]";

        if (type.IsGenericParameter)
            return type.Name;

        if (type.IsGenericType)
            return $"{type.GetGenericTypeDefinition().FullName}<{string.Join(",", type.GetGenericArguments().Select(TypeName))}>";

        return type.FullName ?? type.Name;
    }
}
