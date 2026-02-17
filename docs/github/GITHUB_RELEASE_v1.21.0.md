# v1.21.0 — ASP.NET MVC Support (ToActionResult)

## Result<T>.ToActionResult() — Source-Generated MVC Extensions

New source generator that auto-generates `IActionResult` extension methods for ASP.NET MVC controllers. Same convention-based HTTP mapping as `ToIResult()`, but targeting MVC:

```csharp
// Convention-based — domain errors auto-map to correct HTTP status codes
[HttpGet]
public async Task<IActionResult> GetAll()
    => (await _service.GetAllUsersAsync()).ToActionResult();

[HttpDelete("{id:int}")]
public async Task<IActionResult> Delete(int id)
    => (await _service.DeleteUserAsync(id)).ToDeleteActionResult();
    // NotFoundError → 404, ConflictError → 409, success → 204

// Explicit overload — escape hatch for full control
[HttpGet("{id:int}")]
public async Task<IActionResult> GetById(int id)
{
    return (await _service.GetUserAsync(id))
        .ToActionResult(
            onSuccess: user => Ok(user),
            onFailure: errors => NotFound(errors.First().Message));
}
```

### Generated Methods

| Method | Success | Failure |
|--------|---------|---------|
| `ToActionResult<T>()` | `OkObjectResult` (200) | Auto-mapped via `HttpStatusCode` tag |
| `ToActionResult<T>(onSuccess, onFailure)` | Custom | Custom |
| `ToPostActionResult<T>()` | `CreatedResult` (201) | Auto-mapped |
| `ToPutActionResult<T>()` | `OkObjectResult` (200) | Auto-mapped |
| `ToPatchActionResult<T>()` | `OkObjectResult` (200) | Auto-mapped |
| `ToDeleteActionResult<T>()` | `NoContentResult` (204) | Auto-mapped |

### Error Auto-Mapping (MapErrorToActionResult)

| Domain Error | HTTP | MVC Result Type |
|-------------|------|-----------------|
| `NotFoundError` | 404 | `NotFoundObjectResult` |
| `UnauthorizedError` | 401 | `UnauthorizedResult` |
| `ForbiddenError` | 403 | `ForbidResult` |
| `ConflictError` | 409 | `ConflictObjectResult` |
| `ValidationError` | 422 | `ObjectResult { StatusCode = 422 }` |
| No tag / other | 400 | `ObjectResult { StatusCode = 400 }` |

## FastMvcAPI Demo App

New MVC demo app at `samples/FastMvcAPI.REslava.Result.Demo/` — same domain as the Minimal API demo (Users, Products, Orders) but with `[ApiController]` MVC controllers:

- **Port 5001** (Minimal API demo runs on 5000 — both can run simultaneously)
- 3 controllers: `UsersController`, `ProductsController`, `OrdersController`
- Showcases `ToActionResult()` one-liners and `OneOf.Match()` with MVC result types
- Scalar UI at `/scalar/v1`

## Test Suite

- 2,813 tests passing across net8.0, net9.0, net10.0
- 9 new source generator tests for ResultToActionResult

## NuGet Packages

- [REslava.Result 1.21.0](https://www.nuget.org/packages/REslava.Result/1.21.0)
- [REslava.Result.SourceGenerators 1.21.0](https://www.nuget.org/packages/REslava.Result.SourceGenerators/1.21.0)
- [REslava.Result.Analyzers 1.21.0](https://www.nuget.org/packages/REslava.Result.Analyzers/1.21.0)
