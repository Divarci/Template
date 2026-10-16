using AwesomeAssertions;
using Core.Library.ResultPattern;
using System.Net;

namespace Core.Library.Tests.Validators.StringValidator;

public sealed partial class StringValidatorTests
{
    private const string FieldName = "Title";
    private const int MaxLength = 50;
    private const int MinLength = 5;

    private static string CreateStringOfLength(int length)
    {
        return new string('A', length);
    }

    private static void AssertSuccessResult(Result result)
    {
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static void AssertFailureResult(Result result, HttpStatusCode expectedStatusCode)
    {
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
    }
}
