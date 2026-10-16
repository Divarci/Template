using Core.Library.ResultPattern;
using Core.Library.Validators;
using Xunit;

namespace Core.Library.Tests.Validators.ValueObjectValidator;

public sealed partial class ValueObjectValidatorTests
{
    [Fact]
    public void Validate_WhenValueIsNotNull_ReturnsSuccess()
    {
        // Arrange
        object value = new();

        // Act
        Result result = value.Validate(FieldName);

        // Assert
        AssertSuccessResult(result);
    }
}
