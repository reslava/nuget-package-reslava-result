# REslava.Result v1.53.0

## ✨ What's New

### 🔀 FlowProxy — Always-On Tracing + Debug Mode

`partial class` is now the entry point for pipeline observation. The generator emits two nested proxy types — no call-site changes needed after adding `partial`.

**`REF004` analyzer** fires when `[ResultFlow]` methods are in a non-`partial` class. The code fix adds `partial` automatically.

```csharp
// Add partial — REF004 code fix does this for you
public partial class OrderService
{
    [ResultFlow]
    public Result<Order> PlaceOrder(OrderRequest req) =>
        Result<Order>.Ok(new Order())
            .Bind(ValidateRequest)
            .Ensure(r => r.Stock > 0, "Out of stock")
            .Map(FulfillOrder);
}
```

**`svc.Flow.PlaceOrder(req)`** — always-on: seeds the observer on every call; zero overhead when no observer registered.

**`svc.Flow.Debug.PlaceOrder(req)`** — single-trace mode: captures one execution, saves to `reslava-debug-PlaceOrder.json` in the output directory, then returns normally.

**Static pattern** (service without DI):

```csharp
var flow = new OrderServiceFlowProxy(svc);
var debug = new OrderServiceDebugProxy(svc);
```

> `_Traced` extension methods are **removed** in v1.53.0. Replace `svc.PlaceOrder_Traced(req)` with `svc.Flow.PlaceOrder(req)`.

---

### 💾 Debug Panel — File-Drop Workflow + Multi-File Picker

**`RingBufferObserver.Save(path?)`** — write the ring buffer to disk at any point:

| Call | Output path |
|------|-------------|
| `buf.Save()` | `reslava-traces.json` in `AppContext.BaseDirectory` |
| `buf.Save("orders")` | `reslava-orders.json` in `AppContext.BaseDirectory` |
| `buf.Save("debug-PlaceOrder.json")` | `reslava-debug-PlaceOrder.json` (auto-prefixed) |
| `buf.Save("/tmp/my.json")` | `/tmp/my.json` (exact path, escape hatch) |

`DebugProxy` calls `buf.Save("debug-{method}.json")` automatically — each method writes a separate file so multiple demo sections never overwrite each other.

**VSIX v1.4.0 — Debug panel upgrades:**

- **File watcher** — now watches `**/reslava-*.json` (was `**/reslava-traces.json`); picks up all method-named debug files
- **Multi-file picker** — when >1 `reslava-*.json` file exists, a picker bar appears; labels strip the `reslava-` prefix; switching selection reloads the panel
- **Source badge** — panel header shows ⏳ (no data), 📄 (file), or 🌐 (HTTP) to indicate the active data source
- **"Live Panel" → "Debug panel"** — renamed throughout (command `resultflow.openDebugPanel`, title, CodeLens tooltip, sidebar button)

---

## 🗑️ Breaking Change

**`_Traced` removed** — the `{MethodName}_Traced` generated extension is gone. Migrate:

```csharp
// Before
var result = svc.PlaceOrder_Traced(req);

// After — always-on (same overhead profile)
var result = svc.Flow.PlaceOrder(req);

// After — single-trace debug
var result = svc.Flow.Debug.PlaceOrder(req);
```

---

## 🧪 Tests

**Total: 4,938 tests passing**

---

## NuGet Packages

| Package | Version |
|---------|---------|
| [REslava.Result](https://www.nuget.org/packages/REslava.Result/1.53.0) | 1.53.0 |
| [REslava.Result.Flow](https://www.nuget.org/packages/REslava.Result.Flow/1.53.0) | 1.53.0 |
| [REslava.Result.AspNetCore](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.53.0) | 1.53.0 |
| [REslava.Result.Http](https://www.nuget.org/packages/REslava.Result.Http/1.53.0) | 1.53.0 |
| [REslava.Result.Analyzers](https://www.nuget.org/packages/REslava.Result.Analyzers/1.53.0) | 1.53.0 |
| [REslava.Result.OpenTelemetry](https://www.nuget.org/packages/REslava.Result.OpenTelemetry/1.53.0) | 1.53.0 |
| [REslava.Result.Diagnostics](https://www.nuget.org/packages/REslava.Result.Diagnostics/1.53.0) | 1.53.0 |
| [REslava.ResultFlow](https://www.nuget.org/packages/REslava.ResultFlow/1.53.0) | 1.53.0 |
| [REslava.Result.FluentValidation](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.53.0) | 1.53.0 |

## VS Code Extension

| Extension | Version |
|-----------|---------|
| [REslava.Result Extensions](https://marketplace.visualstudio.com/items?itemName=reslava.reslava-result-extensions) | v1.4.0 |
