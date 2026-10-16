using Core.Library.ResultPattern;
using Core.Library.Validators;
using Xunit;

namespace Core.Library.Tests.Validators.DecimalValidator;

public sealed partial class DecimalValidatorTests
{
    [Fact]
    public void Validate_WhenWithinRange_ReturnsSuccess()
    {
        // Arrange
        decimal value = MinValue + Step;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenExactlyMinValue_ReturnsSuccess()
    {
        // Arrange
        decimal value = MinValue;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenExactlyMaxValue_ReturnsSuccess()
    {
        // Arrange
        decimal value = MaxValue;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenMinValueEqualsMaxValue_ReturnsSuccess()
    {
        // Arrange
        decimal value = MinValue;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MinValue);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenZeroAndNegativeIsNotAllowed_ReturnsSuccess()
    {
        // Arrange
        decimal value = decimal.Zero;

        // Act
        Result result = value.Validate(FieldName, canBeNegative: false);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenNegativeAndNegativeIsAllowed_ReturnsSuccess()
    {
        // Arrange
        decimal value = -Step;

        // Act
        Result result = value.Validate(FieldName, canBeNegative: true);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenNullableValueIsExactlyMinValue_ReturnsSuccess()
    {
        // Arrange
        decimal? value = MinValue;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenNullableValueIsExactlyMaxValue_ReturnsSuccess()
    {
        // Arrange
        decimal? value = MaxValue;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenNullableValueIsZeroAndNegativeIsNotAllowed_ReturnsSuccess()
    {
        // Arrange
        decimal? value = decimal.Zero;

        // Act
        Result result = value.Validate(FieldName, canBeNegative: false);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenNullableValueIsNegativeAndNegativeIsAllowed_ReturnsSuccess()
    {
        // Arrange
        decimal? value = -Step;

        // Act
        Result result = value.Validate(FieldName, canBeNegative: true);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenNullableValueIsNullAndNotRequired_ReturnsSuccess()
    {
        // Arrange
        decimal? value = null;

        // Act
        Result result = value.Validate(FieldName, isRequired: false, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertSuccessResult(result);
    }
}
