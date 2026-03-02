# REslava.Result v1.35.0

Annotate any fluent `Result<T>` pipeline method with `[ResultFlow]` — a Mermaid diagram is generated automatically at compile time. Zero runtime overhead. Zero manual maintenance.

---

## 🗺️ Pipeline Visualization — `[ResultFlow]`

```csharp
[ResultFlow]
public async Task<Result<UserDto>> RegisterAsync(RegisterCommand cmd) =>
    await CreateUser(cmd)
        .EnsureAsync(IsEmailValid, new InvalidEmailError())
        .BindAsync(SaveUser)
        .TapAsync(SendWelcomeEmail)
        .MapAsync(ToDto);
```

After build, the generated constant is ready:

```csharp
// Use it anywhere — zero runtime cost
string diagram = Generated.ResultFlow.UserService_Flows.RegisterAsync;
```

The constant contains a complete Mermaid flowchart:

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
```

Paste it into any Mermaid renderer (GitHub, Notion, VS Code extension, mermaid.live) to see the diagram.

### Supported operation semantics

| Operation | Class | Failure edge |
|-----------|-------|-------------|
| `Ensure`, `EnsureAsync` | `gatekeeper` | ✅ `\|fail\|` |
| `Bind`, `BindAsync` | `transform` | ✅ `\|fail\|` |
| `Map`, `MapAsync` | `transform` | ❌ pure transform |
| `Tap`, `TapAsync` | `sideeffect` | ❌ |
| `TapOnFailure`, `TapOnFailureAsync` | `sideeffect` | ❌ |
| `TapBoth` | `sideeffect` | ❌ |
| `Match`, `MatchAsync` | `terminal` | ❌ no outbound edges |
| `WithSuccess`, `WithError`, `WithSuccessAsync` | *(invisible)* | traversed, not rendered |

### REF001 — non-fluent chain

If the method body cannot be parsed as a fluent chain, an Info-level diagnostic is emitted — no crash, no silent skip:

```
REF001: [ResultFlow] could not extract a fluent chain from 'MethodName'.
Only fluent-style return pipelines are supported. Diagram not generated.
```

---

## 📦 NuGet

| Package | Link |
|---------|------|
| REslava.Result | [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.35.0) |
| REslava.Result.SourceGenerators | [View on NuGet](https://www.nuget.org/packages/REslava.Result.SourceGenerators/1.35.0) |
| REslava.Result.Analyzers | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.35.0) |
| REslava.Result.FluentValidation | [View on NuGet](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.35.0) |
| REslava.Result.Http | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Http/1.35.0) |

---

## Stats

- 3,768 tests passing across net8.0, net9.0, net10.0 (1,157×3) + generator (143) + analyzer (68) + FluentValidation bridge (26) + Http (20×3)
- 128 features across 13 categories
