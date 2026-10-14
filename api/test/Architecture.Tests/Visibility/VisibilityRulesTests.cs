using ArchUnitNET.Fluent;
using ArchUnitNET.xUnitV3;
using AwesomeAssertions;
using Xunit;
using ReflectionAssembly = System.Reflection.Assembly;

namespace Architecture.Tests.Visibility;

public sealed class VisibilityRulesTests
{
    [Fact]
    public void ServiceImplementations_AnyImplementation_AreInternalSealed()
    {
        // Arrange
        IArchRule internalRule = ApiArchitecture.ServiceImplementations
            .Should()
            .BeInternal();
        IArchRule sealedRule = ApiArchitecture.ServiceImplementations
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed();
        IArchRule rule = internalRule.And(sealedRule);

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void UseCaseImplementations_AnyImplementation_AreInternalSealed()
    {
        // Arrange
        IArchRule internalRule = ApiArchitecture.UseCaseImplementations
            .Should()
            .BeInternal();
        IArchRule sealedRule = ApiArchitecture.UseCaseImplementations
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed();
        IArchRule rule = internalRule.And(sealedRule);

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RepositoryImplementations_AnyImplementation_AreInternalSealed()
    {
        // Arrange
        IArchRule internalRule = ApiArchitecture.RepositoryImplementations
            .Should()
            .BeInternal();
        IArchRule sealedRule = ApiArchitecture.RepositoryImplementations
            .And()
            .AreNotAbstract()
            .And()
            .DoNotHaveFullNameMatching(@"^Infra\.Persistence\.Repositories\.Generics\.Repository`1$")
            .Should()
            .BeSealed();
        IArchRule rule = internalRule.And(sealedRule);

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void DomainEventHandlerImplementations_AnyImplementation_AreInternalSealed()
    {
        // Arrange
        IArchRule internalRule = ApiArchitecture.DomainEventHandlers
            .And()
            .DoNotHaveFullNameMatching(@"^Core\.Library\.Contracts\.DomainEvents\.Handle\.DomainEventHandler`1$")
            .Should()
            .BeInternal();
        IArchRule sealedRule = ApiArchitecture.DomainEventHandlers
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed();
        IArchRule rule = internalRule.And(sealedRule);

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EntityRepositoryServiceAndUseCaseInterfaces_AnyEntity_LiveInAggregateInterfacesNamespace()
    {
        // Arrange
        string entityNames = string.Join(
            "|",
            ApiArchitecture.Entities
                .GetObjects(ApiArchitecture.Instance)
                .Select(entity => entity.Name));
        IArchRule rule = ApiArchitecture.ApiInterfaces
            .And()
            .HaveNameMatching($"^I({entityNames})(Repository|Service|UseCase)$")
            .Should()
            .ResideInNamespaceMatching(@"^Core\.Domain\.Entities\.[^.]+\.[^.]+\.Interfaces$");

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [MemberData(nameof(ApiArchitecture.InfraAssemblyNames), MemberType = typeof(ApiArchitecture))]
    public void InfraProject_PublicTypes_AreOnlyItsServiceRegistrar(string assemblyName)
    {
        // Arrange
        ReflectionAssembly infraAssembly = ApiArchitecture.InfraAssemblies.Single(a => a.GetName().Name == assemblyName);
        IArchRule rule = ApiArchitecture.TypesIn(infraAssembly)
            .And()
            .ArePublic()
            .And()
            .DoNotResideInNamespaceMatching($@"^{assemblyName.Replace(".", @"\.")}\.Migrations(\..+)?$")
            .Should()
            .HaveNameMatching("^ServiceRegistrar$");

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }
}
