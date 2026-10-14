using ArchUnitNET.Fluent;
using ArchUnitNET.xUnitV3;
using AwesomeAssertions;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Architecture.Tests.ThirdParty;

public sealed class ThirdPartyConfinementRulesTests
{
    [Fact]
    public void EntityFrameworkCoreAndNpgsql_AnyUsage_IsConfinedToInfraPersistence()
    {
        // Arrange
        IArchRule rule = ArchitectureRules.ThirdPartyConfinedTo(
            ApiArchitecture.InfraPersistence,
            @"^(Microsoft\.EntityFrameworkCore|Npgsql)(\..+)?$");

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void StackExchangeRedis_AnyUsage_IsConfinedToInfraCaching()
    {
        // Arrange
        IArchRule rule = ArchitectureRules.ThirdPartyConfinedTo(
            ApiArchitecture.InfraCaching,
            @"^StackExchange\.Redis(\..+)?$");

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Quartz_AnyUsage_IsConfinedToAdapterScheduler()
    {
        // Arrange
        IArchRule rule = ArchitectureRules.ThirdPartyConfinedTo(
            ApiArchitecture.AdapterScheduler,
            @"^Quartz(\..+)?$");

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AspNetCore_AnyUsage_IsConfinedToAdapterRestful()
    {
        // Arrange
        IArchRule rule = ArchitectureRules.ThirdPartyConfinedTo(
            ApiArchitecture.AdapterRestful,
            @"^Microsoft\.AspNetCore(\..+)?$");

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void CoreAndAppProjects_AnyType_UseNoThirdPartyTypeBeyondExtensionsAbstractions()
    {
        // Arrange
        IArchRule rule = ApiArchitecture.TypesIn(
                ApiArchitecture.CoreLibrary,
                ApiArchitecture.CoreDomain,
                ApiArchitecture.CoreServices,
                ApiArchitecture.AppUseCases,
                ApiArchitecture.AppProcessor)
            .Should()
            .NotDependOnAny(
                Types(true)
                    .That()
                    .DoNotResideInAssemblyMatching(ArchitectureRules.PlatformOrApiAssemblyPattern));

        // Act
        Action act = () => rule.Check(ApiArchitecture.Instance);

        // Assert
        act.Should().NotThrow();
    }
}
