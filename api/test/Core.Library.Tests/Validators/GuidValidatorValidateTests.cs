using AwesomeAssertions;
using Core.Library.ResultPattern;
using Core.Library.Validators;
using System.Net;
using Xunit;

namespace Core.Library.Tests.Validators;

public sealed class GuidValidatorValidateTests
{
    [Fact]
    public void Validate_WhenNullableValueIsNonEmpty_ReturnsOk()
    {
        // Arrange
        Guid? value = Guid.NewGuid();

        // Act
        Result result = value.Validate(fieldName: "EntityId", allowEmpty: false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public void Validate_WhenRequiredNullableValueIsNull_ReturnsUnprocessableEntity()
    {
        // Arrange
        Guid? value = null;

        // Act
        Result result = value.Validate(fieldName: "EntityId");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("EntityId cannot be null");
    }

    [Fact]
    public void Validate_WhenOptionalNullableValueIsNull_ReturnsOk()
    {
        // Arrange
        Guid? value = null;

        // Act
        Result result = value.Validate(fieldName: "EntityId", isRequired: false);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenNullableValueIsEmptyAndEmptyIsNotAllowed_ReturnsUnprocessableEntity()
    {
        // Arrange
        Guid? value = Guid.Empty;

        // Act
        Result result = value.Validate(fieldName: "EntityId", allowEmpty: false);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("EntityId cannot be empty guid");
    }

    [Fact]
    public void Validate_WhenNullableValueIsEmptyAndEmptyIsAllowed_ReturnsOk()
    {
        // Arrange
        Guid? value = Guid.Empty;

        // Act
        Result result = value.Validate(fieldName: "EntityId");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenNullableFieldNameIsBlank_ReturnsInternalServerError()
    {
        // Arrange
        Guid? value = Guid.NewGuid();

        // Act
        Result result = value.Validate(fieldName: string.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void Validate_WhenValueIsEmptyAndEmptyIsNotAllowed_ReturnsUnprocessableEntity()
    {
        // Arrange
        Guid value = Guid.Empty;

        // Act
        Result result = value.Validate(fieldName: "EntityId", allowEmpty: false);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().ContainSingle()
            .Which.Should().Be("EntityId cannot be empty guid");
    }

    [Fact]
    public void Validate_WhenValueIsEmptyAndEmptyIsAllowed_ReturnsOk()
    {
        // Arrange
        Guid value = Guid.Empty;

        // Act
        Result result = value.Validate(fieldName: "EntityId");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenValueIsNonEmpty_ReturnsOk()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        Result result = value.Validate(fieldName: "EntityId", allowEmpty: false);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenFieldNameIsBlank_ReturnsInternalServerError()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        Result result = value.Validate(fieldName: " ");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
