# REslava.Result v1.44.0

`REslava.Result` now targets `netstandard2.0` alongside `net8/9/10`, fixing its invisibility in the default NuGet search and expanding compatibility to .NET Framework, Unity, and Xamarin.

---

## ✨ NuGet Discoverability Fix — `netstandard2.0` Target

**Root cause**: NuGet's default search uses "framework filter mode = all", which requires compatibility with netcoreapp + netstandard + netframework families. Packages targeting only `net8/9/10` fail the netstandard/netframework check and disappear from results — even though all satellite packages (analyzers, generators, FluentValidation bridge) already target `netstandard2.0` and showed up first.

**Fix**: `REslava.Result` now targets all four:
```
netstandard2.0 · net8.0 · net9.0 · net10.0
```

### Compatibility gained

| Runtime | Min version |
|---|---|
| .NET 8 / 9 / 10 | unchanged |
| .NET Framework | 4.6.1+ |
| .NET Core | 2.0+ |
| Mono / Xamarin | all |
| Unity | 2021.2+ |

### Feature availability on netstandard2.0

All core features are available. Two APIs require a modern runtime:

| API | Minimum runtime |
|---|---|
| `IErrorFactory<TSelf>` / `Result.Fail<TError>(string)` | .NET 7+ (`static abstract` runtime support) |
| `CancellationTokenSource.CancelAsync()` in timeout extensions | .NET 8+ |

---

## 🔧 NuGet Metadata Improvements

- **Title**: `REslava.Result – Result Pattern for .NET | Railway-Oriented Programming`
- **Tags added**: `reslava`, `result-pattern`, `railway-oriented-programming`, `functional-programming`

NuGet search weights title and tag matches — these additions improve organic ranking for the most common search terms.

---

## 📦 NuGet

| Package | Link |
|---------|------|
| REslava.Result | [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.44.0) |
| REslava.Result.Flow | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Flow/1.44.0) |
| REslava.Result.AspNetCore | [View on NuGet](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.44.0) |
| REslava.Result.Http | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Http/1.44.0) |
| REslava.Result.Analyzers | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.44.0) |
| REslava.Result.OpenTelemetry | [View on NuGet](https://www.nuget.org/packages/REslava.Result.OpenTelemetry/1.44.0) |
| REslava.ResultFlow | [View on NuGet](https://www.nuget.org/packages/REslava.ResultFlow/1.44.0) |
| REslava.Result.FluentValidation | [View on NuGet](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.44.0) |

---

## Stats

- Tests: >4,500 passing
- 187 features across 15 categories
