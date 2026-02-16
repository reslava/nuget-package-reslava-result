# v1.19.0 — RESL1004 Analyzer + CancellationToken Support

## RESL1004 — Async Result Not Awaited

New analyzer that detects `Task<Result<T>>` assigned to a variable without `await` in async methods — a subtle bug that compiles but produces wrong results at runtime.

```csharp
async Task M()
{
    var result = GetFromDb(id);     // ⚠️ RESL1004: Task<Result<T>> not awaited
    var result = await GetFromDb(id); // ✅ No warning
}
```

**Code fix**: Automatically adds `await` keyword.

**Smart skip rules**: No warning for explicit `Task<...>` type (intentional), non-async methods, or returned tasks.

## CancellationToken Support Throughout

All async methods now accept an optional `CancellationToken cancellationToken = default` parameter:

- **Instance methods**: `TapAsync`, `BindAsync`, `MapAsync`, `MatchAsync`
- **Factory methods**: `TryAsync` (generic and non-generic)
- **Extension methods**: `BindAsync`, `MapAsync`, `TapAsync`, `TapOnFailureAsync`, `WithSuccessAsync`, `EnsureAsync`, `EnsureNotNullAsync`, `SelectManyAsync`, `SelectAsync`, `WhereAsync`, `MatchAsync`

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

var result = await GetUserAsync(id)
    .MapAsync(u => u.Name, cts.Token)
    .EnsureAsync(n => n.Length > 0, new Error("Empty name"), cts.Token);
```

**Source-compatible** — existing code compiles without changes.

## Test Suite

- 2,318 tests passing across net8.0, net9.0, net10.0
- 54 analyzer tests (8 new for RESL1004)
- 13 new CancellationToken tests

## NuGet Packages

- [REslava.Result 1.19.0](https://www.nuget.org/packages/REslava.Result/1.19.0)
- [REslava.Result.Analyzers 1.19.0](https://www.nuget.org/packages/REslava.Result.Analyzers/1.19.0)
- [REslava.Result.SourceGenerators 1.19.0](https://www.nuget.org/packages/REslava.Result.SourceGenerators/1.19.0)
