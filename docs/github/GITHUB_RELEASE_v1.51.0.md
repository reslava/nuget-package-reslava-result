# REslava.Result v1.51.0

## ✨ What's New

### 🔑 Pipeline Identity — PipelineId, NodeId & Namespace

Every `[ResultFlow]` method's registry `_Info` now includes three stable identity fields:

- **`pipelineId`** — deterministic 8-hex-char FNV-1a hash of `(fullyQualifiedType, assembly, methodName, paramTypes)`; stable across builds
- **`nodeIds`** — ordered array of per-node identity hashes; each = `ShortHash(pipelineId, type, method, params, index)`
- **`namespace`** — fully qualified namespace of the containing class

These fields enable a direct link between a runtime `ReasonMetadata` observation and the corresponding node in the generated Mermaid diagram — without any source-file look-up.

Both `REslava.Result.Flow` and `REslava.ResultFlow` packages.

---

### 🔀 `_TypeFlow` Constant

Every `[ResultFlow]` method now generates a second Mermaid constant alongside the main diagram:

```csharp
// Main diagram — structural view
Console.WriteLine(Pipelines_Flows.PlaceOrder);
// → N0 -->|ok| N1 -->|ok| SUCCESS

// Type-flow diagram — type travel visible on edges
Console.WriteLine(Pipelines_Flows.PlaceOrder_TypeFlow);
// → N0 -->|Result<User>| N1 -->|Result<Order>| SUCCESS
```

Useful for type-level documentation, design reviews, and IDE hover previews.

Both packages.

---

### 🏥 VSIX v1.2.1 → v1.2.2 — Health Icon, Namespace Grouping & Loading Indicator

- **Health icon** per method in the ⚡ Flow Catalog tree: ✅ has diagram + declared errors · ⚠️ has diagram, no errors · ❌ no diagram
- **Namespace grouping** — classes grouped under intermediate namespace nodes (top two segments) for large-workspace navigation
- **Loading indicator** — project node shows `$(loading~spin)` while `dotnet build` is running

---

### 🔧 Node SourceLine Fix

Per-node `sourceLine` in `click` directives and registry `_Info` now uses `MemberAccessExpressionSyntax.Name.GetLocation()` — fixing off-by-one on vertical fluent chains. Both packages.

---

### 🔴 FAIL Node Error Annotation

The `FAIL` node in generated Mermaid diagrams now shows the actual error types that can reach it.

**`errorTypes` depth-2 scan** — the generator descends into directly-called methods up to depth 2 when collecting error types. A delegate like `.Bind(o => ValidateOrder(o))` will scan `ValidateOrder`'s body for error constructions. Unconditional and independent of `MaxDepth`. Both packages.

**Inline (1–3 errors):** `FAIL(["fail\nNotFound\nValidation"])` — `Error` suffix stripped, newline-separated.

**Tooltip (4+ errors):** `FAIL(["<span title='NotFound, Validation, ...'>ℹ️fail</span>"])` — hover to see full list.

**`%% pipelineId:` comment** — `_Diagram` and `_TypeFlow` now emit `%% pipelineId: a1b2c3d4` after `flowchart LR`. Both packages.

---

### 🏥 VSIX v1.2.2 — Error Children, Layer Tabs & Walkthrough

- **Error children** — method nodes expand to show each error type as an amber child node; click navigates to error class definition
- **Revised health icons** — ✅ `errorTypes > 0` · ⚠️ Bind/Gatekeeper without errors · ⚪ Terminal-only · ❌ no diagram
- **Layer diagram tabs** — `_LayerView`, `_Stats`, `_ErrorSurface`, `_ErrorPropagation` tab buttons in the diagram panel toolbar
- **First-run walkthrough** — VS Code walkthrough guides through install → attribute → preview
- **Missing package detection** — notifies when no ResultFlow package is in the workspace; "Install Track A / Track B" buttons open a terminal with the right `dotnet add` command; auto-detects project path, Quick Pick for multi-project workspaces
- **Empty-state tree item** — info item with install instructions + link to [REslava.Result.Flow.Demo](https://github.com/reslava/nuget-package-reslava-result/tree/main/samples/REslava.Result.Flow.Demo) when tree is blank after scan

---

### 📦 Two-Track Messaging + NuGet Dependency Fix

**Two-track install table** added to `README.md`, all ResultFlow NuGet READMEs, and the VSIX README — clear Track A (REslava.Result.Flow) vs Track B (REslava.ResultFlow) guidance.

**NuGet transitive dependency fix** — `REslava.Result.Flow`, `REslava.Result.Analyzers`, and `REslava.Result.AspNetCore` now correctly declare `REslava.Result` as a transitive NuGet dependency. Previously, installing these packages did not automatically bring in `REslava.Result`. Stale `REslava.Result.SourceGenerators.Core` v1.9.0 reference also removed from `REslava.Result.AspNetCore`.

---

## 🧪 Tests

**Total: 4,704 tests passing**

---

## NuGet Packages

| Package | Version |
|---------|---------|
| [REslava.Result](https://www.nuget.org/packages/REslava.Result/1.51.0) | 1.51.0 |
| [REslava.Result.Flow](https://www.nuget.org/packages/REslava.Result.Flow/1.51.0) | 1.51.0 |
| [REslava.Result.AspNetCore](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.51.0) | 1.51.0 |
| [REslava.Result.Http](https://www.nuget.org/packages/REslava.Result.Http/1.51.0) | 1.51.0 |
| [REslava.Result.Analyzers](https://www.nuget.org/packages/REslava.Result.Analyzers/1.51.0) | 1.51.0 |
| [REslava.Result.OpenTelemetry](https://www.nuget.org/packages/REslava.Result.OpenTelemetry/1.51.0) | 1.51.0 |
| [REslava.ResultFlow](https://www.nuget.org/packages/REslava.ResultFlow/1.51.0) | 1.51.0 |
| [REslava.Result.FluentValidation](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.51.0) | 1.51.0 |

## VS Code Extension

| Extension | Version |
|-----------|---------|
| [REslava.Result Extensions](https://marketplace.visualstudio.com/items?itemName=reslava.reslava-result-extensions) | v1.2.2 |
