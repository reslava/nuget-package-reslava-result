# REslava.Result v1.47.4

## Dark Theme Diagrams — `[ResultFlow(Theme = ResultFlowTheme.Dark)]`

Add `Theme = ResultFlowTheme.Dark` to any `[ResultFlow]` annotation to emit the full diagram set using a dark colour scheme — optimised for dark-mode editors, MkDocs slate, and presentation slides. Light remains the default; all existing diagrams are unaffected.

```csharp
[ResultFlow(MaxDepth = 2, Theme = ResultFlowTheme.Dark)]
public static Result<StockReservation> FulfillOrder(int productId, int quantity) =>
    FindProduct(productId)
        .Bind(p => WarehouseService.ReserveStock(p, quantity))
        .Map(p  => new StockReservation(p.Id, quantity, p.Price));
```

---

### Dark `classDef` palette

All nine node types have matching dark equivalents — amber `operation`, green `bind`/`map`, blue `gatekeeper`, yellow `sideeffect`, purple `terminal`, teal `success`, red `failure`. The palette is matched to the MkDocs slate colour scheme.

---

### `MermaidInitDark` — edge label and title fixes

Cross-method diagrams use a new `MermaidInitDark` init constant that sets three `themeVariables`:

| Variable | Value | Purpose |
|---|---|---|
| `primaryTextColor` | `#fff` | Front-matter `title:` text |
| `titleColor` | `#fff` | Subgraph label text |
| `edgeLabelBackground` | `#2a2a2a` | Edge label pill background |

Without this, titles and subgraph labels render as black-on-dark — effectively invisible. Applies to the main pipeline diagram only; auxiliary diagrams (`_LayerView`, `_ErrorSurface`, `_ErrorPropagation`) already have explicit node colours via classDef.

---

### All auxiliary diagram types support dark theme

`_LayerView`, `_ErrorSurface`, and `_ErrorPropagation` all accept `darkTheme` and emit the dark palette. Layer subgraph containers use depth-indexed styles (`Layer0_Style`, `Layer1_Style`, …) centralized in `ResultFlowThemes.cs` — one place to tweak colours for all diagram types.

---

### Layer color centralization

`Layer{n}_Style` classDefs now live entirely in `ResultFlowThemes.Light` and `ResultFlowThemes.Dark`. Renderers emit only `class Layer{depth} Layer{depth}_Style` assignment lines — zero hardcoded colour strings outside the themes constants. 2-colour alternating palette: even = blue-lavender, odd = mint.

---

### `FulfillmentService` demo — dark cross-method + layer showcase

New `FulfillmentService` class in the demo project demonstrates all dark diagram types in a cross-layer scenario:

- `[DomainBoundary("Application")]` on `FulfillmentService`
- `[ResultFlow(MaxDepth = 2, Theme = Dark)]` on `FulfillOrder` — traces into `WarehouseService` (Domain)
- Generates: `FulfillOrder`, `FulfillOrder_LayerView`, `FulfillOrder_ErrorSurface`, `FulfillOrder_ErrorPropagation`

---

### MkDocs dark catalog

New `mkdocs/demo/flow-catalog-dark.md` page (`force-dark-mode: true`) showcases all dark diagram views. `mkdocs/demo/index.md` updated with separate Light / Dark catalog buttons.

---

## Stats

- Tests: 4,648 passing (floor: >4,500)
- Features: 197 across 15 categories

---

## NuGet Packages

| Package | Link |
|---|---|
| REslava.Result | [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.47.4) |
| REslava.Result.Flow | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Flow/1.47.4) |
| REslava.Result.AspNetCore | [View on NuGet](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.47.4) |
| REslava.Result.Http | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Http/1.47.4) |
| REslava.Result.Analyzers | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.47.4) |
| REslava.Result.OpenTelemetry | [View on NuGet](https://www.nuget.org/packages/REslava.Result.OpenTelemetry/1.47.4) |
| REslava.ResultFlow | [View on NuGet](https://www.nuget.org/packages/REslava.ResultFlow/1.47.4) |
| REslava.Result.FluentValidation | [View on NuGet](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.47.4) |
