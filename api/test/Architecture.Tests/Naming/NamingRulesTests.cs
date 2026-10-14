using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnitV3;
using AwesomeAssertions;
using Core.Library.Contracts.DomainEvents.Process;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Architecture.Tests.Naming;

public sealed class NamingRulesTests
{
    [Fact]
    public void Interfaces_InApiAssemblies_StartWithI()
    {
        // Arrange
        IArchRule rule = ApiArchitecture.ApiInterfaces
            .Should()
            .HaveNameStartingWith("I");

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ServiceInterfaceImplementations_InApiAssemblies_EndWithService()
    {
        // Arrange
        IArchRule rule = ApiArchitecture.ServiceImplementations
            .Should()
            .HaveNameMatching(@"Service(`\d+)?$");

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void UseCaseInterfaceImplementations_InApiAssemblies_EndWithUseCase()
    {
        // Arrange
        IArchRule rule = ApiArchitecture.UseCaseImplementations
            .Should()
            .HaveNameMatching(@"UseCase(`\d+)?$");

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RepositoryInterfaceImplementations_InApiAssemblies_EndWithRepository()
    {
        // Arrange
        IArchRule rule = ApiArchitecture.RepositoryImplementations
            .Should()
            .HaveNameMatching(@"Repository(`\d+)?$");

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Controllers_InApiAssemblies_EndWithController()
    {
        // Arrange
        IArchRule rule = ApiArchitecture.Controllers
            .Should()
            .HaveNameEndingWith("Controller");

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void DomainEvents_InApiAssemblies_EndWithDomainEvent()
    {
        // Arrange
        IArchRule rule = ApiArchitecture.ApiClasses
            .And()
            .AreAssignableTo(typeof(IDomainEvent))
            .Should()
            .HaveNameEndingWith("DomainEvent");

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void DomainEventHandlers_InApiAssemblies_EndWithDomainEventHandler()
    {
        // Arrange
        IArchRule rule = ApiArchitecture.DomainEventHandlers
            .Should()
            .HaveNameMatching(@"DomainEventHandler(`\d+)?$");

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EntityTypeConfigurations_InApiAssemblies_EndWithConfig()
    {
        // Arrange
        IArchRule rule = ApiArchitecture.ApiClasses
            .And()
            .ImplementAnyInterfaces(
                Interfaces(true)
                    .That()
                    .HaveFullNameMatching(@"^Microsoft\.EntityFrameworkCore\.IEntityTypeConfiguration`1"))
            .Should()
            .HaveNameEndingWith("Config");

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AsyncMethods_InApiAssemblies_EndWithAsync()
    {
        // Arrange
        IArchRule rule = MethodMembers()
            .That()
            .AreDeclaredIn(ApiArchitecture.TypesIn([.. ApiArchitecture.All]))
            .And()
            .FollowCustomPredicate(
                method => method.MethodForm == MethodForm.Normal
                    && !method.IsCompilerGenerated
                    && !method.DeclaringType.IsCompilerGenerated
                    && ArchitectureRules.IsAwaitable(method.ReturnType)
                    && !ArchitectureRules.ImplementsThirdPartyInterfaceMember(method),
                "return a Task or ValueTask and do not implement a third-party interface member")
            .Should()
            .HaveNameMatching(@"^[^(]*Async\(");

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }
}
