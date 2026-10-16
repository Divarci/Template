using AwesomeAssertions;
using Core.Library.ResultPattern;
using System.Net;

namespace Core.Library.Tests.Validators.DateTimeValidator;

public sealed partial class DateTimeValidatorTests
{
    private const string FieldName = "OccurredOnUtc";

    private static readonly DateTime MinValue = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime MaxValue = new(2025, 12, 31, 23, 59, 59, DateTimeKind.Utc);
    private static readonly TimeSpan Step = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan ClockOffset = TimeSpan.FromDays(1);

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
