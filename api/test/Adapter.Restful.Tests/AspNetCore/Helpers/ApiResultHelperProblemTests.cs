using Adapter.Restful.AspNetCore.Helpers;
using AwesomeAssertions;
using Core.Library.ResultPattern;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Xunit;

namespace Adapter.Restful.Tests.AspNetCore.Helpers;

public sealed class ApiResultHelperProblemTests
{
    [Theory]
    [InlineData(HttpStatusCode.NotFound, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4")]
    [InlineData(HttpStatusCode.BadRequest, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1")]
    [InlineData(HttpStatusCode.Unauthorized, "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1")]
    [InlineData(HttpStatusCode.Forbidden, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3")]
    [InlineData(HttpStatusCode.UnprocessableEntity, "https://datatracker.ietf.org/doc/html/rfc4918#section-11.2")]
    [InlineData(HttpStatusCode.Conflict, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1")]
    [InlineData(HttpStatusCode.InternalServerError, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1")]
    public void Problem_WithFailedResult_MapsStatusToRfcType(HttpStatusCode statusCode, string expectedType)
    {
        // Arrange
        Result result = Result.Failure("Something went wrong", statusCode);

        // Act
        ProblemDetails problemDetails = ApiResultHelper.Problem(result);

        // Assert
        problemDetails.Type.Should().Be(expectedType);
        problemDetails.Status.Should().Be((int)statusCode);
    }

    [Fact]
    public void Problem_WithFailedResult_UsesResultMessageAsTitle()
    {
        // Arrange
        Result result = Result.Failure("Note not found.", HttpStatusCode.NotFound);

        // Act
        ProblemDetails problemDetails = ApiResultHelper.Problem(result);

        // Assert
        problemDetails.Title.Should().Be("Note not found.");
    }

    [Fact]
    public void Problem_WithErrorDetails_AddsErrorsExtension()
    {
        // Arrange
        Result result = Result.Failure(
            "Validation failed",
            ErrorDetails.Create(["Title cannot be null or empty."], "trace-1"),
            HttpStatusCode.UnprocessableEntity);

        // Act
        ProblemDetails problemDetails = ApiResultHelper.Problem(result);

        // Assert
        problemDetails.Extensions["errors"].Should().BeEquivalentTo(new List<string> { "Title cannot be null or empty." });
    }

    [Fact]
    public void Problem_WithoutErrorDetails_AddsNoErrorsExtension()
    {
        // Arrange
        Result result = Result.Failure("Note not found.", HttpStatusCode.NotFound);

        // Act
        ProblemDetails problemDetails = ApiResultHelper.Problem(result);

        // Assert
        problemDetails.Extensions.Should().NotContainKey("errors");
    }

    [Fact]
    public void Problem_WithSuccessfulResult_ThrowsInvalidOperationException()
    {
        // Arrange
        Result result = Result.Success("Done", HttpStatusCode.OK);
        Func<ProblemDetails> act = () => ApiResultHelper.Problem(result);

        // Act & Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
