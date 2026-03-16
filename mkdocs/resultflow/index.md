---
hide:
  - navigation
title: ResultFlow — Pipeline Visualization
description: Auto-generate Mermaid pipeline diagrams from your Result pipelines. REslava.Result.Flow for REslava.Result projects (richer, semantic); REslava.ResultFlow for any library (library-agnostic).
tagline: Don't try to understand the pipeline—watch the flow.
---

# ResultFlow — Pipeline Visualization

Auto-generate **Mermaid pipeline diagrams** from your fluent Result pipelines. Add `[ResultFlow]` to any method — the diagram is inserted as a comment with one click, or accessed as a `const string` after build.

!!! tip "Which package?"
    **Using REslava.Result?** → install **`REslava.Result.Flow`** — richer diagrams with typed error edges, entry-point node, and full semantic type travel.

    **Using FluentResults, ErrorOr, LanguageExt, or any other library?** → install **`REslava.ResultFlow`** — library-agnostic, zero configuration for common methods.

```bash
# For REslava.Result projects (recommended)
dotnet add package REslava.Result.Flow

# For any other Result library
dotnet add package REslava.ResultFlow
```

<div class="grid cards" markdown>

-   :material-graph: __`REslava.Result.Flow` — For REslava.Result projects__

    The recommended package. Full Roslyn semantic analysis — typed error edges from method body scanning, entry-point detection, and complete type travel via `IResultBase`. Requires REslava.Result.

    [](installation--reslava.result.flow)

-   :material-chart-timeline-variant: __Pipeline Visualization — `[ResultFlow]`__

    Add one attribute. Get a live Mermaid diagram of every success path, failure branch, async step, and side effect — generated from your code.

    [](pipeline-visualization--resultflow)

-   :material-wrench: __Code Action — Insert Diagram as Comment__

    One click inserts the generated Mermaid diagram directly above the method — no build required. Renders inline in VS Code, GitHub, and Rider.

    [](code-action--insert-diagram-as-comment)

-   :material-lightning-bolt: __Async Step Annotation__

    `*Async` methods are automatically annotated with a ⚡ suffix in the diagram label — no configuration required.

    [](async-step-annotation)

-   :material-swap-horizontal: __Success Type Travel__

    The success type `T` is inferred at each pipeline step and rendered inline — zero configuration, works with any Result library.

    [](success-type-travel)

-   :material-file-cog: __`resultflow.json` — Custom Classification__

    Classify custom or third-party methods. Add a single JSON file and override any built-in classification with your own method names.

    [](resultflow.json--custom-classification)

-   :material-package-variant-closed-check: __`REslava.ResultFlow` — Library-agnostic Alternative__

    Works with ErrorOr, FluentResults, LanguageExt, or any fluent Result library. Syntax-only analysis — no REslava.Result dependency required.

    [](reslava.resultflow--library-agnostic-alternative)

</div>