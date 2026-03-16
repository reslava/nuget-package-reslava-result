# REslava.Result v1.44.1

## Visual Result pipelines for .NET — branding refresh + NuGet metadata update

This patch release updates the library's public-facing descriptions, README hero, and NuGet metadata to reflect what the library actually is today. No API changes. No breaking changes.

---

## What's New

### Library Branding

- **Tagline**: "Visual Result pipelines for .NET"
- **Slogan**: *"Don't try to understand the pipeline—watch the flow."*
- **Showcase Mermaid diagram** — `RegisterUserAsync` pipeline in real generator format added above the fold in README; shows type travel, async markers (`⚡`), and named error edges (`ValidationError`, `ConflictError`, `DatabaseError`)

### NuGet `<Description>` Updated — All 8 Packages

All package descriptions now accurately reflect the full feature set: typed errors, railway-oriented pipelines, pipeline visualization, Roslyn safety analyzers, and framework integrations.

### NuGet README Opening Sections — All 8 Packages

Opening sections in all `docs/nuget/README.*.md` rewritten with updated tagline and feature summaries.

> **Note for `REslava.Result.AspNetCore` users**: the NuGet README was still titled `REslava.Result.SourceGenerators` (old package name). Fixed in this release.

### `REslava.Result.Flow` — Now Clearly the Primary Package

README section 3 and `mkdocs/resultflow/index.md` restructured:
- `REslava.Result.Flow` (full semantic analysis, requires REslava.Result) → **primary**
- `REslava.ResultFlow` (syntax-only, library-agnostic) → **secondary / alternative**

### MkDocs Frontmatter Audit

- `mkdocs/index.md` — mirrored to match new README hero (tagline, slogan, showcase diagram, pillars table)
- `mkdocs/advanced/index.md` — added `description` + `tagline` frontmatter
- `mkdocs/advanced/typed-pipelines/index.md` — added `description` + `tagline` frontmatter
- `mkdocs/testing/index.md` — test count updated to 4,500+; `netstandard2.0` added to TFM list

### Script Fix

`organize_docs.py` — sections 27–28 (Acknowledgments, Contributors) added to the community routing MAPPING. Fixes `27.--acknowledgments.md` and `28.-contributors.md` landing as bad filenames on the next MkDocs generation run.

---

## Stats

- Tests: >4,500 passing (unchanged)
- 187 features across 15 categories

---

## NuGet Packages

| Package | Link |
|---|---|
| REslava.Result | [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.44.1) |
| REslava.Result.Flow | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Flow/1.44.1) |
| REslava.Result.AspNetCore | [View on NuGet](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.44.1) |
| REslava.Result.Http | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Http/1.44.1) |
| REslava.Result.Analyzers | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.44.1) |
| REslava.Result.OpenTelemetry | [View on NuGet](https://www.nuget.org/packages/REslava.Result.OpenTelemetry/1.44.1) |
| REslava.ResultFlow | [View on NuGet](https://www.nuget.org/packages/REslava.ResultFlow/1.44.1) |
| REslava.Result.FluentValidation | [View on NuGet](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.44.1) |
