# Test Naming Conventions — Domain Tests

## Test classes and files

| Element | Pattern | Example |
|---|---|---|
| Test class (all partial files) | `{Subject}Tests` — `public sealed partial class` | `NoteTests`, `StringValidatorTests`, `ResultOfTTests` |
| Base partial file | `{Subject}Tests.cs` | `NoteTests.cs` |
| Operation file | `{Subject}Tests.{Method}.Success.cs` / `.Failure.cs` | `NoteTests.UpdateTitle.Failure.cs` |
| Namespace | the base file's folder | `Core.Domain.Tests.Entities.Samples.Notes` |

## Test methods

**Pattern:** `{MethodUnderTest}_{Scenario}_{ExpectedResult}`

Use the ubiquitous language of `docs/api/domain-driven-design.md` in scenarios and outcomes.

### DO ✅

1. **Use descriptive names that explain the scenario:**
   ```csharp
   Create_WithValidData_ReturnsSuccessResult()
   ```
2. **Include the state or condition being tested:**
   ```csharp
   UpdateTitle_WhenNoteIsDeleted_ReturnsConflictError()
   MarkProcessed_WhenErrorIsSupplied_RecordsError()
   ```
3. **Be specific about the expected outcome:**
   ```csharp
   Create_WithEmptyTitle_ReturnsValidationError()
   SoftDelete_WhenAlreadyDeleted_ReturnsConflictError()
   Create_WithNullModel_ThrowsTemplateException()
   ```
4. **Use action verbs for state-changing outcomes:**
   ```csharp
   Create_RaisesNoteCreatedDomainEvent()
   SoftDelete_SetsIsDeletedToTrue()
   UpdatePinned_ChangesIsPinned()
   ```

### DON'T ❌

1. **Vague names:**
   ```csharp
   Test1()                    // ❌ What does this test?
   CreateTest()               // ❌ Which scenario?
   TestWithInvalidData()      // ❌ Which method? What invalid data?
   ```
2. **Abbreviations or acronyms:**
   ```csharp
   Crt_ValidData_Success()    // ❌
   ```
3. **Mixed patterns:**
   ```csharp
   Create_WithValidData_ReturnsSuccessResult()
   UpdateTitle_ReturnsSuccess_WhenValidTitle()   // ❌ inconsistent order
   WhenDeleted_SoftDelete_ReturnsConflict()      // ❌ inconsistent order
   ```
4. **"Test" in the name** — the class already says it:
   ```csharp
   TestCreate_WithValidData()  // ❌
   ```

---

## Pattern variations

### Several related scenarios — group by the common condition
```csharp
// Validation scenarios
Create_WithEmptyTitle_ReturnsValidationError()
Create_WithWhitespaceTitle_ReturnsValidationError()
Create_WithTooLongTitle_ReturnsValidationError()

// State scenarios
UpdateTitle_WithValidTitle_ReturnsSuccessAndUpdatesTitle()
UpdateTitle_WhenNoteIsDeleted_ReturnsConflictError()
```

### Edge cases — name the boundary
```csharp
Create_WithTooLongTitle_ReturnsValidationError()      // entity: boundary covered on the failure side only
Validate_WhenExactlyMaxLength_ReturnsSuccess()        // validator: both sides of the boundary
```

### Business rules — name the rule
```csharp
UpdateContent_WhenNoteIsDeleted_ReturnsConflictError()
SoftDelete_WhenAlreadyDeleted_ReturnsConflictError()
```

**Remember:** good test names are documentation. Someone should understand the test without reading its body.
