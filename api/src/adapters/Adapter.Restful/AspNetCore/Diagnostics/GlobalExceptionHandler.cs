using Core.Library.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Adapter.Restful.AspNetCore.Diagnostics;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        string traceId = Guid.NewGuid().ToString();

        LogException(exception, traceId);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(BuildProblemDetails(traceId), cancellationToken);

        return true;
    }

    private static ProblemDetails BuildProblemDetails(string traceId) =>
        new()
        {
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred while processing your request.",
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Status = StatusCodes.Status500InternalServerError,
            Extensions =
            {
                ["traceId"] = traceId,
            }
        };

    private void LogException(Exception exception, string traceId)
    {
        switch (exception)
        {
            case TemplateException templateEx:
                _logger.LogError(
                    "TraceId: {TraceId} | Assembly: {Assembly} | Class: {Class} | Method: {Method}: {Message}",
                    traceId,
                    templateEx.AssemblyName,
                    templateEx.ClassName,
                    templateEx.MethodName,
                    templateEx.Message);
                break;

            default:
                _logger.LogError(
                    exception,
                    "Unhandled exception. TraceId: {TraceId}",
                    traceId);
                break;
        }
    }
}
