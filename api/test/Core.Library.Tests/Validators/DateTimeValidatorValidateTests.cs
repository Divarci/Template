using AwesomeAssertions;
using Core.Library.ResultPattern;
using Core.Library.Validators;
using Core.Library.Validators.Enums;
using System.Net;
using Xunit;

namespace Core.Library.Tests.Validators;

public sealed class DateTimeValidatorValidateTests
{
    [Fact]
    public void Validate_WhenNullableValueIsWithinRange_ReturnsOk()
    {
        // Arrange
        DateTime? value = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        // Act
        Result result = value.Validate(
            fieldName: "DueDate",
            minValue: DateTimeValidatorTestData.JanuaryFirst,
            maxValue: DateTimeValidatorTestData.JanuaryThirtyFirst);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public void Validate_WhenRequiredNullableValueIsNull_ReturnsUnprocessableEntity()
    {
        // Arrange
        DateTime? value = null;

        // Act
        Result result = value.Validate(fieldName: "DueDate");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("DueDate cannot be null");
    }

    [Fact]
    public void Validate_WhenOptionalNullableValueIsNull_ReturnsOk()
    {
        // Arrange
        DateTime? value = null;

        // Act
        Result result = value.Validate(fieldName: "DueDate", isRequired: false);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenNullableFutureValueIsRestrictedToPast_ReturnsUnprocessableEntity()
    {
        // Arrange
        DateTime? value = DateTime.UtcNow.AddYears(1);

        // Act
        Result result = value.Validate(fieldName: "BirthDate", restriction: DateTimeRestriction.OnlyPast);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("BirthDate cannot be a future date");
    }

    [Fact]
    public void Validate_WhenNullablePastValueIsRestrictedToFuture_ReturnsUnprocessableEntity()
    {
        // Arrange
        DateTime? value = DateTime.UtcNow.AddYears(-1);

        // Act
        Result result = value.Validate(fieldName: "DueDate", restriction: DateTimeRestriction.OnlyFuture);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("DueDate cannot be a past date");
    }

    [Fact]
    public void Validate_WhenNullableValueIsBeforeMinimum_ReturnsUnprocessableEntity()
    {
        // Arrange
        DateTime? value = DateTimeValidatorTestData.JanuaryFirst.AddDays(-1);

        // Act
        Result result = value.Validate(fieldName: "DueDate", minValue: DateTimeValidatorTestData.JanuaryFirst);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be($"DueDate cannot be earlier than {DateTimeValidatorTestData.JanuaryFirst:yyyy-MM-dd HH:mm:ss}");
    }

    [Fact]
    public void Validate_WhenNullableValueIsAfterMaximum_ReturnsUnprocessableEntity()
    {
        // Arrange
        DateTime? value = DateTimeValidatorTestData.JanuaryThirtyFirst.AddDays(1);

        // Act
        Result result = value.Validate(fieldName: "DueDate", maxValue: DateTimeValidatorTestData.JanuaryThirtyFirst);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be($"DueDate cannot be later than {DateTimeValidatorTestData.JanuaryThirtyFirst:yyyy-MM-dd HH:mm:ss}");
    }

    [Fact]
    public void Validate_WhenNullableMinimumExceedsMaximum_ReturnsBadRequest()
    {
        // Arrange
        DateTime? value = DateTimeValidatorTestData.JanuaryFirst;

        // Act
        Result result = value.Validate(
            fieldName: "DueDate",
            minValue: DateTimeValidatorTestData.JanuaryThirtyFirst,
            maxValue: DateTimeValidatorTestData.JanuaryFirst);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Validate_WhenNullableFieldNameIsBlank_ReturnsInternalServerError()
    {
        // Arrange
        DateTime? value = DateTimeValidatorTestData.JanuaryFirst;

        // Act
        Result result = value.Validate(fieldName: string.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void Validate_WhenPastValueIsRestrictedToPast_ReturnsOk()
    {
        // Arrange
        DateTime value = DateTimeValidatorTestData.JanuaryFirst;

        // Act
        Result result = value.Validate(fieldName: "CreatedOnUtc", restriction: DateTimeRestriction.OnlyPast);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenFutureValueIsRestrictedToPast_ReturnsUnprocessableEntity()
    {
        // Arrange
        DateTime value = DateTime.UtcNow.AddYears(1);

        // Act
        Result result = value.Validate(fieldName: "CreatedOnUtc", restriction: DateTimeRestriction.OnlyPast);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("CreatedOnUtc cannot be a future date");
    }

    [Fact]
    public void Validate_WhenPastValueIsRestrictedToFuture_ReturnsUnprocessableEntity()
    {
        // Arrange
        DateTime value = DateTime.UtcNow.AddYears(-1);

        // Act
        Result result = value.Validate(fieldName: "DueDate", restriction: DateTimeRestriction.OnlyFuture);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("DueDate cannot be a past date");
    }

    [Fact]
    public void Validate_WhenValueIsOutsideRange_ReturnsUnprocessableEntity()
    {
        // Arrange
        DateTime value = DateTimeValidatorTestData.JanuaryThirtyFirst.AddDays(1);

        // Act
        Result result = value.Validate(
            fieldName: "DueDate",
            minValue: DateTimeValidatorTestData.JanuaryFirst,
            maxValue: DateTimeValidatorTestData.JanuaryThirtyFirst);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be($"DueDate cannot be later than {DateTimeValidatorTestData.JanuaryThirtyFirst:yyyy-MM-dd HH:mm:ss}");
    }

    [Fact]
    public void Validate_WhenMinimumExceedsMaximum_ReturnsBadRequest()
    {
        // Arrange
        DateTime value = DateTimeValidatorTestData.JanuaryFirst;

        // Act
        Result result = value.Validate(
            fieldName: "DueDate",
            minValue: DateTimeValidatorTestData.JanuaryThirtyFirst,
            maxValue: DateTimeValidatorTestData.JanuaryFirst);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Validate_WhenFieldNameIsBlank_ReturnsInternalServerError()
    {
        // Arrange
        DateTime value = DateTimeValidatorTestData.JanuaryFirst;

        // Act
        Result result = value.Validate(fieldName: " ");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
