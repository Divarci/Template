# Validator & Result-Pattern Testing Patterns

**CRITICAL:** these are example patterns. Every validator in `Core.Library/Validators/` (`StringValidator`, `GuidValidator`, `DateTimeValidator`, `IntegerValidator`, `DecimalValidator`, `EnumValidator`, `ValueObjectValidator`) and every public member of `Core.Library/ResultPattern/` gets its tests. Read each validator first — its parameters (`isRequired`, `allowEmpty`, `minLength` / `maxLength`, `minValue` / `maxValue`, `restriction`, `isEmail`, …) define the cases.

Unlike entity validation, a validator **is** the rule under test, so it is tested on **both** sides of every boundary: Failure files hold the rejected inputs, Success files the accepted ones.

Status codes a validator returns:
- a rejected value → `UnprocessableEntity`, with one message per broken rule in `ErrorDetails.Errors`;
- a misused validator (empty `fieldName`) → `InternalServerError`; an inconsistent configuration (`minValue` > `maxValue`) → `BadRequest`;
- an accepted value → `OK`.

---

## Validator — failure patterns

### Required value
```csharp
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
}
```

### Maximum length
```csharp
[Fact]
public void Validate_WhenExceedsMaxLength_ReturnsValidationError()
{
    // Arrange
    string value = CreateStringOfLength(MaxLength + 1);

    // Act
    Result result = value.Validate(FieldName, maxLength: MaxLength);

    // Assert
    AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
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
```

### Format
```csharp
[Theory]
[InlineData("invalid")]
[InlineData("invalid@")]
[InlineData("@invalid.com")]
[InlineData("invalid@domain")]
public void Validate_WhenInvalidEmailFormat_ReturnsValidationError(string email)
{
    // Act
    Result result = email.Validate("Email", maxLength: 100, isEmail: true);

    // Assert
    AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
}
```

### Restriction / range
```csharp
[Fact]
public void Validate_WhenOnlyPastAndValueIsInFuture_ReturnsValidationError()
{
    // Arrange
    DateTime value = DateTime.UtcNow.AddDays(1);

    // Act
    Result result = value.Validate(FieldName, restriction: DateTimeRestriction.OnlyPast);

    // Assert
    AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
}

[Fact]
public void Validate_WhenEmptyGuidIsNotAllowed_ReturnsValidationError()
{
    // Act
    Result result = Guid.Empty.Validate(FieldName, allowEmpty: false);

    // Assert
    AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
}
```

### Misuse and configuration
```csharp
[Theory]
[InlineData("")]
[InlineData("   ")]
public void Validate_WhenFieldNameIsEmpty_ReturnsInternalServerError(string fieldName)
{
    // Act
    Result result = Guid.NewGuid().Validate(fieldName);

    // Assert
    AssertFailureResult(result, HttpStatusCode.InternalServerError);
}

[Fact]
public void Validate_WhenMinValueIsGreaterThanMaxValue_ReturnsBadRequest()
{
    // Arrange
    DateTime minValue = DateTime.UtcNow;

    // Act
    Result result = DateTime.UtcNow.Validate(FieldName, minValue: minValue, maxValue: minValue.AddDays(-1));

    // Assert
    AssertFailureResult(result, HttpStatusCode.BadRequest);
}
```

### Several broken rules — one message each
```csharp
[Fact]
public void Validate_WhenSeveralRulesAreBroken_ReturnsOneErrorPerRule()
{
    // Arrange
    string value = "not-an-email-and-too-long";

    // Act
    Result result = value.Validate("Email", maxLength: 5, isEmail: true);

    // Assert
    AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    result.ErrorDetails!.Errors.Should().HaveCount(2);
}
```

---

## Validator — success patterns

```csharp
[Fact]
public void Validate_WhenRequiredAndHasValue_ReturnsSuccess()
{
    // Act
    Result result = "Valid value".Validate(FieldName, maxLength: MaxLength);

    // Assert
    AssertSuccessResult(result);
}

[Fact]
public void Validate_WhenNotRequiredAndNull_ReturnsSuccess()
{
    // Arrange
    string? value = null;

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

[Theory]
[InlineData("test@example.com")]
[InlineData("user.name@domain.org")]
[InlineData("user+tag@example.co.uk")]
public void Validate_WhenValidEmailFormat_ReturnsSuccess(string email)
{
    // Act
    Result result = email.Validate("Email", maxLength: 100, isEmail: true);

    // Assert
    AssertSuccessResult(result);
}
```

---

## Result pattern — patterns

### `CombineValidationErrors` — failure
```csharp
[Fact]
public void CombineValidationErrors_WithMultipleFailures_ReturnsAllErrors()
{
    // Arrange
    List<Result> results =
    [
        string.Empty.Validate("Field1", maxLength: 50),
        string.Empty.Validate("Field2", maxLength: 50)
    ];

    // Act
    Result combinedResult = Result.CombineValidationErrors(results);

    // Assert
    AssertFailureResult(combinedResult, HttpStatusCode.UnprocessableEntity);
    combinedResult.ErrorDetails!.Errors.Should().HaveCount(2);
}
```

### `CombineValidationErrors` — success
```csharp
[Fact]
public void CombineValidationErrors_WhenAllPass_ReturnsSuccess()
{
    // Arrange
    List<Result> results =
    [
        "Value1".Validate("Field1", maxLength: 50),
        "Value2".Validate("Field2", maxLength: 50)
    ];

    // Act
    Result combinedResult = Result.CombineValidationErrors(results);

    // Assert
    AssertSuccessResult(combinedResult);
}
```

### Factories, data guards, and mapping
- `Success` / `Failure` factories — the message, status code, `IsSuccess` / `IsFailure`, and (for `Result<T>`) `Data`.
- `IsFailureAndNoData` — `true` only for a failure without data.
- `PaginatedResult<T>.Success` — `Metadata` (`PageNumber`, `PageSize`, `TotalCount`, `PageCount`, `TotalPages`).
- `MapTo` — a successful result is mapped with its message and status preserved; a failed result without data throws `TemplateException`, per the extension's guard.
- `ErrorDetails.Create` / `Metadata` — the values they carry and the guards they enforce.

Each follows the same Success/Failure split and AAA shape as the validator patterns above.
