# REslava.Result.Http

**HttpClient extensions that return `Result<T>` instead of throwing.**

Map HTTP 4xx/5xx status codes to typed domain errors and wrap network failures in `ExceptionError` — no `try/catch`, no manual status code checks.

[![NuGet](https://img.shields.io/nuget/v/REslava.Result.Http?logo=nuget)](https://www.nuget.org/packages/REslava.Result.Http)
[![Downloads](https://img.shields.io/nuget/dt/REslava.Result.Http)](https://www.nuget.org/packages/REslava.Result.Http)
[![License](https://img.shields.io/badge/license-MIT-green)](https://github.com/reslava/nuget-package-reslava-result/blob/main/LICENSE)

## What It Does

```csharp
// Before — boilerplate repeated in every service/repository
var response = await httpClient.GetAsync($"/api/users/{id}");
if (!response.IsSuccessStatusCode)
    return Result<User>.Fail(new NotFoundError("User", id));
var user = await response.Content.ReadFromJsonAsync<User>();
return Result<User>.Ok(user!);

// After
Result<User> result = await httpClient.GetResult<User>($"/api/users/{id}");
Result<Order> posted = await httpClient.PostResult<CreateOrderDto, Order>("/api/orders", dto);
Result<Order> updated = await httpClient.PutResult<UpdateOrderDto, Order>($"/api/orders/{id}", dto);
Result deleted = await httpClient.DeleteResult($"/api/orders/{id}");
```

## Quick Start

```bash
dotnet add package REslava.Result
dotnet add package REslava.Result.Http
```

```csharp
using REslava.Result.Http;

// In a service/repository
public async Task<Result<User>> GetUserAsync(int id, CancellationToken ct = default)
    => await _httpClient.GetResult<User>($"/api/users/{id}", cancellationToken: ct);

public async Task<Result<User>> CreateUserAsync(CreateUserDto dto, CancellationToken ct = default)
    => await _httpClient.PostResult<CreateUserDto, User>("/api/users", dto, cancellationToken: ct);
```

## Available Methods

| Method | Returns |
|--------|---------|
| `GetResult<T>(string \| Uri)` | `Task<Result<T>>` |
| `PostResult<TBody, TResponse>(string, TBody)` | `Task<Result<TResponse>>` |
| `PutResult<TBody, TResponse>(string, TBody)` | `Task<Result<TResponse>>` |
| `DeleteResult(string)` | `Task<Result>` |
| `DeleteResult<T>(string)` | `Task<Result<T>>` |

All methods accept optional `HttpResultOptions?` and `CancellationToken`.

## Default Error Mapping

| HTTP Status | Error Type | Default Message |
|-------------|-----------|----------------|
| 2xx | ✅ Success — deserializes body | — |
| 404 | `NotFoundError` | `"Resource not found"` |
| 401 | `UnauthorizedError` | `"Authentication required"` |
| 403 | `ForbiddenError` | `"Access denied"` |
| 409 | `ConflictError` | `"A conflict occurred"` |
| 422 | `ValidationError` | `"Validation failed"` |
| Other 4xx/5xx | `Error` | `"HTTP {code}: {reason}"` |
| Network exception | `ExceptionError` | Exception message |

## Configuration

Customise JSON deserialization and/or the status code → error mapping:

```csharp
var options = new HttpResultOptions
{
    // Custom JSON options (default: JsonSerializerDefaults.Web)
    JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    },

    // Custom status code mapper — completely replaces the built-in defaults
    StatusCodeMapper = (statusCode, reasonPhrase) => statusCode switch
    {
        HttpStatusCode.NotFound  => new NotFoundError("Order", requestedId),
        HttpStatusCode.Conflict  => new ConflictError("Order", "number", orderNumber),
        _ => new Error($"HTTP {(int)statusCode}: {reasonPhrase}")
    }
};

Result<Order> result = await httpClient.GetResult<Order>($"/api/orders/{id}", options);
```

## Symmetry with Server Side

`REslava.Result.Http` completes the full round-trip with the server-side source generator:

```
SERVER (outbound): Result<T> → IResult → HTTP response  ← REslava.Result.SourceGenerators
CLIENT (inbound):  HTTP response → Result<T>            ← REslava.Result.Http
```

A server that returns `Result<T>` via `[SmartEndpoint]` can be consumed by a client that
gets `Result<T>` back — no manual status-code inspection at either end.

## Requires

- [REslava.Result](https://www.nuget.org/packages/REslava.Result) (automatically installed as a dependency)

## Links

- [GitHub Repository](https://github.com/reslava/nuget-package-reslava-result)
- [Changelog](https://github.com/reslava/nuget-package-reslava-result/blob/main/CHANGELOG.md)
- [Documentation](https://reslava.github.io/nuget-package-reslava-result)

**MIT License** | .NET 8 / 9 / 10
