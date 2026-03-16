# REslava.Result.FluentValidation

> ⚠️ **Optional — migration bridge only.**
>
> This package is **not needed for new projects**. REslava.Result already includes full validation natively via `[Validate]` (DataAnnotations → `Result<T>`) and the Validation DSL (19 fluent rules). Install this package **only** if your team already uses FluentValidation validators and wants to keep them while adopting REslava.Result.

**FluentValidation bridge for REslava.Result** — keep your existing validators, get `Result<T>` and SmartEndpoints auto-injection for free.

[![NuGet](https://img.shields.io/nuget/v/REslava.Result.FluentValidation?logo=nuget)](https://www.nuget.org/packages/REslava.Result.FluentValidation)
[![Downloads](https://img.shields.io/nuget/dt/REslava.Result.FluentValidation)](https://www.nuget.org/packages/REslava.Result.FluentValidation)
[![License](https://img.shields.io/badge/license-MIT-green)](https://github.com/reslava/nuget-package-reslava-result/blob/main/LICENSE)

## What It Does

A **migration bridge** for teams with existing FluentValidation validators who are adopting REslava.Result. Decorate your request type with `[FluentValidate]` and the generator emits:

- `Validate(IValidator<T> validator)` — sync, runs `validator.Validate()`, returns `Result<T>`
- `ValidateAsync(IValidator<T> validator, CancellationToken)` — async variant

**SmartEndpoints auto-injection** — when a POST/PUT body parameter is decorated with `[FluentValidate]`, the generated Minimal API lambda automatically adds `IValidator<T>` as a parameter and emits the validation guard block.

## Quick Start

```bash
dotnet add package REslava.Result
dotnet add package REslava.Result.SourceGenerators
dotnet add package REslava.Result.FluentValidation   # ← bridge
dotnet add package FluentValidation                  # ← your existing validators
```

```csharp
using REslava.Result.FluentValidation;
using FluentValidation;

// 1. Decorate the request type
[FluentValidate]
public record CreateOrderRequest(string CustomerId, decimal Amount);

// 2. Your existing FluentValidation validator — unchanged
public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

// 3. Register in DI
builder.Services.AddScoped<IValidator<CreateOrderRequest>, CreateOrderRequestValidator>();

// 4. Manual usage
IValidator<CreateOrderRequest> validator = new CreateOrderRequestValidator();
Result<CreateOrderRequest> result = request.Validate(validator);

// 5. SmartEndpoints — validator auto-injected, no extra wiring needed
[AutoGenerateEndpoints(RoutePrefix = "/api/orders")]
public class OrderService
{
    public async Task<Result<Order>> CreateOrder(CreateOrderRequest req) => ...;
}
// app.MapOrderServiceEndpoints();  ← one line, validator injected automatically
```

## Generated Code

For each type decorated with `[FluentValidate]`:

```csharp
// namespace Generated.FluentValidationExtensions
public static class CreateOrderRequestFluentValidationExtensions
{
    public static Result<CreateOrderRequest> Validate(
        this CreateOrderRequest instance,
        IValidator<CreateOrderRequest> validator)
    {
        var result = validator.Validate(instance);
        if (result.IsValid)
            return Result<CreateOrderRequest>.Ok(instance);

        var errors = result.Errors
            .Select(e => (IError)new ValidationError(e.PropertyName, e.ErrorMessage))
            .ToList();
        return Result<CreateOrderRequest>.Fail(errors);
    }

    public static async Task<Result<CreateOrderRequest>> ValidateAsync(
        this CreateOrderRequest instance,
        IValidator<CreateOrderRequest> validator,
        CancellationToken cancellationToken = default) { ... }
}
```

## SmartEndpoints Generated Lambda

```csharp
ordersGroup.MapPost("/", async (
    CreateOrderRequest req,
    IValidator<CreateOrderRequest> reqValidator,   // ← auto-injected
    IOrderService svc,
    CancellationToken cancellationToken) =>
{
    var validation = req.Validate(reqValidator);   // ← auto-emitted
    if (!validation.IsSuccess) return validation.ToIResult();

    var result = await svc.CreateOrder(req, cancellationToken);
    return result.ToIResult();
});
```

## Rules

- `[FluentValidate]` and `[Validate]` **cannot both be applied** to the same type (RESL1006 analyzer error)
- Users install `FluentValidation` (≥ 11.x) separately — this package ships no runtime binary
- Only `POST`/`PUT` body parameters trigger SmartEndpoints auto-injection (GET query params are skipped)

## Requires

- [REslava.Result](https://www.nuget.org/packages/REslava.Result) (core library)
- [REslava.Result.SourceGenerators](https://www.nuget.org/packages/REslava.Result.SourceGenerators) (for SmartEndpoints)
- [FluentValidation](https://www.nuget.org/packages/FluentValidation) ≥ 11.x (installed by user)

## Links

- [GitHub Repository](https://github.com/reslava/nuget-package-reslava-result)
- [Changelog](https://github.com/reslava/nuget-package-reslava-result/blob/main/CHANGELOG.md)

**MIT License** | Works with any .NET project (netstandard2.0)
