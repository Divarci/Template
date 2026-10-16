using AwesomeAssertions;
using Core.Library.ResultPattern;
using Core.Library.Validators;
using System.Net;
using Xunit;

namespace Core.Library.Tests.Validators.DecimalValidator;

public sealed partial class DecimalValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenFieldNameIsEmpty_ReturnsInternalServerError(string? fieldName)
    {
        // Arrange
        decimal value = MinValue;

        // Act
        Result result = value.Validate(fieldName!);

        // Assert
        AssertFailureResult(result, HttpStatusCode.InternalServerError);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenNullableValueAndFieldNameIsEmpty_ReturnsInternalServerError(string? fieldName)
    {
        // Arrange
        decimal? value = MinValue;

        // Act
        Result result = value.Validate(fieldName!);

        // Assert
        AssertFailureResult(result, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void Validate_WhenMinValueIsGreaterThanMaxValue_ReturnsBadRequest()
    {
        // Arrange
        decimal value = MinValue;

        // Act
        Result result = value.Validate(FieldName, minValue: MaxValue, maxValue: MinValue);

        // Assert
        AssertFailureResult(result, HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Validate_WhenNullableValueAndMinValueIsGreaterThanMaxValue_ReturnsBadRequest()
    {
        // Arrange
        decimal? value = MinValue;

        // Act
        Result result = value.Validate(FieldName, minValue: MaxValue, maxValue: MinValue);

        // Assert
        AssertFailureResult(result, HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Validate_WhenNegativeAndNegativeIsNotAllowed_ReturnsValidationError()
    {
        // Arrange
        decimal value = -Step;

        // Act
        Result result = value.Validate(FieldName, canBeNegative: false);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Price cannot be negative");
    }

    [Fact]
    public void Validate_WhenNullableValueIsNegativeAndNegativeIsNotAllowed_ReturnsValidationError()
    {
        // Arrange
        decimal? value = -Step;

        // Act
        Result result = value.Validate(FieldName, canBeNegative: false);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Validate_WhenBelowMinValue_ReturnsValidationError()
    {
        // Arrange
        decimal value = MinValue - Step;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Validate_WhenNullableValueIsBelowMinValue_ReturnsValidationError()
    {
        // Arrange
        decimal? value = MinValue - Step;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Validate_WhenAboveMaxValue_ReturnsValidationError()
    {
        // Arrange
        decimal value = MaxValue + Step;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Validate_WhenNullableValueIsAboveMaxValue_ReturnsValidationError()
    {
        // Arrange
        decimal? value = MaxValue + Step;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Validate_WhenNullableValueIsNullAndRequired_ReturnsValidationError()
    {
        // Arrange
        decimal? value = null;

        // Act
        Result result = value.Validate(FieldName, isRequired: true);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Price cannot be null");
    }

    [Fact]
    public void Validate_WhenSeveralRulesAreBroken_ReturnsOneErrorPerRule()
    {
        // Arrange
        decimal value = -Step;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().HaveCount(2);
    }
}
