using AwesomeAssertions;
using System.Net;
using Xunit;

namespace Core.Library.Tests.ResultPattern.Result;

using ErrorDetails = Core.Library.ResultPattern.ErrorDetails;
using Result = Core.Library.ResultPattern.Result;

public sealed partial class ResultTests
{
    [Fact]
    public void Failure_WithMessageAndStatusCode_ReturnsFailedResult()
    {
        // Act
        Result result = Result.Failure(Message, HttpStatusCode.Conflict);

        // Assert
        AssertFailureResult(result, HttpStatusCode.Conflict);
        result.Message.Should().Be(Message);
        result.ErrorDetails.Should().BeNull();
    }

    [Fact]
    public void Failure_WithErrorDetails_ReturnsFailedResultCarryingErrorDetails()
    {
        // Arrange
        ErrorDetails errorDetails = ErrorDetails.Create("Title cannot be null or empty.");

        // Act
        Result result = Result.Failure(Message, errorDetails, HttpStatusCode.UnprocessableEntity);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.Message.Should().Be(Message);
        result.ErrorDetails.Should().BeSameAs(errorDetails);
    }
}
