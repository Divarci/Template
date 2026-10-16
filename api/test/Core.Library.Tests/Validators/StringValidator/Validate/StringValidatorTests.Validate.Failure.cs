using AwesomeAssertions;
using Core.Library.ResultPattern;
using Core.Library.Validators;
using System.Net;
using Xunit;

namespace Core.Library.Tests.Validators.StringValidator;

public sealed partial class StringValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenFieldNameIsEmpty_ReturnsInternalServerError(string? fieldName)
    {
        // Arrange
        string value = CreateStringOfLength(MaxLength);

        // Act
        Result result = value.Validate(fieldName!, maxLength: MaxLength);

        // Assert
        AssertFailureResult(result, HttpStatusCode.InternalServerError);
        result.Message.Should().Be("fieldName can not be null or empty");
        result.ErrorDetails.Should().BeNull();
    }

    [Fact]
    public void Validate_WhenFieldNameIsEmptyAndValueIsInvalid_ReturnsInternalServerError()
    {
        // Arrange
        string? value = null;

        // Act
        Result result = value.Validate(string.Empty, maxLength: MaxLength, isRequired: true);

        // Assert
        AssertFailureResult(result, HttpStatusCode.InternalServerError);
        result.Message.Should().Be("fieldName can not be null or empty");
        result.ErrorDetails.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenRequiredAndNullOrWhitespace_ReturnsValidationError(string? value)
    {
        // Act
        Result result = value.Validate(FieldName, maxLength: MaxLength, isRequired: true);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Title cannot be null or empty.");
    }

    [Fact]
    public void Validate_WhenRuleIsBroken_ReturnsFieldValidationFailedMessage()
    {
        // Arrange
        string value = string.Empty;

        // Act
        Result result = value.Validate(FieldName, maxLength: MaxLength);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.Message.Should().Be("Title validation failed");
    }

    [Fact]
    public void Validate_WhenExceedsMaxLength_ReturnsValidationError()
    {
        // Arrange
        string value = CreateStringOfLength(MaxLength + 1);

        // Act
        Result result = value.Validate(FieldName, maxLength: MaxLength);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Title must be 50 characters or less.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void Validate_WhenExceedsVariousMaxLengths_ReturnsValidationError(int maxLength)
    {
        // Arrange
        string value = CreateStringOfLength(maxLength + 1);

        // Act
        Result result = value.Validate(FieldName, maxLength);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Validate_WhenNotRequiredAndExceedsMaxLength_ReturnsValidationError()
    {
        // Arrange
        string value = CreateStringOfLength(MaxLength + 1);

        // Act
        Result result = value.Validate(FieldName, maxLength: MaxLength, isRequired: false);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Validate_WhenShorterThanMinLength_ReturnsValidationError()
    {
        // Arrange
        string value = CreateStringOfLength(MinLength - 1);

        // Act
        Result result = value.Validate(FieldName, maxLength: MaxLength, minLength: MinLength);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Title must be at least 5 characters.");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@invalid.com")]
    [InlineData("invalid@domain")]
    [InlineData("in valid@domain.com")]
    public void Validate_WhenInvalidEmailFormat_ReturnsValidationError(string email)
    {
        // Act
        Result result = email.Validate(FieldName, maxLength: MaxLength, isEmail: true);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Validate_WhenSeveralRulesAreBroken_ReturnsOneErrorPerRule()
    {
        // Arrange
        string value = CreateStringOfLength(MaxLength + 1);

        // Act
        Result result = value.Validate(FieldName, maxLength: MaxLength, isEmail: true);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().HaveCount(2);
    }
}
