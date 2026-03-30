# REslava.ResultFlow

**Track B — Library-agnostic** source generator for automatic Mermaid pipeline diagrams — works with any fluent Result library.

Add `[ResultFlow]` to any fluent method → the diagram is **injected as a comment** by the IDE code action (no build needed), or accessed as a `const string` after build. Zero runtime overhead. Zero manual maintenance.

Ships with a **built-in convention dictionary** pre-configured for **REslava.Result**, **ErrorOr**, **LanguageExt**, and **FluentResults**. Any other library can be supported by adding a `resultflow.json` file to your project. Generates `_Diagram` and `_TypeFlow` constants.

> Using **REslava.Result**? Use **Track A** — [`REslava.Result.Flow`](https://www.nuget.org/packages/REslava.Result.Flow) — for richer diagrams: typed error edges from body scanning, full type travel, and the complete constant set (`_LayerView`, `_Stats`, `_ErrorSurface`, `_ErrorPropagation`).

## Installation

```bash
dotnet add package REslava.ResultFlow
```

No extra `using` required — the `[ResultFlow]` attribute is injected into your compilation automatically by the source generator.

## Quick Start

```csharp
[ResultFlow]
public async Task<Result<UserDto>> RegisterAsync(RegisterCommand cmd) =>
    await CreateUser(cmd)
        .EnsureAsync(IsEmailValid, new InvalidEmailError())
        .BindAsync(SaveUser)
        .TapAsync(SendWelcomeEmail)
        .MapAsync(ToDto);
```

Use the **IDE code action** to inject the diagram as a comment directly above the method — no build required:

```csharp
/*
```mermaid
flowchart LR
    N0_EnsureAsync["EnsureAsync"]:::gatekeeper
    ...
```*/
[ResultFlow]
public async Task<Result<UserDto>> RegisterAsync(RegisterCommand cmd) => ...
```

Or access the generated constant after build and paste it into any Mermaid renderer:

```csharp
string diagram = Generated.ResultFlow.UserService_Flows.RegisterAsync;
// Paste into mermaid.live, GitHub, Notion, VS Code, or your wiki
```

## Generated Diagram Examples

**Pipeline** — guard, transforms, side effects, success/failure paths:

![Auto-generated pipeline diagram](https://raw.githubusercontent.com/reslava/nuget-package-reslava-result/main/images/Pipelines_PlaceOrder.svg)

**Node type legend** — all node types with colors and shapes:

![Node type legend](https://raw.githubusercontent.com/reslava/nuget-package-reslava-result/main/images/Legend.svg)

Each operation is color-coded by semantic role:
- **Lavender** — gatekeepers (`Ensure`) — can fail validation
- **Mint** — transforms (`Bind` / `Map`) — shape the result
- **Vanilla** — side effects (`Tap`) — fire-and-forget
- **Soft pink** — failure paths

## Supported Libraries

The built-in convention dictionary covers the most popular Result libraries out of the box — no configuration needed:

| Library | Methods recognized |
|---|---|
| **REslava.Result** | `Ensure`, `Bind`, `Map`, `Tap`, `TapOnFailure`, `TapBoth`, `Match`, `WithSuccess` (+ `Async` variants) |
| **ErrorOr** | `Then`, `ThenAsync`, `Switch`, `SwitchAsync` |
| **LanguageExt** | `Filter`, `Do`, `DoAsync`, `DoLeft`, `DoLeftAsync` |

Any unrecognized method is rendered as a generic **operation** node — the diagram is still generated.

## Custom Method Classification — resultflow.json

Add a `resultflow.json` file to your project to classify custom or third-party methods. Config entries **override** the built-in dictionary.

```json
{
  "mappings": [
    {
      "bind":       ["Chain", "AndThen"],
      "map":        ["Transform"],
      "tap":        ["Log", "Audit"],
      "gatekeeper": ["Require"],
      "terminal":   ["Fold"]
    }
  ]
}
```

Register it as an `AdditionalFile` in your `.csproj`:

```xml
<ItemGroup>
  <AdditionalFiles Include="resultflow.json" />
</ItemGroup>
```

Supported keys: `bind`, `map`, `tap`, `tapOnFailure`, `gatekeeper`, `terminal`.

## Diagram Preview — VS Code Extension (v1.1.0)

Install **[REslava.Result Extensions](https://marketplace.visualstudio.com/items?itemName=reslava.reslava-result-extensions)** from the VS Code Marketplace to get a `▶ Open diagram preview` CodeLens above every `[ResultFlow]` method.

One click opens the rendered Mermaid diagram in a **dedicated side panel** — bundled renderer, works fully offline.

![REslava.Result Extensions — CodeLens, diagram panel, Source/Legend/SVG/PNG toolbar](https://raw.githubusercontent.com/reslava/nuget-package-reslava-result/main/src/REslava.Result.Flow.VSix/images/screenshot.png)

**Panel features:**
- Click any node to navigate to that line in your source (requires `<ResultFlowLinkMode>vscode</ResultFlowLinkMode>` in your `.csproj`)
- **Source** button — view and copy the raw Mermaid DSL
- **Legend** button — node colour guide and interaction hints
- **SVG** / **PNG** buttons — export the diagram to disk (PNG at 2× for high-DPI screens)

## Code Action

The companion analyzer detects `[ResultFlow]` methods that are missing the diagram as a developer comment. A **single-click code fix** inserts the generated Mermaid diagram directly above the method body — so the diagram lives next to the code that produced it.

## Live Panel — `▶ Debug` CodeLens (v1.52.0)

Add [`REslava.Result.Diagnostics`](https://www.nuget.org/packages/REslava.Result.Diagnostics) to expose your `RingBufferObserver` over HTTP. The VSIX `▶ Debug` CodeLens connects to `GET /reslava/traces` and streams execution data into VS Code — trace list, node stepper, and animated replay with diagram node highlight.

```bash
dotnet add package REslava.Result.Diagnostics
```

```csharp
using REslava.Result.Diagnostics;

var buffer = new RingBufferObserver();
PipelineObserver.Register(buffer);

// Standalone (console / worker):
using var host = PipelineTraceHost.Start(buffer, port: 5297);

// ASP.NET Core:
app.MapResultFlowTraces(buffer);
```

## Diagnostics

| ID | Severity | Description |
|---|---|---|
| `REF001` | Info | `[ResultFlow]` could not detect a fluent chain — diagram not generated. Check that the method body is an expression body or ends with a `return` of a fluent chain. |
| `REF003` | Warning | `resultflow.json` could not be parsed — falling back to the built-in convention dictionary. Check the JSON syntax. |

## Documentation

Full documentation: [reslava.github.io/nuget-package-reslava-result](https://reslava.github.io/nuget-package-reslava-result/resultflow/)

**MIT License** | Works with any .NET project (netstandard2.0)
