---
hide:
  - navigation
title: ResultFlow — Diagram Features
description: Compile-time diagram constants — node types, typed edges, multi-branch fan-out, type travel, async markers, and visual features emitted by the ResultFlow generator.
---

# ResultFlow — Diagram Features

<div class="grid cards" markdown>

-   :material-lightning-bolt: __⚡ Async Step Annotation__

    `*Async` methods are automatically annotated with a ⚡ suffix in the diagram label — no configuration required.

    [](async-step-annotation)

-   :material-link: __🔗 Clickable Mermaid Nodes — VS Code Navigation (v1.43.0)__

    When `ResultFlowLinkMode` is set to `vscode`, each node becomes a hyperlink that opens the exact source line in VS Code.

    [](clickable-mermaid-nodes--vs-code-navigation-v1.43.0)

-   :material-graph: __🔀 Cross-Method Pipeline Tracing — `MaxDepth` (v1.45.0)__

    Follow `Bind` calls into other methods and expand them inline as Mermaid subgraphs — one diagram spanning multiple classes and layers.

    [](cross-method-pipeline-tracing--maxdepth-v1.45.0)

-   :material-weather-night: __🌙 Dark Theme Diagrams (v1.47.4)__

    Add `Theme = ResultFlowTheme.Dark` to emit the full diagram set in a dark colour scheme — optimised for dark-mode editors and MkDocs slate.

    [](dark-theme-diagrams-v1.47.4)

-   :material-layers: __🏛️ Domain Boundary Diagrams — `_LayerView`, `_Stats`, `_ErrorSurface`, `_ErrorPropagation` (v1.45.0)__

    Add `[DomainBoundary]` to a class for four additional diagram constants: architecture view, pipeline stats, error surface, and error propagation by layer.

    [](domain-boundary-diagrams--_layerview-_stats-_errorsurface-_errorpropagation-v1.45.0)

-   :material-table-search: __🔍 Error Taxonomy Map (v1.54.0)__

    Both packages emit `_ErrorTaxonomy` — a markdown table of every detectable error type per method, generated at compile time with `certain` / `inferred` confidence levels.

    [](error-taxonomy-map-v1.54.0)

-   :material-tag-outline: __🏷️ Error Type Annotation on Failure Edges (v1.40.0)__

    Failure edges are annotated with the error type name when a step argument is a direct error constructor or static factory call.

    [](error-type-annotation-on-failure-edges-v1.40.0)

-   :material-circle-off-outline: __🔴 FAIL Node Error Annotation (v1.51.0)__

    The `FAIL` node in generated diagrams now displays the actual error types that can reach it — making failure surfaces visible without opening source.

    [](fail-node-error-annotation-v1.51.0)

-   :material-source-branch: __🔀 Match — Multi-Branch Fan-Out (v1.46.0)__

    `Match` renders as a decision hexagon with one typed fail edge per explicitly-typed lambda parameter — all converging on the shared `FAIL` terminal.

    [](match--multi-branch-fan-out-v1.46.0)

-   :material-map-legend: __🗺️ Node Type Legend — `Legend` constant (v1.47.2)__

    One `Legend` constant per `*_Flows` class shows all 9 node types — shapes, colours, and the entry-arrow symbol — in a single Mermaid diagram.

    [](node-type-legend--legend-constant-v1.47.2)

-   :material-file-document-outline: __📄 Sidecar Markdown Constant — Pipeline Docs Alongside Code (v1.43.0)__

    For every `[ResultFlow]` method the generator emits a `{MethodName}_Sidecar` constant — the Mermaid diagram wrapped in a fenced block ready for GitHub or VS Code preview.

    [](sidecar-markdown-constant--pipeline-docs-alongside-code-v1.43.0)

-   :material-tag-check: __🏷️ Success Type Travel__

    `REslava.ResultFlow` infers the success type `T` at each pipeline step and renders it inline in the node label — zero configuration, works with any Result library.

    [](success-type-travel)

-   :material-transit-connection-variant: __🔀 Type-Flow Diagram — `_TypeFlow` (v1.51.0)__

    Every `[ResultFlow]` method generates a `{MethodName}_TypeFlow` constant that labels every success edge with the `Result<T>` type flowing through it.

    [](type-flow-diagram--_typeflow-v1.51.0)

</div>