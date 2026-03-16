# REslava.Result.OpenTelemetry

OpenTelemetry integration for [`REslava.Result`](https://www.nuget.org/packages/REslava.Result). Seeds `ResultContext` from the active span and writes error tags as span attributes.

## Installation

```bash
dotnet add package REslava.Result.OpenTelemetry
```

Requires `REslava.Result` ≥ 1.42.0.

## What it does

| Method | Description |
|---|---|
| `.WithOpenTelemetry()` | Seeds `ResultContext.CorrelationId` from `Activity.Current.TraceId` and `OperationName` from `Activity.Current.DisplayName`. No-op when no active span. |
| `.WriteErrorTagsToSpan()` | On failure, writes each `error.Tags` entry as a span attribute via `Activity.Current.SetTag()`. No-op when no active span or result is success. |

Both methods return the original result unchanged — safe to inline in a pipeline.

## Quick Start

```csharp
using REslava.Result.OpenTelemetry;

var result = await FindUser(userId)
    .WithOpenTelemetry()          // seed CorrelationId + OperationName from active span
    .BindAsync(EnrichUser)
    .EnsureAsync(IsActive, new ForbiddenError())
    .WriteErrorTagsToSpan();      // write error tags to span on failure
```

## Context propagation

`.WithOpenTelemetry()` integrates with `ResultContext` — the context then propagates automatically through `Bind`, `Map`, `Ensure`, and other pipeline operators. Error enrichment happens at the end of the pipeline, not at each step.

```csharp
// What gets set on ResultContext:
// CorrelationId = Activity.Current.TraceId.ToString()
// OperationName = Activity.Current.DisplayName
```

## Zero-cost when inactive

All methods check `Activity.Current == null` and return immediately — no allocations, no reflection, no overhead outside of an active OpenTelemetry span.

## Documentation

Full documentation: [reslava.github.io/nuget-package-reslava-result](https://reslava.github.io/nuget-package-reslava-result/)

**MIT License** | .NET 8 / 9 / 10
