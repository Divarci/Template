using Core.Library.ResultPattern;
using Core.Library.Validators;
using Core.Library.Validators.Enums;
using Xunit;

namespace Core.Library.Tests.Validators.EnumValidator;

public sealed partial class EnumValidatorTests
{
    [Theory]
    [InlineData(DateTimeRestriction.None)]
    [InlineData(DateTimeRestriction.OnlyPast)]
    [InlineData(DateTimeRestriction.OnlyFuture)]
    public void Validate_WhenValueIsDefined_ReturnsSuccess(DateTimeRestriction value)
    {
        // Act
        Result result = value.Validate(FieldName);

        // Assert
        AssertSuccessResult(result);
    }

    [Theory]
    [InlineData(DateTimeRestriction.None)]
    [InlineData(DateTimeRestriction.OnlyPast)]
    [InlineData(DateTimeRestriction.OnlyFuture)]
    public void Validate_WhenNullableValueIsDefined_ReturnsSuccess(DateTimeRestriction definedValue)
    {
        // Arrange
        DateTimeRestriction? value = definedValue;

        // Act
        Result result = value.Validate(FieldName);

        // Assert
        AssertSuccessResult(result);
    }

    [Fact]
    public void Validate_WhenNullableValueIsNullAndNotRequired_ReturnsSuccess()
    {
        // Arrange
        DateTimeRestriction? value = null;

        // Act
        Result result = value.Validate(FieldName, isRequired: false);

        // Assert
        AssertSuccessResult(result);
    }
}
