using AwesomeAssertions;
using Core.Library.ResultPattern;
using Core.Library.Validators;
using Core.Library.Validators.Enums;
using System.Net;
using Xunit;

namespace Core.Library.Tests.Validators;

public sealed class EnumValidatorValidateTests
{
    [Fact]
    public void Validate_WhenNullableValueIsDefined_ReturnsOk()
    {
        // Arrange
        DateTimeRestriction? value = DateTimeRestriction.OnlyPast;

        // Act
        Result result = value.Validate(fieldName: "Restriction");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public void Validate_WhenRequiredNullableValueIsNull_ReturnsUnprocessableEntity()
    {
        // Arrange
        DateTimeRestriction? value = null;

        // Act
        Result result = value.Validate(fieldName: "Restriction");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Restriction cannot be null");
    }

    [Fact]
    public void Validate_WhenOptionalNullableValueIsNull_ReturnsOk()
    {
        // Arrange
        DateTimeRestriction? value = null;

        // Act
        Result result = value.Validate(fieldName: "Restriction", isRequired: false);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenNullableValueIsUndefined_ReturnsUnprocessableEntity()
    {
        // Arrange
        DateTimeRestriction? value = (DateTimeRestriction)99;

        // Act
        Result result = value.Validate(fieldName: "Restriction");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Restriction has an invalid value");
    }

    [Fact]
    public void Validate_WhenNullableFieldNameIsBlank_ReturnsInternalServerError()
    {
        // Arrange
        DateTimeRestriction? value = DateTimeRestriction.None;

        // Act
        Result result = value.Validate(fieldName: string.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void Validate_WhenValueIsDefined_ReturnsOk()
    {
        // Arrange
        DateTimeRestriction value = DateTimeRestriction.OnlyFuture;

        // Act
        Result result = value.Validate(fieldName: "Restriction");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenValueIsUndefined_ReturnsUnprocessableEntity()
    {
        // Arrange
        DateTimeRestriction value = (DateTimeRestriction)99;

        // Act
        Result result = value.Validate(fieldName: "Restriction");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Restriction has an invalid value");
    }

    [Fact]
    public void Validate_WhenFieldNameIsBlank_ReturnsInternalServerError()
    {
        // Arrange
        DateTimeRestriction value = DateTimeRestriction.None;

        // Act
        Result result = value.Validate(fieldName: " ");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
