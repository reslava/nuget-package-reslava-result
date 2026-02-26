---
hide:
  - navigation
title: All Features
description: The definitive feature checklist. Know what's in the box before you write a line of code.
tagline: Know exactly what you're getting.
---

# Features reference by category

## 1. Core Library — `REslava.Result`

| Feature | Short Description | Version | Docs |
|---------|------------------|---------|------|
| `Result<T>` type | Type-safe success/failure container | v1.0.0 | `## REslava.Result Core Library` |
| `Result<T>.Ok(value)` / `Fail(error)` | Factory methods | v1.0.0 | `## REslava.Result Core Library` |
| `Result<T>.IsSuccess` / `IsFailure` | Status properties | v1.0.0 | `## REslava.Result Core Library` |
| `Result<T>.Value` | Value access (unsafe without IsSuccess guard) | v1.0.0 | `## REslava.Result Core Library` |
| `GetValueOr()` / `TryGetValue()` | Safe value access | v1.0.0 | `## 📐 REslava.Result Core Library` — `### Complete Method Catalog` |
| `Match()` | Exhaustive pattern matching | v1.0.0 | `## REslava.Result Core Library` |
| `Bind()` / `BindAsync()` | Monadic chaining | v1.0.0 | `## REslava.Result Core Library` |
| `Map()` / `MapAsync()` | Value transformation | v1.0.0 | `## REslava.Result Core Library` |
| `Tap()` / `TapAsync()` | Side-effects without changing result | v1.0.0 | `## REslava.Result Core Library` |
| `TapOnFailure()` | Side-effect only on failure | v1.20.0 | `## 📐 REslava.Result Core Library` — `### Tap on Failure` |
| `Ensure()` / `EnsureNotNull()` | Guard conditions | v1.0.0 | `## 📐 REslava.Result Core Library` — `### Complete Method Catalog` |
| `WithSuccess()` / `WithError()` / `WithTag()` | Fluent builders | v1.0.0 | `## ⚠️ Error Types` — `### Rich Error Context` |
| `Result.OkIf()` / `FailIf()` | Conditional factory (lazy + async overloads) | v1.20.0 | `## 📐 REslava.Result Core Library` — `### Conditional Factories` |
| `Result.Try()` / `TryAsync()` | Exception-safe factory (wraps try-catch) | v1.20.0 | `## 📐 REslava.Result Core Library` — `### Exception Wrapping` |
| `Result.Combine()` / `Merge()` | Aggregate multiple results | v1.20.0 | `## 📐 REslava.Result Core Library` — `### Complete Method Catalog` |
| `Result.WhenAll()` | Run concurrent async Results, aggregate errors | v1.18.0 | `## REslava.Result Core Library` — `### Async Patterns` |
| `Result.Retry()` | Retry with delay + exponential backoff | v1.18.0 | `## REslava.Result Core Library` — `### Async Patterns` |
| `.Timeout()` | Enforce time limit on async operation | v1.18.0 | `## REslava.Result Core Library` — `### Async Patterns` |
| `CancellationToken` support | All async methods accept CancellationToken | v1.19.0 | `## 📐 REslava.Result Core Library` — `### CancellationToken Support` |
| LINQ integration | `from x in result select ...`, `Sequence()`, `Traverse()` | v1.12.0 | `## REslava.Result Core Library` — `### LINQ Integration` |
| CRTP fluent chaining | `WithTag()`, `WithMessage()` type-safe on all Reason types | v1.9.4 | `## REslava.Result Core Library` — `### CRTP Pattern` |
| Functional composition | `Compose()`, higher-order functions | v1.12.0 | `## REslava.Result Core Library` — `### Advanced Extensions` |
| `Maybe<T>` | Null-safe optional type | v1.12.0 | `## Advanced Patterns` — `### Maybe<T>` |
| `OneOf<T1,T2>` | 2-type discriminated union | v1.12.0 | `## Advanced Patterns` — `### OneOf` |
| `OneOf<T1,T2,T3>` | 3-type discriminated union | v1.12.0 | `## Advanced Patterns` — `### OneOf` |
| `OneOf<T1,T2,T3,T4>` | 4-type discriminated union | v1.12.0 | `## Advanced Patterns` — `### OneOf` |
| `OneOf<T1..T5>` | 5-type discriminated union | v1.27.0 | `## Advanced Patterns` — `### OneOf` |
| `OneOf<T1..T6>` | 6-type discriminated union | v1.27.0 | `## Advanced Patterns` — `### OneOf` |
| OneOf chain extensions | `ToFourWay`, `ToFiveWay`, `ToSixWay` + down-conversions across full 2↔3↔4↔5↔6 arity chain | v1.27.0 | `## Advanced Patterns` — `### OneOf` |
| Native Validation DSL | 19 fluent methods on `ValidatorRuleBuilder<T>`: `NotEmpty`, `MaxLength`, `EmailAddress`, `Range`, `Positive`, `NonNegative`, etc. `Expression<Func<T,TProperty>>` auto-infers field names | v1.27.0 | `## Validation Rules` — `### Validation DSL` |
| JSON Serialization | `AddREslavaResultConverters()` for Result, OneOf, Maybe | v1.17.0 | `## REslava.Result Core Library` — `#### JSON Serialization` |
| Performance optimizations | Cached `Errors`/`Successes`, immutable design | v1.20.0 | `## Advanced Patterns` — `### Performance Patterns` |

---

## 2. Error Types — `REslava.Result`

| Feature | Short Description | Version | Docs |
|---------|------------------|---------|------|
| `Error` | Generic application error, base concrete type | v1.0.0 | `## ⚠️ Error Types` — `### Generic Errors` |
| `ValidationError` | HTTP 422, optional `FieldName` property | v1.20.0 | `## Advanced Patterns` — `### Domain Error Hierarchy` |
| `NotFoundError` | HTTP 404, `(entityName, id)` constructor | v1.20.0 | `## Advanced Patterns` — `### Domain Error Hierarchy` |
| `ConflictError` | HTTP 409, `(entityName, conflictField, value)` | v1.20.0 | `## Advanced Patterns` — `### Domain Error Hierarchy` |
| `UnauthorizedError` | HTTP 401, default message | v1.20.0 | `## Advanced Patterns` — `### Domain Error Hierarchy` |
| `ForbiddenError` | HTTP 403, `(action, resource)` constructor | v1.20.0 | `## Advanced Patterns` — `### Domain Error Hierarchy` |
| `ExceptionError` | Wraps .NET Exception with StackTrace tag | v1.0.0 | `## ⚠️ Error Types` — `### Generic Errors` |
| `ConversionError` | Implicit conversion failure, Severity=Warning tag | v1.0.0 | `## ⚠️ Error Types` |
| `Success` | Success reason with message | v1.0.0 | `## ⚠️ Error Types` — `### Success — Success Reasons` |
| `IError.Tags` dict | Rich metadata: HttpStatusCode, ErrorType, custom | v1.9.4 | `## Advanced Patterns` — `### Rich Error Context` |
| `WithTag(key, value)` | Fluent tag builder on any Reason | v1.9.4 | `## Advanced Patterns` — `### Rich Error Context` |
| HttpStatusCode convention | Domain errors carry tag; read by ToIResult/ToActionResult | v1.20.0 | `## Complete Architecture` — `### Error → HTTP Status Code Convention` |

---

## 3. Source Generators — SmartEndpoints

| Feature | Short Description | Version | Docs |
|---------|------------------|---------|------|
| `[AutoGenerateEndpoints]` | Class-level: auto-generates Minimal API endpoints for all Result/OneOf methods | v1.9.4 | `## 🚀 SmartEndpoints` — `### [AutoGenerateEndpoints]` |
| `RoutePrefix` property | Override route prefix; default derived from class name | v1.20.0 | `## 🚀 SmartEndpoints` — `### [AutoGenerateEndpoints]` property table |
| `[AutoMapEndpoint(route)]` | Method-level: explicit route/method/name/tags override | v1.9.4 | `## 🚀 SmartEndpoints` — `### [AutoMapEndpoint]` with full property table |
| `[SmartAllowAnonymous]` | Per-method: override class RequiresAuth, allow anonymous | v1.13.0 | `## 🚀 SmartEndpoints` — `### Authorization` |
| `[SmartFilter(typeof(T))]` | Per-method: generates `.AddEndpointFilter<T>()`, stackable | v1.23.0 | `## 🚀 SmartEndpoints` — `### Endpoint Filters` |
| `RequiresAuth` / `Policies` / `Roles` | Class-level auth: `.RequireAuthorization()` / policies / roles | v1.13.0 | `## 🚀 SmartEndpoints` — `### Authorization` |
| `AllowAnonymous` on `[AutoMapEndpoint]` | Method-level auth override | v1.13.0 | `## 🚀 SmartEndpoints` — `### Authorization` |
| `CacheSeconds` (class + method) | Output caching for GET endpoints; -1 = opt-out | v1.23.0 | `## 🚀 SmartEndpoints` — `### Output Caching & Rate Limiting` |
| `RateLimitPolicy` (class + method) | `.RequireRateLimiting("policy")`; "none" = opt-out | v1.23.0 | `## 🚀 SmartEndpoints` — `### Output Caching & Rate Limiting` |
| `EndpointMappingStrategy` enum | Convention / Explicit / All — controls which methods are mapped | v1.9.4 | `## 🚀 SmartEndpoints` — `### Mapping Strategy` |
| Convention-based HTTP verb | `Get*` → GET, `Create*/Add*` → POST, `Update*` → PUT, `Delete*` → DELETE | v1.9.4 | `## 🚀 SmartEndpoints` — `### HTTP Verb Convention` |
| Convention-based route | `Get*(int id)` → `/{id}`, no-id variants → base route | v1.9.4 | `## 🚀 SmartEndpoints` — `### HTTP Verb Convention` |
| OpenAPI auto-generation | `.Produces<T>()`, `.WithSummary()`, `.WithTags()`, `.WithName()` from return types | v1.12.0 | `## 🚀 SmartEndpoints` — `### OpenAPI Auto-Generation` |
| SmartEndpoints + `[Validate]` auto-validation | Inject `.Validate()` for body params with `[Validate]` type | v1.26.0 | `## 🚀 SmartEndpoints` — `### Auto-Validation` |
| CancellationToken in SmartEndpoints | Service methods with `CancellationToken` param → auto-injected in generated lambda | v1.27.0 | `## 🚀 SmartEndpoints` — `### CancellationToken` |
| SmartEndpoints + `[FluentValidate]` auto-injection | Body param with `[FluentValidate]` → `IValidator<T>` injected + guard emitted | v1.28.0 | `## 🚀 SmartEndpoints` — `### FluentValidation Bridge` |

---

## 4. Source Generators — Result → IResult (Minimal API)

| Feature | Short Description | Version | Docs |
|---------|------------------|---------|------|
| `[GenerateResultExtensions]` | Trigger attribute (assembly-level) to enable ResultToIResult | v1.9.4 | `## 🚀 ASP.NET Integration` — `### ResultToIResult` |
| `ToIResult()` | `Result<T>` → HTTP response; reads HttpStatusCode tag | v1.9.4 | `## ASP.NET Integration` — `### ResultToIResult Extensions` |
| `ToPostResult()` | → 201 Created on success | v1.9.4 | `## ASP.NET Integration` — `### ResultToIResult Extensions` |
| `ToPutResult()` / `ToPatchResult()` | → 200 on success | v1.9.4 | `## ASP.NET Integration` — `### ResultToIResult Extensions` |
| `ToDeleteResult()` | → 204 No Content on success | v1.9.4 | `## ASP.NET Integration` — `### ResultToIResult Extensions` |
| Smart HTTP mapping | Reads `HttpStatusCode` tag; 404→NotFound, 401→Unauthorized, etc. | v1.20.0 | `## ASP.NET Integration` — `### Smart HTTP Mapping` |
| `[GenerateOneOf2ExtensionsAttribute]` | Trigger for OneOf2 → IResult | v1.9.4 | `## 🔀 OneOf to IResult` |
| `[GenerateOneOf3ExtensionsAttribute]` | Trigger for OneOf3 → IResult | v1.9.4 | `## 🔀 OneOf to IResult` |
| `[GenerateOneOf4ExtensionsAttribute]` | Trigger for OneOf4 → IResult | v1.12.0 | `## 🔀 OneOf to IResult` |
| `OneOf<T1,T2>.ToIResult()` | One-liner HTTP response for 2-type unions | v1.9.4 | `## 🔀 OneOf to IResult` — `### OneOf<T1,T2>.ToIResult()` |
| `OneOf<T1,T2,T3>.ToIResult()` | One-liner HTTP response for 3-type unions | v1.9.4 | `## 🔀 OneOf to IResult` — `### OneOf<T1,T2,T3>.ToIResult()` |
| `OneOf<T1,T2,T3,T4>.ToIResult()` | One-liner HTTP response for 4-type unions | v1.12.0 | `## 🔀 OneOf to IResult` — `### OneOf<T1,T2,T3,T4>.ToIResult()` |
| OneOf HTTP method variants | `ToPostResult()`, `ToPutResult()`, etc. for OneOf | v1.9.4 | `## 🔀 OneOf to IResult` — `### HTTP Method Variants` |

---

## 5. Source Generators — Result/OneOf → ActionResult (MVC)

| Feature | Short Description | Version | Docs |
|---------|------------------|---------|------|
| `[GenerateActionResultExtensions]` | Trigger attribute (assembly-level) for ResultToActionResult | v1.21.0 | `## 🚀 ASP.NET Integration` — `### ResultToActionResult` |
| `ToActionResult()` | `Result<T>` → `IActionResult`; reads HttpStatusCode tag | v1.21.0 | `## ASP.NET Integration` — `### ResultToActionResult Extensions` |
| `ToPostActionResult()` | → 201 Created | v1.21.0 | `## ASP.NET Integration` — `### ResultToActionResult Extensions` |
| `ToPutActionResult()` / `ToPatchActionResult()` | → 200 | v1.21.0 | `## ASP.NET Integration` — `### ResultToActionResult Extensions` |
| `ToDeleteActionResult()` | → 204 No Content | v1.21.0 | `## ASP.NET Integration` — `### ResultToActionResult Extensions` |
| `ToActionResult(onSuccess, onFailure)` | Explicit overload — escape hatch | v1.21.0 | `## ASP.NET Integration` — `### ResultToActionResult Extensions` |
| `OneOf.ToActionResult()` (2/3/4 arities) | MVC one-liner for discriminated unions | v1.22.0 | `## ASP.NET Integration` — `### OneOfToActionResult Extensions` |
| MVC error auto-mapping table | NotFoundError→NotFoundObjectResult, etc. | v1.21.0 | `## ASP.NET Integration` — `### ResultToActionResult Extensions` |

---

## 6. Source Generators — Validate

| Feature | Short Description | Version | Docs |
|---------|------------------|---------|------|
| `[Validate]` attribute | Marker for `.Validate()` extension method generation | v1.24.0 | `## Validation Attributes` |
| Generated `.Validate()` extension | Delegates to `Validator.TryValidateObject`, returns `Result<T>` | v1.24.0 | `## Validation Attributes` |
| All 20+ DataAnnotations supported | `[Required]`, `[Range]`, `[StringLength]`, etc. | v1.24.0 | `## Validation Attributes` |
| `ValidationError.FieldName` populated | Per-field errors from `MemberNames.FirstOrDefault()` | v1.24.0 | `## Validation Attributes` |
| Pipeline: `.Validate().ToIResult()` | One-liner validate + respond | v1.24.0 | `## Validation Attributes` |

---

## 7. Source Generators — Problem Details

| Feature | Short Description | Version | Docs |
|---------|------------------|---------|------|
| `[MapToProblemDetails]` | Maps error class to RFC 7807 ProblemDetails response | v1.12.0 | `## 🚀 ASP.NET Integration` — `### Problem Details Integration` |
| `StatusCode` property | HTTP status override | v1.12.0 | `## 🚀 ASP.NET Integration` — `### Problem Details Integration` |
| `Type` property | RFC 7807 type URI | v1.12.0 | `## 🚀 ASP.NET Integration` — `### Problem Details Integration` |
| `Title` property | Human-readable error title | v1.12.0 | `## 🚀 ASP.NET Integration` — `### Problem Details Integration` |
| `IncludeTags` property | Include error Tags in ProblemDetails.Extensions | v1.12.0 | `## 🚀 ASP.NET Integration` — `### Problem Details Integration` |

---

## 8. Analyzers — `REslava.Result.Analyzers`

| Feature | Short Description | Version | Docs |
|---------|------------------|---------|------|
!!! warning "| RESL1001 | Unsafe `.Value` access without `IsSuccess` guard — Warning + Code Fix | v1.14.0 | `## 🛡️ Safety Analyzers` — `### RESL1001` |"

!!! warning "| RESL1002 | Discarded `Result<T>` return value — Warning | v1.14.0 | `## 🛡️ Safety Analyzers` — `### RESL1002` |"

!!! warning "| RESL1003 | Prefer `Match()` over `if`-check — Info suggestion | v1.14.2 | `## 🛡️ Safety Analyzers` — `### RESL1003` |"

!!! warning "| RESL1004 | `Task<Result<T>>` not awaited — Warning + Code Fix | v1.19.0 | `## 🛡️ Safety Analyzers` — `### RESL1004` |"

!!! warning "| RESL1005 | Suggest domain-specific error type — Info | v1.26.0 | `## 🛡️ Safety Analyzers` — `### RESL1005` |"

!!! warning "| RESL1006 | Both `[Validate]` and `[FluentValidate]` on same type — Error | v1.28.0 | `## 🛡️ Safety Analyzers` — `### RESL1006` |"

!!! warning "| RESL2001 | Unsafe `OneOf.AsT*` without `IsT*` guard — Warning + Code Fix | v1.14.2 | `## 🛡️ Safety Analyzers` — `### RESL2001` |"

!!! warning "| Code Fix: RESL1001 | Wrap in `if (IsSuccess)` guard or replace with `.Match()` | v1.14.2 | `## 🛡️ Safety Analyzers` — `### RESL1001` |"

!!! warning "| Code Fix: RESL1004 | Add `await` keyword | v1.19.0 | `## 🛡️ Safety Analyzers` — `### RESL1004` |"

!!! warning "| Code Fix: RESL2001 | Replace `.AsT*` with exhaustive `.Match()` | v1.14.2 | `## 🛡️ Safety Analyzers` — `### RESL2001` |"


---

## Summary

!!! new "**v1.28.0** — 109 features across 11 categories."


| Category | Total Features |
|----------|---------------|
| Core Library | 26 |
| Error Types | 12 |
| SmartEndpoints | 18 |
| Result/OneOf → IResult | 14 |
| Result/OneOf → ActionResult | 8 |
| Validate | 5 |
| Problem Details | 5 |
| Analyzers | 10 |
| OneOf (incl. OneOf5/6 + chains) | 8 |
| Validation DSL | 1 |
| FluentValidation Bridge | 2 |
| **Total** | **109** |

---

**See also**
- [API Documentation](../api-doc/index.md)
- [API Reference](../api/index.html)