---
hide:
  - navigation
title: ResultFlow — Runtime Observation
description: Pipeline runtime tracing, FlowProxy, RingBufferObserver, Debug Panel, pipeline identity, and registry generator — what happens at execution time.
---

# ResultFlow — Runtime Observation

<div class="grid cards" markdown>

-   :material-transit-connection: __🔀 FlowProxy — Always-On Tracing + Debug Mode (v1.53.0)__

    Make your class `partial` and the generator emits a FlowProxy — always-on tracing via `svc.Flow.Process()` and single-trace debug via `svc.Flow.Debug.Process()`.

    [](flowproxy--always-on-tracing--debug-mode-v1.53.0)

-   :material-microscope: __🔬 Pipeline Runtime Observation — FlowProxy + RingBufferObserver (v1.52.0)__

    Per-node output values, error types, and elapsed milliseconds for every execution. `RingBufferObserver` keeps the last N traces in memory with zero allocation overhead.

    [](pipeline-runtime-observation--flowproxy--ringbufferobserver-v1.52.0)

-   :material-folder-open: __💾 Debug Panel — File-Drop Workflow + Multi-File Picker (v1.53.0)__

    `RingBufferObserver.Save()` persists traces as JSON. The VSIX watches `**/reslava-*.json` and opens the Debug panel automatically — no HTTP server, no blocking call.

    [](debug-panel--file-drop-workflow--multi-file-picker-v1.53.0)

-   :material-wrench: __🛠️ Debug Panel — nodeId Subchain Fix + VSIX v1.4.1 (v1.54.0)__

    Generator fix for `_nodeIds_` arrays in cross-method pipelines. VSIX v1.4.1 polish: subgraph ENTRY highlight, node sort by index, output word-wrap, toolbar button fix.

    [](debug-panel--nodeid-subchain-fix--vsix-v1.4.1-v1.54.0)

-   :material-key: __🔑 Pipeline Identity — PipelineId, NodeId & Namespace (v1.51.0)__

    Each `[ResultFlow]` registry `_Info` now includes a stable `PipelineId` (FNV-1a hash), `NodeId` array, and `Namespace` — enabling runtime ↔ diagram correlation.

    [](pipeline-identity--pipelineid-nodeid--namespace-v1.51.0)

-   :material-card-bulleted: __🗂️ Pipeline Registry Generator (v1.50.0)__

    Both packages emit an always-on `{Class}_Registry.g.cs` alongside the diagram constants — a static registry of every `[ResultFlow]` method with its metadata.

    [](pipeline-registry-generator-v1.50.0)

</div>