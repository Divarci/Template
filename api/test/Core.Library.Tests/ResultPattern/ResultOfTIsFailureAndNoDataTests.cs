using AwesomeAssertions;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Library.Tests.ResultPattern;

public sealed class ResultOfTIsFailureAndNoDataTests
{
    [Fact]
    public void IsFailureAndNoData_WhenResultSucceeds_ReturnsFalse()
    {
        // Arrange
        Result<string> result = Result<string>.Success(
            "Retrieved",
            "payload",
            HttpStatusCode.OK);

        // Act
        bool isFailureAndNoData = result.IsFailureAndNoData;

        // Assert
        isFailureAndNoData.Should().BeFalse();
    }

    [Fact]
    public void IsFailureAndNoData_WhenReferencePayloadFailureHasNoData_ReturnsTrue()
    {
        // Arrange
        Result<string> result = Result<string>.Failure("Not found", HttpStatusCode.NotFound);

        // Act
        bool isFailureAndNoData = result.IsFailureAndNoData;

        // Assert
        isFailureAndNoData.Should().BeTrue();
    }

    [Fact]
    public void IsFailureAndNoData_WhenValuePayloadFailureHasDefaultData_ReturnsTrue()
    {
        // Arrange
        Result<int> result = Result<int>.Failure("Count failed", HttpStatusCode.InternalServerError);

        // Act
        bool isFailureAndNoData = result.IsFailureAndNoData;

        // Assert
        isFailureAndNoData.Should().BeTrue();
    }

    [Fact]
    public void IsFailureAndNoData_WhenValuePayloadSucceedsWithDefaultData_ReturnsFalse()
    {
        // Arrange
        Result<int> result = Result<int>.Success(
            "Counted",
            0,
            HttpStatusCode.OK);

        // Act
        bool isFailureAndNoData = result.IsFailureAndNoData;

        // Assert
        isFailureAndNoData.Should().BeFalse();
    }
}
