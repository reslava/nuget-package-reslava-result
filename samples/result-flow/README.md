# Result.Flow — Pipeline Visualization Sample (REslava.Result-native)

> **REslava.Result.Flow is the REslava.Result-native source generator.**
> It uses `IResultBase` and `IError` as Roslyn anchors to produce richer diagrams than the library-agnostic `REslava.ResultFlow`.
> If your project uses a different Result library (ErrorOr, FluentResults, LanguageExt), use [`REslava.ResultFlow`](../resultflow/) instead.

---

## What is Result.Flow?

`REslava.Result.Flow` is a **Roslyn source generator** that walks your fluent pipeline chain at compile time and emits a **Mermaid flowchart** as a `const string` — zero runtime cost, zero manual maintenance.

What makes it richer than the library-agnostic `REslava.ResultFlow`:

- **Type Travel** — every node label shows the success type flowing through it
  - `"Bind<br/>User → Product"` when the type changes
  - `"Ensure<br/>Order"` when the type is unchanged
- **Typed Error edges** — failure edges are labelled with the actual `IError` subtype
  - `"-->|UserNotFoundError| FAIL"` instead of the generic `"-->|fail| FAIL"`

```csharp
[ResultFlow]
public static Result<Order> PlaceOrder(int userId, int productId) =>
    FindUser(userId)                  // → Result<User>
        .Bind(_ => FindProduct(productId))  // → Result<Product>  label: "Bind<br/>User → Product"
        .Bind(p  => BuildOrder(userId, p)); // → Result<Order>    label: "Bind<br/>Product → Order"
```

Generated diagram (type travel + typed error edges):

```
flowchart LR
    N0_Bind["Bind<br/>User → Product"]:::transform
    N0_Bind -->|ok| N1_Bind
    N0_Bind -->|UserNotFoundError| F0["Failure"]:::failure
    N1_Bind["Bind<br/>Product → Order"]:::transform
    N1_Bind -->|ProductNotFoundError| F1["Failure"]:::failure
```

Paste into [mermaid.live](https://mermaid.live) to visualize the pipeline instantly.

---

## Node colours

| Colour | Role | Methods |
|--------|------|---------|
| Teal | **Ok seed** | `Result<T>.Ok(...)` |
| Lavender | **Gatekeeper** | `Ensure`, `EnsureAsync`, `Filter` |
| Mint | **Transform** | `Bind`, `BindAsync`, `Map`, `MapAsync`, `Or`, `OrElse` |
| Vanilla | **Side effect** | `Tap`, `TapAsync`, `TapOnFailure`, `TapBoth`, `MapError` |
| Pink | **Failure** | failure exit branches |
| White | **Terminal** | `Match`, `MatchAsync`, `Switch` |

---

## Run

```bash
dotnet run
```

Output: all 5 pipeline diagrams printed to the terminal, followed by runtime verification of all test cases. Copy any diagram into [mermaid.live](https://mermaid.live).

## What this sample covers

| Pipeline | Methods | Demonstrates |
|----------|---------|--------------|
| `ValidateOrder` | `Ok` + `Ensure` × 3 | Guard chain — `Ok` seed node, typed errors on Ensure |
| `PlaceOrder` | `Bind` × 2 | Risk chain — type travel User → Product → Order, typed error edges |
| `ProcessCheckout` | `Bind` × 2 + `Map` | Full type travel User → Product → Order → string |
| `PlaceOrderAsync` | `BindAsync` + `EnsureAsync` + `MapAsync` | Async pipeline — ⚡ markers, type travel, typed errors |
| `AdminCheckout` | `Ensure` + `BindAsync` × 2 + `EnsureAsync` + `TapAsync` + `MapAsync` | All node kinds, async, end-to-end type travel |

---

## Custom domain errors — typed failure edges

Typed error edges appear automatically when your lambda bodies return concrete `IError` subclasses:

```csharp
sealed class UserNotFoundError(int id)     : NotFoundError($"User {id} not found") { }
sealed class ProductNotFoundError(int id)  : NotFoundError($"Product {id} not found") { }
sealed class OutOfStockError(string name)  : ValidationError("stock", $"'{name}' is out of stock") { }
```

The generator scans lambda / method bodies, finds `new UserNotFoundError(...)`, and emits:

```
N0_Bind -->|UserNotFoundError| F0["Failure"]:::failure
```

No annotation required — just return a concrete `IError` instance.

---

## How the constant is accessed

```csharp
using Generated.ResultFlow;

// Class: Pipelines  →  Generated class: Pipelines_Flows
// Method: PlaceOrder →  Constant:       Pipelines_Flows.PlaceOrder

Console.WriteLine(Pipelines_Flows.PlaceOrder);
```

The constant is a `const string` — embedded in the assembly at compile time with **zero runtime overhead**.

---

## Install

```bash
dotnet add package REslava.Result.Flow
```

Add `using REslava.Result.Flow;` and annotate any fluent method with `[ResultFlow]`:

```csharp
using REslava.Result.Flow;

[ResultFlow]
public static Result<Order> PlaceOrder(int userId, int productId) => ...
```

---

## Which package should I use?

| | `REslava.ResultFlow` | `REslava.Result.Flow` |
|---|---|---|
| **Library** | Any (FluentResults, ErrorOr, LanguageExt, …) | REslava.Result only |
| **Type Travel** | ✅ (since v1.38.0) | ✅ |
| **Typed Error edges** | ❌ | ✅ |
| **Node kinds** | Bind / Map only (+ `resultflow.json` extensions) | All 5 kinds (Ok, Gatekeeper, Transform, SideEffect, Terminal) |

> **Choose `REslava.Result.Flow` when you use REslava.Result** — it produces richer diagrams with no extra configuration.
