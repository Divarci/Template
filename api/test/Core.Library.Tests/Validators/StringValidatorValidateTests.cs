using AwesomeAssertions;
using Core.Library.ResultPattern;
using Core.Library.Validators;
using System.Net;
using Xunit;

namespace Core.Library.Tests.Validators;

public sealed class StringValidatorValidateTests
{
    [Fact]
    public void Validate_WhenValueIsWithinLimits_ReturnsOk()
    {
        // Arrange
        string value = "Shopping list";

        // Act
        Result result = value.Validate(fieldName: "Title", maxLength: 200);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Message.Should().Be("Validation successful");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenRequiredValueIsMissing_ReturnsUnprocessableEntity(string? value)
    {
        // Arrange
        string fieldName = "Title";

        // Act
        Result result = value.Validate(fieldName: fieldName, maxLength: 200);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.Message.Should().Be("Title validation failed");
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Title cannot be null or empty.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenOptionalValueIsMissing_ReturnsOk(string? value)
    {
        // Arrange
        string fieldName = "Content";

        // Act
        Result result = value.Validate(
            fieldName: fieldName,
            maxLength: 200,
            isRequired: false);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenValueIsAtMaximumLength_ReturnsOk()
    {
        // Arrange
        string value = new('a', 10);

        // Act
        Result result = value.Validate(fieldName: "Title", maxLength: 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenValueExceedsMaximumLength_ReturnsUnprocessableEntity()
    {
        // Arrange
        string value = new('a', 11);

        // Act
        Result result = value.Validate(fieldName: "Title", maxLength: 10);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Title must be 10 characters or less.");
    }

    [Fact]
    public void Validate_WhenValueIsAtMinimumLength_ReturnsOk()
    {
        // Arrange
        string value = "abc";

        // Act
        Result result = value.Validate(
            fieldName: "Code",
            maxLength: 10,
            minLength: 3);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenValueIsShorterThanMinimumLength_ReturnsUnprocessableEntity()
    {
        // Arrange
        string value = "ab";

        // Act
        Result result = value.Validate(
            fieldName: "Code",
            maxLength: 10,
            minLength: 3);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Code must be at least 3 characters.");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@EXAMPLE.ORG")]
    public void Validate_WhenEmailIsWellFormed_ReturnsOk(string value)
    {
        // Arrange
        string fieldName = "Email";

        // Act
        Result result = value.Validate(
            fieldName: fieldName,
            maxLength: 100,
            isEmail: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("user")]
    [InlineData("user@example")]
    [InlineData("user @example.com")]
    public void Validate_WhenEmailIsMalformed_ReturnsUnprocessableEntity(string value)
    {
        // Arrange
        string fieldName = "Email";

        // Act
        Result result = value.Validate(
            fieldName: fieldName,
            maxLength: 100,
            isEmail: true);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Email must be a valid email address.");
    }

    [Fact]
    public void Validate_WhenSeveralRulesFail_ReturnsEveryError()
    {
        // Arrange
        string value = "abcdefghijkl";

        // Act
        Result result = value.Validate(
            fieldName: "Email",
            maxLength: 10,
            isEmail: true);

        // Assert
        result.ErrorDetails!.Errors.Should().BeEquivalentTo(
            "Email must be 10 characters or less.",
            "Email must be a valid email address.");
    }
}
