using AwesomeAssertions;
using Core.Library.ResultPattern;
using Core.Library.Validators;
using Core.Library.Validators.Enums;
using System.Net;
using Xunit;

namespace Core.Library.Tests.Validators.DateTimeValidator;

public sealed partial class DateTimeValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenFieldNameIsEmpty_ReturnsInternalServerError(string? fieldName)
    {
        // Arrange
        DateTime value = MinValue;

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
        DateTime? value = MinValue;

        // Act
        Result result = value.Validate(fieldName!);

        // Assert
        AssertFailureResult(result, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void Validate_WhenMinValueIsGreaterThanMaxValue_ReturnsBadRequest()
    {
        // Arrange
        DateTime value = MinValue;

        // Act
        Result result = value.Validate(FieldName, minValue: MaxValue, maxValue: MinValue);

        // Assert
        AssertFailureResult(result, HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Validate_WhenNullableValueAndMinValueIsGreaterThanMaxValue_ReturnsBadRequest()
    {
        // Arrange
        DateTime? value = MinValue;

        // Act
        Result result = value.Validate(FieldName, minValue: MaxValue, maxValue: MinValue);

        // Assert
        AssertFailureResult(result, HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Validate_WhenOnlyPastAndValueIsInFuture_ReturnsValidationError()
    {
        // Arrange
        DateTime value = DateTime.UtcNow.Add(ClockOffset);

        // Act
        Result result = value.Validate(FieldName, restriction: DateTimeRestriction.OnlyPast);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("OccurredOnUtc cannot be a future date");
    }

    [Fact]
    public void Validate_WhenNullableValueIsInFutureAndOnlyPast_ReturnsValidationError()
    {
        // Arrange
        DateTime? value = DateTime.UtcNow.Add(ClockOffset);

        // Act
        Result result = value.Validate(FieldName, restriction: DateTimeRestriction.OnlyPast);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Validate_WhenOnlyFutureAndValueIsInPast_ReturnsValidationError()
    {
        // Arrange
        DateTime value = DateTime.UtcNow.Subtract(ClockOffset);

        // Act
        Result result = value.Validate(FieldName, restriction: DateTimeRestriction.OnlyFuture);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("OccurredOnUtc cannot be a past date");
    }

    [Fact]
    public void Validate_WhenNullableValueIsInPastAndOnlyFuture_ReturnsValidationError()
    {
        // Arrange
        DateTime? value = DateTime.UtcNow.Subtract(ClockOffset);

        // Act
        Result result = value.Validate(FieldName, restriction: DateTimeRestriction.OnlyFuture);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Validate_WhenEarlierThanMinValue_ReturnsValidationError()
    {
        // Arrange
        DateTime value = MinValue.Subtract(Step);

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("OccurredOnUtc cannot be earlier than 2025-01-01 00:00:00");
    }

    [Fact]
    public void Validate_WhenNullableValueIsEarlierThanMinValue_ReturnsValidationError()
    {
        // Arrange
        DateTime? value = MinValue.Subtract(Step);

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Validate_WhenLaterThanMaxValue_ReturnsValidationError()
    {
        // Arrange
        DateTime value = MaxValue.Add(Step);

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("OccurredOnUtc cannot be later than 2025-12-31 23:59:59");
    }

    [Fact]
    public void Validate_WhenNullableValueIsLaterThanMaxValue_ReturnsValidationError()
    {
        // Arrange
        DateTime? value = MaxValue.Add(Step);

        // Act
        Result result = value.Validate(FieldName, minValue: MinValue, maxValue: MaxValue);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Validate_WhenNullableValueIsNullAndRequired_ReturnsValidationError()
    {
        // Arrange
        DateTime? value = null;

        // Act
        Result result = value.Validate(FieldName, isRequired: true);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("OccurredOnUtc cannot be null");
    }

    [Fact]
    public void Validate_WhenSeveralRulesAreBroken_ReturnsOneErrorPerRule()
    {
        // Arrange
        DateTime value = DateTime.UtcNow.Add(ClockOffset);

        // Act
        Result result = value.Validate(
            FieldName,
            minValue: MinValue,
            maxValue: MaxValue,
            restriction: DateTimeRestriction.OnlyPast);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().HaveCount(2);
    }
}
