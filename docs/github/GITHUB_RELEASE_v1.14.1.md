# REslava.Result v1.14.1 — Source Generator Consolidation

> **Internal refactor: 15 near-identical files → 7 shared arity-parameterized implementations.**

---

## What's Changed

### Consolidated OneOf2/3/4ToIResult into OneOfToIResult

The `OneOf2ToIResult`, `OneOf3ToIResult`, and `OneOf4ToIResult` source generators were ~95% copy-pasted code differing only by type arity. This release consolidates them into a single `OneOfToIResult` directory with shared implementations.

**Before:** 15 files across 3 directories (5 files each)
**After:** 7 files in 1 directory

| Component | Before | After |
|-----------|--------|-------|
| Generators | 3 separate full implementations | 3 thin wrappers (~10 lines each) |
| Orchestrators | 3 identical orchestrators | 1 shared `OneOfToIResultOrchestrator(arity)` |
| Attribute Generators | 6 files (2 per arity) | 2 shared files |
| Code Generators | 3 identical generators | 1 shared `OneOfToIResultExtensionGenerator(arity)` |
| Tests | 3 test files (14 tests) | 1 unified file (12 tests) |

**No API changes** — the generated code output is identical. This is purely an internal maintainability improvement.

---

## Package Updates

| Package | Version | Description |
|---------|---------|-------------|
| `REslava.Result` | v1.14.1 — [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.14.1) | Core library |
| `REslava.Result.SourceGenerators` | v1.14.1 — [View on NuGet](https://www.nuget.org/packages/REslava.Result.SourceGenerators/1.14.1) | ASP.NET source generators |
| `REslava.Result.Analyzers` | v1.14.1 — [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.14.1) | Roslyn safety analyzers |

---

## Testing

- **12 unified OneOfToIResult tests** (4 per arity: generate extensions, generate attributes, no-usage guard, type combinations)
- **1,976+ total tests** across all packages and TFMs
- All tests green in CI

---

## Breaking Changes

None. Fully backward compatible with v1.14.0.

---

**MIT License** | [Full Changelog](../../CHANGELOG.md)
