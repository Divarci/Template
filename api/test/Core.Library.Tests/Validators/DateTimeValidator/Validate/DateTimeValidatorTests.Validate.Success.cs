using Core.Library.ResultPattern;
using Core.Library.Validators;
using Core.Library.Validators.Enums;
using Xunit;

namespace Core.Library.Tests.Validators.DateTimeValidator;

public sealed partial class DateTimeValidatorTests
{
    [Fact]
    public void Validate_WhenNoRestrictionOrRange_ReturnsSuccess()
    {
        // Arrange
        DateTime value = DateTime.UtcNow.Add(ClockOffset);

        // Act
        Result result = value.Validate(FieldName);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenOnlyPastAndValueIsInPast_ReturnsSuccess()
    {
        // Arrange
        DateTime value = DateTime.UtcNow.Subtract(ClockOffset);

        // Act
        Result result = value.Validate(FieldName, restriction: DateTimeRestriction.OnlyPast);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenNullableValueIsInPastAndOnlyPast_ReturnsSuccess()
    {
        // Arrange
        DateTime? value = DateTime.UtcNow.Subtract(ClockOffset);

        // Act
        Result result = value.Validate(FieldName, restriction: DateTimeRestriction.OnlyPast);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenOnlyFutureAndValueIsInFuture_ReturnsSuccess()
    {
        // Arrange
        DateTime value = DateTime.UtcNow.Add(ClockOffset);

        // Act
        Result result = value.Validate(FieldName, restriction: DateTimeRestriction.OnlyFuture);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenNullableValueIsInFutureAndOnlyFuture_ReturnsSuccess()
    {
        // Arrange
        DateTime? value = DateTime.UtcNow.Add(ClockOffset);

        // Act
        Result result = value.Validate(FieldName, restriction: DateTimeRestriction.OnlyFuture);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenExactlyMinValue_ReturnsSuccess()
    {
        // Arrange
        DateTime value = MinValue;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenExactlyMaxValue_ReturnsSuccess()
    {
        // Arrange
        DateTime value = MaxValue;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenMinValueEqualsMaxValue_ReturnsSuccess()
    {
        // Arrange
        DateTime value = MinValue;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MinValue);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenNullableValueIsExactlyMinValue_ReturnsSuccess()
    {
        // Arrange
        DateTime? value = MinValue;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenNullableValueIsExactlyMaxValue_ReturnsSuccess()
    {
        // Arrange
        DateTime? value = MaxValue;

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenNullableValueIsNullAndNotRequired_ReturnsSuccess()
    {
        // Arrange
        DateTime? value = null;

        // Act
        Result result = value.Validate(
            FieldName,
            isRequired: false,
            minValue: MinValue,
            maxValue: MaxValue,
            restriction: DateTimeRestriction.OnlyPast);

        // Assert
        AssertSuccessResult(result);
    }
}
