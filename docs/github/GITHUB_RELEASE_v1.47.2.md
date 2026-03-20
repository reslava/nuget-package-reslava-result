# REslava.Result v1.47.2

## Show All Clearly — Async Strip, Bind/Map Split, Title Types, Legend, Predicate Tooltips

Every `[ResultFlow]` diagram is now more readable at a glance. Five polish improvements shipped together.

### Async suffix strip

Node labels and frontmatter titles no longer repeat the word `Async` — the `⚡` marker already communicates that. Every async node tightens up:

```
Before: FindProductAsync ⚡
After:  FindProduct⚡
```

Both the node label and the frontmatter `title:` line are updated. Applies to all async nodes and the chain seed call.

---

### Bind vs Map visual distinction

`Bind` and `Map` now render with distinct styles — same green family, clearly different weight:

| Node kind | classDef | Style |
|---|---|---|
| `Bind` / `Or` / `OrElse` / `MapError` | `:::bind` | Thick dark-green border (`stroke:#1a5c3c,stroke-width:3px`) |
| `Map` / `MapAsync` | `:::map` | Plain green, no border |
| Subgraph border | `:::transform` | Unchanged (subgraph use only) |

Previously both used `:::transform`. The split makes it immediately obvious where the pipeline can fail (Bind) vs where it simply transforms (Map).

---

### Title type annotation

The frontmatter title now includes the output type of the pipeline:

```
---
title: PlaceOrder⚡ → ⟨Order⟩
---
```

Rules:
- Async suffix stripped (`PlaceOrder⚡` not `PlaceOrderAsync`)
- Output type is the value side of the last visible node's return type
- `Result<T,TError>` → shows `T` only (TError is on the fail edges)
- Non-generic pipeline (`Result`) → `→ ⟨⟩`

---

### `Legend` constant

One `Legend` constant is now emitted per `*_Flows` class (not per method). It contains a compact Mermaid diagram showing all 9 node types — shapes, colors, and the entry-arrow symbol — in a horizontal row:

```csharp
// Access at runtime:
Console.WriteLine(OrderService_Flows.Legend);
```

Paste into any Mermaid renderer to review the complete visual vocabulary at a glance.

---

### Gatekeeper predicate tooltip

`Ensure` and `Filter` nodes now embed the predicate text as a `<span title='...'>` HTML tooltip when the first argument is a lambda:

```csharp
.Ensure(p => p.Stock > 0, new OutOfStockError())
// → node label: <span title='p.Stock > 0'>Ensure</span>
```

The tooltip is visible on hover in VS Code Markdown Preview and in Mermaid.js. GitHub strips HTML in Mermaid SVG — the node still renders correctly, the tooltip just isn't visible there.

---

## Stats

- Tests: 4,648 passing (floor: >4,500)
- Features: 197 across 15 categories

---

## NuGet Packages

| Package | Link |
|---|---|
| REslava.Result | [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.47.2) |
| REslava.Result.Flow | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Flow/1.47.2) |
| REslava.Result.AspNetCore | [View on NuGet](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.47.2) |
| REslava.Result.Http | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Http/1.47.2) |
| REslava.Result.Analyzers | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.47.2) |
| REslava.Result.OpenTelemetry | [View on NuGet](https://www.nuget.org/packages/REslava.Result.OpenTelemetry/1.47.2) |
| REslava.ResultFlow | [View on NuGet](https://www.nuget.org/packages/REslava.ResultFlow/1.47.2) |
| REslava.Result.FluentValidation | [View on NuGet](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.47.2) |
