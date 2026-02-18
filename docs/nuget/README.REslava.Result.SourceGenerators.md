# REslava.Result.SourceGenerators

**Zero-boilerplate ASP.NET endpoint generation** — write business logic, Minimal API + MVC endpoints generate themselves.

[![NuGet](https://img.shields.io/nuget/v/REslava.Result.SourceGenerators?logo=nuget)](https://www.nuget.org/packages/REslava.Result.SourceGenerators)
[![Downloads](https://img.shields.io/nuget/dt/REslava.Result.SourceGenerators)](https://www.nuget.org/packages/REslava.Result.SourceGenerators)
[![License](https://img.shields.io/badge/license-MIT-green)](https://github.com/reslava/nuget-package-reslava-result/blob/main/LICENSE)

## What It Does

This package contains **9 Roslyn source generators** that eliminate ASP.NET boilerplate:

**Minimal API**
- **SmartEndpoints** — auto-generates complete endpoint registration from your service classes
- **ResultToIResult** — converts `Result<T>` to `IResult` with domain error-aware HTTP status codes
- **OneOfToIResult** — converts `OneOf<T1,...,T4>` to `IResult` with tag-based + heuristic error mapping

**MVC Controllers**
- **ResultToActionResult** — converts `Result<T>` to `IActionResult` with convention-based HTTP mapping
- **OneOfToActionResult** — converts `OneOf<T1,...,T4>` to `IActionResult` with domain error auto-mapping

**Cross-cutting**
- **OpenAPI metadata** — auto-generates `.Produces<T>()`, `.WithSummary()`, `.WithTags()` with accurate error status codes
- **Authorization** — generates `.RequireAuthorization()`, `.AllowAnonymous()` from attributes

## Before / After

```csharp
// Before: Manual endpoint registration (30+ lines per controller)
app.MapGet("/api/products", async (IProductService svc) => {
    var result = await svc.GetProducts();
    return result.Match(
        products => Results.Ok(products),
        errors => Results.BadRequest(errors));
}).WithName("GetProducts").WithTags("Products").Produces<List<Product>>(200).Produces(400);

app.MapGet("/api/products/{id}", async (int id, IProductService svc) => { /* ... */ });
app.MapPost("/api/products", async (CreateProductRequest req, IProductService svc) => { /* ... */ });
// ... repeat for every endpoint

// After: SmartEndpoints (just your business logic)
[AutoGenerateEndpoints(RoutePrefix = "/api/products")]
public class ProductService(AppDbContext db)
{
    public async Task<Result<List<Product>>> GetProducts()
        => Result.Ok(await db.Products.ToListAsync());

    public async Task<Result<Product>> GetProductById(int id)
        => await db.Products.FindAsync(id) is { } p
            ? Result.Ok(p)
            : Result.Fail<Product>("Not found");

    public async Task<Result<Product>> CreateProduct(CreateProductRequest request)
        => Result.Ok(db.Products.Add(new Product(request)).Entity);
}

// In Program.cs — one line:
app.MapProductServiceEndpoints();
```

## Quick Start

```bash
dotnet add package REslava.Result
dotnet add package REslava.Result.SourceGenerators
```

```csharp
using REslava.Result;

[AutoGenerateEndpoints(RoutePrefix = "/api/users")]
public class UserService(UserRepository repo)
{
    public async Task<Result<User>> GetUserById(int id)
        => await repo.FindAsync(id) is { } user
            ? user    // implicit conversion to Result<User>
            : new NotFoundError($"User {id} not found");
}

// Program.cs
app.MapUserServiceEndpoints();  // auto-generated extension method
```

The generator infers:
- **HTTP method** from name: `Get*` -> GET, `Create*`/`Add*` -> POST, `Update*` -> PUT, `Delete*` -> DELETE
- **Routes** with `{id}` parameters when methods have an `id` parameter
- **DI** via ASP.NET parameter binding (services injected as lambda parameters)

## Requires

- [REslava.Result](https://www.nuget.org/packages/REslava.Result) (core library)

## Links

- [GitHub Repository](https://github.com/reslava/nuget-package-reslava-result) — Full documentation, architecture guide
- [Minimal API Demo](https://github.com/reslava/nuget-package-reslava-result/tree/main/samples/FastMinimalAPI.REslava.Result.Demo)
- [MVC Demo](https://github.com/reslava/nuget-package-reslava-result/tree/main/samples/FastMvcAPI.REslava.Result.Demo)
- [Changelog](https://github.com/reslava/nuget-package-reslava-result/blob/main/CHANGELOG.md)

**MIT License** | .NET 8 / 9 / 10
