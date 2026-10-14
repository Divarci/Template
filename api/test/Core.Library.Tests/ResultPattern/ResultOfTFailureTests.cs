using AwesomeAssertions;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Library.Tests.ResultPattern;

public sealed class ResultOfTFailureTests
{
    [Fact]
    public void Failure_WithMessageAndStatus_ReturnsFailedResultWithoutData()
    {
        // Arrange
        string message = "Not found";

        // Act
        Result<string> result = Result<string>.Failure(message, HttpStatusCode.NotFound);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Message.Should().Be("Not found");
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Data.Should().BeNull();
        result.ErrorDetails.Should().BeNull();
    }

    [Fact]
    public void Failure_WithErrorDetails_ReturnsFailedResultCarryingErrorDetails()
    {
        // Arrange
        ErrorDetails errorDetails = ErrorDetails.Create("Title cannot be null or empty.");

        // Act
        Result<string> result = Result<string>.Failure(
            "Validation failed",
            errorDetails,
            HttpStatusCode.UnprocessableEntity);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails.Should().BeSameAs(errorDetails);
        result.Data.Should().BeNull();
    }
}
