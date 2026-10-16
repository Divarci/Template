using AwesomeAssertions;
using Core.Library.Validators;
using System.Net;
using Xunit;

namespace Core.Library.Tests.ResultPattern.Result;

using Result = Core.Library.ResultPattern.Result;

public sealed partial class ResultTests
{
    [Fact]
    public void CombineValidationErrors_WithMultipleFailures_ReturnsAllErrors()
    {
        // Arrange
        List<Result> results =
        [
            string.Empty.Validate(FirstFieldName, maxLength: MaxLength),
            string.Empty.Validate(SecondFieldName, maxLength: MaxLength)
        ];

        // Act
        Result combinedResult = Result.CombineValidationErrors(results);

        // Assert
        AssertFailureResult(combinedResult, HttpStatusCode.UnprocessableEntity);
        combinedResult.Message.Should().Be("Validation failed");
        combinedResult.ErrorDetails!.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void CombineValidationErrors_WithMixedResults_ReturnsOnlyFailureErrors()
    {
        // Arrange
        List<Result> results =
        [
            "Value1".Validate(FirstFieldName, maxLength: MaxLength),
            string.Empty.Validate(SecondFieldName, maxLength: MaxLength)
        ];

        // Act
        Result combinedResult = Result.CombineValidationErrors(results);

        // Assert
        AssertFailureResult(combinedResult, HttpStatusCode.UnprocessableEntity);
        combinedResult.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Content cannot be null or empty.");
    }

    [Fact]
    public void CombineValidationErrors_WithFailureWithoutErrorDetails_ReturnsValidationErrorWithoutErrors()
    {
        // Arrange
        List<Result> results =
        [
            Result.Failure(Message, HttpStatusCode.Conflict)
        ];

        // Act
        Result combinedResult = Result.CombineValidationErrors(results);

        // Assert
        AssertFailureResult(combinedResult, HttpStatusCode.UnprocessableEntity);
        combinedResult.ErrorDetails!.Errors.Should().BeEmpty();
    }
}
