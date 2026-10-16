using AwesomeAssertions;
using Core.Library.ResultPattern;
using System.Net;

namespace Core.Library.Tests.Validators.ValueObjectValidator;

public sealed partial class ValueObjectValidatorTests
{
    private const string FieldName = "Address";

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
