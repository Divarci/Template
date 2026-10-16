using AwesomeAssertions;
using Core.Library.ResultPattern;
using System.Net;

namespace Core.Library.Tests.Validators.DecimalValidator;

public sealed partial class DecimalValidatorTests
{
    private const string FieldName = "Price";
    private const decimal MinValue = 10.50m;
    private const decimal MaxValue = 100.25m;
    private const decimal Step = 0.01m;

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
