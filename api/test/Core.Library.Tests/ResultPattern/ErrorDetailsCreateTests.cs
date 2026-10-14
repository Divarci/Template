using AwesomeAssertions;
using Core.Library.ResultPattern;
using Xunit;

namespace Core.Library.Tests.ResultPattern;

public sealed class ErrorDetailsCreateTests
{
    [Fact]
    public void Create_WithErrorListAndTraceId_KeepsBoth()
    {
        // Arrange
        List<string> errors = ["First error", "Second error"];

        // Act
        ErrorDetails errorDetails = ErrorDetails.Create(errors, "trace-1");

        // Assert
        errorDetails.TraceId.Should().Be("trace-1");
        errorDetails.Errors.Should().Equal("First error", "Second error");
    }

    [Fact]
    public void Create_WithSingleErrorAndTraceId_WrapsErrorInList()
    {
        // Arrange
        string error = "Only error";

        // Act
        ErrorDetails errorDetails = ErrorDetails.Create(error, "trace-1");

        // Assert
        errorDetails.TraceId.Should().Be("trace-1");
        errorDetails.Errors.Should().Equal("Only error");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithoutTraceId_GeneratesGuidTraceId(string? traceId)
    {
        // Arrange
        string error = "Only error";

        // Act
        ErrorDetails errorDetails = ErrorDetails.Create(error, traceId);

        // Assert
        Guid.TryParse(errorDetails.TraceId, out _).Should().BeTrue();
    }

    [Fact]
    public void Create_WithErrorListAndNoTraceId_GeneratesGuidTraceId()
    {
        // Arrange
        List<string> errors = ["First error"];

        // Act
        ErrorDetails errorDetails = ErrorDetails.Create(errors);

        // Assert
        Guid.TryParse(errorDetails.TraceId, out _).Should().BeTrue();
    }
}
