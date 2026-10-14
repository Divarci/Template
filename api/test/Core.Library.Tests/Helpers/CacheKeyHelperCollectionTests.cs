using AwesomeAssertions;
using Core.Library.Helpers;
using Xunit;

namespace Core.Library.Tests.Helpers;

public sealed class CacheKeyHelperCollectionTests
{
    [Fact]
    public void Collection_WithParameters_ReturnsLowerCasedKeyJoinedByUnderscore()
    {
        // Arrange
        object[] parameters = [2, 10, "Title", true, "Shop", string.Empty];

        // Act
        string key = CacheKeyHelper.Collection(
            "Note",
            "GetPaginatedAsync",
            parameters);

        // Assert
        key.Should().Be("collection_note:getpaginatedasync_2_10_title_true_shop_");
    }

    [Fact]
    public void Collection_WithAnyInput_StartsWithCollectionPrefix()
    {
        // Arrange
        object[] parameters = [1, 20];

        // Act
        string key = CacheKeyHelper.Collection(
            "AuditLog",
            "GetPaginatedAsync",
            parameters);

        // Assert
        key.Should().StartWith("collection_");
    }

    [Fact]
    public void Collection_WithoutParameters_EndsWithMethodSeparator()
    {
        // Arrange
        string methodName = "GetAll";

        // Act
        string key = CacheKeyHelper.Collection("Note", methodName);

        // Assert
        key.Should().Be("collection_note:getall_");
    }
}
