# REslava.Result

**Railway-oriented programming for .NET** — type-safe error handling without exceptions.

[![NuGet](https://img.shields.io/nuget/v/REslava.Result?logo=nuget)](https://www.nuget.org/packages/REslava.Result)
[![Downloads](https://img.shields.io/nuget/dt/REslava.Result)](https://www.nuget.org/packages/REslava.Result)
[![License](https://img.shields.io/badge/license-MIT-green)](https://github.com/reslava/nuget-package-reslava-result/blob/main/LICENSE)

## Why REslava.Result?

- **Result\<T\> + OneOf + Maybe\<T\>** in a single zero-dependency package
- **Implicit conversions** — just `return user;` or `return new NotFoundError();`
- **LINQ query syntax** — compose results with `from ... in ... select`
- **Roslyn safety analyzers** — catch unsafe `.Value` access at compile time
- **SmartEndpoints** — auto-generate complete ASP.NET Minimal API endpoints (separate package)

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
| [REslava.Result.SourceGenerators](https://www.nuget.org/packages/REslava.Result.SourceGenerators) | Auto-generate ASP.NET Minimal API endpoints, IResult conversions, and OpenAPI metadata |
| [REslava.Result.Analyzers](https://www.nuget.org/packages/REslava.Result.Analyzers) | Roslyn analyzers that catch unsafe Result/OneOf usage at compile time |

## Links

- [GitHub Repository](https://github.com/reslava/nuget-package-reslava-result) — Full documentation, architecture guide, samples
- [Changelog](https://github.com/reslava/nuget-package-reslava-result/blob/main/CHANGELOG.md)
- [API Samples](https://github.com/reslava/nuget-package-reslava-result/tree/main/samples)

**MIT License** | .NET 8 / 9 / 10
