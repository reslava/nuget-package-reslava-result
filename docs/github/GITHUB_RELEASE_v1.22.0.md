# v1.22.0 — Domain Error Completeness

## OneOf<>.ToActionResult() — MVC One-Liners

New source generator that auto-generates `IActionResult` extension methods for `OneOf<T1,...,T4>` in MVC controllers. Domain errors auto-map via `IError.Tags["HttpStatusCode"]`:

```csharp
// BEFORE (v1.21.0) — manual .Match() for every OneOf return
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
{
    var result = await _service.CreateOrderAsync(request);
    return result.Match(
        notFound => new NotFoundObjectResult(notFound.Message) as IActionResult,
        conflict => new ConflictObjectResult(conflict.Message),
        validation => new ObjectResult(validation.Message) { StatusCode = 422 },
        order => new OkObjectResult(order));
}

// AFTER (v1.22.0) — one-liner, domain errors auto-mapped
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    => (await _service.CreateOrderAsync(request)).ToActionResult();
    // NotFoundError → 404, ConflictError → 409, ValidationError → 422, success → 200
```

## OneOfToIResult: Tag-Based Error Mapping Fix

`MapErrorToHttpResult` now checks `IError.Tags["HttpStatusCode"]` first before falling back to type-name heuristics. Domain errors with custom tags now map correctly in both Minimal API and MVC scenarios.

## SmartEndpoints: Accurate OpenAPI Error Docs

- `ValidationError` / `Invalid` now maps to **422** in OpenAPI docs (was 400)
- `Result<T>` endpoints now declare `.Produces(400)`, `.Produces(404)`, `.Produces(409)`, `.Produces(422)` — reflecting the full range of domain errors that `MapErrorToIResult` can dispatch

## Test Suite

- 2,825 tests passing across net8.0, net9.0, net10.0
- 12 new source generator tests for OneOfToActionResult

## NuGet Packages

- [REslava.Result 1.22.0](https://www.nuget.org/packages/REslava.Result/1.22.0)
- [REslava.Result.SourceGenerators 1.22.0](https://www.nuget.org/packages/REslava.Result.SourceGenerators/1.22.0)
- [REslava.Result.Analyzers 1.22.0](https://www.nuget.org/packages/REslava.Result.Analyzers/1.22.0)
