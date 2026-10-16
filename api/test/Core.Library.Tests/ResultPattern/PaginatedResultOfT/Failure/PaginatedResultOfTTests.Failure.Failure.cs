using AwesomeAssertions;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Library.Tests.ResultPattern.PaginatedResultOfT;

public sealed partial class PaginatedResultOfTTests
{
    [Fact]
    public void Failure_WithMessageAndStatusCode_ReturnsFailedResultWithoutDataOrMetadata()
    {
        // Act
        PaginatedResult<string[]> result = PaginatedResult<string[]>.Failure(Message, HttpStatusCode.InternalServerError);

        // Assert
        AssertFailureResult(result, HttpStatusCode.InternalServerError);
        result.Message.Should().Be(Message);
        result.Data.Should().BeNull();
        result.Metadata.Should().BeNull();
    }
}
