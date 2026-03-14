# REslava.Result v1.40.0

Structured diagnostic metadata for all error types: `ReasonMetadata` (caller capture via `[CallerMemberName/FilePath/LineNumber]`), `IReasonMetadata` capability interface, `ReasonMetadataExtensions`, static error factories with auto-tags, three new Roslyn analyzers (RESL1010, RESL2002, RESL1021), and `ResultFlow` body-scan error-type hints on failure edges.

---

## ✨ `ReasonMetadata` — Structured Diagnostic Metadata

A new `sealed record` capturing system/diagnostic context at the call site — separate from `Tags` (user/business data).

```csharp
// Three data planes:
// Message  — human explanation         (string)
// Tags     — user / business metadata  (ImmutableDictionary<string, object>)
// Metadata — system / diagnostic info  (ReasonMetadata)

var error = NotFoundError.For<Order>("abc123");

Console.WriteLine(error.Metadata.CallerMember);   // "PlaceOrder"   (method that called For<>)
Console.WriteLine(error.Metadata.CallerFile);      // "OrderService.cs"
Console.WriteLine(error.Metadata.CallerLine);      // 42
```

`ReasonMetadata.Empty` is a zero-allocation singleton used when no capture is needed. All CRTP operations (`WithMessage`, `WithTag`, `WithTags`, `WithTagsFrom`) preserve `Metadata` on the copy.

---

## ✨ `IReasonMetadata` — Capability Interface

A secondary opt-in interface (same pattern as `IAsyncDisposable`) allowing metadata access from `IReason`-typed references without breaking existing external implementations.

```csharp
// Pattern matching (idiomatic C#)
foreach (var reason in result.Reasons)
{
    if (reason is IReasonMetadata m)
        logger.Debug("Error from {Caller}", m.Metadata.CallerMember);
}

// Or via extension method (more ergonomic)
var caller = reason.TryGetMetadata()?.CallerMember;
```

All built-in `Reason` subclasses implement `IReasonMetadata`. External `IReason` implementations can also implement it to participate in diagnostic tooling without being forced to.

---

## ✨ `ReasonMetadataExtensions`

Two ergonomic helpers for working with `IReasonMetadata` from `IReason`-typed code:

```csharp
ReasonMetadata? meta = reason.TryGetMetadata();   // null if not IReasonMetadata
bool hasCaller      = reason.HasCallerInfo();      // true when CallerMember is non-null
```

---

## ✨ Static Error Factories with `[CallerMemberName]` Capture

All concrete error types now expose static factory methods that capture the call site via `[CallerMemberName]`. The old multi-parameter constructors are marked `[Obsolete]` — no runtime behavior change.

```csharp
// Before (deprecated — CallerMember captures the *constructor* name, not the user's method)
new ValidationError("Amount", "Amount is required");

// After — CallerMember = the method that called .Field()
ValidationError.Field("Amount", "Amount is required");
ForbiddenError.For("cancel", "order");
ConflictError.Duplicate("Order", "OrderId", orderId);
ConflictError.Duplicate<Order>("OrderId", orderId);   // entity name from typeof(T).Name
```

Factories also set structured auto-tags (`entityName`, `fieldName`, `conflictField`, etc.) on the error's `Tags` dictionary — no separate `WithTag()` call needed.

---

## ✨ JSON Serialization — `Metadata` Round-Trip

`ReasonJsonConverter` now writes a `"metadata"` key when `Metadata != Empty` and reads it back on deserialization. Backward-compatible — a missing key deserializes as `Empty`.

```json
{
  "$type": "NotFoundError",
  "message": "Order abc123 not found",
  "tags": { "entityName": "Order", "id": "abc123" },
  "metadata": { "callerMember": "PlaceOrder", "callerFile": "OrderService.cs", "callerLine": 42 }
}
```

---

## ✨ `RESL1010` — Unhandled Failure Path

Warns when a `Result<T>` local variable is never checked for failure before leaving its enclosing block.

```csharp
// ⚠️ RESL1010 — result is declared but never failure-checked
var result = GetUser(id);
return result.Value;   // throws InvalidOperationException on failure — silently

// ✅ OK — any failure-aware usage suppresses the warning
var result = GetUser(id);
if (result.IsFailure) return BadRequest(result.Reasons);
return result.Value;
```

Suppressed by: `IsSuccess`, `IsFailure`, `Match`, `Switch`, `TapOnFailure`, `Bind`, `Map`, `Ensure`, `GetValueOr`, `TryGetValue`, `Or`, `OrElse`, `MapError`, or returning the variable directly.

---

## ✨ `RESL2002` — Non-Exhaustive `ErrorsOf.Match()`

Warns at compile time when `Match()` is called with fewer handler lambdas than the `ErrorsOf<T1..Tn>` union has type arguments — preventing a runtime `InvalidOperationException`.

```csharp
ErrorsOf<ValidationError, InventoryError, PaymentError> error = ...;

// ⚠️ RESL2002 — 2 handlers for a 3-slot union
error.Match(
    v => HandleValidation(v),
    i => HandleInventory(i));

// ✅ OK
error.Match(
    v => HandleValidation(v),
    i => HandleInventory(i),
    p => HandlePayment(p));
```

---

## ✨ `RESL1021` — Multi-Argument `IError`/`IReason` Constructor

Warns when an `IError` or `IReason` implementation exposes a public constructor with 2+ required non-optional parameters. Multi-arg constructors prevent correct `[CallerMemberName]` capture.

```csharp
// ⚠️ RESL1021 — CallerMember will capture "InsufficientStockError", not the caller's method
public class InsufficientStockError : Error
{
    public InsufficientStockError(string sku, int requested) : base($"{sku}: only {requested} available") { }
}

// ✅ Use a static factory — CallerMember correctly captures the call site
public class InsufficientStockError : Error
{
    private InsufficientStockError(string message) : base(message) { }
    public static InsufficientStockError For(string sku, int requested,
        [CallerMemberName] string? callerMember = null)
        => new($"{sku}: only {requested} available") { Metadata = ReasonMetadata.FromCaller(callerMember) };
}
```

Allowed shapes (no warning): `()`, `(string)`, `(string, Exception)`, `[Obsolete]`-marked ctors, non-public ctors.

---

## ✨ `ResultFlow` — `ErrorHint` on Body-Scan Failure Edges

For `Result<T>` pipelines (body-scan mode), `ResultFlowChainExtractor` now syntactically extracts an error type hint from step arguments. When a step passes `new SomeError(...)` or `SomeError.Factory(...)`, the failure edge in the Mermaid diagram names the error type.

```
// Before (body-scan — no error type info)
N1_ValidateOrder -->|fail| F1["Failure"]:::failure

// After (body-scan with ErrorHint extraction)
N1_ValidateOrder -->|"fail: ValidationError"| F1["Failure"]:::failure
```

Type-read mode (for `Result<T, TError>` pipelines) still takes precedence over `ErrorHint`.

---

## 📦 NuGet

| Package | Link |
|---------|------|
| REslava.Result | [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.40.0) |
| REslava.Result.AspNetCore | [View on NuGet](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.40.0) |
| REslava.Result.Analyzers | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.40.0) |
| REslava.Result.FluentValidation | [View on NuGet](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.40.0) |
| REslava.Result.Http | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Http/1.40.0) |
| REslava.ResultFlow | [View on NuGet](https://www.nuget.org/packages/REslava.ResultFlow/1.40.0) |

---

## Stats

- 4,328 tests passing across net8.0, net9.0, net10.0 (1,306×3) + AspNetCore (131) + ResultFlow (58) + analyzer (114) + FluentValidation (26) + Http (20×3)
- 145 features across 13 categories
