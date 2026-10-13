using Adapter.Restful.AspNetCore.Helpers;
using Core.Library.Exceptions;
using Core.Library.ResultPattern;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Adapter.Restful.Controllers;

[ApiController]
public class BaseController : ControllerBase
{
    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if (result is null)
            throw new TemplateException(
                assemblyName: "Adapter.Restful",
                className: nameof(BaseController),
                methodName: nameof(HandleResult),
                message: "Result cannot be null.");

        if (result.IsFailure)
            return StatusCode((int)result.StatusCode, ApiResultHelper.Problem(result));

        return StatusCode((int)result.StatusCode, result);
    }

    protected ActionResult HandleResult<T, M>(Result<T> result, Func<T, M> mapper)
    {
        if (result is null)
            throw new TemplateException(
                assemblyName: "Adapter.Restful",
                className: nameof(BaseController),
                methodName: nameof(HandleResult),
                message: "Result cannot be null.");

        if (result.IsFailure)
            return StatusCode((int)result.StatusCode, ApiResultHelper.Problem(result));

        Result<M> mappedResult = result.MapTo(mapper);

        return StatusCode((int)result.StatusCode, mappedResult);
    }

    protected ActionResult HandleResult(Result result)
    {
        if (result is null)
            throw new TemplateException(
                assemblyName: "Adapter.Restful",
                className: nameof(BaseController),
                methodName: nameof(HandleResult),
                message: "Result cannot be null.");

        if (result.IsFailure)
            return StatusCode((int)result.StatusCode, ApiResultHelper.Problem(result));

        if (result.StatusCode == HttpStatusCode.NoContent)
            return NoContent();

        return StatusCode((int)result.StatusCode, result);
    }

    protected ActionResult HandleResult<T>(PaginatedResult<T> result)
    {
        if (result is null)
            throw new TemplateException(
                assemblyName: "Adapter.Restful",
                className: nameof(BaseController),
                methodName: nameof(HandleResult),
                message: "Result cannot be null.");

        if (result.IsFailure)
            return StatusCode((int)result.StatusCode, ApiResultHelper.Problem(result));

        return StatusCode((int)result.StatusCode, result);
    }
}
