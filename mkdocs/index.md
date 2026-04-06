---
hide:
  - navigation
title: Visual Result pipelines for .NET
description: A Result pattern library for .NET with typed errors, Railway-Oriented pipelines, and automatic flow diagrams generated from your code.
tagline: Don't try to understand the pipeline—watch the flow.
---

# REslava.Result — Visual Result pipelines for .NET
<div align="center" markdown>
![.NET](https://img.shields.io/badge/.NET-512BD4?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?&logo=csharp&logoColor=white)
![NuGet Version](https://img.shields.io/nuget/v/REslava.Result.AspNetCore?style=flat&logo=nuget)
![License](https://img.shields.io/badge/license-MIT-green)
[![GitHub contributors](https://img.shields.io/github/contributors/reslava/REslava.Result)](https://GitHub.com/reslava/REslava.Result/graphs/contributors/) 
[![GitHub Stars](https://img.shields.io/github/stars/reslava/REslava.Result)](https://github.com/reslava/REslava.Result/stargazers) 
[![NuGet Downloads](https://img.shields.io/nuget/dt/REslava.Result)](https://www.nuget.org/packages/REslava.Result)
![Test Coverage](https://img.shields.io/badge/coverage-95%25-brightgreen)
![Test Suite](https://img.shields.io/badge/tests-3960%20passing-brightgreen)
</div>

**Visual Result pipelines for .NET**

*Don't try to understand the pipeline—watch the flow.*

Build Railway-Oriented pipelines with strongly typed errors, automatically generated flow diagrams, and built-in runtime observation — **trace every node, debug visually.**

| | |
|---|---|
| ✏️ **Write** | Add `[ResultFlow]` — Mermaid diagram constants generated at compile time |
| ▶️ **Run** | Make your class `partial` — FlowProxy traces every node (output, type, elapsed ms) |
| 🐛 **Debug** | Drop a `reslava-*.json` — VS Code Debug Panel replays the trace on your live diagram |

**:material-source-repository: [nuget-package-reslava-result GitHub repo](https://github.com/reslava/nuget-package-reslava-result)**

![Auto-generated pipeline diagram — full type travel, typed error edges, async markers](https://raw.githubusercontent.com/reslava/nuget-package-reslava-result/main/images/Pipelines_AdminCheckout.svg)

*[`[ResultFlow]`](resultflow) — one attribute, live Mermaid diagram of your pipeline. Type travel, async markers, named error edges — generated from your code.*

![VS Code sidebar, diagram panel and Debug Panel walkthrough](https://raw.githubusercontent.com/reslava/nuget-package-reslava-result/main/images/result-flow.gif)

!!! tip "🎯 Typed Error Pipelines — compile-time failure edges with `Result<TValue, TError>` (v1.39.0)"
    Replace `IEnumerable<IError>` bags with **exact, exhaustive error types**. Each `Bind` step grows the error union by one slot (`ErrorsOf<T1,T2,...>`). At the callsite, `Match` is exhaustive — the compiler tells you if you missed a case.

    [→ Typed Error Pipelines](advanced/typed-pipelines){ .md-button }

!!! tip "🗺️ See how your `Result<T>` / `Result<T, TError>` flows — before it runs. Pipeline Visualization."
    Annotate any fluent pipeline with `[ResultFlow]` and with **single-click code action** ResultFlow will insert a **Mermaid diagram comment** — every success path, failure branch, and side effect visualized. For `Result<T, TError>` pipelines, failure edges show the **exact error type** (`ErrorsOf<ValidationError, InventoryError>`) — no body scanning, reads directly from the return type.

    [→ ResultFlow](resultflow){ .md-button }

!!! info "⚡ Quick start — Pipeline Visualization"
    Choose your track based on which Result library you use:

    | | Track A | Track B |
    |---|---|---|
    | **Use when** | Using REslava.Result | Any other Result library — ErrorOr, LanguageExt, FluentResults, or custom |
    | **Install** | `REslava.Result` + `REslava.Result.Flow` | `REslava.ResultFlow` |
    | **Analysis** | Full semantic — typed error edges, type travel, FAIL annotation, body scanning | Syntax-only — library-agnostic, convention file |
    | **Diagram constants** | `_Diagram` · `_TypeFlow` · `_LayerView` · `_Stats` · `_ErrorSurface` · `_ErrorPropagation` | `_Diagram` · `_TypeFlow` |

    **Track A:**  `dotnet add package REslava.Result` + `dotnet add package REslava.Result.Flow`

    **Track B:**  `dotnet add package REslava.ResultFlow`

    **VS Code extension (both tracks):** [REslava.Result Extensions](https://marketplace.visualstudio.com/items?itemName=reslava.reslava-result-extensions) — Flow Catalog sidebar + `▶ Open diagram preview` CodeLens.

!!! note "📚 New to functional programming? Start with the progressive tutorial series."
    9 self-contained lessons that teach **functional & railway-oriented programming** step by step — from plain C# exceptions all the way to async pipelines and ASP.NET. Each lesson is a standalone `dotnet run`, no setup required.

    Learn all three packages progressively: **REslava.Result** · **REslava.ResultFlow** · **REslava.Result.AspNetCore**

    YouTube video series — coming soon.

    [→ Tutorial Lessons](https://github.com/reslava/nuget-package-reslava-result/tree/main/samples/lessons/){ .md-button }

---

<div class="grid cards" markdown>

-   :material-rocket-launch: __Getting Started__  
    Installation, quick start, and the transformation (70-90% less code).
    [](getting-started)

-   :material-cube-outline: __Core Concepts__  
    Functional programming foundation: Result, composition, async, Maybe, OneOf, validation, and more.
    [](core-concepts)

-   :material-api: __ASP.NET Integration__
    Minimal API, MVC, SmartEndpoints, OpenAPI, authorization, and problem details.
    [](aspnet)
    {: .is-featured }

-   :material-target: __Typed Error Pipelines__
    `ErrorsOf<T1..T8>` + `Result<TValue, TError>` — compile-time typed failure edges, exhaustive match.
    [](advanced/typed-pipelines)

-   :material-shield-check: __Safety Analyzers__
    7 Roslyn diagnostics + 3 code fixes — catch `Result<T>` and `OneOf` mistakes at compile time.
    [](advanced/safety-analyzers)

-   :material-puzzle: __Architecture & Design__
    How the library is built – SOLID, package structure, and the source generator pipeline.
    [](advanced/architecture)

-   :material-school: __Tutorial Series__
    9 progressive lessons — functional & railway-oriented programming from scratch. `REslava.Result` · `REslava.ResultFlow` · `REslava.Result.AspNetCore`. YouTube series coming soon.
    [](https://github.com/reslava/nuget-package-reslava-result/tree/main/samples/lessons/)

-   :material-language-csharp: __Code examples__
    Code examples: Fast APIs, Console and quick code examples.
    [](code-examples)

-   :material-test-tube: __Testing & Quality__
    3,339+ tests, CI/CD, real‑world impact, and production benefits.
    [](testing)

-   :material-play-circle: __Demo Project__
    Runnable console app — every `[ResultFlow]` and `[DomainBoundary]` feature with live generated diagrams.
    [](demo)

-   :material-bug-play: __Live Panel & Diagnostics__
    `REslava.Result.Diagnostics` — HTTP trace endpoint + VSIX Live panel with History / Single / Step / Replay modes. `▶ Debug` CodeLens streams per-node execution data into VS Code.
    [](reference/features)

-   :material-book-open-variant: __Reference__
    Version history, roadmap, and API documentation.
    [](reference)

-   :material-account-group: __Community__  
    Contributing, license, and acknowledgments.
    [](community)

</div>

---

## Why REslava.Result?

> **A Result pattern library for .NET with typed errors, Railway-Oriented pipelines, and automatic flow diagrams generated from your code.**

**What REslava.Result gives you:**

| Pillar | Features |
|---|---|
| **Railway-oriented pipelines** | `Result<T>`, `Bind`, `Map`, `Ensure`, `Tap`, `Or`, `MapError` — sync + async |
| **Typed error pipelines** | `Result<T,TError>`, `ErrorsOf<T1..T8>` — compile-time error type safety |
| **Rich domain errors** | `ValidationError`, `ForbiddenError`, `NotFoundError`, `ConflictError`, `ExceptionError` + `TagKey<T>`, `DomainTags`, `SystemTags` |
| **Error metadata & context** | `ReasonMetadata` (caller info), `ResultContext` (entity / correlation / tenant / operation), auto-enrichment |
| **Pipeline visualization** | `[ResultFlow]` → auto-generated Mermaid diagrams from your code |
| **Framework integrations** | ASP.NET Core Smart Endpoints, FluentValidation, OpenTelemetry, HTTP client mapping |
| **Safety analyzers** | RESL10xx / RESL20xx Roslyn analyzers enforcing correct usage patterns |

⚡ **[Performance benchmarks](reference/performance/)** — `Ok` creation **9.6× faster** than FluentResults · failure handling **6.8× faster** than exceptions · measured on .NET 9 with BenchmarkDotNet.

!!! example "Feature Comparison"
    | | REslava.Result | FluentResults | ErrorOr | LanguageExt |
    |---|:---:|:---:|:---:|:---:|
    | Result&lt;T&gt; pattern | ✅ | ✅ | ✅ | ✅ |
    | ✨ **Pipeline visualization (`[ResultFlow]`)** | **✅** | — | — | — |
    | **OneOf** discriminated unions | ✅ (2-8 types) | — | — | ✅ |
    | **Typed error pipelines** (`ErrorsOf` + `Result<T,TError>`) | **✅** | — | — | — |
    | Maybe&lt;T&gt; | ✅ | — | — | ✅ |
    | **ASP.NET source generators** (Minimal API + MVC) | **✅** | — | — | — |
    | **SmartEndpoints** (zero-boilerplate APIs) | **✅** | — | — | — |
    | **OpenAPI** metadata auto-generation | **✅** | — | — | — |
    | Authorization & Policy support | **✅** | — | — | — |
    | **Roslyn safety analyzers** | **✅** | — | — | — |
    | **JSON serialization** (System.Text.Json) | **✅** | — | — | — |
    | **Async patterns** (WhenAll, Retry, Timeout) | **✅** | — | — | — |
    | **Domain error hierarchy** (NotFound, Validation, etc.) | **✅** | — | Partial | — |
    | Validation framework | ✅ | Basic | — | ✅ |
    | **FluentValidation bridge** *(optional, migration only)* | **✅** | — | — | — |
    | Zero dependencies (core) | ✅ | ✅ | ✅ | — |
    | **[`Ok` creation speed](reference/performance/)** | **5.9 ns / 48 B** | 57 ns / 112 B | — | — |
    | **[Failure path vs exceptions](reference/performance/)** | **6.8× faster** | ~5.8× faster | — | — |

---

## Ready to Transform Your Error Handling?

**📖 [Start with the Getting Started Guide](getting-started)**

---

<div align="center" markdown>

**⭐ Star this REslava.Result repository if you find it useful!**

Made with ❤️ by [Rafa Eslava](https://github.com/reslava) for the developer community

[Report Bug](https://github.com/reslava/nuget-package-reslava-result/issues) • [Request Feature](https://github.com/reslava/nuget-package-reslava-result/issues) • [Discussions](https://github.com/reslava/nuget-package-reslava-result/discussions)

</div>