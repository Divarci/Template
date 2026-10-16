using AwesomeAssertions;
using Core.Library.ResultPattern;
using Core.Library.Validators;
using System.Net;
using Xunit;

namespace Core.Library.Tests.Validators.ValueObjectValidator;

public sealed partial class ValueObjectValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenFieldNameIsEmpty_ReturnsInternalServerError(string? fieldName)
    {
        // Arrange
        object value = new();

        // Act
        Result result = value.Validate(fieldName!);

        // Assert
        AssertFailureResult(result, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void Validate_WhenValueIsNull_ReturnsValidationError()
    {
        // Arrange
        object? value = null;

        // Act
        Result result = value.Validate(FieldName);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Address cannot be null");
    }
}
