---
hide:
  - navigation
title: Source Generators
description: "Internals of REslava.Result.SourceGenerators: two-phase pipeline, SOLID design, auto-detection, generated output, and incremental Roslyn rebuilds."
tagline: Zero boilerplate. Full visibility.
---

# Source Generator Architecture

Deep-dive into how `REslava.Result.SourceGenerators` works — from attribute detection to generated output.

<div class="grid cards" markdown>

-   :material-code-braces: __Source Generators Package__
    Zero-boilerplate code generation — SmartEndpoints, IResult, ActionResult, and Validate generators.
    [](source-generators-reslava.result.sourcegenerators)

-   :material-cog: __Two-Phase Pipeline__
    How generators run in two phases — attribute registration then source output.
    [](two-phase-pipeline)

-   :material-auto-fix: __Smart Auto-Detection__
    Zero-config setup detection for OneOf arities — how v1.10.0 eliminated all boilerplate.
    [](smart-auto-detection-v1.10.0)

-   :material-map-marker-path: __Error → HTTP Status Convention__
    Convention-based name matching that maps error types to HTTP status codes automatically.
    [](error--http-status-code-convention)

-   :material-layers: __Core Library Components__
    Shared orchestrators, attribute generators, and code generators used by all source generators.
    [](source-generator-core-library-components)

-   :material-file-tree: __Generated Output Structure__
    What files the generators emit and where they land in your project.
    [](generated-output-structure)

-   :material-wrench-cog: __Build Integration__
    Automatic MSBuild integration — how generators hook into the build pipeline.
    [](build-integration)

-   :material-shape: __SOLID Design (v1.9.4+)__
    Three-class generator pattern (SRP) — orchestrator, attribute generator, code generator.
    [](solid-design-v1.9.4)

-   :material-lightning-bolt: __Incremental Rebuilds__
    How `RegisterSourceOutput` + `SyntaxValueProvider` keeps builds fast in large solutions.
    [](incremental-rebuilds)

</div>