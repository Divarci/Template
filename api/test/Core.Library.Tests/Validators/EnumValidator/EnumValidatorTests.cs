using AwesomeAssertions;
using Core.Library.ResultPattern;
using System.Net;

namespace Core.Library.Tests.Validators.EnumValidator;

public sealed partial class EnumValidatorTests
{
    private const string FieldName = "Restriction";
    private const int UndefinedEnumValue = 99;

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
