# 📚 REslava.Result Samples

**Working examples demonstrating REslava.Result, Source Generators, and Analyzers — updated for v1.28.0.**

---

## 📂 Sample Projects

| Project | Framework | Description |
|---------|-----------|-------------|
| **[FastMinimalAPI.REslava.Result.Demo](FastMinimalAPI.REslava.Result.Demo/)** | .NET 10 Minimal API | Full-featured demo with SmartEndpoints, OneOf, `[Validate]`, DSL, `[FluentValidate]`, EF Core, Scalar UI |
| **[FastMvcAPI.REslava.Result.Demo](FastMvcAPI.REslava.Result.Demo/)** | .NET 10 MVC API | Full-featured demo with MVC controllers, explicit `.Validate()`, OneOf, CancellationToken, EF Core, Scalar UI |
| **[REslava.Result.Samples.Console](REslava.Result.Samples.Console/)** | .NET 10 Console | Core library — 16 examples: Result&lt;T&gt;, Maybe&lt;T&gt;, OneOf5/6, Validation DSL, Async patterns |

---

## 🚀 Quick Start

### Web API Demo (FastMinimalAPI)

```bash
cd samples/FastMinimalAPI.REslava.Result.Demo
dotnet run
# Open http://localhost:5000/scalar/v1 for Scalar API docs
```

**Features demonstrated (v1.28.0):**
- `[AutoGenerateEndpoints]` SmartEndpoints with auto HTTP method/route inference
- `[Validate]` on request DTOs → auto-injected validation guard in POST/PUT lambdas (v1.24.0)
- Native Validation DSL — 19 fluent rules in `SmartValidationController` (v1.27.0)
- `[FluentValidate]` migration bridge → `IValidator<T>` auto-injected (v1.28.0, optional)
- `CancellationToken` auto-injection in SmartEndpoints service methods (v1.27.0)
- `Result<T>.ToIResult()` and `OneOf<...>.ToIResult()` extensions
- Authorization support (`RequiresAuth`, `Roles`, `AllowAnonymous`)
- OpenAPI metadata auto-generation, EF Core In-Memory database

**Validation endpoints:**
- `GET  /api/smart/validation/demo` — baseline (no validation)
- `POST /api/smart/validation/with-validate` — `[Validate]` + DataAnnotations auto-guard
- `POST /api/smart/validation/with-dsl` — Validation DSL inside service
- `GET  /api/smart/fluent-validation/fluent-demo` — FluentValidation bridge baseline
- `POST /api/smart/fluent-validation/fluent-product` — `[FluentValidate]` auto-injection

### Web MVC Demo (FastMvcAPI)

```bash
cd samples/FastMvcAPI.REslava.Result.Demo
dotnet run
# Open http://localhost:5001/scalar/v1 for Scalar API docs
```

**Features demonstrated (v1.28.0):**
- MVC controllers with `ToActionResult()` one-liners and `Match()` escape hatch
- Explicit `.Validate()` call pattern (visible guard in controller actions)
- `[Validate]` on `CreateProductRequest` / `CreateOrderRequest`
- `CancellationToken` in controller actions
- `OneOf<T1,T2,T3,T4>.ToActionResult()` auto-mapped status codes

### Console Samples

```bash
cd samples/REslava.Result.Samples.Console
dotnet run
```

**16 examples (v1.28.0):**
- Examples 1–8: Core — Result&lt;T&gt;, validation pipeline, error handling, LINQ, async
- Examples 9–13: Advanced — Maybe&lt;T&gt;, OneOf2/3/4, conversions, integration
- Example 14: **Validation DSL** — all 19 fluent rules (NotEmpty, MaxLength, EmailAddress, Range, Positive…)
- Example 15: **OneOf5/OneOf6** — 5/6-way discriminated unions + chain extensions (ToFiveWay, ToSixWay, down-conversions)
- Example 16: **Advanced Async Patterns** — WhenAll, Retry (backoff), Timeout, TapOnFailure, OkIf/FailIf, Try

---

## 📚 Related Documentation

- **[Main README](../README.md)** — Full project overview and API reference
- **[CHANGELOG](../CHANGELOG.md)** — Version history
- **[Custom Generator Guide](../docs/how-to-create-custom-generator.md)** — Build your own source generators
