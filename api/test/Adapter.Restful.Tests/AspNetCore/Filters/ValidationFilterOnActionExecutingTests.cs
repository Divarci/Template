using Adapter.Restful.AspNetCore.Filters;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Xunit;

namespace Adapter.Restful.Tests.AspNetCore.Filters;

public sealed class ValidationFilterOnActionExecutingTests
{
    [Fact]
    public void OnActionExecuting_WhenModelStateIsValid_LeavesResultUnset()
    {
        // Arrange
        ValidationFilter filter = new();
        ActionExecutingContext context = ValidationFilterTestData.CreateContext(new ModelStateDictionary());

        // Act
        filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeNull();
    }

    [Fact]
    public void OnActionExecuting_WhenModelStateIsInvalid_ShortCircuitsWithUnprocessableEntity()
    {
        // Arrange
        ValidationFilter filter = new();
        ModelStateDictionary modelState = new();
        modelState.AddModelError("Title", "The Title field is required.");
        ActionExecutingContext context = ValidationFilterTestData.CreateContext(modelState);

        // Act
        filter.OnActionExecuting(context);

        // Assert
        ObjectResult result = context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        ProblemDetails problemDetails = result.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails.Title.Should().Be("Validation failed");
        problemDetails.Type.Should().Be("https://datatracker.ietf.org/doc/html/rfc4918#section-11.2");
    }

    [Fact]
    public void OnActionExecuting_WhenModelStateIsInvalid_ReportsErrorsPerField()
    {
        // Arrange
        ValidationFilter filter = new();
        ModelStateDictionary modelState = new();
        modelState.AddModelError("Title", "The Title field is required.");
        modelState.AddModelError("Title", "The field Title must be a string with a maximum length of 200.");
        modelState.AddModelError("Content", "The field Content must be a string with a maximum length of 5000.");
        ActionExecutingContext context = ValidationFilterTestData.CreateContext(modelState);

        // Act
        filter.OnActionExecuting(context);

        // Assert
        ProblemDetails problemDetails = (ProblemDetails)((ObjectResult)context.Result!).Value!;
        Dictionary<string, string[]> errors = problemDetails.Extensions["errors"]
            .Should().BeOfType<Dictionary<string, string[]>>().Subject;
        errors.Should().HaveCount(2);
        errors["Title"].Should().Equal(
            "The Title field is required.",
            "The field Title must be a string with a maximum length of 200.");
        errors["Content"].Should().Equal("The field Content must be a string with a maximum length of 5000.");
    }
}
