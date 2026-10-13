using System.Net;
using System.Text.Json.Serialization;

namespace Core.Library.ResultPattern;

public class PaginatedResult<T> : Result<T>
{
    [JsonConstructor]
    private PaginatedResult(HttpStatusCode statusCode, bool isFailure,
        string message, ErrorDetails? errorDetails, T? data, Metadata? metadata)
        : base(statusCode, isFailure, message, errorDetails, data)
    {
        Metadata = metadata;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Metadata? Metadata { get; set; }

    public static PaginatedResult<T> Success(T data, string message,
        int pageNumber, int pageSize, int totalCount, int pageCount)
        => new(HttpStatusCode.OK, false, message, null,
            data, new(pageNumber, pageSize, totalCount, pageCount));

    public static new PaginatedResult<T> Failure(string message, HttpStatusCode statusCode)
        => new(statusCode, true, message, null, default, null);
}
