---
hide:
  - navigation
title: ResultFlow
description: Source generator that auto-generates Mermaid pipeline diagrams at compile time. Add [ResultFlow] to any fluent method and get a const string diagram — zero runtime overhead, zero manual maintenance.
tagline: Your pipeline diagram, written by the compiler.
---

# ResultFlow

Auto-generate **Mermaid pipeline diagrams** for any fluent Result pipeline — at compile time.

Add `[ResultFlow]` to a method. Build. The diagram is a `const string` in the generated code — paste it into any Mermaid renderer to instantly visualize the data flow.

```bash
dotnet add package REslava.ResultFlow
```

<div class="grid cards" markdown>

-   :material-graph: __`[ResultFlow]` Attribute__

    Annotate any fluent pipeline method. The source generator walks the chain and emits a Mermaid flowchart constant — no runtime cost, no maintenance.

    [](pipeline-visualization--resultflow)

-   :material-book-open-variant: __Convention Dictionary__

    Built-in support for **REslava.Result**, **ErrorOr**, and **LanguageExt** — classify Ensure, Bind, Map, Tap, Match, Filter, Then, and more out of the box.

    [](pipeline-visualization--resultflow)

-   :material-file-cog: __`resultflow.json` Configuration__

    Escape hatch for custom or third-party libraries. Add a single JSON file and override any built-in classification with your own method names.

    [](pipeline-visualization--resultflow)

-   :material-wrench: __Code Action — Insert Diagram__

    The companion analyzer detects missing diagram comments on `[ResultFlow]` methods. One click inserts the generated Mermaid diagram directly above the method body.

    [](pipeline-visualization--resultflow)

</div>
