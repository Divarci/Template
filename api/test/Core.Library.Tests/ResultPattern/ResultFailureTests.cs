using AwesomeAssertions;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Library.Tests.ResultPattern;

public sealed class ResultFailureTests
{
    [Fact]
    public void Failure_WithMessageAndStatus_ReturnsFailedResultWithoutErrorDetails()
    {
        // Arrange
        string message = "Not found";

        // Act
        Result result = Result.Failure(message, HttpStatusCode.NotFound);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Not found");
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.ErrorDetails.Should().BeNull();
    }

    [Fact]
    public void Failure_WithErrorDetails_ReturnsFailedResultCarryingErrorDetails()
    {
        // Arrange
        ErrorDetails errorDetails = ErrorDetails.Create("Title cannot be null or empty.", "trace-1");

        // Act
        Result result = Result.Failure(
            "Validation failed",
            errorDetails,
            HttpStatusCode.UnprocessableEntity);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails.Should().BeSameAs(errorDetails);
    }
}
