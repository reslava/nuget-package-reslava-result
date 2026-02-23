# v1.25.0 — Documentation Site & API Reference

## Documentation Website

The library now has a fully generated documentation website at
[reslava.github.io/nuget-package-reslava-result](https://reslava.github.io/nuget-package-reslava-result/)
built automatically from `README.md` on every push to main.

- **MkDocs Material** site with dark/light mode, search, social cards, git revision dates
- **8 navigation sections**: Getting Started · Core Concepts · ASP.NET · Architecture · Code Examples · Testing · Reference · Community
- Automatic pipeline: `mdsplit` splits README → 9 Python scripts transform/organize → `mkdocs build`

## DocFX API Reference

Auto-generated API reference from XML docs at
[/reference/api](https://reslava.github.io/nuget-package-reslava-result/reference/api/index.html)
with a new Bootstrap landing page:

- **Namespace cards** with descriptions and direct links (REslava.Result, AdvancedPatterns, Extensions, Serialization)
- **Core Types at a Glance** grid — Result&lt;T&gt;, Maybe&lt;T&gt;, OneOf&lt;T&gt;, Error hierarchy, Reason&lt;T&gt;, Extensions
- Quick-links to main docs, GitHub repo, and all 3 NuGet packages

## CI/CD Improvements

- **CI workflow** now uses path allowlist (`src/**`, `tests/**`, `Directory.Build.props`) — no longer fires on docs-only commits
- **Docs workflow** trigger extended to include `docfx/**`; self-reference path corrected
- `organize_docs.py` path typo fixed (`reference/api-doc` was `reference/api-docs`)

## Test Suite

- 2,843 tests passing across net8.0, net9.0, net10.0

## NuGet Packages

- [REslava.Result 1.25.0](https://www.nuget.org/packages/REslava.Result/1.25.0)
- [REslava.Result.SourceGenerators 1.25.0](https://www.nuget.org/packages/REslava.Result.SourceGenerators/1.25.0)
- [REslava.Result.Analyzers 1.25.0](https://www.nuget.org/packages/REslava.Result.Analyzers/1.25.0)
