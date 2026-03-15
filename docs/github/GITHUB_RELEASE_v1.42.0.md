# REslava.Result v1.42.0

`ResultContext` — pipeline context propagation, automatic error enrichment, and a new `REslava.Result.OpenTelemetry` package. Context flows from the pipeline entry point through every operator, automatically tagging errors with entity/correlation/tenant metadata.

---

## ✨ `ResultContext` — Pipeline Context Propagation

A lightweight `sealed record` embedded in every `Result<T>` and `Result<T,TError>`. It carries entity/correlation/tenant metadata through the pipeline without polluting method signatures.

```csharp
var result = Result<Order>.Ok(order)
    .WithContext(entityId: orderId.ToString(), correlationId: traceId, tenantId: tenantId)
    .Bind(ValidateStock)
    .Bind(ProcessPayment)
    .Map(o => new OrderDto(o));

// Context flows through every step:
result.Context?.Entity        // "OrderDto" — Map updated to typeof(TOut).Name
result.Context?.EntityId      // orderId    — from WithContext
result.Context?.CorrelationId // traceId    — from WithContext
result.Context?.TenantId      // tenantId   — from WithContext
```

**Fields**: `Entity`, `EntityId`, `CorrelationId`, `OperationName`, `TenantId` (all nullable strings).

---

## ✨ Auto-seeding + `.WithContext()`

`Ok()` and `Fail()` auto-seed `Context.Entity` from `typeof(T).Name`. Runtime values are added with `.WithContext()` which merges incrementally:

```csharp
Result<Order>.Ok(order).Context?.Entity   // "Order" — no code needed

// Merge runtime values — calling twice preserves earlier fields
Result<Order>.Ok(order)
    .WithContext(entityId: "42", correlationId: "trace-1")
    .WithContext(entityId: "99");  // only overrides EntityId
// CorrelationId = "trace-1" is preserved
```

---

## ✨ Error Auto-Enrichment (Non-Overwriting)

When any pipeline step produces an error, `ResultContext` fields are injected as tags automatically. Tags already set by the error's factory are **never overwritten**:

```csharp
var result = Result<Order>.Ok(order)
    .WithContext(entityId: "42", correlationId: "trace-1")
    .Ensure(_ => false, new Error("guard failed"));

var error = result.Errors[0];
error.Tags["Entity"]        // "Order"   ← auto-injected from context
error.Tags["EntityId"]      // "42"      ← auto-injected from context
error.Tags["CorrelationId"] // "trace-1" ← auto-injected from context

// Factory-set tags always win:
var factoryError = new Error("conflict").WithTag(DomainTags.Entity, "FactoryEntity");
// → error.Tags["Entity"] = "FactoryEntity" (not overwritten)
// → error.Tags["EntityId"] = "42"          (filled from context)
```

New `DomainTags` keys: `CorrelationId`, `OperationName`, `TenantId`.

---

## ✨ `REslava.Result.OpenTelemetry` — New Package

Zero-cost OpenTelemetry integration. No-op when no active span is present.

```xml
<PackageReference Include="REslava.Result.OpenTelemetry" Version="1.42.0" />
```

### `.WithOpenTelemetry()` — seed context from active span

```csharp
var result = Result<Order>.Ok(order)
    .WithOpenTelemetry()           // CorrelationId ← TraceId, OperationName ← DisplayName
    .WithContext(entityId: orderId.ToString())
    .Bind(ValidateStock)
    .Bind(ProcessPayment)
    .WriteErrorTagsToSpan();       // push error tags to span on failure
```

### `.WriteErrorTagsToSpan()` — write error tags to active span

On failure, writes every error tag (`Entity`, `EntityId`, `CorrelationId`, etc.) as span attributes on `Activity.Current`. Passes through unchanged on success.

Both methods are available on `Result<T>`, `Result`, and `Result<T,TError>`.

---

## ✨ ResultFlow Context Integration

- **`WithContext` — Invisible node**: `.WithContext(...)` is classified as `NodeKind.Invisible` in both `REslava.Result.Flow` and `REslava.ResultFlow` — not rendered as a pipeline step in diagrams
- **Context footer comment**: `REslava.Result.Flow` extracts literal `operationName`/`correlationId` values from `.WithContext(...)` calls and appends them as a `%%` comment block in the Mermaid output

---

## 🔧 Generator Fix

`FluentValidateExtensionGenerator` and `ValidateExtensionGenerator` updated to emit `ValidationError.Field(fieldName, message)` instead of the deprecated 2-arg constructor. Resolves compile-time warnings in projects using `[FluentValidate]` or `[Validate]`.

---

## 📦 NuGet

| Package | Link |
|---------|------|
| REslava.Result | [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.42.0) |
| REslava.Result.Flow | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Flow/1.42.0) |
| REslava.Result.AspNetCore | [View on NuGet](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.42.0) |
| REslava.Result.Http | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Http/1.42.0) |
| REslava.Result.Analyzers | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.42.0) |
| REslava.ResultFlow | [View on NuGet](https://www.nuget.org/packages/REslava.ResultFlow/1.42.0) |
| REslava.Result.FluentValidation | [View on NuGet](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.42.0) |
| REslava.Result.OpenTelemetry | [View on NuGet](https://www.nuget.org/packages/REslava.Result.OpenTelemetry/1.42.0) |

---

## Stats

- Tests: >4,400 passing (same floor — no new hundred crossed)
- 182 features across 15 categories
