using AwesomeAssertions;
using Core.Library.ResultPattern;
using Core.Library.Validators;
using System.Net;
using Xunit;

namespace Core.Library.Tests.Validators;

public sealed class ValueObjectValidatorValidateTests
{
    [Fact]
    public void Validate_WhenValueIsPresent_ReturnsOk()
    {
        // Arrange
        ErrorDetails value = ErrorDetails.Create("placeholder", "trace-1");

        // Act
        Result result = value.Validate(fieldName: "Address");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Message.Should().Be("Validation successful");
    }

    [Fact]
    public void Validate_WhenValueIsNull_ReturnsUnprocessableEntity()
    {
        // Arrange
        ErrorDetails? value = null;

        // Act
        Result result = value.Validate(fieldName: "Address");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.Message.Should().Be("Address validation failed");
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("Address cannot be null");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WhenFieldNameIsBlank_ReturnsInternalServerError(string fieldName)
    {
        // Arrange
        ErrorDetails value = ErrorDetails.Create("placeholder", "trace-1");

        // Act
        Result result = value.Validate(fieldName: fieldName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
