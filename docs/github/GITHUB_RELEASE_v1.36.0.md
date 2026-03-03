# REslava.Result v1.36.0

`REslava.ResultFlow` is now a **standalone, library-agnostic** package. Add `[ResultFlow]` to any fluent method — the IDE injects the Mermaid diagram as a comment with a single click. No build required.

---

## 🗺️ REslava.ResultFlow — Standalone Package

`REslava.ResultFlow` is now independent of `REslava.Result`. It works with **any fluent Result library** — install it alongside ErrorOr, LanguageExt, FluentResults, or any custom library.

```bash
dotnet add package REslava.ResultFlow
```

Ships with a built-in convention dictionary pre-configured for REslava.Result, ErrorOr, and LanguageExt. Any other library can be supported via `resultflow.json`.

---

## 🛠️ Code Action — Insert Diagram as Comment

The new `REF002` analyzer fires on every `[ResultFlow]` method with a detectable chain. A **single-click code action** inserts the full Mermaid diagram as a block comment directly above the method — no build required:

```csharp
/*
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
*/
[ResultFlow]
public async Task<Result<UserDto>> RegisterAsync(RegisterCommand cmd) =>
    await CreateUser(cmd)
        .EnsureAsync(IsEmailValid, new InvalidEmailError())
        .BindAsync(SaveUser)
        .TapAsync(SendWelcomeEmail)
        .MapAsync(ToDto);
```

The diagram also remains available as a `const string` after build:

```csharp
string diagram = Generated.ResultFlow.UserService_Flows.RegisterAsync;
```

---

## 🌐 Convention Dictionary — ErrorOr & LanguageExt

Built-in support expanded to cover the most popular Result libraries — zero configuration:

| Library | New methods recognized |
|---|---|
| **ErrorOr** | `Then`, `ThenAsync`, `Switch`, `SwitchAsync` |
| **LanguageExt** | `Filter`, `Do`, `DoAsync`, `DoLeft`, `DoLeftAsync` |

---

## ⚙️ resultflow.json — Custom Method Classification

Add a `resultflow.json` AdditionalFile to classify custom or third-party library methods. Config entries **override** the built-in dictionary:

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

```xml
<ItemGroup>
  <AdditionalFiles Include="resultflow.json" />
</ItemGroup>
```

A `REF003` Warning is emitted if the file cannot be parsed — the generator falls back to the built-in convention dictionary.

---

## ⚠️ Breaking Changes

### `[ResultFlow]` namespace changed

```diff
- using REslava.Result.SourceGenerators;
+ using REslava.ResultFlow;
```

```diff
- <PackageReference Include="REslava.Result.SourceGenerators" Version="1.35.0" />
+ <PackageReference Include="REslava.ResultFlow" Version="1.36.0" />
```

### `REslava.Result.SourceGenerators` renamed to `REslava.Result.AspNetCore`

```diff
- <PackageReference Include="REslava.Result.SourceGenerators" Version="1.35.0" />
+ <PackageReference Include="REslava.Result.AspNetCore" Version="1.36.0" />
```

No stub/compatibility package — update the reference directly.

---

## 📦 NuGet

| Package | Link |
|---------|------|
| REslava.Result | [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.36.0) |
| REslava.Result.AspNetCore | [View on NuGet](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.36.0) |
| REslava.Result.Analyzers | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.36.0) |
| REslava.Result.FluentValidation | [View on NuGet](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.36.0) |
| REslava.Result.Http | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Http/1.36.0) |
| REslava.ResultFlow | [View on NuGet](https://www.nuget.org/packages/REslava.ResultFlow/1.36.0) |

---

## Stats

- 3,795 tests passing across net8.0, net9.0, net10.0 (1,157×3) + generator (143) + ResultFlow (27) + analyzer (68) + FluentValidation bridge (26) + Http (20×3)
- 133 features across 13 categories
