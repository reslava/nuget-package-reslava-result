# v1.27.0 — Validation DSL + OneOf5/6 + SmartEndpoints CancellationToken + XML Docs

## Native Validation DSL

19 new extension methods on `ValidatorRuleBuilder<T>` — use property selectors (`Expression<Func<T, TProperty>>`) for auto-inferred field names and zero magic strings:

```csharp
var validator = Validator.For<CreateOrderRequest>()
    .NotEmpty(x => x.CustomerName)
    .NotWhiteSpace(x => x.Description)
    .MinLength(x => x.Code, 3)
    .MaxLength(x => x.Code, 50)
    .EmailAddress(x => x.ContactEmail)
    .Range(x => x.Quantity, 1, 1000)
    .Positive(x => x.UnitPrice)
    .NonNegative(x => x.DiscountAmount)
    .NotEmpty(x => x.Tags)          // IEnumerable overload
    .Build();
```

| Category | Methods |
|----------|---------|
| Strings | `NotEmpty`, `NotWhiteSpace`, `MinLength`, `MaxLength`, `EmailAddress`, `Matches`, `StartsWith`, `EndsWith`, `Contains` |
| Numeric | `Range`, `Min`, `Max`, `Positive<T,TNum>`, `NonNegative<T,TNum>` |
| Collections | `NotEmpty<T,TItem>` |
| General | `NotNull` |

## OneOf5 and OneOf6 Support

`OneOf<T1,T2,T3,T4,T5>` and `OneOf<T1,T2,T3,T4,T5,T6>` arities added alongside existing 2–4. Includes `ToIResult()`, `ToActionResult()`, exhaustive `Match`, `MapToT1..6`, and arity-chain downcast extensions.

Also fixed: `OneOf4.ToIResult()` / `ToActionResult()` now correctly maps all 4 arms (was using 3-arm logic).

## SmartEndpoints CancellationToken Threading

When a service method declares `CancellationToken cancellationToken = default`, the generated Minimal API lambda now automatically threads it through — zero extra wiring required:

```csharp
// Service method
public async Task<Result<Order>> GetOrderById(int id, CancellationToken cancellationToken = default)
    => ...;

// Generated lambda (CancellationToken automatically added)
group.MapGet("/{id}", async (int id, IOrderService svc, CancellationToken cancellationToken) =>
{
    var result = await svc.GetOrderById(id, cancellationToken);
    return result.ToIResult();
});
```

## XML Documentation (DocFX API Reference)

All public APIs now carry XML doc comments. The full API reference is published via DocFX and browsable at the documentation site — every class, method, and parameter is documented inline.

## Test Suite

- 3,313 tests passing across net8.0, net9.0, net10.0 + generator (131) + analyzer (68) tests

## NuGet Packages

- [REslava.Result 1.27.0](https://www.nuget.org/packages/REslava.Result/1.27.0)
- [REslava.Result.SourceGenerators 1.27.0](https://www.nuget.org/packages/REslava.Result.SourceGenerators/1.27.0)
- [REslava.Result.Analyzers 1.27.0](https://www.nuget.org/packages/REslava.Result.Analyzers/1.27.0)
