---
title: Demo Project
description: A runnable console app showcasing every REslava.Result.Flow feature — see real generated diagrams from real application code.
---

# 🎮 Demo Project

> **Real code. Real diagrams.** The `REslava.Result.Flow.Demo` project is a runnable console app that exercises every `[ResultFlow]` and `[DomainBoundary]` feature end-to-end — guard chains, risk chains, async pipelines, cross-method tracing, clickable nodes, and domain boundary analysis.

**[:material-github: View source on GitHub](https://github.com/reslava/nuget-package-reslava-result/tree/main/samples/REslava.Result.Flow.Demo)**

```bash
cd samples/REslava.Result.Flow.Demo
dotnet run
```

---

## Features demonstrated

| # | Feature | Version |
|---|---------|---------|
| 1 | Guard chain — `Ensure × 3` with typed error edges | v1.37.0 |
| 2 | Risk chain — `Bind × 2` with type travel | v1.38.0 |
| 3 | Multi-hop type travel — `User → Product → Order → string` | v1.38.0 |
| 4 | Async pipeline — `⚡` step markers + typed errors | v1.38.0 |
| 5 | Full pipeline — all NodeKinds end-to-end | v1.38.0 |
| 6 | Cross-method tracing — `[ResultFlow(MaxDepth = 2)]` expands `Bind` into a `subgraph` | v1.45.0 |
| 7 | Sidecar constant — writes `PlaceOrderCross.ResultFlow.md` to disk | v1.43.0 |
| 8 | Clickable nodes — `ResultFlowLinkMode = vscode` | v1.43.0 |
| 9 | Domain boundary diagrams — `[DomainBoundary]` triggers `_LayerView`, `_Stats`, `_ErrorSurface`, `_ErrorPropagation` | v1.45.0 |
| 10 | Match multi-branch fan-out — hexagon + typed N-branch `-->|TypeName| FAIL` edges | v1.46.0 |
| 11 | Architectural Flow Catalog — all generated diagrams published to MkDocs via `scripts/generate_flow_catalog.py` | v1.47.0 |

---

## What the generator produces

Every `[ResultFlow]`-annotated method gets a set of generated constants in a `*_Flows.g.cs` file:

| Constant | What it shows |
|---|---|
| `{MethodName}` (no suffix) | Full pipeline — success path, failure edges, async markers |
| `{MethodName}_LayerView` | Layer → Class → Method subgraphs, color-coded by domain layer |
| `{MethodName}_Stats` | Step count, async steps, error types, layers crossed, max depth |
| `{MethodName}_ErrorSurface` | Fail-edges only — the full failure surface at a glance |
| `{MethodName}_ErrorPropagation` | Error types grouped by the layer they originate from |
| `{MethodName}_Sidecar` | Same as pipeline, wrapped in a fenced markdown block (write to disk) |

[→ See all generated diagrams from the demo](flow-catalog.md){ .md-button }
