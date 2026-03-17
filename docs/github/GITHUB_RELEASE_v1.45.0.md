# REslava.Result v1.45.0

## Domain Boundary Diagrams — architecture visualization across layers

`[DomainBoundary]` annotation triggers four new diagram constants that reveal the architectural picture of your Result pipeline: layer view, stats, error surface, and error propagation map.

---

## What's New

### Domain Boundary Diagrams (`REslava.Result.Flow`)

Apply `[DomainBoundary("LayerName")]` to a class or method and `[ResultFlow]` generates four additional constants alongside `_Diagram`:

| Constant | Type | What it shows |
|---|---|---|
| `_LayerView` | `flowchart TD` | Layer → Class → Method subgraphs, color-coded by layer |
| `_Stats` | Markdown table | Step count, async steps, error types, layers crossed, max depth |
| `_ErrorSurface` | `flowchart LR` | Fail-edges only — the complete error surface at a glance |
| `_ErrorPropagation` | `flowchart TD` | Error types grouped by the layer they originate from |

```csharp
[DomainBoundary("Application")]
static class OrderService
{
    [ResultFlow(MaxDepth = 2)]
    public static Result<Order> PlaceOrder(int userId, int productId) => ...
}

// Generated at compile time:
Console.WriteLine(OrderService_Flows.PlaceOrder_LayerView);
Console.WriteLine(OrderService_Flows.PlaceOrder_Stats);
Console.WriteLine(OrderService_Flows.PlaceOrder_ErrorSurface);
Console.WriteLine(OrderService_Flows.PlaceOrder_ErrorPropagation);
```

### Automatic Layer Detection

`LayerDetector` resolves the architectural layer for each pipeline node using a priority chain:
1. `[DomainBoundary]` method attribute (explicit — highest priority)
2. `[DomainBoundary]` class attribute (applies to all methods in the class)
3. Namespace heuristics: `*.Domain.*` → Domain, `*.Application.*` / `*.UseCases.*` → Application, `*.Infrastructure.*` / `*.Repositories.*` → Infrastructure, `*.Controllers.*` / `*.Presentation.*` → Presentation

### `[DomainBoundary]` on Classes (`REslava.Result`)

`AttributeUsage` extended to include `AttributeTargets.Class`. Annotating a class applies the layer to all its methods — method-level annotation always takes priority.

### Layer Subgraph Coloring

Both `_LayerView` and `_ErrorPropagation` emit Mermaid `class {SubgraphId} {classDef}` directives to color outer layer subgraph containers:
- Application → green
- Domain → amber
- Infrastructure → purple
- Presentation → blue

Class-level subgraphs remain neutral (grey) — intentional three-tier visual hierarchy.

### README Hero — Two Contrasting Diagrams

The README hero now shows two diagrams: `_LayerView` (`flowchart TD`) and `_Diagram` (`flowchart LR`).

### Public Diagram Gallery

New [`mkdocs/resultflow/diagrams/`](https://reslava.github.io/nuget-package-reslava-result/resultflow/diagrams/) page showcasing all diagram types on the same `PlaceOrder` scenario.

### NuGet README Diagrams

`REslava.Result` and `REslava.Result.Flow` NuGet READMEs now include rendered Mermaid diagrams — visible directly on NuGet.org.

---

## Stats

- Tests: ~4,680 passing (floor: >4,500)
- Features: 192 across 15 categories

---

## NuGet Packages

| Package | Link |
|---|---|
| REslava.Result | [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.45.0) |
| REslava.Result.Flow | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Flow/1.45.0) |
| REslava.Result.AspNetCore | [View on NuGet](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.45.0) |
| REslava.Result.Http | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Http/1.45.0) |
| REslava.Result.Analyzers | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.45.0) |
| REslava.Result.OpenTelemetry | [View on NuGet](https://www.nuget.org/packages/REslava.Result.OpenTelemetry/1.45.0) |
| REslava.ResultFlow | [View on NuGet](https://www.nuget.org/packages/REslava.ResultFlow/1.45.0) |
| REslava.Result.FluentValidation | [View on NuGet](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.45.0) |
