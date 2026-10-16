using AwesomeAssertions;
using Core.Library.ResultPattern;
using System.Net;

namespace Core.Library.Tests.ResultPattern.PaginatedResultOfT;

public sealed partial class PaginatedResultOfTTests
{
    private const string Message = "List retrieved successfully";
    private const int PageNumber = 2;
    private const int PageSize = 10;
    private const int TotalCount = 25;
    private const int PageCount = 3;

    private static string[] CreatePageData()
    {
        return ["First note", "Second note"];
    }

    private static void AssertSuccessResult<T>(PaginatedResult<T> result)
    {
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().NotBeNull();
    }

    private static void AssertFailureResult<T>(PaginatedResult<T> result, HttpStatusCode expectedStatusCode)
    {
        result.IsFailureAndNoData.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
    }
}
