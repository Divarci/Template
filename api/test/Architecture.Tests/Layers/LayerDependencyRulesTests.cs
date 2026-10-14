using ArchUnitNET.Fluent;
using ArchUnitNET.xUnitV3;
using AwesomeAssertions;
using Xunit;
using ReflectionAssembly = System.Reflection.Assembly;

namespace Architecture.Tests.Layers;

public sealed class LayerDependencyRulesTests
{
    [Fact]
    public void CoreLibrary_AnyType_DependsOnNoOtherApiAssembly()
    {
        // Arrange
        IArchRule rule = ArchitectureRules.AssemblyMustNotDependOn(
            ApiArchitecture.CoreLibrary,
            [.. ApiArchitecture.All.Except([ApiArchitecture.CoreLibrary])]);

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void CoreDomain_AnyType_DependsOnlyOnCoreLibrary()
    {
        // Arrange
        IArchRule rule = ArchitectureRules.AssemblyMustNotDependOn(
            ApiArchitecture.CoreDomain,
            [.. ApiArchitecture.All.Except([ApiArchitecture.CoreDomain, ApiArchitecture.CoreLibrary])]);

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void CoreServices_AnyType_DependsOnNoInfraAdapterOrApplicationAssembly()
    {
        // Arrange
        IArchRule rule = ArchitectureRules.AssemblyMustNotDependOn(
            ApiArchitecture.CoreServices,
            [
                .. ApiArchitecture.InfraAssemblies,
                .. ApiArchitecture.AdapterAssemblies,
                ApiArchitecture.AppUseCases,
                ApiArchitecture.AppProcessor,
            ]);

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AppUseCases_AnyType_DependsOnNoInfraAdapterServiceOrProcessorAssembly()
    {
        // Arrange
        IArchRule rule = ArchitectureRules.AssemblyMustNotDependOn(
            ApiArchitecture.AppUseCases,
            [
                .. ApiArchitecture.InfraAssemblies,
                .. ApiArchitecture.AdapterAssemblies,
                ApiArchitecture.CoreServices,
                ApiArchitecture.AppProcessor,
            ]);

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AppProcessor_AnyType_DependsOnNoInfraAdapterServiceOrUseCaseAssembly()
    {
        // Arrange
        IArchRule rule = ArchitectureRules.AssemblyMustNotDependOn(
            ApiArchitecture.AppProcessor,
            [
                .. ApiArchitecture.InfraAssemblies,
                .. ApiArchitecture.AdapterAssemblies,
                ApiArchitecture.CoreServices,
                ApiArchitecture.AppUseCases,
            ]);

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [MemberData(nameof(ApiArchitecture.InfraAssemblyNames), MemberType = typeof(ApiArchitecture))]
    public void InfraProject_AnyType_DependsOnNoApplicationAdapterServiceOrOtherInfraAssembly(string assemblyName)
    {
        // Arrange
        ReflectionAssembly infraAssembly = ApiArchitecture.InfraAssemblies.Single(a => a.GetName().Name == assemblyName);
        IArchRule rule = ArchitectureRules.AssemblyMustNotDependOn(
            infraAssembly,
            [
                ApiArchitecture.AppUseCases,
                ApiArchitecture.AppProcessor,
                .. ApiArchitecture.AdapterAssemblies,
                ApiArchitecture.CoreServices,
                .. ApiArchitecture.InfraAssemblies.Except([infraAssembly]),
            ]);

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }
}
