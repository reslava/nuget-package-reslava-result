# REslava.Result.Flow — Feature Demo

A runnable console app showcasing every `REslava.Result.Flow` feature. Run it, read the generated diagrams, and verify runtime behaviour — all in one go.

```bash
dotnet run
```

---

## Features demonstrated

| # | Feature | Version |
|---|---------|---------|
| 1 | Guard chain — `Ensure × 3` with typed error edges | v1.37.0 |
| 2 | Risk chain — `Bind × 2` with type travel | v1.38.0 |
| 3 | Multi-hop type travel — `User → Product → Order → string` | v1.38.0 |
| 4 | Async pipeline — `⚡` step markers + typed errors | v1.38.0 |
| 5 | Full pipeline — all NodeKinds end-to-end | v1.38.0 |
| 6 | Cross-method tracing — `[ResultFlow(MaxDepth = 2)]` expands `Bind` into a `subgraph` | v1.45.0 |
| 7 | Sidecar constant — writes `PlaceOrderCross.ResultFlow.md` to disk | v1.43.0 |
| 8 | Clickable nodes — `ResultFlowLinkMode = vscode` | v1.43.0 |
| 9 | Domain boundary diagrams — `[DomainBoundary]` triggers `_LayerView`, `_Stats`, `_ErrorSurface`, `_ErrorPropagation` | v1.45.0 |
| 10 | Match multi-branch fan-out — hexagon + typed N-branch `-->|TypeName| FAIL` edges | v1.46.0 |
| 11 | Architectural Flow Catalog — all generated diagrams published to MkDocs via `scripts/generate_flow_catalog.py` | v1.47.0 |

---

## NodeKind colour reference

| NodeKind | `classDef` | Colour | Role |
|---|---|---|---|
| Root / entry | `operation` | amber `#fef0e3` | First step / unclassified entry point |
| `Bind`, `Map`, `Or` | `transform` | green `#e3f0e8` | Success-path transforms |
| `Ensure`, `Filter` | `gatekeeper` | blue `#e3e9fa` | Conditional pass/fail |
| `Tap`, `TapOnFailure` | `sideeffect` | yellow `#fff4d9` | Side-effects |
| `Match` | `terminal` | purple `#f2e3f5` | Pipeline endpoint |
| `FAIL` node | `failure` | red `#f8e3e3` | Error sink |

---

## Cross-method tracing (section 6)

`[ResultFlow(MaxDepth = 2)]` causes the generator to follow `Bind` lambdas into called methods and stitch their pipelines as Mermaid `subgraph` blocks:

```csharp
[ResultFlow(MaxDepth = 2)]
public static Result<Order> PlaceOrderCross(int userId, int productId) =>
    FindUser(userId)
        .Bind(u => ValidateUser(u))        // ← generator traces into ValidateUser
        .Bind(_ => FindProduct(productId))
        .Map(p  => new Order(0, userId, p.Price));
```

The generated diagram expands `ValidateUser` as a named `subgraph` connected to the outer pipeline with `-->|ok|`.

`MaxDepth` controls recursion depth (default: 2). Set `MaxDepth = 0` to disable cross-method tracing.

Both simple (`x => ValidateUser(x)`) and qualified (`x => SomeClass.Method(x)`) lambda calls are traced.

---

## Sidecar constant (section 7)

Every `[ResultFlow]` method generates a `{MethodName}_Sidecar` constant alongside the diagram — the same Mermaid diagram wrapped in a fenced markdown block. Write it to disk for GitHub rendering or VS Code preview:

```csharp
File.WriteAllText("PlaceOrderCross.ResultFlow.md", OrderService_Flows.PlaceOrderCross_Sidecar);
// Open in VS Code (Ctrl+Shift+V) → renders the diagram immediately
```

---

## Clickable nodes (section 8)

Add to `.csproj` to make every Mermaid node a clickable link that opens the source line in VS Code:

```xml
<PropertyGroup>
  <ResultFlowLinkMode>vscode</ResultFlowLinkMode>
</PropertyGroup>
```

Clicking a node in the VS Code Mermaid preview (`Ctrl+Shift+V`) jumps directly to the method call in the source file.

---

## Domain boundary diagrams (section 9)

Apply `[DomainBoundary("LayerName")]` to a class or method to assign it to an architectural layer. The generator then produces four additional constants alongside `_Diagram`:

| Constant | Type | What it shows |
|---|---|---|
| `_LayerView` | `flowchart TD` | Layer → Class → Method subgraphs, color-coded by layer |
| `_Stats` | Markdown table | Step count, async steps, error types, layers crossed, max depth |
| `_ErrorSurface` | `flowchart LR` | Fail-edges only — the full failure surface at a glance |
| `_ErrorPropagation` | `flowchart TD` | Error types grouped by the layer they originate from |

```csharp
[DomainBoundary("Domain")]
static class UserService { ... }

[DomainBoundary("Application")]
static class OrderService
{
    [ResultFlow(MaxDepth = 2)]
    public static Result<Order> PlaceOrderCross(...) => ...
}

// Generated:
Console.WriteLine(OrderService_Flows.PlaceOrderCross_LayerView);
Console.WriteLine(OrderService_Flows.PlaceOrderCross_Stats);
Console.WriteLine(OrderService_Flows.PlaceOrderCross_ErrorSurface);
Console.WriteLine(OrderService_Flows.PlaceOrderCross_ErrorPropagation);
```

`[DomainBoundary]` can be placed on a **class** (applies to all its methods) or on a **method** directly (higher priority). If absent, the generator falls back to namespace heuristics (`*.Domain.*` → Domain, `*.Application.*` → Application, etc.).

---

## Install

```bash
dotnet add package REslava.Result.Flow
```

```csharp
using REslava.Result.Flow;

[ResultFlow]
public static Result<Order> PlaceOrder(int userId, int productId) => ...
```

Generated constant available at `Generated.ResultFlow.{ClassName}_Flows.{MethodName}`.

---

## Architectural Flow Catalog (section 11)

All diagrams generated from this demo are published to the live MkDocs documentation site via `scripts/generate_flow_catalog.py`. The script scans `obj/Generated/**/*_Flows.g.cs`, extracts every Mermaid constant, and writes `mkdocs/reference/flow-catalog/index.md` — regenerated automatically on every release.

Run it manually against any project:

```bash
python scripts/generate_flow_catalog.py --project path/to/MyProject --output path/to/output.md
```

---

## Which package?

| | `REslava.ResultFlow` | `REslava.Result.Flow` |
|---|---|---|
| **Works with** | Any Result library | REslava.Result only |
| **Type travel** | ✅ | ✅ |
| **Typed error edges** | ❌ | ✅ |
| **Cross-method tracing** | ✅ (syntax) | ✅ (semantic) |
| **Sidecar constant** | ✅ | ✅ |
| **Clickable nodes** | ✅ | ✅ |
| **`_LayerView`** | ❌ | ✅ |
| **`_Stats`** | ❌ | ✅ |
| **`_ErrorSurface`** | ❌ | ✅ |
| **`_ErrorPropagation`** | ❌ | ✅ |
| **Match typed N-branch** | ❌ (generic 2-branch) | ✅ |
