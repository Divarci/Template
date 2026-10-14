using AwesomeAssertions;
using Core.Library.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace Adapter.Restful.Tests.AspNetCore.Diagnostics;

public sealed class GlobalExceptionHandlerTryHandleAsyncTests
{
    [Fact]
    public async Task TryHandleAsync_WithAnyException_ReturnsTrue()
    {
        // Arrange
        GlobalExceptionHandlerTestSetup setup = new();

        // Act
        bool handled = await setup.Handler.TryHandleAsync(
            setup.HttpContext,
            new InvalidOperationException("Boom"),
            TestContext.Current.CancellationToken);

        // Assert
        handled.Should().BeTrue();
    }

    [Fact]
    public async Task TryHandleAsync_WithAnyException_SetsInternalServerErrorStatus()
    {
        // Arrange
        GlobalExceptionHandlerTestSetup setup = new();

        // Act
        await setup.Handler.TryHandleAsync(
            setup.HttpContext,
            new InvalidOperationException("Boom"),
            TestContext.Current.CancellationToken);

        // Assert
        setup.HttpContext.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task TryHandleAsync_WithAnyException_WritesProblemDetailsWithTraceId()
    {
        // Arrange
        GlobalExceptionHandlerTestSetup setup = new();

        // Act
        await setup.Handler.TryHandleAsync(
            setup.HttpContext,
            new InvalidOperationException("Boom"),
            TestContext.Current.CancellationToken);

        // Assert
        setup.HttpContext.Response.Body.Position = 0;
        using JsonDocument document = await JsonDocument.ParseAsync(
            setup.HttpContext.Response.Body,
            cancellationToken: TestContext.Current.CancellationToken);
        JsonElement body = document.RootElement;
        body.GetProperty("status").GetInt32().Should().Be(StatusCodes.Status500InternalServerError);
        body.GetProperty("title").GetString().Should().Be("Internal Server Error");
        body.GetProperty("detail").GetString().Should().Be("An unexpected error occurred while processing your request.");
        body.GetProperty("type").GetString().Should().Be("https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1");
        Guid.TryParse(body.GetProperty("traceId").GetString(), out _).Should().BeTrue();
    }

    [Fact]
    public async Task TryHandleAsync_WithSensitiveExceptionMessage_DoesNotLeakItToClient()
    {
        // Arrange
        GlobalExceptionHandlerTestSetup setup = new();
        string sensitiveMessage = "Internal detail: db-internal-host unreachable";
        Exception exception = new InvalidOperationException(sensitiveMessage, new TimeoutException("Inner detail"));

        // Act
        await setup.Handler.TryHandleAsync(
            setup.HttpContext,
            exception,
            TestContext.Current.CancellationToken);

        // Assert
        setup.HttpContext.Response.Body.Position = 0;
        using StreamReader reader = new(setup.HttpContext.Response.Body);
        string body = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
        body.Should().NotContain(sensitiveMessage);
        body.Should().NotContain("Inner detail");
        body.Should().NotContain(nameof(InvalidOperationException));
    }

    [Fact]
    public async Task TryHandleAsync_WithUnexpectedException_LogsErrorWithException()
    {
        // Arrange
        GlobalExceptionHandlerTestSetup setup = new();
        InvalidOperationException exception = new("Boom");

        // Act
        await setup.Handler.TryHandleAsync(
            setup.HttpContext,
            exception,
            TestContext.Current.CancellationToken);

        // Assert
        setup.Logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_WithDomainException_LogsErrorWithStructuredContext()
    {
        // Arrange
        GlobalExceptionHandlerTestSetup setup = new();
        TemplateException exception = new(
            "Core.Library",
            "Metadata",
            ".ctor",
            "PageSize must be greater than zero.");

        // Act
        await setup.Handler.TryHandleAsync(
            setup.HttpContext,
            exception,
            TestContext.Current.CancellationToken);

        // Assert
        setup.Logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => $"{state}".Contains("Assembly: Core.Library | Class: Metadata | Method: .ctor")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
