# v1.28.0 — FluentValidation Bridge + RESL1006 Analyzer

## REslava.Result.FluentValidation (new package)

Migration bridge for teams with existing FluentValidation validators who are adopting REslava.Result. Zero changes to existing validators required.

```bash
dotnet add package REslava.Result.FluentValidation
dotnet add package FluentValidation   # your existing validators
```

Decorate your request type with `[FluentValidate]` — the generator emits two extension methods:

```csharp
using REslava.Result.FluentValidation;

[FluentValidate]
public record CreateOrderRequest(string CustomerId, decimal Amount);

// Generated in namespace Generated.FluentValidationExtensions:
// .Validate(IValidator<T>)           — sync
// .ValidateAsync(IValidator<T>, CT)  — async
```

## SmartEndpoints Auto-Injection

When a POST/PUT body parameter is decorated with `[FluentValidate]`, SmartEndpoints automatically injects `IValidator<T>` as a lambda parameter and emits the validation guard — no extra wiring:

```csharp
// Generated lambda
ordersGroup.MapPost("", async (
    CreateOrderRequest req,
    IValidator<CreateOrderRequest> reqValidator,   // ← auto-injected
    IOrderService svc, CancellationToken ct) =>
{
    var validation = req.Validate(reqValidator);
    if (!validation.IsSuccess) return validation.ToIResult();
    return (await svc.CreateOrder(req, ct)).ToIResult();
});
```

Register in DI once — nothing else needed:
```csharp
builder.Services.AddScoped<IValidator<CreateOrderRequest>, CreateOrderRequestValidator>();
```

## RESL1006 Analyzer

Compile-error when `[Validate]` and `[FluentValidate]` are both applied to the same type (they would generate conflicting `.Validate()` extension methods):

```
RESL1006 error: 'CreateOrderRequest' has both [Validate] and [FluentValidate] applied.
These attributes generate conflicting '.Validate()' extension methods. Remove one.
```

## Test Suite

- 3,339 tests passing across net8.0, net9.0, net10.0 + generator (131) + analyzer (68) + FluentValidation bridge (26)

## NuGet Packages

- [REslava.Result 1.28.0](https://www.nuget.org/packages/REslava.Result/1.28.0)
- [REslava.Result.SourceGenerators 1.28.0](https://www.nuget.org/packages/REslava.Result.SourceGenerators/1.28.0)
- [REslava.Result.Analyzers 1.28.0](https://www.nuget.org/packages/REslava.Result.Analyzers/1.28.0)
- [REslava.Result.FluentValidation 1.28.0](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.28.0)
