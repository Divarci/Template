# Technology Stack — Service Tests

| Technology | Purpose |
|---|---|
| **xUnit v3** (`xunit.v3`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`) | Test framework and runner |
| **AwesomeAssertions** | Fluent `.Should()` assertions — the Apache-2.0 continuation of FluentAssertions, same API |
| **Moq** | Mocking — `new Mock<T>()`; **4.20.2 or later only** (4.20.0 and 4.20.1 shipped SponsorLink and are forbidden) |
| **.NET 10** (`net10.0`) | Target framework |

- **FluentAssertions is not used** (commercial licence from v8); no other assertion or mocking library (Shouldly, NSubstitute, FakeItEasy, …) is added; xUnit's `Assert.*` is not used.
- Exact package versions live in the test `.csproj` — never duplicated here.

## Project setup

`Core.Services.Tests` is a `Microsoft.NET.Sdk` project:

- `net10.0`, `Nullable` and `ImplicitUsings` enabled, `IsPackable=false`.
- `OutputType=Exe` (xUnit v3) and `IsTestingPlatformApplication=false` — tests run through classic VSTest `dotnet test`.
- References **only** `Core.Services` (which brings `Core.Domain` and `Core.Library` transitively).
- Registered in `Template.slnx` under the `/api/test/` folder.
- **Internal access.** Services are `internal sealed` (`docs/api/coding-standards.md` — Access Modifiers), so `Core.Services.csproj` declares `<InternalsVisibleTo Include="Core.Services.Tests" />` and `<InternalsVisibleTo Include="DynamicProxyGenAssembly2" />` (Moq must proxy `ILogger<{Entity}Service>`, whose type argument is internal). That `.csproj` change is application code, owned by `coder`.
