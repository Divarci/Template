using AwesomeAssertions;
using Core.Library.ResultPattern;
using Core.Library.Validators;
using Xunit;

namespace Core.Library.Tests.Validators.StringValidator;

public sealed partial class StringValidatorTests
{
    [Fact]
    public void Validate_WhenRequiredAndHasValue_ReturnsSuccess()
    {
        // Act
        Result result = "Valid value".Validate(FieldName, maxLength: MaxLength);

        // Assert
        AssertSuccessResult(result);
        result.Message.Should().Be("Validation successful");
        result.ErrorDetails.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenNotRequiredAndNullOrWhitespace_ReturnsSuccess(string? value)
    {
        // Act
        Result result = value.Validate(FieldName, maxLength: MaxLength, isRequired: false);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenExactlyMaxLength_ReturnsSuccess()
    {
        // Arrange
        string value = CreateStringOfLength(MaxLength);

        // Act
        Result result = value.Validate(FieldName, maxLength: MaxLength);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenExactlyMinLength_ReturnsSuccess()
    {
        // Arrange
        string value = CreateStringOfLength(MinLength);

        // Act
        Result result = value.Validate(FieldName, maxLength: MaxLength, minLength: MinLength);

        // Assert
        AssertSuccessResult(result);
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.org")]
    [InlineData("user+tag@example.co.uk")]
    [InlineData("USER@EXAMPLE.COM")]
    public void Validate_WhenValidEmailFormat_ReturnsSuccess(string email)
    {
        // Act
        Result result = email.Validate(FieldName, maxLength: MaxLength, isEmail: true);

        // Assert
        AssertSuccessResult(result);
    }
}
