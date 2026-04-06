# REslava.Result v1.54.0

## ✨ What's New

### 🔍 Error Taxonomy Generator

Both `REslava.Result.Flow` and `REslava.ResultFlow` now emit a `_ErrorTaxonomy` constant at compile time — a markdown table of every detectable error type per method. No attribute or opt-in required: any class with at least one `[ResultFlow]` method gets it automatically.

**Two confidence levels:**

| Source | Confidence |
|---|---|
| `Result<T, TError>` return type | `certain` |
| `Fail(new XxxError(...))` in method body | `inferred` |
| `Ensure(..., new XxxError(...))` in method body | `inferred` |

```csharp
public partial class OrderService
{
    [ResultFlow]
    public Result<Order, NotFoundError> GetOrder(int id) => ...;

    [ResultFlow]
    public Result<Order> PlaceOrder(OrderRequest req) =>
        Result<Order>.Ok(new Order())
            .Bind(ValidateRequest)
            .Ensure(HasStock, new OutOfStockError("no stock"))
            .Map(FulfillOrder);
}
```

Generated in `Generated.ErrorTaxonomy.OrderService_ErrorTaxonomy`:

```csharp
public const string _ErrorTaxonomy = @"
| Method | Error Type | Confidence |
|---|---|---|
| GetOrder | NotFoundError | certain |
| PlaceOrder | OutOfStockError | inferred |
| PlaceOrder | ValidationError | inferred |
";
```

`REslava.ResultFlow` (syntax-only / library-agnostic) uses a name heuristic: types ending in `Error` or `Exception` are matched instead of checking the `IError` interface.

---

### 🛠️ Debug Panel — nodeId Subchain Fix

**Generator fix** — In `MaxDepth > 0` pipelines, inner method calls (e.g. `ValidateUser`) run their own hooks on the **same** `PipelineState`. The outer `_nodeIds_` array previously only listed outer chain steps, so inner hooks consumed `NodeIndex` slots silently and outer steps fell back to `fd319c15:3`-style fallback IDs.

The generator now emits inner nodeIds in execution order:

```
_nodeIds_PlaceOrderCross:
  "6e5097fb"  ← outer Bind → ValidateUser (subgraph)
  "879c46d8"  ← inner Bind: IsActive check
  "c882fd49"  ← inner Bind: Role check
  "3d5813e1"  ← FindProduct
  "eefef016"  ← Map
```

---

### ⚡ VSIX v1.4.1 — Debug Panel Polish

- **Subgraph ENTRY highlight** — stepping into a cross-method node now highlights `ENTRY_{nodeId}` (the actual Mermaid node inside the subgraph) rather than emitting a no-op `class` directive on the subgraph container
- **Sort by `nodeIndex`** — nodes sorted on arrival; JSON completion order ≠ execution order for cross-method pipelines
- **Node output word-wrap** — output values no longer cut off at panel width; `word-break: break-word` lets long values wrap naturally
- **Debug toolbar button fix** — was calling unregistered `resultflow.openLivePanel`; now correctly calls `resultflow.openDebugPanel`
- **File picker sync** — watcher auto-load passes the loaded file as `selectedPath` so the picker reflects the active file

---

## 🧪 Tests

**Total: 4,957 tests passing**

---

## NuGet Packages

| Package | Version |
|---------|---------|
| [REslava.Result](https://www.nuget.org/packages/REslava.Result/1.54.0) | 1.54.0 |
| [REslava.Result.Flow](https://www.nuget.org/packages/REslava.Result.Flow/1.54.0) | 1.54.0 |
| [REslava.Result.AspNetCore](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.54.0) | 1.54.0 |
| [REslava.Result.Http](https://www.nuget.org/packages/REslava.Result.Http/1.54.0) | 1.54.0 |
| [REslava.Result.Analyzers](https://www.nuget.org/packages/REslava.Result.Analyzers/1.54.0) | 1.54.0 |
| [REslava.Result.OpenTelemetry](https://www.nuget.org/packages/REslava.Result.OpenTelemetry/1.54.0) | 1.54.0 |
| [REslava.Result.Diagnostics](https://www.nuget.org/packages/REslava.Result.Diagnostics/1.54.0) | 1.54.0 |
| [REslava.ResultFlow](https://www.nuget.org/packages/REslava.ResultFlow/1.54.0) | 1.54.0 |
| [REslava.Result.FluentValidation](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.54.0) | 1.54.0 |

## VS Code Extension

| Extension | Version |
|-----------|---------|
| [REslava.Result Extensions](https://marketplace.visualstudio.com/items?itemName=reslava.reslava-result-extensions) | v1.4.1 |
