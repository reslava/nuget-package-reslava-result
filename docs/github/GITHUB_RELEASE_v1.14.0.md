# REslava.Result v1.14.0 — Safety Analyzers

> **Compile-time diagnostics that catch Result&lt;T&gt; misuse before it reaches production.**

---

## What's New

### NEW: REslava.Result.Analyzers NuGet Package

A companion Roslyn analyzer package that provides **compile-time safety checks** for `Result<T>` usage patterns.

```bash
dotnet add package REslava.Result.Analyzers
```

#### RESL1001 — Unsafe `.Value` Access

Warns when `Result<T>.Value` is accessed without first checking `IsSuccess` or `IsFailed`.

```csharp
var result = GetUser(id);
var name = result.Value;        // Warning RESL1001

if (result.IsSuccess)
    var name = result.Value;    // OK — guarded
```

**Detects 5 guard patterns:**
- `if (result.IsSuccess) { ... }`
- `if (!result.IsFailed) { ... }`
- `if (result.IsFailed) { ... } else { result.Value }` (else branch)
- `if (result.IsFailed) return;` followed by `.Value` (early return)
- `if (!result.IsSuccess) throw ...;` followed by `.Value` (early throw)

#### RESL1002 — Discarded `Result<T>` Return Value

Warns when a method returning `Result<T>` or `Task<Result<T>>` is called and the return value is ignored.

```csharp
Save();                         // Warning RESL1002 — errors silently lost
await SaveAsync();              // Warning RESL1002

var result = Save();            // OK — assigned
return Save();                  // OK — returned
Process(Save());                // OK — passed as argument
```

### Package Improvements

- Package icon now appears on all 3 NuGet packages
- Package README included in REslava.Result.Analyzers
- Release pipeline builds and publishes all 3 packages

---

## Package Updates

| Package | Version | Description |
|---------|---------|-------------|
| `REslava.Result` | **v1.14.0** | Core library |
| `REslava.Result.SourceGenerators` | **v1.14.0** | ASP.NET source generators |
| `REslava.Result.Analyzers` | **v1.14.0** | Roslyn safety analyzers (NEW) |

---

## Testing

- **18 analyzer tests** (10 for RESL1001, 8 for RESL1002)
- **634 core library tests** x 3 TFMs (net8.0, net9.0, net10.0)
- All tests green in CI

---

## Installation

```bash
# All three packages
dotnet add package REslava.Result
dotnet add package REslava.Result.SourceGenerators
dotnet add package REslava.Result.Analyzers
```

Zero configuration — analyzers activate automatically when the package is installed.

---

## Breaking Changes

None. Fully backward compatible with v1.13.0.

---

**MIT License** | [Full Changelog](../../CHANGELOG.md)
