# REslava.Result v1.41.0

Typed tag access layer (`TagKey<T>`, `DomainTags`, `SystemTags`), typed error construction (`IErrorFactory<TSelf>`, `Result.Fail<TError>(string)`), runtime-to-diagram correlation (`ReasonMetadata.PipelineStep`/`NodeId`), and `REslava.Result.Flow` chain-extraction fixes (Gap 1: lambda body step name, Gap 3: variable initializer root) with Mermaid node correlation comments.

---

## ✨ Typed Tag Access — `TagKey<T>`, `DomainTags`, `SystemTags`

A type-safe accessor layer on top of the existing `ImmutableDictionary<string, object>` tag storage.

```csharp
// Read with type safety — no casts, no magic strings
if (error.TryGet(DomainTags.Entity, out var entity))
    Console.WriteLine($"Entity: {entity}");      // string — no cast

// Write with type safety
var err = new Error("Something went wrong")
    .WithTag(DomainTags.Entity, "Order")
    .WithTag(SystemTags.ErrorCode, "ERR_001");

// Built-in domain errors use DomainTags automatically
var notFound = NotFoundError.For<Order>("abc");
notFound.TryGet(DomainTags.Entity, out var name);  // "Order"
notFound.TryGet(DomainTags.EntityId, out var id);  // "abc"
```

**`DomainTags`** — predefined domain keys: `Entity`, `EntityId`, `Field`, `Value`, `Operation`
**`SystemTags`** — predefined integration keys: `HttpStatus`, `ErrorCode`, `RetryAfter`, `Service`

---

## ✨ `ReasonTagExtensions` — `TryGet<T>` + `Has<T>`

Typed reads on any `IReason` — null-safe, no cast exceptions.

```csharp
// TryGet<T> — typed retrieval
if (reason.TryGet(DomainTags.Field, out string? fieldName))
    Console.WriteLine($"Field: {fieldName}");

// Has<T> — existence check
if (reason.Has(SystemTags.HttpStatus))
    // ...
```

---

## ✨ `IErrorFactory<TSelf>` — Generic Error Construction

C# 11 static abstract interface enabling type-parameterized error creation.

```csharp
// New: Result.Fail<TError>(string) — no new keyword needed
var result = Result.Fail<NotFoundError>("Order not found");
var typed  = Result<Order>.Fail<ValidationError>("Amount is required");

// How IErrorFactory<TSelf> is declared
public interface IErrorFactory<TSelf> where TSelf : IErrorFactory<TSelf>
{
    static abstract TSelf Create(string message);
}

// Built-in errors — all implement it:
// Error, NotFoundError, ConflictError, ValidationError, ForbiddenError, UnauthorizedError
```

Custom errors can implement `IErrorFactory<TSelf>` to participate in typed factory overloads:

```csharp
public class InsufficientStockError : Error, IErrorFactory<InsufficientStockError>
{
    private InsufficientStockError(string message) : base(message) { }
    public static InsufficientStockError Create(string message) => new(message);
}

// Now usable with typed overload
var result = Result.Fail<InsufficientStockError>("Not enough stock");
```

---

## ✨ `ReasonMetadata.PipelineStep` + `NodeId` — Runtime → Diagram Correlation

Two new fields on `ReasonMetadata` linking runtime errors to their diagram node.

```csharp
// Set at call site (e.g. in a pipeline step)
var error = NotFoundError.For<Order>(id) with
{
    Metadata = error.Metadata with
    {
        PipelineStep = "FindOrder",
        NodeId = "N0_FindOrder"    // matches the generated Mermaid node
    }
};

// At error handling — correlate back to diagram
if (result.IsFailure)
{
    var meta = result.Errors[0].TryGetMetadata();
    Console.WriteLine(meta?.NodeId);        // "N0_FindOrder"
    Console.WriteLine(meta?.PipelineStep);  // "FindOrder"
}
```

---

## ✨ `REslava.Result.Flow` — Chain Extraction Fixes

### Gap 1: Lambda Body Method Name

`.Bind(x => SaveUser(x))` now renders step label `"SaveUser"` in the generated Mermaid diagram instead of `"Bind"`. Single-expression lambdas with a direct method call are unwrapped automatically.

```
// Before
N1["Bind"]:::bind

// After (Gap 1 fix)
N1["SaveUser"]:::bind
```

### Gap 3: Variable Initializer Resolution

Chains that start from a local variable are now correctly detected.

```csharp
// Before: chain root not detected — REF001 emitted, no diagram generated
var result = FindOrder(id);
return result
    .Bind(ValidateOrder)
    .Map(o => new OrderDto(o));

// After (Gap 3 fix): FindOrder correctly seeds the chain root
```

### Mermaid Node Correlation Block

Every generated diagram now ends with a correlation comment block:

```
%% --- Node correlation (ReasonMetadata.NodeId / PipelineStep) ---
%%   N0_FindOrder → FindOrder
%%   N1_ValidateOrder → ValidateOrder
%%   N2_Map → Map
```

Use these node IDs to set `ReasonMetadata.NodeId` at runtime for end-to-end traceability.

---

## 📦 NuGet

| Package | Link |
|---------|------|
| REslava.Result | [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.41.0) |
| REslava.Result.Flow | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Flow/1.41.0) |
| REslava.Result.AspNetCore | [View on NuGet](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.41.0) |
| REslava.Result.Http | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Http/1.41.0) |
| REslava.Result.Analyzers | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.41.0) |
| REslava.ResultFlow | [View on NuGet](https://www.nuget.org/packages/REslava.ResultFlow/1.41.0) |
| REslava.Result.FluentValidation | [View on NuGet](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.41.0) |

---

## Stats

- Tests: >4,300 passing (same floor — no new hundred crossed)
- 169 features across 13 categories
