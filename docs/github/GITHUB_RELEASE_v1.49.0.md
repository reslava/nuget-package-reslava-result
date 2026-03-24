# REslava.Result v1.49.0

## ✨ What's New

### 🖥️ VSIX v1.1.0 — WebviewPanel Diagram Renderer

The **REslava.Result Extensions** VS Code extension (v1.1.0) now renders pipeline diagrams in a dedicated **`vscode.WebviewPanel`** — no more sidecar Markdown files, no more dependency on VS Code's Markdown preview or third-party Mermaid extensions.

- **Bundled Mermaid v10.9.5** — fully offline, no CDN, no external dependencies, no conflicts
- **One panel per method** — rapid CodeLens clicks reveal the existing panel instead of opening duplicates
- **Light / dark background** — follows your `ResultFlowDefaultTheme` MSBuild property automatically

---

### 🔗 Node-Click Navigation

Click any diagram node to jump directly to that line in your source file.

Requires `ResultFlowLinkMode=vscode` in your `.csproj` or `Directory.Build.props`:

```xml
<PropertyGroup>
  <ResultFlowLinkMode>vscode</ResultFlowLinkMode>
</PropertyGroup>
```

Then rebuild — the generator embeds `vscode://file/...` links into each node. Click a **Gatekeeper**, **Bind**, **Tap**, or **Terminal** node to navigate instantly.

---

### 🧰 Diagram Panel Toolbar

Each panel now includes four toolbar buttons:

| Button | Action |
|---|---|
| **Source** | Collapsible panel — view and copy the raw Mermaid DSL |
| **Legend** | Collapsible panel — node colour swatches and interaction hints |
| **SVG** | Export the diagram as an SVG file via save dialog |
| **PNG** | Export at 2× resolution for high-DPI screens |

---

### 🔧 Bug Fixes

#### `REslava.Result.Flow` — MSBuild `CompilerVisibleProperty` parity

New `build/REslava.Result.Flow.props` and `buildTransitive/REslava.Result.Flow.props` now expose `ResultFlowLinkMode` and `ResultFlowDefaultTheme` as `CompilerVisibleProperty` entries — the same as `REslava.ResultFlow` already had. This ensures the generator picks up these MSBuild properties correctly in all build configurations.

#### MermaidRenderer — Gatekeeper tooltip quote escaping (both packages)

Gatekeeper predicate tooltips (`<span title='...'>`) now escape `"` → `"` and `'` → `'` (Unicode curly quotes). Prevents Mermaid parse errors when predicate expressions contain quotes or apostrophes (e.g. `u.Status != "Inactive"` or `u.Name.Contains("O'Brien")`).

---

## 🧪 Tests

**Total: 4,688 tests passing**

---

## NuGet Packages

| Package | Version |
|---------|---------|
| [REslava.Result](https://www.nuget.org/packages/REslava.Result/1.49.0) | 1.49.0 |
| [REslava.Result.Flow](https://www.nuget.org/packages/REslava.Result.Flow/1.49.0) | 1.49.0 |
| [REslava.Result.AspNetCore](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.49.0) | 1.49.0 |
| [REslava.Result.Http](https://www.nuget.org/packages/REslava.Result.Http/1.49.0) | 1.49.0 |
| [REslava.Result.Analyzers](https://www.nuget.org/packages/REslava.Result.Analyzers/1.49.0) | 1.49.0 |
| [REslava.Result.OpenTelemetry](https://www.nuget.org/packages/REslava.Result.OpenTelemetry/1.49.0) | 1.49.0 |
| [REslava.ResultFlow](https://www.nuget.org/packages/REslava.ResultFlow/1.49.0) | 1.49.0 |
| [REslava.Result.FluentValidation](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.49.0) | 1.49.0 |

## VS Code Extension

| Extension | Version |
|-----------|---------|
| [REslava.Result Extensions](https://marketplace.visualstudio.com/items?itemName=reslava.reslava-result-extensions) | v1.1.0 |
