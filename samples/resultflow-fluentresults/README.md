# ResultFlow + FluentResults — Library-Agnostic Pipeline Sample

> **REslava.ResultFlow is library-agnostic.**
> It works with [FluentResults](https://github.com/altmann/FluentResults), [ErrorOr](https://github.com/amantinband/error-or), [LanguageExt](https://github.com/louthy/language-ext), or any fluent Result library.
> This sample uses FluentResults — no dependency on REslava.Result whatsoever.

---

## What is ResultFlow?

`REslava.ResultFlow` is a **Roslyn source generator** that walks your fluent pipeline chain at compile time and emits a **Mermaid flowchart** as a `const string` — zero runtime cost, zero manual maintenance.

It recognises `Bind` and `Map` from its built-in convention dictionary — **zero configuration required**.

```csharp
[ResultFlow]
public static Result<Order> PlaceOrder(int userId, int productId) =>
    FindUser(userId)                      // → Result<User>
        .Bind(_ => FindProduct(productId))  // → Result<Product>  label: "Bind<br/>User → Product"
        .Bind(p  => BuildOrder(userId, p)); // → Result<Order>    label: "Bind<br/>Product → Order"
```

Generated diagram (with type travel since v1.38.0):

```
flowchart LR
    N0_Bind["Bind<br/>User → Product"]:::transform
    N0_Bind -->|ok| N1_Bind
    N0_Bind -->|fail| F0["Failure"]:::failure
    N1_Bind["Bind<br/>Product → Order"]:::transform
    N1_Bind -->|fail| F1["Failure"]:::failure
    classDef transform fill:#e3f0e8,color:#2f7a5c
    classDef failure fill:#f8e3e3,color:#b13e3e
```

Paste into [mermaid.live](https://mermaid.live) to visualize the pipeline instantly.

---

## Node colours

| Colour | Role | Methods |
|--------|------|---------|
| Mint | **TransformWithRisk** | `Bind` — can fail |
| Mint | **PureTransform** | `Map` — always succeeds |
| Pink | **Failure** | failure exit branches |

---

## Run

```bash
dotnet run
```

Output: all 3 pipeline diagrams printed to the terminal, followed by runtime verification of 8 test cases. Copy any diagram into [mermaid.live](https://mermaid.live).

## What this sample covers

| Pipeline | Methods | Demonstrates |
|----------|---------|--------------|
| `PlaceOrder` | `Bind` × 2 | Risk chain — type travel User → Product → Order |
| `ProcessCheckout` | `Bind` × 2 + `Map` | Full type travel User → Product → Order → string |
| `ValidateAndPlace` | `Bind` × 4 (incl. inline guards) | Guard chain via Bind — stock check + price limit |

---

## Type Travel (v1.38.0)

Each node label shows the FluentResults `Result<T>` success type at that step:

```
Bind<br/>User → Product    ← type changed: User in, Product out
Bind<br/>Product            ← type unchanged (inline guard)
Bind<br/>Product → Order    ← type changed again
```

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
dotnet add package REslava.ResultFlow
dotnet add package FluentResults
```

Add `using REslava.ResultFlow;` and annotate any fluent method with `[ResultFlow]`:

```csharp
using REslava.ResultFlow;

[ResultFlow]
public static Result<Order> PlaceOrder(int userId, int productId) => ...
```

---

## Adding custom method names

The built-in convention dictionary covers `Bind` and `Map` out of the box. To add library-specific names, create a `resultflow.json` in your project root:

```json
{
  "bind": ["Then", "ThenDo"],
  "map":  ["Select"]
}
```

---

## Which package should I use?

| | `REslava.ResultFlow` | `REslava.Result.Flow` |
|---|---|---|
| **Library** | Any (FluentResults, ErrorOr, LanguageExt, …) | REslava.Result only |
| **Type Travel** | ✅ (since v1.38.0) | ✅ |
| **Typed Error edges** | ❌ | ✅ |
| **Node kinds** | Bind / Map (+ `resultflow.json` extensions) | All 5 kinds |

> **Use this package when you are NOT using REslava.Result.** If your project uses REslava.Result, prefer [`REslava.Result.Flow`](../result-flow/) for richer diagrams with typed error edges and more node kinds.
