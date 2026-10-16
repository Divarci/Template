using AwesomeAssertions;
using Core.Library.ResultPattern;
using Core.Library.Validators;
using System.Net;
using Xunit;

namespace Core.Library.Tests.Validators.GuidValidator;

public sealed partial class GuidValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenFieldNameIsEmpty_ReturnsInternalServerError(string? fieldName)
    {
        // Arrange
        Guid value = Guid.NewGuid();

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
        Guid? value = Guid.NewGuid();

        // Act
        Result result = value.Validate(fieldName!);

        // Assert
        AssertFailureResult(result, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void Validate_WhenEmptyGuidIsNotAllowed_ReturnsValidationError()
    {
        // Act
        Result result = Guid.Empty.Validate(FieldName, allowEmpty: false);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("EntityId cannot be empty guid");
    }

    [Fact]
    public void Validate_WhenNullableValueIsEmptyGuidAndEmptyIsNotAllowed_ReturnsValidationError()
    {
        // Arrange
        Guid? value = Guid.Empty;

        // Act
        Result result = value.Validate(FieldName, allowEmpty: false);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Validate_WhenNullableValueIsNullAndRequired_ReturnsValidationError()
    {
        // Arrange
        Guid? value = null;

        // Act
        Result result = value.Validate(FieldName, isRequired: true);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("EntityId cannot be null");
    }
}
