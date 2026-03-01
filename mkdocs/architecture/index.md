---
hide:
  - navigation
title: Architecture
description: "Understand the internals: incremental Roslyn generators, analyzer pipelines, and the three-package architecture. Extend with confidence."
tagline: Peek inside. No magic. Just smart code.
---

# Architecture & Design

Peek under the hood – how REslava.Result is built and how its source generators work.

<div class="grid cards" markdown>

-   :material-sitemap: __Base Library__
    Visual diagrams, component breakdown, and SOLID principles in action.
    [](base-library-reslava.result)

-   :material-check-all: __SOLID Principles in Action__
    How each SOLID principle is applied across the core library and generators.
    [](solid-principles-in-action)

-   :material-connection: __How Components Work Together__
    End-to-end flow: attribute → generator → compiled output → runtime.
    [](how-components-work-together)

-   :material-package: __Package Structure__
    What each NuGet package contains and how they integrate.
    [](nuget-package-contents)

-   :material-cog: __Source Generator Architecture__
    Two-phase pipeline, SOLID design, auto-detection, generated output, and incremental rebuilds.
    [](source-generators/)    

-   :simple-solid: __SOLID Architecture__
    SOLID Principles Implementation.
    [](solid/solid-architecture)

-   :simple-uml: __REslava.Result Core Type Hierarchy__
    UML class diagrams illustrating the core type hierarchy – Reason & Error hierarchy (incl. domain errors), Result&lt;T&gt;, Maybe&lt;T&gt;, and OneOf unions.
    [](solid/uml-v1.25.0-core)
    {: .is-featured }

-   :simple-uml: __REslava.Result.SourceGenerators__
    UML diagrams detailing the source generator architecture – all generators (IResult, ActionResult, SmartEndpoints, Validate), shared orchestrators, and the two‑phase pipeline.
    [](solid/uml-v1.25.0-generators)

</div>