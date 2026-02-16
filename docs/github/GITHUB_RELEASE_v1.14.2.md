# REslava.Result v1.14.2 — Analyzers Phase 2 + 3: Code Fixes, RESL1003, RESL2001

> **4 diagnostics, 2 code fixes, 46 analyzer tests — catch Result and OneOf mistakes at compile time.**

---

## What's New

### RESL1003 — Prefer Match() Over If-Check (Info)

Suggests using `Match()` when both `.Value` and `.Errors` are accessed in complementary `if`/`else` branches:

```csharp
// ℹ️ RESL1003: Consider using Match()
if (result.IsSuccess) { var x = result.Value; }
else { var e = result.Errors; }

// ✅ Cleaner:
var x = result.Match(v => v, e => HandleErrors(e));
```

### RESL2001 — Unsafe OneOf.AsT* Access (Warning + Code Fix)

Warns when `.AsT1`–`.AsT4` is accessed on `OneOf<T1,...>` without checking `.IsT*`:

```csharp
var user = oneOf.AsT1;  // ⚠️ RESL2001: Access to '.AsT1' without checking '.IsT1'

// 💡 Code Fix: Replace with Match()
var user = oneOf.Match(t1 => t1, t2 => throw new NotImplementedException());
```

### RESL1001 Code Fixes (2 options)

The existing unsafe `.Value` analyzer now offers automatic fixes:

- **Fix A**: Wrap in `if (result.IsSuccess) { ... }` guard
- **Fix B**: Replace `.Value` with `.Match(v => v, e => default)`

### Shared Guard Detection

Extracted `GuardDetectionHelper` with parameterized `GuardConfig` — shared between RESL1001 (Result) and RESL2001 (OneOf) analyzers. Supports:
- `if (x.IsT1) { x.AsT1 }` — positive guard
- `if (!x.IsT1) return; x.AsT1` — early return
- `if (x.IsT2) { ... } else { x.AsT1 }` — else branch

---

## Diagnostic Summary

| ID | Title | Severity | Code Fix |
|----|-------|----------|----------|
| RESL1001 | Unsafe Result<T>.Value access | Warning | Yes (2 options) |
| RESL1002 | Discarded Result<T> return value | Warning | — |
| RESL1003 | Prefer Match() over if-check | Info | — |
| RESL2001 | Unsafe OneOf.AsT* access | Warning | Yes (Match) |

---

## Package Updates

| Package | Version | Description |
|---------|---------|-------------|
| `REslava.Result` | v1.14.2 — [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.14.2) | Core library |
| `REslava.Result.SourceGenerators` | v1.14.2 — [View on NuGet](https://www.nuget.org/packages/REslava.Result.SourceGenerators/1.14.2) | ASP.NET source generators |
| `REslava.Result.Analyzers` | v1.14.2 — [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.14.2) | Roslyn safety analyzers |

---

## Testing

- **46 analyzer tests** (10 RESL1001, 8 RESL1002, 6 RESL1001 fix, 8 RESL1003, 10 RESL2001, 4 RESL2001 fix)
- **2,004 total tests** across all packages and TFMs
- All tests green

---

## Breaking Changes

None. Fully backward compatible.

---

**MIT License** | [Full Changelog](../../CHANGELOG.md)
