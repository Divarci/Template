using AwesomeAssertions;
using Core.Library.Helpers;
using Xunit;

namespace Core.Library.Tests.Helpers;

public sealed class CacheKeyHelperSingleTests
{
    [Fact]
    public void Single_WithOneSegment_ReturnsLowerCasedEntityAndLabelledValue()
    {
        // Arrange
        Guid id = Guid.Parse("6F9619FF-8B86-D011-B42D-00C04FC964FF");

        // Act
        string key = CacheKeyHelper.Single("Note", ("id", id));

        // Assert
        key.Should().Be("note:id(6f9619ff-8b86-d011-b42d-00c04fc964ff)");
    }

    [Fact]
    public void Single_WithSeveralSegments_JoinsSegmentsWithColon()
    {
        // Arrange
        Guid id = Guid.Parse("6F9619FF-8B86-D011-B42D-00C04FC964FF");

        // Act
        string key = CacheKeyHelper.Single(
            "AuditLog",
            ("Id", id),
            ("Version", 2));

        // Assert
        key.Should().Be("auditlog:id(6f9619ff-8b86-d011-b42d-00c04fc964ff):version(2)");
    }

    [Fact]
    public void Single_WithSameInputs_ReturnsSameKey()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        string key = CacheKeyHelper.Single("Note", ("id", id));

        // Assert
        key.Should().Be(CacheKeyHelper.Single("NOTE", ("ID", id)));
    }
}
