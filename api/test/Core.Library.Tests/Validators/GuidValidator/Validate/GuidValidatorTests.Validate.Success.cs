using Core.Library.ResultPattern;
using Core.Library.Validators;
using Xunit;

namespace Core.Library.Tests.Validators.GuidValidator;

public sealed partial class GuidValidatorTests
{
    [Fact]
    public void Validate_WhenValueIsNotEmptyAndEmptyIsNotAllowed_ReturnsSuccess()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        Result result = value.Validate(FieldName, allowEmpty: false);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenEmptyGuidIsAllowed_ReturnsSuccess()
    {
        // Act
        Result result = Guid.Empty.Validate(FieldName, allowEmpty: true);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenNullableValueIsNotEmptyAndEmptyIsNotAllowed_ReturnsSuccess()
    {
        // Arrange
        Guid? value = Guid.NewGuid();

        // Act
        Result result = value.Validate(FieldName, allowEmpty: false);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenNullableValueIsEmptyGuidAndEmptyIsAllowed_ReturnsSuccess()
    {
        // Arrange
        Guid? value = Guid.Empty;

        // Act
        Result result = value.Validate(FieldName, allowEmpty: true);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenNullableValueIsNullAndNotRequired_ReturnsSuccess()
    {
        // Arrange
        Guid? value = null;

        // Act
        Result result = value.Validate(FieldName, isRequired: false, allowEmpty: false);

        // Assert
        AssertSuccessResult(result);
    }
}
