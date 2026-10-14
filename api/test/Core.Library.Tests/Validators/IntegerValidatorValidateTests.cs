using AwesomeAssertions;
using Core.Library.ResultPattern;
using Core.Library.Validators;
using System.Net;
using Xunit;

namespace Core.Library.Tests.Validators;

public sealed class IntegerValidatorValidateTests
{
    [Fact]
    public void Validate_WhenNullableValueIsWithinRange_ReturnsOk()
    {
        // Arrange
        int? value = 5;

        // Act
        Result result = value.Validate(
            fieldName: "Quantity",
            minValue: 1,
            maxValue: 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public void Validate_WhenRequiredNullableValueIsNull_ReturnsUnprocessableEntity()
    {
        // Arrange
        int? value = null;

        // Act
        Result result = value.Validate(fieldName: "Quantity");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.Message.Should().Be("Quantity validation failed");
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Quantity cannot be null");
    }

    [Fact]
    public void Validate_WhenOptionalNullableValueIsNull_ReturnsOk()
    {
        // Arrange
        int? value = null;

        // Act
        Result result = value.Validate(fieldName: "Quantity", isRequired: false);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenNullableValueIsNegativeAndNegativesAreNotAllowed_ReturnsUnprocessableEntity()
    {
        // Arrange
        int? value = -1;

        // Act
        Result result = value.Validate(fieldName: "Quantity");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Quantity cannot be negative");
    }

    [Fact]
    public void Validate_WhenNullableValueIsNegativeAndNegativesAreAllowed_ReturnsOk()
    {
        // Arrange
        int? value = -1;

        // Act
        Result result = value.Validate(fieldName: "Offset", canBeNegative: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, "Quantity cannot be less than 1")]
    [InlineData(11, "Quantity cannot be greater than 10")]
    public void Validate_WhenNullableValueIsOutsideRange_ReturnsUnprocessableEntity(int input, string expectedError)
    {
        // Arrange
        int? value = input;

        // Act
        Result result = value.Validate(
            fieldName: "Quantity",
            minValue: 1,
            maxValue: 10);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be(expectedError);
    }

    [Fact]
    public void Validate_WhenNullableMinimumExceedsMaximum_ReturnsBadRequest()
    {
        // Arrange
        int? value = 5;

        // Act
        Result result = value.Validate(
            fieldName: "Quantity",
            minValue: 10,
            maxValue: 1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Be("minValue cannot be greater than maxValue");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WhenNullableFieldNameIsBlank_ReturnsInternalServerError(string fieldName)
    {
        // Arrange
        int? value = 5;

        // Act
        Result result = value.Validate(fieldName: fieldName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    public void Validate_WhenValueIsAtRangeBoundary_ReturnsOk(int value)
    {
        // Arrange
        string fieldName = "Quantity";

        // Act
        Result result = value.Validate(
            fieldName: fieldName,
            minValue: 1,
            maxValue: 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenValueIsNegativeAndNegativesAreNotAllowed_ReturnsUnprocessableEntity()
    {
        // Arrange
        int value = -1;

        // Act
        Result result = value.Validate(fieldName: "Quantity");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Quantity cannot be negative");
    }

    [Theory]
    [InlineData(0, "Quantity cannot be less than 1")]
    [InlineData(11, "Quantity cannot be greater than 10")]
    public void Validate_WhenValueIsOutsideRange_ReturnsUnprocessableEntity(int value, string expectedError)
    {
        // Arrange
        string fieldName = "Quantity";

        // Act
        Result result = value.Validate(
            fieldName: fieldName,
            minValue: 1,
            maxValue: 10);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be(expectedError);
    }

    [Fact]
    public void Validate_WhenMinimumExceedsMaximum_ReturnsBadRequest()
    {
        // Arrange
        int value = 5;

        // Act
        Result result = value.Validate(
            fieldName: "Quantity",
            minValue: 10,
            maxValue: 1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Validate_WhenFieldNameIsBlank_ReturnsInternalServerError()
    {
        // Arrange
        int value = 5;

        // Act
        Result result = value.Validate(fieldName: string.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
