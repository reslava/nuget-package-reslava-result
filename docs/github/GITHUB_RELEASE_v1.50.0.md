# REslava.Result v1.50.0

## ✨ What's New

### 🗂️ Pipeline Registry Generator

Both `REslava.Result.Flow` and `REslava.ResultFlow` now ship a new always-on **`ResultFlowRegistryGenerator`** that runs alongside the existing diagram generator.

For every class with at least one `[ResultFlow]` method, the generator emits a `{ClassName}_PipelineRegistry.g.cs` file with two constants per method:

```csharp
// All pipeline method names in this class
public const string _Methods = "[\"PlaceOrder\",\"CancelOrder\"]";

// Rich metadata — deserialize with JsonSerializer
public const string PlaceOrder_Info = "{\"returnType\":\"Order\",\"nodeCount\":5,\"nodeKinds\":[\"Root\",\"Bind\",\"Gatekeeper\",\"Tap\",\"Terminal\"],\"errorTypes\":[\"ValidationError\",\"NotFoundError\"],\"hasDiagram\":true,\"sourceLine\":42}";
```

**`_Info` fields:** `returnType` · `nodeCount` · `nodeKinds` (array) · `errorTypes` (array) · `hasDiagram` · `sourceLine`

To opt out for a specific project:

```xml
<PropertyGroup>
  <ResultFlowRegistry>false</ResultFlowRegistry>
</PropertyGroup>
```

---

### ⚡ Flow Catalog Sidebar (VSIX v1.2.0)

The **REslava.Result Extensions** VS Code extension (v1.2.0) adds a dedicated **⚡ Flow Catalog** activity bar panel listing every `[ResultFlow]` method across your entire workspace — no need to navigate to a source file first.

- **Project nodes** — green when built (registry found), red when a build is needed
- **Method nodes** — show `⚡` for async, return type, node count; hover for full tooltip
- **Click a method** — opens its diagram preview instantly
- **Right-click a project** → **Build Project** — runs `dotnet build --no-incremental` and auto-refreshes
- **↺ button** — manual full workspace rescan
- **Stats bar** — total projects · pipelines · nodes above the tree

The sidebar reads `*_PipelineRegistry.g.cs` files from `obj/` — build at least once to populate it.

---

### ⊞ Single / Multiple Window Mode (VSIX v1.2.0)

Control whether diagram previews share one panel or open one per method.

- **Single mode** (default) — all pipeline previews reuse one shared panel; title updates to the current method
- **Multiple mode** — each method gets its own panel

Toggle with the **⊞** button in the sidebar toolbar or the **Single / Multi** button inside the diagram panel. Persists via the `reslava.diagramWindowMode` VS Code setting (`single` / `multiple`).

---

## 🧪 Tests

**Total: 4,688 tests passing**

---

## NuGet Packages

| Package | Version |
|---------|---------|
| [REslava.Result](https://www.nuget.org/packages/REslava.Result/1.50.0) | 1.50.0 |
| [REslava.Result.Flow](https://www.nuget.org/packages/REslava.Result.Flow/1.50.0) | 1.50.0 |
| [REslava.Result.AspNetCore](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.50.0) | 1.50.0 |
| [REslava.Result.Http](https://www.nuget.org/packages/REslava.Result.Http/1.50.0) | 1.50.0 |
| [REslava.Result.Analyzers](https://www.nuget.org/packages/REslava.Result.Analyzers/1.50.0) | 1.50.0 |
| [REslava.Result.OpenTelemetry](https://www.nuget.org/packages/REslava.Result.OpenTelemetry/1.50.0) | 1.50.0 |
| [REslava.ResultFlow](https://www.nuget.org/packages/REslava.ResultFlow/1.50.0) | 1.50.0 |
| [REslava.Result.FluentValidation](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.50.0) | 1.50.0 |

## VS Code Extension

| Extension | Version |
|-----------|---------|
| [REslava.Result Extensions](https://marketplace.visualstudio.com/items?itemName=reslava.reslava-result-extensions) | v1.2.0 |
