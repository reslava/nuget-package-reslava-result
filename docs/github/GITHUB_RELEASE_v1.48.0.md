# REslava.Result v1.48.0

## ✨ What's New

### 🖊️ CodeLens — `▶ Open diagram preview` (REslava.Result Extensions VSIX)

A new VS Code extension — **REslava.Result Extensions** — adds a `▶ Open diagram preview` CodeLens above every `[ResultFlow]`-annotated method. Always visible, no cursor required.

**Install from the VS Code Marketplace:** search **REslava.Result Extensions**.

**4-step fallback chain on click:**

1. Scans workspace for `*_Flows.g.cs` → extracts Mermaid constant → opens preview ✅
2. If not built: auto-runs "Insert diagram as comment" Roslyn code action → extracts from inserted comment → opens preview ✅
3. If a `` /* ```mermaid...``` */ `` comment already exists in source → extracts directly → opens preview ✅
4. Shows *"Diagram not ready yet — try again in a moment"*

> The VSIX is separate from the NuGet packages. `Ctrl+.` lightbulb keeps the existing **Insert / Refresh diagram** actions unchanged.

---

### 🎨 Solution-Wide Default Theme — `ResultFlowDefaultTheme`

Set a diagram theme for the whole solution without touching individual `[ResultFlow]` attributes:

```xml
<!-- Directory.Build.props -->
<PropertyGroup>
  <ResultFlowDefaultTheme>Dark</ResultFlowDefaultTheme>
</PropertyGroup>
```

**Priority chain:** `[ResultFlow(Theme = Dark)]` → `<ResultFlowDefaultTheme>` → `Light` (built-in default)

Applies to both `REslava.Result.Flow` and `REslava.ResultFlow`. Accepted values: `Light` / `Dark` (case-insensitive).

---

### 🖼️ NuGet README PNG Images

Diagram images in NuGet README now served as **transparent-background PNGs** from `raw.githubusercontent.com`. `mermaid-to-svg.sh` emits `.png` alongside `.svg` via `mmdc --backgroundColor transparent`. Local SVG packing (v1.47.5) has been reverted.

---

## 🧪 Tests

12 new tests across both packages verifying:
- `build_property.ResultFlowDefaultTheme = Dark` picked up by generator
- Method-level `[ResultFlow(Theme = Dark)]` overrides MSBuild default
- Case-insensitive parsing
- No-property → Light default

**Total: 4,688 tests passing**

---

## NuGet Packages

| Package | Version |
|---------|---------|
| [REslava.Result](https://www.nuget.org/packages/REslava.Result/1.48.0) | 1.48.0 |
| [REslava.Result.Flow](https://www.nuget.org/packages/REslava.Result.Flow/1.48.0) | 1.48.0 |
| [REslava.Result.AspNetCore](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.48.0) | 1.48.0 |
| [REslava.Result.Http](https://www.nuget.org/packages/REslava.Result.Http/1.48.0) | 1.48.0 |
| [REslava.Result.Analyzers](https://www.nuget.org/packages/REslava.Result.Analyzers/1.48.0) | 1.48.0 |
| [REslava.Result.OpenTelemetry](https://www.nuget.org/packages/REslava.Result.OpenTelemetry/1.48.0) | 1.48.0 |
| [REslava.ResultFlow](https://www.nuget.org/packages/REslava.ResultFlow/1.48.0) | 1.48.0 |
| [REslava.Result.FluentValidation](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.48.0) | 1.48.0 |
