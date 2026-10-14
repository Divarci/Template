using AwesomeAssertions;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Library.Tests.ResultPattern;

public sealed class ResultCombineValidationErrorsTests
{
    [Fact]
    public void CombineValidationErrors_WhenEveryResultSucceeds_ReturnsOk()
    {
        // Arrange
        List<Result> results =
        [
            Result.Success("Validation successful", HttpStatusCode.OK),
            Result.Success("Validation successful", HttpStatusCode.OK),
        ];

        // Act
        Result combined = Result.CombineValidationErrors(results);

        // Assert
        combined.IsSuccess.Should().BeTrue();
        combined.StatusCode.Should().Be(HttpStatusCode.OK);
        combined.Message.Should().Be("Validation succeeded.");
    }

    [Fact]
    public void CombineValidationErrors_WhenListIsEmpty_ReturnsOk()
    {
        // Arrange
        List<Result> results = [];

        // Act
        Result combined = Result.CombineValidationErrors(results);

        // Assert
        combined.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CombineValidationErrors_WhenSomeResultsFail_ReturnsUnprocessableEntity()
    {
        // Arrange
        List<Result> results =
        [
            Result.Success("Validation successful", HttpStatusCode.OK),
            Result.Failure(
                "Title validation failed",
                ErrorDetails.Create("Title error"),
                HttpStatusCode.UnprocessableEntity),
        ];

        // Act
        Result combined = Result.CombineValidationErrors(results);

        // Assert
        combined.IsFailure.Should().BeTrue();
        combined.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        combined.Message.Should().Be("Validation failed");
    }

    [Fact]
    public void CombineValidationErrors_WhenSeveralResultsFail_AggregatesEveryError()
    {
        // Arrange
        List<Result> results =
        [
            Result.Failure(
                "Title validation failed",
                ErrorDetails.Create(["Title error 1", "Title error 2"]),
                HttpStatusCode.UnprocessableEntity),
            Result.Success("Validation successful", HttpStatusCode.OK),
            Result.Failure(
                "Content validation failed",
                ErrorDetails.Create("Content error"),
                HttpStatusCode.UnprocessableEntity),
        ];

        // Act
        Result combined = Result.CombineValidationErrors(results);

        // Assert
        combined.ErrorDetails!.Errors.Should().Equal(
            "Title error 1",
            "Title error 2",
            "Content error");
    }

    [Fact]
    public void CombineValidationErrors_WhenFailureHasNoErrorDetails_ReturnsFailureWithoutErrors()
    {
        // Arrange
        List<Result> results =
        [
            Result.Failure("Field validation failed", HttpStatusCode.InternalServerError),
        ];

        // Act
        Result combined = Result.CombineValidationErrors(results);

        // Assert
        combined.IsFailure.Should().BeTrue();
        combined.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        combined.ErrorDetails!.Errors.Should().BeEmpty();
    }
}
