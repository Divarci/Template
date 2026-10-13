using System.Net;
using System.Text.Json.Serialization;

namespace Core.Library.ResultPattern;

public class Result
{
    protected Result(HttpStatusCode statusCode,
        bool isFailure, string message, ErrorDetails? errorDetails)
    {
        StatusCode = statusCode;
        IsFailure = isFailure;
        Message = message;
        ErrorDetails = errorDetails;
    }

    [JsonIgnore]
    public HttpStatusCode StatusCode { get; set; }

    [JsonIgnore]
    public bool IsFailure { get; set; }

    [JsonIgnore]
    public bool IsSuccess => !IsFailure;

    public string Message { get; set; } = default!;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ErrorDetails? ErrorDetails { get; set; }

    public static Result Success(string message, HttpStatusCode statusCode)
        => new(statusCode, false, message, null);

    public static Result Failure(string message, HttpStatusCode statusCode)
        => new(statusCode, true, message, null);

    public static Result Failure(string message, ErrorDetails errorDetails,
        HttpStatusCode statusCode)
        => new(statusCode, true, message, errorDetails);

    public static Result CombineValidationErrors(List<Result> results)
    {
        List<Result> failures = [.. results.Where(r => r.IsFailure)];

        if (!(failures.Count > 0))
            return Success(
                message: "Validation succeeded.",
                statusCode: HttpStatusCode.OK);

        List<string> errors = [.. failures.SelectMany(r => r.ErrorDetails?.Errors ?? [])];

        return Failure(
            message: "Validation failed",
            errorDetails: ErrorDetails.Create(errors),
            statusCode: HttpStatusCode.UnprocessableEntity);
    }
}
