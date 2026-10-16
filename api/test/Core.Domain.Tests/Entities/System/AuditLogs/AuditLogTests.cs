using AwesomeAssertions;
using Core.Library.ResultPattern;
using System.Net;

namespace Core.Domain.Tests.Entities.System.AuditLogs;

public sealed partial class AuditLogTests
{
    private const int DescriptionMaxLength = 500;
    private const string ValidDescription = "Note created successfully";

    private static string CreateStringOfLength(int length)
    {
        return new string('A', length);
    }

    private static void AssertSuccessResult<T>(Result<T> result, HttpStatusCode expectedStatusCode)
    {
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
        result.Data.Should().NotBeNull();
    }

    private static void AssertFailureResult<T>(Result<T> result, HttpStatusCode expectedStatusCode)
    {
        result.IsFailureAndNoData.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
    }
}
