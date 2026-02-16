# REslava.Result.Analyzers

**Compile-time safety for Result\<T\> and OneOf** — catch mistakes before they reach production.

[![NuGet](https://img.shields.io/nuget/v/REslava.Result.Analyzers?logo=nuget)](https://www.nuget.org/packages/REslava.Result.Analyzers)
[![Downloads](https://img.shields.io/nuget/dt/REslava.Result.Analyzers)](https://www.nuget.org/packages/REslava.Result.Analyzers)
[![License](https://img.shields.io/badge/license-MIT-green)](https://github.com/reslava/nuget-package-reslava-result/blob/main/LICENSE)

## Diagnostics

| ID | Title | Severity | Code Fix |
|----|-------|----------|----------|
| **RESL1001** | Unsafe `Result<T>.Value` access without `IsSuccess` check | Warning | Yes (2 options) |
| **RESL1002** | Discarded `Result<T>` return value | Warning | -- |
| **RESL1003** | Prefer `Match()` over if-check when both branches access `.Value`/`.Errors` | Info | -- |
| **RESL2001** | Unsafe `OneOf.AsT*` access without `IsT*` check | Warning | Yes |

## Examples

```csharp
var result = GetUser(id);

var user = result.Value;           // Warning RESL1001: Unsafe access without IsSuccess check

DoSomething();                     // Warning RESL1002: Result<T> return value discarded
// (DoSomething returns Result<T> but it's ignored)

if (result.IsSuccess)              // Info RESL1003: Consider using Match()
    Console.Write(result.Value);
else
    Console.Write(result.Errors);

var item = oneOf.AsT1;             // Warning RESL2001: Access to '.AsT1' without checking '.IsT1'
```

## Code Fixes

**RESL1001** offers two automatic fixes:
```csharp
// Fix A: Wrap in guard
if (result.IsSuccess) { var user = result.Value; }

// Fix B: Replace with Match
var user = result.Match(v => v, e => default);
```

**RESL2001** replaces unsafe access with Match:
```csharp
// Before
var user = oneOf.AsT1;

// After (auto-fixed)
var user = oneOf.Match(t1 => t1, t2 => throw new NotImplementedException());
```

## Quick Start

```bash
dotnet add package REslava.Result.Analyzers
```

That's it. Zero configuration needed. Diagnostics appear in your IDE immediately.

## Requires

- [REslava.Result](https://www.nuget.org/packages/REslava.Result) (core library)

## Links

- [GitHub Repository](https://github.com/reslava/nuget-package-reslava-result) — Full documentation
- [Changelog](https://github.com/reslava/nuget-package-reslava-result/blob/main/CHANGELOG.md)

**MIT License** | Works with any .NET version (netstandard2.0 analyzer)
