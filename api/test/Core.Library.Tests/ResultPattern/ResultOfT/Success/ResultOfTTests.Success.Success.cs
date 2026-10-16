using AwesomeAssertions;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Library.Tests.ResultPattern.ResultOfT;

public sealed partial class ResultOfTTests
{
    [Fact]
    public void Success_WithData_ReturnsSuccessfulResultCarryingData()
    {
        // Act
        Result<string> result = Result<string>.Success(Message, Data, HttpStatusCode.OK);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        result.Message.Should().Be(Message);
        result.Data.Should().Be(Data);
        result.ErrorDetails.Should().BeNull();
        result.IsFailureAndNoData.Should().BeFalse();
    }

    [Fact]
    public void Success_WithDefaultValuePayload_IsNotFailureAndNoData()
    {
        // Act
        Result<bool> result = Result<bool>.Success(Message, false, HttpStatusCode.OK);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        result.Data.Should().BeFalse();
        result.IsFailureAndNoData.Should().BeFalse();
    }
}
