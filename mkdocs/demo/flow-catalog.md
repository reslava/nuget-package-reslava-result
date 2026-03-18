---
title: Flow Catalog — Demo Project
description: All pipeline and architecture diagrams auto-generated from the REslava.Result.Flow.Demo project — updated on every release.
---

# 🗺️ Flow Catalog — Demo Project

> Every diagram below is **auto-generated** from the [`REslava.Result.Flow.Demo`](https://github.com/reslava/nuget-package-reslava-result/tree/main/samples/REslava.Result.Flow.Demo) project by `scripts/generate_flow_catalog.py` — the same output you get when you annotate your own methods with `[ResultFlow]`.

The script scans `obj/Generated/**/*_Flows.g.cs`, extracts every Mermaid constant, and publishes it to the documentation site on every release. Zero manual work.

!!! info
    This catalog is regenerated automatically on every release. Do not edit manually.

---

## Run it yourself

Generate the catalog against any project that has `EmitCompilerGeneratedFiles=true`:

```bash
# Against the demo (default)
python scripts/generate_flow_catalog.py

# Against your own project
python scripts/generate_flow_catalog.py \
    --project path/to/MyProject \
    --output path/to/output.md
```

First build the target project so the generated `.cs` files exist:

```bash
dotnet build samples/REslava.Result.Flow.Demo
python scripts/generate_flow_catalog.py
```

---

[→ View the full Architectural Flow Catalog](../reference/flow-catalog){ .md-button .md-button--primary }

The catalog groups every generated diagram by class → method → view type (Pipeline, Layer View, Stats, Error Surface, Error Propagation), so you can browse the full output of the demo project end-to-end.
