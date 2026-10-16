using AwesomeAssertions;
using Core.Library.Validators;
using System.Net;
using Xunit;

namespace Core.Library.Tests.ResultPattern.Result;

using Result = Core.Library.ResultPattern.Result;

public sealed partial class ResultTests
{
    [Fact]
    public void CombineValidationErrors_WhenAllPass_ReturnsSuccess()
    {
        // Arrange
        List<Result> results =
        [
            "Value1".Validate(FirstFieldName, maxLength: MaxLength),
            "Value2".Validate(SecondFieldName, maxLength: MaxLength)
        ];

        // Act
        Result combinedResult = Result.CombineValidationErrors(results);

        // Assert
        AssertSuccessResult(combinedResult, HttpStatusCode.OK);
        combinedResult.Message.Should().Be("Validation succeeded.");
        combinedResult.ErrorDetails.Should().BeNull();
    }

    [Fact]
    public void CombineValidationErrors_WithEmptyList_ReturnsSuccess()
    {
        // Arrange
        List<Result> results = [];

        // Act
        Result combinedResult = Result.CombineValidationErrors(results);

        // Assert
        AssertSuccessResult(combinedResult, HttpStatusCode.OK);
    }
}
