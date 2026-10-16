using AwesomeAssertions;
using Core.Library.ResultPattern;
using Core.Library.Validators;
using Core.Library.Validators.Enums;
using System.Net;
using Xunit;

namespace Core.Library.Tests.Validators.EnumValidator;

public sealed partial class EnumValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenFieldNameIsEmpty_ReturnsInternalServerError(string? fieldName)
    {
        // Arrange
        DateTimeRestriction value = DateTimeRestriction.OnlyPast;

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
        DateTimeRestriction? value = DateTimeRestriction.OnlyPast;

        // Act
        Result result = value.Validate(fieldName!);

        // Assert
        AssertFailureResult(result, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void Validate_WhenValueIsNotDefined_ReturnsValidationError()
    {
        // Arrange
        DateTimeRestriction value = (DateTimeRestriction)UndefinedEnumValue;

        // Act
        Result result = value.Validate(FieldName);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Restriction has an invalid value");
    }

    [Fact]
    public void Validate_WhenNullableValueIsNotDefined_ReturnsValidationError()
    {
        // Arrange
        DateTimeRestriction? value = (DateTimeRestriction)UndefinedEnumValue;

        // Act
        Result result = value.Validate(FieldName);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Validate_WhenNullableValueIsNullAndRequired_ReturnsValidationError()
    {
        // Arrange
        DateTimeRestriction? value = null;

        // Act
        Result result = value.Validate(FieldName, isRequired: true);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Restriction cannot be null");
    }
}
