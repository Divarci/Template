using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json.Serialization;

namespace Core.Library.ResultPattern;

public class Result<T> : Result
{
    protected Result(HttpStatusCode statusCode,
        bool isFailure, string message, ErrorDetails? errorDetails, T? data)
        : base(statusCode, isFailure, message, errorDetails)
    {
        Data = data;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public T? Data { get; set; }

    // EqualityComparer.Default handles value-typed payloads (Guid, bool, …) for which a null check never holds.
    [JsonIgnore]
    [MemberNotNullWhen(returnValue: false, nameof(Data))]
    public bool IsFailureAndNoData => IsFailure && EqualityComparer<T>.Default.Equals(Data, default);

    public static Result<T> Success(string message, T data, HttpStatusCode statusCode)
        => new(statusCode, false, message, null, data);

    public static new Result<T> Failure(string message, HttpStatusCode statusCode)
        => new(statusCode, true, message, null, default);

    public static new Result<T> Failure(string message, ErrorDetails errorDetails,
        HttpStatusCode statusCode)
        => new(statusCode, true, message, errorDetails, default);
}
