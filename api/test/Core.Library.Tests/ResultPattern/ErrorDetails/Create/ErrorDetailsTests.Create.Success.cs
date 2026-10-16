using AwesomeAssertions;
using Xunit;

namespace Core.Library.Tests.ResultPattern.ErrorDetails;

using ErrorDetails = Core.Library.ResultPattern.ErrorDetails;

public sealed partial class ErrorDetailsTests
{
    [Fact]
    public void Create_WithErrorList_CarriesErrors()
    {
        // Arrange
        List<string> errors = [FirstError, SecondError];

        // Act
        ErrorDetails errorDetails = ErrorDetails.Create(errors);

        // Assert
        errorDetails.Errors.Should().Equal(FirstError, SecondError);
    }

    [Fact]
    public void Create_WithSingleError_CarriesOneError()
    {
        // Act
        ErrorDetails errorDetails = ErrorDetails.Create(FirstError);

        // Assert
        errorDetails.Errors.Should().ContainSingle()
            .Which.Should().Be(FirstError);
    }

    [Fact]
    public void Create_WithErrorListAndTraceId_UsesSuppliedTraceId()
    {
        // Arrange
        List<string> errors = [FirstError];

        // Act
        ErrorDetails errorDetails = ErrorDetails.Create(errors, TraceId);

        // Assert
        errorDetails.TraceId.Should().Be(TraceId);
    }

    [Fact]
    public void Create_WithSingleErrorAndTraceId_UsesSuppliedTraceId()
    {
        // Act
        ErrorDetails errorDetails = ErrorDetails.Create(FirstError, TraceId);

        // Assert
        errorDetails.TraceId.Should().Be(TraceId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithErrorListAndWithoutTraceId_GeneratesGuidTraceId(string? traceId)
    {
        // Arrange
        List<string> errors = [FirstError];

        // Act
        ErrorDetails errorDetails = ErrorDetails.Create(errors, traceId);

        // Assert
        AssertGeneratedTraceId(errorDetails.TraceId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithSingleErrorAndWithoutTraceId_GeneratesGuidTraceId(string? traceId)
    {
        // Act
        ErrorDetails errorDetails = ErrorDetails.Create(FirstError, traceId);

        // Assert
        AssertGeneratedTraceId(errorDetails.TraceId);
    }

    [Fact]
    public void Create_CalledTwiceWithoutTraceId_GeneratesDistinctTraceIds()
    {
        // Act
        ErrorDetails firstErrorDetails = ErrorDetails.Create(FirstError);
        ErrorDetails secondErrorDetails = ErrorDetails.Create(FirstError);

        // Assert
        secondErrorDetails.TraceId.Should().NotBe(firstErrorDetails.TraceId);
    }
}
