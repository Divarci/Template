namespace Core.Library.ResultPattern;

public class ErrorDetails
{
    private ErrorDetails(string traceId, List<string> errors)
    {
        TraceId = traceId;
        Errors = errors;
    }

    public string TraceId { get; set; }
    public List<string> Errors { get; set; }

    public static ErrorDetails Create(List<string> errors, string? traceId = null)
        => new(string.IsNullOrWhiteSpace(traceId)
            ? Guid.NewGuid().ToString()
            : traceId, errors);

    public static ErrorDetails Create(string error, string? traceId = null)
        => new(string.IsNullOrWhiteSpace(traceId)
            ? Guid.NewGuid().ToString()
            : traceId, [error]);
}
