# Technology Stack — Domain Tests

| Technology | Purpose |
|---|---|
| **xUnit v3** (`xunit.v3`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`) | Test framework and runner |
| **AwesomeAssertions** | Fluent `.Should()` assertions — the Apache-2.0 continuation of FluentAssertions, same API |
| **.NET 10** (`net10.0`) | Target framework |

- **No mocking library.** Domain tests never reference Moq or any other mocking framework.
- **FluentAssertions is not used** (commercial licence from v8). No other assertion library is added; xUnit's `Assert.*` is not used.
- Exact package versions live in the test `.csproj` files — never duplicated here.

## Project setup

`Core.Domain.Tests` and `Core.Library.Tests` are `Microsoft.NET.Sdk` projects:

- `net10.0`, `Nullable` and `ImplicitUsings` enabled, `IsPackable=false`.
- `OutputType=Exe` (xUnit v3) and `IsTestingPlatformApplication=false` — tests run through classic VSTest `dotnet test`.
- Each references **only** its source project: `Core.Domain.Tests` → `Core.Domain`, `Core.Library.Tests` → `Core.Library`.
- Both are registered in `Template.slnx` under the `/api/test/` folder.
- The subjects are `public` types, so no `InternalsVisibleTo` is needed.
