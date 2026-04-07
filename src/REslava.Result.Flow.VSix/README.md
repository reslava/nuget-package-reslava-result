# REslava.Result Extensions

VS Code companion for the [REslava.Result](https://www.nuget.org/packages/REslava.Result) NuGet library.

Adds a **Flow Catalog sidebar**, **CodeLens**, and a **gutter icon** to every `[ResultFlow]` method — browse all pipelines across your workspace, click any method to open a live Mermaid diagram.

![VS Code sidebar, diagram panel and Debug Panel walkthrough](images/result-flow.gif)

---

## Features

### ⚡ Flow Catalog sidebar
A dedicated activity bar panel lists every `[ResultFlow]` method across your entire workspace, grouped by project and class — no need to navigate to a source file first.

- **Project nodes** turn green when the project has been built (registry found), red when a build is needed
- **Method nodes** show `⚡` for async methods, return type, and node count as inline description
- **Hover a method** to see the full tooltip: return type, node count, node kinds, and error types
- **Click a method** to open its diagram preview instantly
- **Right-click a project** → **Build Project** runs `dotnet build --no-incremental` and auto-refreshes the tree when done
- **↺ button** in the panel header manually refreshes the full workspace scan
- **Stats bar** above the tree shows total projects · pipelines · nodes

### ▶ Open diagram preview — CodeLens
A `▶ Open diagram preview` CodeLens appears above every method decorated with `[ResultFlow]`.
Click it to open the pipeline flowchart in a **WebviewPanel** beside your editor.

- Light and dark themes follow your `ResultFlowDefaultTheme` MSBuild property
- Works fully offline — Mermaid renderer is bundled, no internet connection required

### Single / multiple window mode
Control whether clicking a pipeline reuses one shared panel or opens a new one per method.

- Toggle with the **⊞** button in the sidebar toolbar or the **Single / Multi** button in the diagram panel toolbar
- Persists across sessions via the `reslava.diagramWindowMode` VS Code setting (`single` by default)

### Click nodes to navigate to source
When `ResultFlowLinkMode=vscode` is set in your `.csproj`, every node in the diagram is clickable.
Clicking a node navigates VS Code to the exact line of that method call in your source file.

### Diagram toolbar
Each panel includes a toolbar:

| Button | Action |
|---|---|
| **Source** | Toggle the raw Mermaid DSL panel (with Copy button) |
| **Legend** | Toggle the node-kind colour legend + interaction hints |
| **SVG** | Export the diagram as an SVG file |
| **PNG** | Export the diagram as a 2× high-DPI PNG file |
| **Single / Multi** | Toggle single/multiple window mode |

### Auto-refresh on save
Open diagram panels refresh automatically when you save a C# file.
The extension re-reads the generated `*_Flows.g.cs` after each save and updates any open panel silently.
A 500 ms debounce absorbs format-on-save double-saves.

### Orange R gutter icon
A branded gutter icon marks every `[ResultFlow]` attribute line so pipelines are visible at a glance while scrolling.

---

## Requirements

Install one of the two ResultFlow NuGet packages in your project:

| | Track A | Track B |
|---|---|---|
| **Package** | [REslava.Result.Flow](https://www.nuget.org/packages/REslava.Result.Flow) | [REslava.ResultFlow](https://www.nuget.org/packages/REslava.ResultFlow) |
| **Use when** | Using REslava.Result | Any other Result library (ErrorOr, LanguageExt, FluentResults, custom) |
| **Analysis** | Full semantic — typed error edges, type travel, FAIL annotation, body scanning | Syntax-only — library-agnostic, convention file |
| **Diagram constants** | `_Diagram` · `_TypeFlow` · `_LayerView` · `_Stats` · `_ErrorSurface` · `_ErrorPropagation` | `_Diagram` · `_TypeFlow` |

**Track A:**
```bash
dotnet add package REslava.Result.Flow
```

**Track B:**
```bash
dotnet add package REslava.ResultFlow
```

Both packages generate `*_Flows.g.cs` files at build time. The extension reads these files to render the diagram — **build your project at least once** to generate them.

---

## How it works

When you click `▶ Open diagram preview`, the extension tries four steps in order to find the diagram:

1. Reads the generated `*_Flows.g.cs` file from your `obj/` folder *(fastest — works after any build)*
2. Asks Roslyn to run the "Insert diagram as comment" code action and reads the result
3. Reads an existing `/*```mermaid...```*/` block comment in your source
4. Shows a *"diagram not ready yet"* message if none of the above succeeds

The diagram is then rendered in a dedicated **WebviewPanel** using a bundled copy of Mermaid — no Markdown preview, no external extensions, no internet required.

---

## Enable node-click navigation

Add this to your `.csproj` to make every diagram node clickable:

```xml
<PropertyGroup>
  <ResultFlowLinkMode>vscode</ResultFlowLinkMode>
</PropertyGroup>
```

Then rebuild — the generator will embed `vscode://file/...` links in the diagram. Click any node to jump to that line in your editor.

---

### ▶ Debug CodeLens & Debug Panel (v1.53.0)

A `▶ Debug` CodeLens appears on every line calling `.Flow.Debug.{method}(...)`. Clicking it opens the **Debug Panel** beside your editor.

**What the Debug Panel shows:**
- **Trace list** — each recorded pipeline execution: pass/fail icon, method name, node count, elapsed ms
- **Node stepper** — step through every node in a trace; current step highlighted in the Mermaid diagram
- **Animated replay** — plays through all nodes automatically
- **File picker** — when multiple `reslava-*.json` trace files exist, switch between them without closing the panel
- **Source badge** — `📄 file` or `🌐 http` shows how trace data arrived

**File-drop workflow (no extra setup):**

Make your class `partial`:

```csharp
public partial class OrderService
{
    [ResultFlow]
    public Result<Order> Process(int userId, int productId) => ...
}
```

Then call via the generated **FlowProxy**:

```csharp
// Single-trace debug — auto-saves, panel auto-opens:
svc.Flow.Debug.Process(userId, productId);
// → reslava-debug-Process.json written to bin/
// → VSIX file watcher fires → Debug panel opens automatically

// Always-on tracing — save manually when ready:
var result = svc.Flow.Process(userId, productId);
ringBuffer.Save();  // → reslava-traces.json → Debug panel auto-loads
```

The VSIX watches `**/reslava-*.json` — **no manual panel open needed**.

---

### 🤖 AI features — Generate Test & Explain Failure (v1.55.0)

Two AI-powered buttons appear in the **Debug Panel node stepper** when a trace is selected:

| Button | When visible | What it does |
|---|---|---|
| **🧪 Test** | Any trace | Generates a compilable MSTest unit test from the real execution data |
| **🔍 Explain** | Failing traces only | Explains in 3–5 sentences why the pipeline failed and where the root cause is |

Both use the captured `PipelineTrace` — real inputs, real outputs, real error types — not log text. This dramatically reduces hallucination compared to log-based AI tools.

#### Setup

No extra setup if you have **Claude Code** installed. The extension calls `claude -p` automatically and uses your existing Pro subscription.

If `claude` is not in your terminal PATH:

1. Open a terminal and run `claude --version` — if it fails, add the Claude Code CLI to your system PATH and restart VS Code.
2. Alternatively, set `resultflow.anthropicApiKey` in VS Code Settings as a fallback (free tier available at [console.anthropic.com](https://console.anthropic.com)).

#### How to use

1. Open the **Debug Panel** (CodeLens `▶ Debug` or sidebar button)
2. Click a trace row to enter **Step view**
3. Click **🧪 Test** or **🔍 Explain** — a progress notification appears while the model runs
4. Result opens in a panel beside your editor with a **Copy** button

---

## Links

- [GitHub Repository](https://github.com/reslava/nuget-package-reslava-result)
- [REslava.Result on NuGet](https://www.nuget.org/packages/REslava.Result)
- [REslava.Result.Flow on NuGet](https://www.nuget.org/packages/REslava.Result.Flow)
- [REslava.ResultFlow on NuGet](https://www.nuget.org/packages/REslava.ResultFlow)
