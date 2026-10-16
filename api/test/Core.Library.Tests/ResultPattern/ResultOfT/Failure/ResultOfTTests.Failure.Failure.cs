using AwesomeAssertions;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Library.Tests.ResultPattern.ResultOfT;

using ErrorDetails = Core.Library.ResultPattern.ErrorDetails;

public sealed partial class ResultOfTTests
{
    [Fact]
    public void Failure_WithMessageAndStatusCode_ReturnsFailedResultWithoutData()
    {
        // Act
        Result<string> result = Result<string>.Failure(Message, HttpStatusCode.NotFound);

        // Assert
        AssertFailureResult(result, HttpStatusCode.NotFound);
        result.Message.Should().Be(Message);
        result.Data.Should().BeNull();
        result.ErrorDetails.Should().BeNull();
        result.IsFailureAndNoData.Should().BeTrue();
    }

    [Fact]
    public void Failure_WithErrorDetails_ReturnsFailedResultCarryingErrorDetails()
    {
        // Arrange
        ErrorDetails errorDetails = ErrorDetails.Create("Title cannot be null or empty.");

        // Act
        Result<string> result = Result<string>.Failure(Message, errorDetails, HttpStatusCode.UnprocessableEntity);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails.Should().BeSameAs(errorDetails);
        result.IsFailureAndNoData.Should().BeTrue();
    }

    [Fact]
    public void Failure_WithValueTypePayload_IsFailureAndNoData()
    {
        // Act
        Result<int> result = Result<int>.Failure(Message, HttpStatusCode.InternalServerError);

        // Assert
        AssertFailureResult(result, HttpStatusCode.InternalServerError);
        result.IsFailureAndNoData.Should().BeTrue();
    }

    [Fact]
    public void Failure_WhenDataIsAssignedAfterwards_IsNotFailureAndNoData()
    {
        // Arrange
        Result<string> result = Result<string>.Failure(Message, HttpStatusCode.Conflict);

        // Act
        result.Data = Data;

        // Assert
        AssertFailureResult(result, HttpStatusCode.Conflict);
        result.IsFailureAndNoData.Should().BeFalse();
    }
}
