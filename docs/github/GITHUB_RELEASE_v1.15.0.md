# REslava.Result v1.15.0 — Project Cleanup & Documentation Refresh

> **Leaner repo, zero Node.js dependencies, refreshed docs — same 2,004+ tests, all green.**

---

## What's Changed

### Removed: Node.js Toolchain

The project no longer depends on Node.js for any workflow:
- `package.json`, `package-lock.json`, `.versionrc.json` — replaced by `Directory.Build.props` + GitHub Actions `release.yml`
- `.husky/` + `commitlint.config.js` — commit validation now handled by CI/CD pipeline
- `scripts/` (5 files) — PowerShell/Bash scripts superseded by CI/CD pipeline

### Removed: Legacy Directories

- `templates/` — incomplete, unpublished dotnet template
- `samples/NuGetValidationTest/` — stale test project referencing v1.9.0 packages

### Documentation Refresh

- Standardized emoji: `🏗️` → `📐` across 34 markdown files (fixed Unicode anchor link issues)
- Updated README.md Roadmap (v1.15.0 current)
- Removed speculative "Future Versions" section
- Rewrote `samples/README.md` to reflect actual sample projects
- Updated test counts to 2,004+

---

## Package Updates

| Package | Version | Description |
|---------|---------|-------------|
| `REslava.Result` | v1.15.0 — [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.15.0) | Core library |
| `REslava.Result.SourceGenerators` | v1.15.0 — [View on NuGet](https://www.nuget.org/packages/REslava.Result.SourceGenerators/1.15.0) | ASP.NET source generators |
| `REslava.Result.Analyzers` | v1.15.0 — [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.15.0) | Roslyn safety analyzers |

---

## Testing

- **2,004+ total tests** across all packages and TFMs
- All tests green — no functional changes in this release

---

## Breaking Changes

None. This is a cleanup-only release with no API or behavioral changes.

---

**MIT License** | [Full Changelog](../../CHANGELOG.md)
