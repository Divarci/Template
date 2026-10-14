using AwesomeAssertions;
using Xunit;

namespace Architecture.Tests;

public sealed class AssemblyDiscoveryApiAssemblyNamesTests
{
    [Fact]
    public void ApiAssemblyNames_WhenRunUnderTestHost_FindsAtLeastTheNineCurrentApiAssemblies()
    {
        // Arrange
        int currentApiAssemblyCount = 9;

        // Act
        string[] names = AssemblyDiscovery.ApiAssemblyNames();

        // Assert
        names.Should().HaveCountGreaterThanOrEqualTo(currentApiAssemblyCount);
    }
}
