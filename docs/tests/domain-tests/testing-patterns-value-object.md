# Value Object Testing Patterns

**CRITICAL:** these are example patterns. A real value object may have more methods and rules — every public member gets its tests. The codebase has no value object yet; these patterns apply from the first one (`docs/api/domain-driven-design.md` — Value Object Design Decisions).

---

## Failure patterns

**Note:** Failure files must also contain the **validation** scenarios of the value object — one test per rule. They depend on the value object and are mandatory.

### Create — failure
```csharp
[Fact]
public void Create_WithNullModel_ThrowsTemplateException()
{
    // Arrange
    {ValueObject}Model model = null!;

    // Act
    Action act = () => {ValueObject}.Create(model);

    // Assert
    act.Should().Throw<TemplateException>();
}

[Fact]
public void Create_WithEmpty{Field1}_ReturnsValidationError()
{
    // Arrange
    {ValueObject}Model model = CreateValid{ValueObject}Model({field1}: string.Empty);

    // Act
    Result<{ValueObject}> result = {ValueObject}.Create(model);

    // Assert
    AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
}
```

---

## Success patterns

**Note:** Success files **never** contain validation scenarios — the Failure file covers the boundaries.

### Create — success
```csharp
[Fact]
public void Create_WithValidData_ReturnsSuccessResult()
{
    // Arrange
    {ValueObject}Model model = CreateValid{ValueObject}Model(
        {field1}: "Value1",
        {field2}: "Value2");

    // Act
    Result<{ValueObject}> result = {ValueObject}.Create(model);

    // Assert
    AssertSuccessResult(result, HttpStatusCode.Created);
    result.Data!.{Field1}.Should().Be("Value1");
    result.Data.{Field2}.Should().Be("Value2");
}
```

### Equality — success
Value objects are `sealed record`s: two instances with the same values are equal.

```csharp
[Fact]
public void Create_WithSameValues_ReturnsEqualValueObjects()
{
    // Arrange
    {ValueObject} first = CreateValid{ValueObject}();

    // Act
    {ValueObject} second = CreateValid{ValueObject}();

    // Assert
    second.Should().Be(first);
    second.GetHashCode().Should().Be(first.GetHashCode());
}

[Fact]
public void Create_WithDifferentValues_ReturnsUnequalValueObjects()
{
    // Arrange
    {ValueObject} first = CreateValid{ValueObject}({field1}: "Value1");

    // Act
    {ValueObject} second = CreateValid{ValueObject}({field1}: "Other");

    // Assert
    second.Should().NotBe(first);
}
```

### Immutability — success
Immutability is a compile-time property (`init`-only properties); assert it where it is observable — e.g. a `with` expression produces a new instance and leaves the original unchanged.

```csharp
[Fact]
public void With_ChangingAField_LeavesOriginalUnchanged()
{
    // Arrange
    {ValueObject} original = CreateValid{ValueObject}({field1}: "Value1");

    // Act
    {ValueObject} changed = original with { {Field1} = "Other" };

    // Assert
    original.{Field1}.Should().Be("Value1");
    changed.Should().NotBeSameAs(original);
}
```
