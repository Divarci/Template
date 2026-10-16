using AwesomeAssertions;
using System.Net;
using Xunit;

namespace Core.Library.Tests.ResultPattern.Result;

using Result = Core.Library.ResultPattern.Result;

public sealed partial class ResultTests
{
    [Fact]
    public void Success_WithMessageAndStatusCode_ReturnsSuccessfulResult()
    {
        // Act
        Result result = Result.Success(Message, HttpStatusCode.OK);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        result.Message.Should().Be(Message);
        result.ErrorDetails.Should().BeNull();
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.NoContent)]
    public void Success_WithStatusCode_PreservesStatusCode(HttpStatusCode statusCode)
    {
        // Act
        Result result = Result.Success(Message, statusCode);

        // Assert
        AssertSuccessResult(result, statusCode);
    }
}
