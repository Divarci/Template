using AwesomeAssertions;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Library.Tests.ResultPattern;

public sealed class ResultSuccessTests
{
    [Fact]
    public void Success_WithMessageAndStatus_ReturnsSuccessfulResult()
    {
        // Arrange
        string message = "Done";

        // Act
        Result result = Result.Success(message, HttpStatusCode.NoContent);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Message.Should().Be("Done");
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        result.ErrorDetails.Should().BeNull();
    }
}
