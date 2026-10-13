using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Adapter.Restful.AspNetCore.Filters;

internal sealed class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
            return;

        Dictionary<string, string[]> errors = context.ModelState
            .Where(ms => ms.Value?.Errors.Count > 0)
            .ToDictionary(
                ms => ms.Key,
                ms => ms.Value!.Errors
                    .Select(e => e.ErrorMessage)
                    .ToArray());

        ProblemDetails problemDetails = new()
        {
            Title = "Validation failed",
            Type = "https://datatracker.ietf.org/doc/html/rfc4918#section-11.2",
            Status = StatusCodes.Status422UnprocessableEntity,
            Extensions =
            {
                ["errors"] = errors
            }
        };

        context.Result = new ObjectResult(problemDetails)
        {
            StatusCode = StatusCodes.Status422UnprocessableEntity
        };
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
