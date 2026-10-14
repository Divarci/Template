using Adapter.Restful.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Adapter.Restful.Tests;

internal sealed class GlobalExceptionHandlerTestSetup
{
    public GlobalExceptionHandlerTestSetup()
    {
        HttpContext.Response.Body = new MemoryStream();
        Handler = new GlobalExceptionHandler(Logger.Object);
    }

    public DefaultHttpContext HttpContext { get; } = new();

    public Mock<ILogger<GlobalExceptionHandler>> Logger { get; } = new();

    public GlobalExceptionHandler Handler { get; }
}
