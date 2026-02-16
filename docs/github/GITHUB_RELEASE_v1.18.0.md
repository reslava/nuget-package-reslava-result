# REslava.Result v1.18.0 — Task-Based Async Patterns

> **WhenAll, Retry, and Timeout — the async primitives your Result code needs for production.**

---

## What's Changed

### Async Patterns (WhenAll, Retry, Timeout)

Three new async primitives that eliminate boilerplate in concurrent and resilient code.

**WhenAll — Typed tuple results from concurrent operations:**

```csharp
// Run multiple async results concurrently — typed tuples!
var result = await Result.WhenAll(GetUser(id), GetAccount(id));
var (user, account) = result.Value;

// 3 and 4-arity overloads
var result = await Result.WhenAll(taskA, taskB, taskC);

// Collection variant → Result<ImmutableList<T>>
var result = await Result.WhenAll(userIds.Select(id => GetUser(id)));
```

If all succeed → `Ok` with tuple. If any fail → aggregates ALL errors from ALL failed tasks. Faulted/cancelled tasks are automatically wrapped in `ExceptionError`.

**Retry — With configurable backoff:**

```csharp
// Simple retry with constant delay
var result = await Result.Retry(
    () => CallExternalApi(),
    maxRetries: 3,
    delay: TimeSpan.FromSeconds(1));

// Exponential backoff
var result = await Result.Retry(
    () => CallExternalApi(),
    maxRetries: 3,
    delay: TimeSpan.FromSeconds(1),
    backoffFactor: 2.0);

// With cancellation
var result = await Result.Retry(
    () => CallExternalApi(),
    maxRetries: 3,
    cancellationToken: ct);
```

All attempt errors are accumulated with attempt markers. `OperationCanceledException` stops retrying immediately.

**Timeout — Enforce time limits:**

```csharp
var result = await GetSlowData().Timeout(TimeSpan.FromSeconds(5));

// Check for timeout
if (result.Errors.Any(e => e.Tags.ContainsKey("TimeoutTag")))
    Console.WriteLine("Operation timed out");
```

---

## Package Updates

| Package | Version | Description |
|---------|---------|-------------|
| `REslava.Result` | v1.18.0 — [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.18.0) | Core library |
| `REslava.Result.SourceGenerators` | v1.18.0 — [View on NuGet](https://www.nuget.org/packages/REslava.Result.SourceGenerators/1.18.0) | ASP.NET source generators |
| `REslava.Result.Analyzers` | v1.18.0 — [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.18.0) | Roslyn safety analyzers |

---

## Testing

- **2,271 total tests** across all packages and TFMs (41 new async pattern tests per TFM)
- All tests green on net8.0, net9.0, and net10.0

---

## Breaking Changes

None. This is a purely additive release.

---

**MIT License** | [Full Changelog](../../CHANGELOG.md)
