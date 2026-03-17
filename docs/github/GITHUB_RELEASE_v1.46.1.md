# REslava.Result v1.46.1

## NuGet README — SVG diagrams

NuGet.org does not render Mermaid. The `REslava.Result` and `REslava.Result.Flow` NuGet README pages now display rendered pipeline diagrams via hosted SVG images.

---

## What changed

### REslava.Result NuGet README

Mermaid block replaced with two clickable SVG images:

- **Pipeline flowchart** — success path, typed error edges, async step markers
- **Architecture layer view** — Domain / Application / Infrastructure boundaries, auto-detected from namespaces

### REslava.Result.Flow NuGet README

Mermaid block replaced with two clickable SVG images:

- **Cross-method pipeline tracing** — `[ResultFlow(MaxDepth = 2)]` expands `Bind` lambdas into named `subgraph` blocks
- **Match multi-branch fan-out** — hexagon node with one typed `-->|ErrorType| FAIL` edge per branch

All images link to the GitHub repository. No API changes.

---

## Stats

- Tests: 4,634 passing (floor: >4,500)
- Features: 197 across 15 categories

---

## NuGet Packages

| Package | Link |
|---|---|
| REslava.Result | [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.46.1) |
| REslava.Result.Flow | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Flow/1.46.1) |
| REslava.Result.AspNetCore | [View on NuGet](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.46.1) |
| REslava.Result.Http | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Http/1.46.1) |
| REslava.Result.Analyzers | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.46.1) |
| REslava.Result.OpenTelemetry | [View on NuGet](https://www.nuget.org/packages/REslava.Result.OpenTelemetry/1.46.1) |
| REslava.ResultFlow | [View on NuGet](https://www.nuget.org/packages/REslava.ResultFlow/1.46.1) |
| REslava.Result.FluentValidation | [View on NuGet](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.46.1) |
