# v1.20.0 — Domain Errors + Internal Quality Release

## Structured Error Hierarchy (Domain Errors)

Five built-in domain error types with HTTP semantics — no more reinventing `NotFoundError` in every project:

```csharp
// Built-in domain errors with HTTP status codes
Result<User>.Fail(new NotFoundError("User", userId));        // 404
Result<User>.Fail(new ValidationError("Email", "Invalid"));  // 422
Result<User>.Fail(new ConflictError("User", "email", email));// 409
Result<User>.Fail(new UnauthorizedError());                  // 401
Result<User>.Fail(new ForbiddenError("delete", "admin"));    // 403

// All carry HttpStatusCode tag — auto-mapped by ResultToIResult generator
var result = await _service.GetUserAsync(id);
return result.ToIResult(); // NotFoundError → 404, ValidationError → 422, etc.

// Pattern matching
if (result.Errors.First() is NotFoundError notFound)
    Console.WriteLine($"{notFound.Tags["EntityName"]} not found");

// Fluent chaining preserved via CRTP
var error = new NotFoundError("User", 42)
    .WithTag("RequestId", "abc-123")
    .WithMessage("Custom message");   // Returns NotFoundError, not base type
```

## ResultToIResult Generator — Domain Error-Aware

The `ToIResult()` family now reads `HttpStatusCode` tags from domain errors instead of always returning 400:

| Error Type | HTTP Result |
|---|---|
| `NotFoundError` (404) | `Results.NotFound(message)` |
| `UnauthorizedError` (401) | `Results.Unauthorized()` |
| `ForbiddenError` (403) | `Results.Forbid()` |
| `ConflictError` (409) | `Results.Conflict(message)` |
| `ValidationError` (422) | `Results.Problem(detail, statusCode: 422)` |
| No tag / other | `Results.Problem(detail, statusCode: 400)` |

Also supports legacy `StatusCode` tag for backward compatibility.

## Test Coverage Hardening

123 new tests filling critical gaps in core factory methods and extensions:

- **OkIf/FailIf** — 39 tests (all overloads: non-generic, generic, lazy, async)
- **Try/TryAsync** — 15 tests (success, exception, custom handler, cancellation)
- **Merge/Combine** — 18 tests (all success, mixed, empty, parallel async)
- **TapOnFailure/TapBoth** — 30 tests (single error, all errors, async variants)
- **LINQ Task extensions** — 21 tests (SelectManyAsync, SelectAsync, WhereAsync)

## Internal Quality Fixes

- **ExceptionError namespace** — moved from global namespace to `REslava.Result`
- **Result\<T\> constructors** — changed from `public` to `internal` (prevents invalid state construction)
- **Cached computed properties** — `Errors`/`Successes` lazy-cached, `IsFailed` uses `Errors.Count > 0`
- **Result.ToString()** — now returns `Result: IsSuccess='True', Reasons=[...]`
- **SmartEndpoints route prefix** — convention-based from class name instead of hard-coded `/api/test`
- **Dead code cleanup** — deleted 7 files (duplicate HttpStatusCodeMapper, orphan generators, stale tests)
- **ValidationResult.Failure** — now creates `ValidationError` instead of generic `Error`

## Demo App: Library Domain Errors Showcase

Deleted 12 custom error classes across 3 files, replaced with 5 library domain errors. Simplified OneOf signatures (e.g., `OneOf4` → `OneOf2` by collapsing validation subtypes).

## Test Suite

- 2,798 tests passing across net8.0, net9.0, net10.0
- 150 new tests in this release
- 54 analyzer tests, 56 source generator tests

## NuGet Packages

- [REslava.Result 1.20.0](https://www.nuget.org/packages/REslava.Result/1.20.0)
- [REslava.Result.SourceGenerators 1.20.0](https://www.nuget.org/packages/REslava.Result.SourceGenerators/1.20.0)
- [REslava.Result.Analyzers 1.20.0](https://www.nuget.org/packages/REslava.Result.Analyzers/1.20.0)
