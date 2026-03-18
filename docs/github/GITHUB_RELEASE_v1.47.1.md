# REslava.Result v1.47.1

## Diagram Title & Root Entry Node — Every Pipeline Now Fully Self-Documenting

Two pieces of information that were missing from every generated `[ResultFlow]` diagram are now always present.

### What's new

**Mermaid frontmatter title** — the annotated method name is rendered as a native heading above the diagram in all Mermaid-capable viewers:

```
---
title: PlaceOrder
---
flowchart LR
    ...
```

**Root entry node (`ENTRY_ROOT`)** — the chain seed call (e.g. `FindUser`) is rendered as a labelled amber `:::operation` node with a thick `==>` arrow to the first pipeline step. Async seed calls receive a `⚡` marker. The full call surface is now visible without reading the source code:

```
ENTRY_ROOT["FindUser<br/>→ User"]:::operation ==> N0_Bind
```

Both `REslava.Result.Flow` (semantic) and `REslava.ResultFlow` (syntax-only) updated in parity.

---

### Code action fixes

The **Insert / Refresh `[ResultFlow]` diagram comment** code action received three fixes:

- **Title + entry node** — previously the action called the renderer with no arguments; title and `ENTRY_ROOT` were absent from all inserted comments
- **Refresh parity** (`REslava.ResultFlow`) — now replaces an existing diagram block in-place instead of inserting a duplicate; shows "Refresh" title when a block already exists
- **CRLF normalisation** — normalises comment content to `\n` before insertion, preventing a spurious blank line in the correlation block on a second refresh on Windows

---

### `generate_flow_catalog.py` fix

The catalog script now correctly parses constants in the new compact `@"content";` format — previously it only matched the old multi-line `@"\n...\n";` format and silently produced an empty catalog after the verbatim string change.

---

## Stats

- Tests: 4,638 passing (floor: >4,500)
- Features: 197 across 15 categories

---

## NuGet Packages

| Package | Link |
|---|---|
| REslava.Result | [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.47.1) |
| REslava.Result.Flow | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Flow/1.47.1) |
| REslava.Result.AspNetCore | [View on NuGet](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.47.1) |
| REslava.Result.Http | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Http/1.47.1) |
| REslava.Result.Analyzers | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.47.1) |
| REslava.Result.OpenTelemetry | [View on NuGet](https://www.nuget.org/packages/REslava.Result.OpenTelemetry/1.47.1) |
| REslava.ResultFlow | [View on NuGet](https://www.nuget.org/packages/REslava.ResultFlow/1.47.1) |
| REslava.Result.FluentValidation | [View on NuGet](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.47.1) |
