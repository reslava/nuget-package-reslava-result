# REslava.ResultFlow

**Library-agnostic** source generator for automatic Mermaid pipeline diagrams — works with any fluent Result library.

Add `[ResultFlow]` to any fluent method → the diagram is **injected as a comment** by the IDE code action (no build needed), or accessed as a `const string` after build. Zero runtime overhead. Zero manual maintenance.

Ships with a **built-in convention dictionary** pre-configured for **REslava.Result**, **ErrorOr**, **LanguageExt**, and **FluentResults**. Any other library can be supported by adding a `resultflow.json` file to your project.

> Using **REslava.Result**? Consider [`REslava.Result.Flow`](https://www.nuget.org/packages/REslava.Result.Flow) for richer diagrams with typed error edges and full semantic analysis.

## Installation

```bash
dotnet add package REslava.ResultFlow
```

No extra `using` required — the `[ResultFlow]` attribute is injected into your compilation automatically by the source generator.

## Quick Start

```csharp
[ResultFlow]
public async Task<Result<UserDto>> RegisterAsync(RegisterCommand cmd) =>
    await CreateUser(cmd)
        .EnsureAsync(IsEmailValid, new InvalidEmailError())
        .BindAsync(SaveUser)
        .TapAsync(SendWelcomeEmail)
        .MapAsync(ToDto);
```

Use the **IDE code action** to inject the diagram as a comment directly above the method — no build required:

```csharp
/*
```mermaid
flowchart LR
    N0_EnsureAsync["EnsureAsync"]:::gatekeeper
    ...
```*/
[ResultFlow]
public async Task<Result<UserDto>> RegisterAsync(RegisterCommand cmd) => ...
```

Or access the generated constant after build and paste it into any Mermaid renderer:

```csharp
string diagram = Generated.ResultFlow.UserService_Flows.RegisterAsync;
// Paste into mermaid.live, GitHub, Notion, VS Code, or your wiki
```

## Generated Diagram Example

```
flowchart LR
    N0_EnsureAsync["EnsureAsync"]:::gatekeeper
    N0_EnsureAsync -->|pass| N1_BindAsync
    N0_EnsureAsync -->|fail| F0["Failure"]:::failure
    N1_BindAsync["BindAsync"]:::transform
    N1_BindAsync -->|ok| N2_TapAsync
    N1_BindAsync -->|fail| F1["Failure"]:::failure
    N2_TapAsync["TapAsync"]:::sideeffect
    N2_TapAsync --> N3_MapAsync
    N3_MapAsync["MapAsync"]:::transform
    classDef gatekeeper fill:#e3e9fa,color:#3f5c9a
    classDef transform fill:#e3f0e8,color:#2f7a5c
    classDef sideeffect fill:#fff4d9,color:#b8882c
    classDef failure fill:#f8e3e3,color:#b13e3e
```

Each operation is color-coded by semantic role:
- **Lavender** — gatekeepers (`Ensure`) — can fail validation
- **Mint** — transforms (`Bind` / `Map`) — shape the result
- **Vanilla** — side effects (`Tap`) — fire-and-forget
- **Soft pink** — failure paths

## Supported Libraries

The built-in convention dictionary covers the most popular Result libraries out of the box — no configuration needed:

| Library | Methods recognized |
|---|---|
| **REslava.Result** | `Ensure`, `Bind`, `Map`, `Tap`, `TapOnFailure`, `TapBoth`, `Match`, `WithSuccess` (+ `Async` variants) |
| **ErrorOr** | `Then`, `ThenAsync`, `Switch`, `SwitchAsync` |
| **LanguageExt** | `Filter`, `Do`, `DoAsync`, `DoLeft`, `DoLeftAsync` |

Any unrecognized method is rendered as a generic **operation** node — the diagram is still generated.

## Custom Method Classification — resultflow.json

Add a `resultflow.json` file to your project to classify custom or third-party methods. Config entries **override** the built-in dictionary.

```json
{
  "mappings": [
    {
      "bind":       ["Chain", "AndThen"],
      "map":        ["Transform"],
      "tap":        ["Log", "Audit"],
      "gatekeeper": ["Require"],
      "terminal":   ["Fold"]
    }
  ]
}
```

Register it as an `AdditionalFile` in your `.csproj`:

```xml
<ItemGroup>
  <AdditionalFiles Include="resultflow.json" />
</ItemGroup>
```

Supported keys: `bind`, `map`, `tap`, `tapOnFailure`, `gatekeeper`, `terminal`.

## Code Action

The companion analyzer detects `[ResultFlow]` methods that are missing the diagram as a developer comment. A **single-click code fix** inserts the generated Mermaid diagram directly above the method body — so the diagram lives next to the code that produced it.

## Diagnostics

| ID | Severity | Description |
|---|---|---|
| `REF001` | Info | `[ResultFlow]` could not detect a fluent chain — diagram not generated. Check that the method body is an expression body or ends with a `return` of a fluent chain. |
| `REF003` | Warning | `resultflow.json` could not be parsed — falling back to the built-in convention dictionary. Check the JSON syntax. |

## Documentation

Full documentation: [reslava.github.io/nuget-package-reslava-result](https://reslava.github.io/nuget-package-reslava-result/resultflow/)

**MIT License** | Works with any .NET project (netstandard2.0)
