using AwesomeAssertions;
using Core.Library.ResultPattern;
using System.Net;

namespace Core.Library.Tests.ResultPattern.ResultOfT;

public sealed partial class ResultOfTTests
{
    private const string Message = "Note retrieved successfully";
    private const string Data = "Shopping list";

    private static void AssertSuccessResult<T>(Result<T> result, HttpStatusCode expectedStatusCode)
    {
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(expectedStatusCode);
    }

    private static void AssertFailureResult<T>(Result<T> result, HttpStatusCode expectedStatusCode)
    {
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(expectedStatusCode);
    }
}
