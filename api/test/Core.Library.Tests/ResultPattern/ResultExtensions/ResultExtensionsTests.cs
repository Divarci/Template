using Core.Library.ResultPattern;
using System.Globalization;
using System.Net;

namespace Core.Library.Tests.ResultPattern.ResultExtensions;

public sealed partial class ResultExtensionsTests
{
    private const string Message = "Note retrieved successfully";
    private const string Title = "Shopping list";
    private const int PageNumber = 1;
    private const int PageSize = 10;
    private const int TotalCount = 2;
    private const int PageCount = 1;

    private static Result<T> CreateFailedResultWithData<T>(T data)
    {
        Result<T> result = Result<T>.Success(Message, data, HttpStatusCode.OK);

        result.IsFailure = true;
        result.StatusCode = HttpStatusCode.Conflict;

        return result;
    }

    private static PaginatedResult<T> CreateFailedPaginatedResultWithData<T>(T data)
    {
        PaginatedResult<T> result = PaginatedResult<T>.Success(
            data,
            Message,
            PageNumber,
            PageSize,
            TotalCount,
            PageCount);

        result.IsFailure = true;
        result.StatusCode = HttpStatusCode.Conflict;

        return result;
    }

    private static int MapToLength(string value)
    {
        return value.Length;
    }

    private static int[] MapToLengths(string[] values)
    {
        return [.. values.Select(MapToLength)];
    }

    private static string MapToText(int value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
}
