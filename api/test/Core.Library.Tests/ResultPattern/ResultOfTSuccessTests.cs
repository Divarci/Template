using AwesomeAssertions;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Library.Tests.ResultPattern;

public sealed class ResultOfTSuccessTests
{
    [Fact]
    public void Success_WithData_ReturnsSuccessfulResultCarryingData()
    {
        // Arrange
        string data = "payload";

        // Act
        Result<string> result = Result<string>.Success(
            "Retrieved",
            data,
            HttpStatusCode.OK);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Retrieved");
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().Be("payload");
        result.ErrorDetails.Should().BeNull();
    }
}
