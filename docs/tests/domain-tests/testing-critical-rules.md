# Testing Critical Rules — Domain Tests

## ✅ DO

1. **Always verify the result pattern** — `IsSuccess` / `IsFailure` on every test.
2. **Always verify entity state changes** — the property a method changes, and that a failed call left the state unchanged where that matters.
3. **Always assert the exact `HttpStatusCode`** on `result.StatusCode`:
   - `Create()` success → `Created`; behaviour methods and `SoftDelete()` success → `OK`.
   - Validation failure → `UnprocessableEntity`.
   - State conflict (e.g. the entity is already deleted) → `Conflict`.
4. **Always test ALL validations** — every rule the entity's `ValidateProperties` / `ValidateBusiness` enforces, through the public method that runs it.
5. **Always test business rules** — every guard and invariant.
6. **Always use helper methods** from the base partial file for models, entities, states, and boundary strings.
7. **Always test immutability** for value objects.
8. **Always investigate the domain's business logic** first — read the entity, its `Validate` partial, and the validators it calls; derive the helpers and scenarios from them.
9. **Always follow the AAA pattern** with the mandatory `// Arrange` / `// Act` / `// Assert` markers.
10. **Always use descriptive test names** (`testing-naming-conventions.md`).
11. **Entity validation is tested on the failure side only** — every rule gets a Failure test; success tests never re-test validation (the valid-data success test is enough). Validators in `Core.Library` are the exception: they are tested on both sides of every boundary (`testing-patterns-validation.md`).
12. **Always test the null-model guard** — a factory or behaviour method that takes a model throws `TemplateException` for `null` (`docs/api/domain-driven-design.md`).
13. **Always verify raised domain events** when a method raises one — type and the carried id.

---

## ❌ DON'T

1. **Don't skip entity-state verification:**
   - ❌ `result.IsSuccess.Should().BeTrue();` alone.
   - ✅ Verify the `Result` **and** the entity state.

2. **Don't test infrastructure concerns** — no database, repository, or external service. Pure domain logic only.

3. **Don't mock in domain tests:**
   - Domain tests never use Moq or any other mocking framework.
   - Entities and validators have no dependencies to mock.
   - If you need a mock, you are testing the wrong layer.

4. **Don't forget edge cases** — boundary values (max length, min value), `null` inputs, empty and whitespace strings, empty collections, every state transition.

5. **Don't test several concerns in one test** — one operation and one scenario per test.

6. **Don't use magic numbers or strings:**
   - ❌ `string title = new('x', 201);`
   - ✅ `string title = CreateStringOfLength(TitleMaxLength + 1);`

7. **Don't ignore check order** — an entity may check state **before** it validates (e.g. `IsDeleted` → `Conflict` before an invalid value → `UnprocessableEntity`). Read the method and assert the status its first failing check produces.

8. **Don't duplicate assertion logic** — use the base file's assertion helpers.

9. **Don't test private methods** — `Validate`, `ValidateState`, `ValidateProperties`, and `ValidateBusiness` are covered through the public methods that call them.

10. **Don't skip failure scenarios** — happy paths alone are insufficient. Test every validation failure, every business-rule violation, and every state conflict.

11. **Don't test success cases of entity validation** (see DO 11).

12. **Don't build entities around their factory** — no reflection, no private-setter tricks, no bypassing validation. Reach every state through `Create()` and the behaviour methods.

13. **Don't depend on the clock exactly** — compare time-based values with a tolerance (`BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1))`) or relative values; never sleep.
