# REslava.Result v1.38.0

Four additions: a new migration analyzer, two `REslava.ResultFlow` diagram enhancements, and a new REslava.Result-native companion package for full typed pipeline visualization.

---

## 🔬 RESL1009 — Migrate try/catch to Result\<T\>.Try

New Roslyn analyzer with code fix that detects `try/catch` blocks returning `Result<T>` and offers a one-click migration to the functional equivalent.

```csharp
// Before — RESL1009 warning
public Result<User> GetUser(int id)
{
    try
    {
        return Result<User>.Ok(_repo.FindById(id));
    }
    catch (Exception ex)
    {
        return Result<User>.Fail(new ExceptionError(ex));
    }
}

// After — one-click fix
public Result<User> GetUser(int id) =>
    Result<User>.Try(() => _repo.FindById(id));
```

Also detects async `try/catch` blocks and migrates to `Result<T>.TryAsync(...)`.

---

## ⚡ RF-1 — Async Step Annotation

`REslava.ResultFlow` now appends ⚡ to any pipeline step that resolves via `await`. No configuration required — the annotation is inferred from the syntax tree.

```
flowchart TD
    N0["EnsureAsync ⚡<br/>User"] --> N1["BindAsync ⚡<br/>User"]
    N1 --> N2["MapAsync ⚡<br/>User → UserDto"]
```

Sync steps are unchanged; only `await`-resolved steps get the marker.

---

## 🏷️ RF-2 — Success Type Travel

`REslava.ResultFlow` now infers the success type `T` at each pipeline step using generic Roslyn type extraction — no `IResultBase`, no `IError`, works with any Result library.

```
flowchart TD
    N0["CreateUser<br/>User"] --> N1["EnsureAsync ⚡<br/>User"]
    N1 --> N2["BindAsync ⚡<br/>User"]
    N2 --> N3["MapAsync ⚡<br/>User → UserDto"]
```

Label rules:
- Type-preserving step → `"MethodName<br/>T"`
- Type-changing step → `"MethodName<br/>T → U"`
- Non-generic / unresolvable return → `"MethodName"` (no regression)

---

## 🔬 RF-3 — REslava.Result.Flow Native Companion Package

New `REslava.Result.Flow` package — the REslava.Result-specific counterpart to the library-agnostic `REslava.ResultFlow`.

Uses `IResultBase` and `IError` as Roslyn anchors to infer both success types **and** typed error surfaces. Failure edges in the generated diagram are annotated with the specific error type raised at each step.

```
flowchart TD
    N0["CreateUser<br/>User"] --> N1["EnsureAsync ⚡<br/>User"]
    N1 -->|"InvalidEmailError"| F0["Failure"]:::failure
    N1 --> N2["BindAsync ⚡<br/>User"]
    N2 -->|"SaveError"| F1["Failure"]:::failure
    N2 --> N3["MapAsync ⚡<br/>User → UserDto"]
```

| | `REslava.ResultFlow` | `REslava.Result.Flow` |
|---|---|---|
| Library dependency | None | Requires `REslava.Result` |
| Success type travel | ✅ | ✅ |
| Async annotation | ✅ | ✅ |
| Typed failure edges | ❌ | ✅ |

---

## 📦 NuGet

| Package | Link |
|---------|------|
| REslava.Result | [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.38.0) |
| REslava.Result.AspNetCore | [View on NuGet](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.38.0) |
| REslava.Result.Analyzers | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.38.0) |
| REslava.Result.FluentValidation | [View on NuGet](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.38.0) |
| REslava.Result.Http | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Http/1.38.0) |
| REslava.ResultFlow | [View on NuGet](https://www.nuget.org/packages/REslava.ResultFlow/1.38.0) |
| REslava.Result.Flow ✨ NEW | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Flow/1.38.0) |

---

## Stats

- 3,994 tests passing across net8.0, net9.0, net10.0 (1,216×3) + generator (131) + ResultFlow (39) + analyzer (79) + FluentValidation bridge (26) + Http (20×3) + Result.Flow (11)
- 140 features across 13 categories
