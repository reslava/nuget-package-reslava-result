# REslava.Result v1.33.0

Two things this release: a new `REslava.Result.Http` package that turns every `HttpClient` call into a typed `Result<T>`, and a full documentation restructure that makes the reference site easier to navigate.

---

## 📡 REslava.Result.Http — New Package

Every HTTP response and network failure becomes a typed result — no more `try/catch` around `HttpClient`:

```csharp
using REslava.Result.Http;

// GET
Result<UserDto> result = await client.GetResult<UserDto>("/api/users/42");

// POST
Result<UserDto> result = await client.PostResult<CreateUserDto, UserDto>("/api/users", dto);

// PUT
Result<UserDto> result = await client.PutResult<UpdateUserDto, UserDto>("/api/users/42", dto);

// DELETE (no body)
Result deleted = await client.DeleteResult("/api/users/42");

// DELETE (with response body)
Result<UserDto> deleted = await client.DeleteResult<UserDto>("/api/users/42");
```

### Status code → typed error mapping

| HTTP Status | Error Type | Default message |
|-------------|------------|-----------------|
| 404 Not Found | `NotFoundError` | `Resource not found` |
| 401 Unauthorized | `UnauthorizedError` | — |
| 403 Forbidden | `ForbiddenError` | — |
| 409 Conflict | `ConflictError` | `A conflict occurred` |
| 422 Unprocessable Entity | `ValidationError` | `Validation failed` |
| Other 4xx / 5xx | `Error` | `HTTP {code}: {reason}` |
| Network / timeout | `ExceptionError` | wraps the exception |

### Custom mapping

```csharp
var options = new HttpResultOptions
{
    StatusCodeMapper = (statusCode, reason) =>
        statusCode == HttpStatusCode.NotFound
            ? new NotFoundError("User", id)
            : new Error($"HTTP {(int)statusCode}: {reason}"),

    JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
};

Result<UserDto> result = await client.GetResult<UserDto>("/api/users/42", options);
```

Targets net8.0, net9.0, net10.0. Requires `REslava.Result`.

---

## 📚 Documentation Restructure

- **README / TOC**: Sections renumbered and reorganised for clarity; Http package added to companion packages table
- **MkDocs reference site**: Architecture section split — Source Generator content moved to its own sub-folder (`Architecture › Source Generators`); fixed orphaned pages; added pipeline diagnostic tooling

---

## 📦 NuGet

| Package | Link |
|---------|------|
| REslava.Result | [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.33.0) |
| REslava.Result.SourceGenerators | [View on NuGet](https://www.nuget.org/packages/REslava.Result.SourceGenerators/1.33.0) |
| REslava.Result.Analyzers | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.33.0) |
| REslava.Result.FluentValidation | [View on NuGet](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.33.0) |
| REslava.Result.Http | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Http/1.33.0) |

---

## Stats

- 3,756 tests passing across net8.0, net9.0, net10.0 (1,157×3) + generator (131) + analyzer (68) + FluentValidation bridge (26) + Http (20×3)
- 123 features across 12 categories
