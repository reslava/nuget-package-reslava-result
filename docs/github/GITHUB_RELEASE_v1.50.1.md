# REslava.Result v1.50.1

## 🐛 Bug Fixes

### Registry Generator — non-Result return types

Both `REslava.Result.Flow` and `REslava.ResultFlow` registry generators now correctly include `[ResultFlow]`-decorated methods **regardless of return type**.

Previously, methods that return a non-`Result` type (e.g. Match-terminal pipelines that call `.Match()` and return `string`, `bool`, etc.) were excluded from `{ClassName}_PipelineRegistry.g.cs`. These methods are valid `[ResultFlow]` pipelines and should always appear in the registry and the VSIX sidebar.

---

### Registry Generator — `sourceLine` off-by-one

`sourceLine` in `{MethodName}_Info` JSON is now stored **1-based** (Roslyn `StartLinePosition.Line + 1`). Previously it was stored as 0-based, causing the VSIX "Go to Source" command to navigate to the line immediately above the method declaration.

---

### VSIX v1.2.1 — sidebar stats

The stats bar above the ⚡ Flow Catalog tree now counts only projects that have at least one `[ResultFlow]` method visible in the tree. Previously the count reflected every `.csproj` found in the workspace, inflating the project number to include test projects, demo projects, etc.

---

## 🧪 Tests

**Total: 4,688 tests passing**

---

## NuGet Packages

| Package | Version |
|---------|---------|
| [REslava.Result](https://www.nuget.org/packages/REslava.Result/1.50.1) | 1.50.1 |
| [REslava.Result.Flow](https://www.nuget.org/packages/REslava.Result.Flow/1.50.1) | 1.50.1 |
| [REslava.Result.AspNetCore](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.50.1) | 1.50.1 |
| [REslava.Result.Http](https://www.nuget.org/packages/REslava.Result.Http/1.50.1) | 1.50.1 |
| [REslava.Result.Analyzers](https://www.nuget.org/packages/REslava.Result.Analyzers/1.50.1) | 1.50.1 |
| [REslava.Result.OpenTelemetry](https://www.nuget.org/packages/REslava.Result.OpenTelemetry/1.50.1) | 1.50.1 |
| [REslava.ResultFlow](https://www.nuget.org/packages/REslava.ResultFlow/1.50.1) | 1.50.1 |
| [REslava.Result.FluentValidation](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.50.1) | 1.50.1 |

## VS Code Extension

| Extension | Version |
|-----------|---------|
| [REslava.Result Extensions](https://marketplace.visualstudio.com/items?itemName=reslava.reslava-result-extensions) | v1.2.1 |
