---
title: Typed Error Pipelines
description: Replace runtime error bags with compile-time typed failure edges. Result<TValue, TError> + ErrorsOf<T1..T8> — exhaustive Match, no missed error cases.
tagline: The compiler knows your failure modes.
---

# 🎯 Typed Error Pipelines

Use `Result<TValue, TError>` when you want **compile-time-known, exhaustive failure edges** — every error type in the pipeline is explicit in the signature. Use `Result<T>` when you prefer a shared `IEnumerable<IError>` bag and runtime polymorphism (e.g. FluentValidation-style lists).

---

## `ErrorsOf<T1..T8>` — Typed Error Union

`ErrorsOf<T1, T2, ...>` is a discriminated union over error types. It inherits `OneOfBase` (same shared dispatch as `OneOf`) and implements `IError` — so it is itself a valid error usable as `TError` in `Result<TValue, TError>`.

All type slots must implement `IError` (`where T1 : IError where T2 : IError ...`).

```csharp
// Implicit conversions from each Ti — no explicit wrapping needed
ErrorsOf<ValidationError, InventoryError> err = new ValidationError("Amount required");

// Exhaustive match over the active case
string message = err.Match(
    v => v.Message,
    i => i.Message);

// IError implementation delegates to the active case
Console.WriteLine(err.Message); // "Amount required"

// IsT1..T8 / AsT1..T8
if (err.IsT2) { var inv = err.AsT2; /* InventoryError */ }
```

---

## `Result<TValue, TError>` — Factory and Accessors

```csharp
// Factory
var ok   = Result<Order, ValidationError>.Ok(order);
var fail = Result<Order, ValidationError>.Fail(new ValidationError("Amount required"));

// Accessors
bool succeeded = ok.IsSuccess;    // true
bool failed    = ok.IsFailure;    // false
Order value    = ok.Value;        // throws InvalidOperationException on failure
ValidationError err = fail.Error; // throws InvalidOperationException on success
```

---

## Full Pipeline Walkthrough

Each step declares a single concrete error type. `Bind` widens the union by exactly one slot per step — the error surface grows automatically:

```csharp
// Steps — clean single-error signatures
Result<Order, ValidationError> Validate(CheckoutRequest req) => ...
Result<Order, InventoryError>  ReserveInventory(Order order) => ...
Result<Order, PaymentError>    ProcessPayment(Order order)   => ...
Result<Order, DatabaseError>   CreateOrder(Order order)      => ...

// Pipeline — union grows: E1 → E1,E2 → E1,E2,E3 → E1,E2,E3,E4
Result<Order, ErrorsOf<ValidationError, InventoryError, PaymentError, DatabaseError>>
Checkout(CheckoutRequest request) =>
    Validate(request)
        .Bind(ReserveInventory)
        .Bind(ProcessPayment)
        .Bind(CreateOrder);

// Callsite — exhaustive match, compile-time safe
var result = Checkout(request);
result.Error.Match(
    v => HandleValidation(v),
    i => HandleInventory(i),
    p => HandlePayment(p),
    d => HandleDatabase(d));
```

---

## Operations Table

| Method | Effect |
|--------|--------|
| `Bind(next)` ×7 | Chain step, grow union by one slot |
| `Map(mapper)` | Transform value, error type unchanged |
| `Tap(action)` | Side effect on success, returns original result |
| `TapOnFailure(action)` | Side effect on failure, returns original result |
| `Ensure(pred, error)` ×7 | Guard — widen union by one slot if predicate fails |
| `EnsureAsync(pred, error)` ×7 | Async guard — same widening |
| `MapError(mapper)` | Translate error surface (`TErrorIn → TErrorOut`) |

---

## `Ensure` — Guard with Union Widening

`Ensure` adds a guard condition. Each call widens the error union by one slot when the predicate fails:

```csharp
Result<Order, ValidationError> Validate(CheckoutRequest req) => ...

// First Ensure: Result<Order, ValidationError>
//            → Result<Order, ErrorsOf<ValidationError, CreditLimitError>>
var guarded = Validate(request)
    .Ensure(order => order.Amount > 0,      new CreditLimitError("Amount must be positive"))
    .Ensure(order => order.Amount < 10_000, new CreditLimitError("Amount exceeds limit"));

// EnsureAsync — predicate is async
var asyncGuarded = await result
    .EnsureAsync(order => CheckCreditAsync(order), new CreditLimitError("Credit check failed"));
```

---

## `MapError` — Layer-Boundary Translation

Collapse or translate the error union at layer boundaries — e.g. convert a domain union into a single API error type:

```csharp
// Collapse union into a single domain error for the HTTP layer
Result<Order, DomainError> adapted = result.MapError(e => e.Match(
    v => new DomainError(v.Message),
    i => new DomainError(i.Message),
    p => new DomainError(p.Message),
    d => new DomainError(d.Message)));
```

---

## `Result.Flow` — Type-Read Mode

When a `[ResultFlow]`-annotated method returns `Result<T, TError>`, failure edges in the generated Mermaid diagram show the **exact error type** — no body scanning needed. The generator reads `TypeArguments[1]` directly from the Roslyn return type symbol.

```
N0_Bind["Bind<br/>Order → Order"]:::transform
N0_Bind -->|"fail: ErrorsOf&lt;ValidationError, InventoryError&gt;"| F0["Failure"]:::failure
```

See the [ResultFlow](/resultflow/index.md#resultflow) section for the full `[ResultFlow]` / Mermaid diagram feature.