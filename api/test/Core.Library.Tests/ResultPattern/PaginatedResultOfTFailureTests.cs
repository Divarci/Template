using AwesomeAssertions;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Library.Tests.ResultPattern;

public sealed class PaginatedResultOfTFailureTests
{
    [Fact]
    public void Failure_WithMessageAndStatus_ReturnsFailedResultWithoutDataOrMetadata()
    {
        // Arrange
        string message = "Query failed";

        // Act
        PaginatedResult<string[]> result = PaginatedResult<string[]>.Failure(message, HttpStatusCode.InternalServerError);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Message.Should().Be("Query failed");
        result.Data.Should().BeNull();
        result.Metadata.Should().BeNull();
        result.IsFailureAndNoData.Should().BeTrue();
    }
}
