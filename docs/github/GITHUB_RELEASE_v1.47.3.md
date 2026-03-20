# REslava.Result v1.47.3

## SVG Single Source of Truth — All Diagram Showcases Automated

Replace all non-code Mermaid showcase blocks across GitHub README, NuGet READMEs, and MkDocs pages with auto-generated SVG image links. After this release, future diagram visual changes require only running `scripts/svg.sh` — no document edits needed.

---

### `scripts/svg.sh` — local SVG orchestrator

A new script drives the full pipeline in one command:

```bash
bash scripts/svg.sh
```

Steps:
1. `dotnet build samples/REslava.Result.Flow.Demo` — refreshes generated `*_Flows.g.cs` constants
2. `generate_flow_catalog.py --export-mmd images/` — exports each constant as a `.mmd` file
3. `mermaid-to-svg.sh` — converts `.mmd` → `.svg` via `mmdc` + SVGO

SVGs are committed as static assets — `mmdc` requires Puppeteer/Chromium, which is too heavy for CI.

---

### `generate_flow_catalog.py --export-mmd`

New export mode writes `{ClassName}_{ConstantName}.mmd` files — the `{ClassName}_` prefix avoids collisions when multiple classes share a method name (e.g. `Pipelines_PlaceOrder.mmd` vs `OrderService_PlaceOrder.mmd`). `Legend.mmd` is exported once with no class prefix. Stats and Sidecar constants are skipped.

---

### Auto width detection

`mermaid-to-svg.sh` now detects diagram orientation per file:

| Orientation | SVGO_WIDTH |
|---|---|
| `flowchart TD` (LayerView, ErrorPropagation) | 450 |
| `flowchart LR` (everything else) | 900 |

`images/svgo.config.js` reads `process.env.SVGO_WIDTH || '900'` instead of hardcoding `'900'` — one config file, no duplication.

---

### Demo project extended

`InventoryService` and `WarehouseService` added to the Demo project, providing variety for LayerView / ErrorSurface / ErrorPropagation diagrams beyond the existing `OrderService` scenario:

- `InventoryService` — `[DomainBoundary("Infrastructure")]`, two `[ResultFlow(MaxDepth = 2)]` methods (`CheckStock`, `ReserveStock`)
- `WarehouseService` — `[DomainBoundary("Domain")]`, fluent `ReserveStock` pipeline (allows cross-layer ErrorPropagation tracing)

---

### Legend Guard tooltip

The `Legend` constant's Guard node now uses `<span title='hover shows condition'>Guard</span>` — the same tooltip pattern as real Gatekeeper nodes in actual pipeline diagrams. The note text is updated to `⚡ = async | Guard: condition shown on hover`, making the tooltip feature self-documenting in the legend. Applies to both `REslava.Result.Flow` and `REslava.ResultFlow`.

---

## Stats

- Tests: 4,648 passing (floor: >4,500)
- Features: 197 across 15 categories

---

## NuGet Packages

| Package | Link |
|---|---|
| REslava.Result | [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.47.3) |
| REslava.Result.Flow | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Flow/1.47.3) |
| REslava.Result.AspNetCore | [View on NuGet](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.47.3) |
| REslava.Result.Http | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Http/1.47.3) |
| REslava.Result.Analyzers | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.47.3) |
| REslava.Result.OpenTelemetry | [View on NuGet](https://www.nuget.org/packages/REslava.Result.OpenTelemetry/1.47.3) |
| REslava.ResultFlow | [View on NuGet](https://www.nuget.org/packages/REslava.ResultFlow/1.47.3) |
| REslava.Result.FluentValidation | [View on NuGet](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.47.3) |
