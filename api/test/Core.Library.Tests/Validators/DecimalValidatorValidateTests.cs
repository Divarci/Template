using AwesomeAssertions;
using Core.Library.ResultPattern;
using Core.Library.Validators;
using System.Net;
using Xunit;

namespace Core.Library.Tests.Validators;

public sealed class DecimalValidatorValidateTests
{
    [Fact]
    public void Validate_WhenNullableValueIsWithinRange_ReturnsOk()
    {
        // Arrange
        decimal? value = 5.5m;

        // Act
        Result result = value.Validate(
            fieldName: "Price",
            minValue: 1m,
            maxValue: 10m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public void Validate_WhenRequiredNullableValueIsNull_ReturnsUnprocessableEntity()
    {
        // Arrange
        decimal? value = null;

        // Act
        Result result = value.Validate(fieldName: "Price");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Price cannot be null");
    }

    [Fact]
    public void Validate_WhenOptionalNullableValueIsNull_ReturnsOk()
    {
        // Arrange
        decimal? value = null;

        // Act
        Result result = value.Validate(fieldName: "Price", isRequired: false);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenNullableValueIsNegativeAndNegativesAreNotAllowed_ReturnsUnprocessableEntity()
    {
        // Arrange
        decimal? value = -0.01m;

        // Act
        Result result = value.Validate(fieldName: "Price");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Price cannot be negative");
    }

    [Fact]
    public void Validate_WhenNullableValueIsNegativeAndNegativesAreAllowed_ReturnsOk()
    {
        // Arrange
        decimal? value = -0.01m;

        // Act
        Result result = value.Validate(fieldName: "Balance", canBeNegative: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenNullableValueIsBelowMinimum_ReturnsUnprocessableEntity()
    {
        // Arrange
        decimal? value = 0.5m;

        // Act
        Result result = value.Validate(fieldName: "Price", minValue: 1m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Price cannot be less than 1");
    }

    [Fact]
    public void Validate_WhenNullableValueIsAboveMaximum_ReturnsUnprocessableEntity()
    {
        // Arrange
        decimal? value = 10.5m;

        // Act
        Result result = value.Validate(fieldName: "Price", maxValue: 10m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Price cannot be greater than 10");
    }

    [Fact]
    public void Validate_WhenNullableMinimumExceedsMaximum_ReturnsBadRequest()
    {
        // Arrange
        decimal? value = 5m;

        // Act
        Result result = value.Validate(
            fieldName: "Price",
            minValue: 10m,
            maxValue: 1m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Validate_WhenNullableFieldNameIsBlank_ReturnsInternalServerError()
    {
        // Arrange
        decimal? value = 5m;

        // Act
        Result result = value.Validate(fieldName: " ");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void Validate_WhenValueIsAtRangeBoundary_ReturnsOk()
    {
        // Arrange
        decimal value = 10m;

        // Act
        Result result = value.Validate(
            fieldName: "Price",
            minValue: 1m,
            maxValue: 10m);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenValueIsNegativeAndNegativesAreNotAllowed_ReturnsUnprocessableEntity()
    {
        // Arrange
        decimal value = -1m;

        // Act
        Result result = value.Validate(fieldName: "Price");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Price cannot be negative");
    }

    [Fact]
    public void Validate_WhenValueIsOutsideRange_ReturnsUnprocessableEntity()
    {
        // Arrange
        decimal value = 11m;

        // Act
        Result result = value.Validate(
            fieldName: "Price",
            minValue: 1m,
            maxValue: 10m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Price cannot be greater than 10");
    }

    [Fact]
    public void Validate_WhenMinimumExceedsMaximum_ReturnsBadRequest()
    {
        // Arrange
        decimal value = 5m;

        // Act
        Result result = value.Validate(
            fieldName: "Price",
            minValue: 10m,
            maxValue: 1m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Validate_WhenFieldNameIsBlank_ReturnsInternalServerError()
    {
        // Arrange
        decimal value = 5m;

        // Act
        Result result = value.Validate(fieldName: string.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
