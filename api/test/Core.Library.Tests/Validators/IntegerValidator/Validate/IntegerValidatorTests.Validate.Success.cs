using Core.Library.ResultPattern;
using Core.Library.Validators;
using Xunit;

namespace Core.Library.Tests.Validators.IntegerValidator;

public sealed partial class IntegerValidatorTests
{
    [Fact]
    public void Validate_WhenWithinRange_ReturnsSuccess()
    {
        // Arrange
        int value = MinValue + 1;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenExactlyMinValue_ReturnsSuccess()
    {
        // Arrange
        int value = MinValue;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenExactlyMaxValue_ReturnsSuccess()
    {
        // Arrange
        int value = MaxValue;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenMinValueEqualsMaxValue_ReturnsSuccess()
    {
        // Arrange
        int value = MinValue;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MinValue);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenZeroAndNegativeIsNotAllowed_ReturnsSuccess()
    {
        // Arrange
        int value = 0;

        // Act
        Result result = value.Validate(FieldName, canBeNegative: false);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenNegativeAndNegativeIsAllowed_ReturnsSuccess()
    {
        // Arrange
        int value = -1;

        // Act
        Result result = value.Validate(FieldName, canBeNegative: true);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenNullableValueIsExactlyMinValue_ReturnsSuccess()
    {
        // Arrange
        int? value = MinValue;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenNullableValueIsExactlyMaxValue_ReturnsSuccess()
    {
        // Arrange
        int? value = MaxValue;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenNullableValueIsZeroAndNegativeIsNotAllowed_ReturnsSuccess()
    {
        // Arrange
        int? value = 0;

        // Act
        Result result = value.Validate(FieldName, canBeNegative: false);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenNullableValueIsNegativeAndNegativeIsAllowed_ReturnsSuccess()
    {
        // Arrange
        int? value = -1;

        // Act
        Result result = value.Validate(FieldName, canBeNegative: true);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenNullableValueIsNullAndNotRequired_ReturnsSuccess()
    {
        // Arrange
        int? value = null;

        // Act
        Result result = value.Validate(FieldName, isRequired: false, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertSuccessResult(result);
    }
}
