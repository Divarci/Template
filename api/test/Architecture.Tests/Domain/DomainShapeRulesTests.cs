using ArchUnitNET.Fluent;
using ArchUnitNET.xUnitV3;
using AwesomeAssertions;
using Core.Library.Contracts.DomainEvents.Process;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Architecture.Tests.Domain;

public sealed class DomainShapeRulesTests
{
    [Fact]
    public void Entities_DerivingFromBaseEntity_ArePublicSealed()
    {
        // Arrange
        IArchRule rule = ApiArchitecture.Entities
            .Should()
            .BePublic()
            .AndShould()
            .BeSealed();

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Entities_DerivingFromBaseEntity_ExposeNoPublicConstructor()
    {
        // Arrange
        IArchRule rule = MethodMembers()
            .That()
            .AreConstructors()
            .And()
            .ArePublic()
            .And()
            .AreDeclaredIn(ApiArchitecture.Entities)
            .Should()
            .NotExist();

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Entities_DerivingFromBaseEntity_ExposeNoPublicPropertySetter()
    {
        // Arrange
        IArchRule rule = PropertyMembers()
            .That()
            .AreDeclaredIn(ApiArchitecture.Entities)
            .Should()
            .NotHavePublicSetter();

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void DomainEvents_DerivingFromDomainEvent_AreSealedRecords()
    {
        // Arrange
        IArchRule rule = ApiArchitecture.ApiClasses
            .And()
            .AreAssignableTo(typeof(DomainEvent))
            .And()
            .AreNot(typeof(DomainEvent))
            .Should()
            .BeSealed()
            .AndShould()
            .BeRecord();

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void CreateAndUpdateModels_InApiAssemblies_AreSealedRecords()
    {
        // Arrange
        IArchRule rule = ApiArchitecture.ApiClasses
            .And()
            .HaveNameMatching("(Create|Update)Model$")
            .Should()
            .BeSealed()
            .AndShould()
            .BeRecord();

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }
}
