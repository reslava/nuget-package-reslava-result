# REslava.Result

**Visual Result pipelines for .NET** — typed errors, Railway-Oriented pipelines, automatic flow diagrams.

*Don't try to understand the pipeline—watch the flow.*

[![NuGet](https://img.shields.io/nuget/v/REslava.Result?logo=nuget)](https://www.nuget.org/packages/REslava.Result)
[![Downloads](https://img.shields.io/nuget/dt/REslava.Result)](https://www.nuget.org/packages/REslava.Result)
[![License](https://img.shields.io/badge/license-MIT-green)](https://github.com/reslava/nuget-package-reslava-result/blob/main/LICENSE)

<p align="center">
  <a href="https://github.com/reslava/nuget-package-reslava-result">
    <img src="https://raw.githubusercontent.com/reslava/nuget-package-reslava-result/main/images/diagram-pipeline.svg" width="700" />
  </a>
</p>
<p align="center"><em>Auto-generated pipeline diagram — success path, typed error edges, async steps</em></p>

<p align="center">
  <a href="https://github.com/reslava/nuget-package-reslava-result">
    <img src="https://raw.githubusercontent.com/reslava/nuget-package-reslava-result/main/images/layerview-architecture-diagram.svg" width="700" />
  </a>
</p>
<p align="center"><em>Architecture layer view — Domain / Application / Infrastructure boundaries, auto-detected from namespaces</em></p>

<p align="center"><a href="https://github.com/reslava/nuget-package-reslava-result">→ Full documentation and diagram gallery on GitHub</a></p>

## Why REslava.Result?

- **Result\<T\> + OneOf (2–8 types) + Maybe\<T\>** in a single zero-dependency package
- Use **Result<TValue, TError>** and **ErrorsOf<T1..T8>** — **Typed Error Pipelines** when you want compile-time-known, exhaustive failure edges
- **Domain error hierarchy** — `NotFoundError`, `ValidationError`, `ConflictError`, `UnauthorizedError`, `ForbiddenError` with HTTP status code tags
- **Implicit conversions** — just `return user;` or `return new NotFoundError();`
- **LINQ query syntax** — compose results with `from ... in ... select`
- **Async patterns** — `WhenAll` (typed tuples), `Retry` (exponential backoff), `Timeout`
- **JSON serialization** — `System.Text.Json` converters for `Result<T>`, `OneOf`, `Maybe<T>`
- **Native Validation DSL** — 19 fluent extension methods (`NotEmpty`, `MaxLength`, `EmailAddress`, `Range`, `Positive`, ...) on `ValidatorRuleBuilder<T>` with auto-inferred field names
- **Roslyn safety analyzers** — catch unsafe `.Value` access at compile time (separate package)
- **ASP.NET integration** — auto-generate Minimal API + MVC endpoints with domain error mapping (separate package)

No other .NET library combines all of these.

## Before / After

```csharp
// Before: Exception-based control flow
try {
    var user = await GetUser(id);
    var account = await GetAccount(user.AccountId);
    return Ok(new Summary(user.Name, account.Balance));
}
catch (NotFoundException ex) { return NotFound(ex.Message); }
catch (ValidationException ex) { return BadRequest(ex.Message); }

// After: Railway-oriented with REslava.Result
var result = await GetUser(id)
    .BindAsync(user => GetAccount(user.AccountId))
    .MapAsync(account => new Summary(account.User.Name, account.Balance));

return result.Match(
    onSuccess: summary => Results.Ok(summary),
    onFailure: errors => Results.BadRequest(errors));
```

## Quick Start

```bash
dotnet add package REslava.Result
```

```csharp
using REslava.Result;

// Create results
Result<User> success = new User("Alice", "alice@example.com");  // implicit conversion
Result<User> failure = new NotFoundError("User not found");     // implicit conversion

// Transform and compose
var result = await GetUser(id)
    .Map(user => user.Email)
    .Ensure(email => email.Contains("@"), "Invalid email")
    .Bind(email => SendWelcomeEmail(email));

// Handle both cases
var message = result.Match(
    onSuccess: value => $"Sent to {value}",
    onFailure: errors => $"Failed: {errors.First().Message}");
```

## Companion Packages

| Package | Description |
|---------|-------------|
| [REslava.Result.Flow](https://www.nuget.org/packages/REslava.Result.Flow) | Visualises `Result<T>` pipelines with **full type travel and typed error surface** in generated Mermaid diagrams |
| [REslava.Result.AspNetCore](https://www.nuget.org/packages/REslava.Result.AspNetCore) | Auto-generate ASP.NET endpoints (Minimal API + MVC), IResult/IActionResult conversions, OneOf extensions, OpenAPI metadata |
| [REslava.Result.Http](https://www.nuget.org/packages/REslava.Result.Http) | Wrap `HttpClient` calls so every HTTP response and network failure becomes a typed `Result<T>` |
| [REslava.Result.Analyzers](https://www.nuget.org/packages/REslava.Result.Analyzers) | Roslyn analyzers that catch unsafe Result/OneOf usage at compile time |
| [REslava.Result.OpenTelemetry](https://www.nuget.org/packages/REslava.Result.OpenTelemetry) | Zero-cost OpenTelemetry integration — seeds `ResultContext` from the active span and writes error tags as span attributes on failure |
| [FluentValidation](https://www.nuget.org/packages/FluentValidation) | ≥ 11.x (installed by user) ⚠️ **Optional**  FluentValidation bridge |

## Links

- [GitHub Repository](https://github.com/reslava/nuget-package-reslava-result) — Full documentation, architecture guide, samples
- [Changelog](https://github.com/reslava/nuget-package-reslava-result/blob/main/CHANGELOG.md)
- [API Samples](https://github.com/reslava/nuget-package-reslava-result/tree/main/samples)

**MIT License** | .NET Framework 4.6.1+ / .NET Core 2.0+ / .NET 8 / 9 / 10