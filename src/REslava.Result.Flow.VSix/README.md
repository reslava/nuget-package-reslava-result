# REslava.Result Extensions

VS Code companion for the [REslava.Result](https://www.nuget.org/packages/REslava.Result) NuGet library.

Adds **CodeLens** and a **gutter icon** to every `[ResultFlow]` method — click once to open a live Mermaid pipeline diagram in a dedicated side panel.

---

## Features

### ▶ Open diagram preview — CodeLens
A `▶ Open diagram preview` CodeLens appears above every method decorated with `[ResultFlow]`.
Click it to open the pipeline flowchart in a **WebviewPanel** beside your editor.

- One panel per method — rapid double-clicks reveal the existing panel instead of opening a duplicate
- Light and dark themes follow your `ResultFlowDefaultTheme` MSBuild property
- Works fully offline — Mermaid renderer is bundled, no internet connection required

### Click nodes to navigate to source
When `ResultFlowLinkMode=vscode` is set in your `.csproj`, every node in the diagram is clickable.
Clicking a node navigates VS Code to the exact line of that method call in your source file.

### Toolbar
Each panel includes a toolbar with four buttons:

| Button | Action |
|---|---|
| **Source** | Toggle the raw Mermaid DSL panel (with Copy button) |
| **Legend** | Toggle the node-kind colour legend + interaction hints |
| **SVG** | Export the diagram as an SVG file |
| **PNG** | Export the diagram as a 2× high-DPI PNG file |

### Orange R gutter icon
A branded gutter icon marks every `[ResultFlow]` attribute line so pipelines are visible at a glance while scrolling.

![CodeLens, diagram panel, and toolbar in action](https://raw.githubusercontent.com/reslava/nuget-package-reslava-result/main/src/REslava.Result.Flow.VSix/images/screenshot.png)

---

## Requirements

Install one of the two ResultFlow NuGet packages in your project:

| Package | Description |
|---|---|
| [REslava.Result.Flow](https://www.nuget.org/packages/REslava.Result.Flow) | Full semantic analysis — requires `REslava.Result` |
| [REslava.ResultFlow](https://www.nuget.org/packages/REslava.ResultFlow) | Syntax-only, library-agnostic |

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

## Links

- [GitHub Repository](https://github.com/reslava/nuget-package-reslava-result)
- [REslava.Result on NuGet](https://www.nuget.org/packages/REslava.Result)
- [REslava.Result.Flow on NuGet](https://www.nuget.org/packages/REslava.Result.Flow)
- [REslava.ResultFlow on NuGet](https://www.nuget.org/packages/REslava.ResultFlow)
