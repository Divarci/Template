using AwesomeAssertions;
using System.Net;

namespace Core.Library.Tests.ResultPattern.Result;

using Result = Core.Library.ResultPattern.Result;

public sealed partial class ResultTests
{
    private const string Message = "Operation completed";
    private const string FirstFieldName = "Title";
    private const string SecondFieldName = "Content";
    private const int MaxLength = 50;

    private static void AssertSuccessResult(Result result, HttpStatusCode expectedStatusCode)
    {
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(expectedStatusCode);
    }

    private static void AssertFailureResult(Result result, HttpStatusCode expectedStatusCode)
    {
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(expectedStatusCode);
    }
}
