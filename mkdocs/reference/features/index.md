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
| `Result.Catch<TException>()` / `CatchAsync<TException>()` | Inline typed exception handling in pipelines — converts `ExceptionError` wrapping `TException` to a different `IError`; Task extension also catches direct throws | v1.29.0 | `## 📐 REslava.Result Core Library` — `### 🪤 Inline Exception Handling` |
| `Result.WithActivity(Activity?)` | Enriches an existing OTel Activity span with outcome tags (`result.outcome`, `result.error.type`, etc.) and status — side-effect (Tap-style), null-safe | v1.30.0 | `## 📐 REslava.Result Core Library` — `### 📡 OpenTelemetry Integration` |
| `Result.WithLogger(ILogger, string)` / `LogOnFailure(ILogger, string)` | Tap-style ILogger integration — Debug on success, Warning on domain failure, Error on ExceptionError; structured log properties; Task extensions with CancellationToken | v1.31.0 | `## 📐 REslava.Result Core Library` — `### 📝 Structured Logging` |
| `Result.Recover()` / `RecoverAsync()` | Recovery from any failure — transforms failure into a new `Result<T>` (success or failure); error list passed to recovery func; both `Result` and `Result<T>`; Task extensions with CancellationToken | v1.31.0 | `## 📐 REslava.Result Core Library` — `### 🔄 Railway Recovery` |
| `Result.Filter()` / `FilterAsync()` | Converts success to failure when predicate fails; `Func<T, IError>` error factory enables value-dependent contextual messages; 3 sync overloads (factory / static IError / string); async predicate variant; Task extensions | v1.31.0 | `## 📐 REslava.Result Core Library` — `### 🔍 Predicate Filtering` |
| `Result.Validate()` | Applicative validation — runs 2/3/4 independent `Result<T>` validations simultaneously, accumulates ALL errors, maps heterogeneous success values to `Result<TResult>` via mapper func | v1.32.0 | — |
| `Switch()` / `SwitchAsync()` | Void side-effect dispatch — routes success/failure to two actions; Task extensions enable end-of-chain dispatch after async pipelines | v1.37.0 | — |
| `MapError()` / `MapErrorAsync()` | Transforms errors in the failure path — symmetric counterpart to `Map`; success passes through unchanged; Task extensions | v1.37.0 | — |
| `Or()` / `OrElse()` / `OrElseAsync()` | Fallback result on failure — simpler API than `Recover`; `Or(fallback)` eager, `OrElse(factory)` lazy with errors; Task extensions | v1.37.0 | — |
| `Result<T>.Deconstruct()` | C# 8+ tuple unpacking — `(value, errors)` 2-way and `(isSuccess, value, errors)` 3-way for `Result<T>`; `(isSuccess, errors)` for non-generic `Result` | v1.32.0 | — |
| `Result.Combine()` / `Merge()` | Aggregate multiple results | v1.20.0 | `## 📐 REslava.Result Core Library` — `### Complete Method Catalog` |
| `Result.WhenAll()` | Run concurrent async Results, aggregate errors | v1.18.0 | `## REslava.Result Core Library` — `### Async Patterns` |
| `Result.Retry()` | Retry with delay + exponential backoff | v1.18.0 | `## REslava.Result Core Library` — `### Async Patterns` |
| `.Timeout()` | Enforce time limit on async operation | v1.18.0 | `## REslava.Result Core Library` — `### Async Patterns` |
| `CancellationToken` support | All async methods accept CancellationToken | v1.19.0 | `## 📐 REslava.Result Core Library` — `### CancellationToken Support` |
| LINQ integration | `from x in result select ...`, `Sequence()`, `Traverse()` | v1.12.0 | `## REslava.Result Core Library` — `### LINQ Integration` |
| CRTP fluent chaining | `WithTag()`, `WithMessage()` type-safe on all Reason types | v1.9.4 | `## REslava.Result Core Library` — `### CRTP Pattern` |
| Functional composition | `Compose()`, higher-order functions | v1.12.0 | `## REslava.Result Core Library` — `### Advanced Extensions` |
| `Maybe<T>` | Null-safe optional type | v1.12.0 | `## Advanced Patterns` — `### Maybe<T>` |
| `Maybe<T>` ↔ `Result<T>` interop | `maybe.ToResult(errorFactory/error/string)` + `result.ToMaybe()` — bidirectional bridge; `ToMaybe` discards errors | v1.32.0 | — |
| `OneOf<T1,T2>` | 2-type discriminated union | v1.12.0 | `## Advanced Patterns` — `### OneOf` |
| `OneOf<T1,T2,T3>` | 3-type discriminated union | v1.12.0 | `## Advanced Patterns` — `### OneOf` |
| `OneOf<T1,T2,T3,T4>` | 4-type discriminated union | v1.12.0 | `## Advanced Patterns` — `### OneOf` |
| `OneOf<T1..T5>` | 5-type discriminated union | v1.27.0 | `## Advanced Patterns` — `### OneOf` |
| `OneOf<T1..T6>` | 6-type discriminated union | v1.27.0 | `## Advanced Patterns` — `### OneOf` |
| `OneOf<T1..T7>` | 7-type discriminated union (sealed class) | v1.39.0 | `## OneOf Unions` — `### 8.1.` |
| `OneOf<T1..T8>` | 8-type discriminated union (sealed class) | v1.39.0 | `## OneOf Unions` — `### 8.1.` |
| `OneOf` sealed class | All `OneOf<>` types converted from `readonly struct` to `sealed class`; copy → reference semantics ⚠️ breaking | v1.39.0 | `## OneOf Unions` — `### 8.1.` |
| `OneOfBase<T1..T8>` | Abstract base class with all shared dispatch (`IsT1..T8`, `AsT1..T8`, `Match`, `Switch`, equality); inherited by `OneOf` and `ErrorsOf` | v1.39.0 | `## OneOf Unions` |
| `IOneOf<T1..T8>` | Shared interface implemented by `OneOf<>` and `ErrorsOf<>`; enables generic programming over any discriminated union | v1.39.0 | `## OneOf Unions` |
| `ErrorsOf<T1..T8>` | Typed error union; `where Ti : IError`; implements `IError` (delegates to active case); implicit conversions; factory `FromT1..FromT8` | v1.39.0 | `## Typed Error Pipelines` — `### 9.1.` |
| `Result<TValue, TError>` | Typed result with concrete error type; `Ok(value)` / `Fail(error)`; `IsSuccess`, `IsFailure`, `Value`, `Error` | v1.39.0 | `## Typed Error Pipelines` — `### 9.2.` |
| `Bind` ×7 — typed pipeline | Grows error union one slot per step: `Result<TIn,T1> → Result<TOut, ErrorsOf<T1,T2>>` through 7→8 slots | v1.39.0 | `## Typed Error Pipelines` — `### 9.3.` |
| `Map` — typed pipeline | Transforms success value; `TError` unchanged; single generic overload | v1.39.0 | `## Typed Error Pipelines` — `### 9.4.` |
| `Tap` / `TapOnFailure` — typed pipeline | Side effects on success / failure; original result returned unchanged | v1.39.0 | `## Typed Error Pipelines` — `### 9.4.` |
| `Ensure` ×7 — typed pipeline | Guard conditions that widen the error union by one slot when predicate fails; same O(n) growth as `Bind` | v1.39.0 | `## Typed Error Pipelines` — `### 9.5.` |
| `EnsureAsync` ×7 — typed pipeline | Async variant of `Ensure`; predicate is `Func<TValue, Task<bool>>` | v1.39.0 | `## Typed Error Pipelines` — `### 9.5.` |
| `MapError` — typed pipeline | Translates error surface via `Func<TErrorIn, TErrorOut>`; collapse unions, adapt at layer boundaries | v1.39.0 | `## Typed Error Pipelines` — `### 9.6.` |
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
| `TagKey<T>` | `abstract record TagKey(string Name)` + `sealed record TagKey<T>(string Name) : TagKey(Name)` — typed accessor into `ImmutableDictionary<string, object>` Tags | v1.41.0 | `## Error Types` — `### Typed Tags` |
| `DomainTags` | Predefined typed domain tag keys: `Entity`, `EntityId`, `Field`, `Value`, `Operation` | v1.41.0 | `## Error Types` — `### Typed Tags` |
| `SystemTags` | Predefined typed integration tag keys: `HttpStatus`, `ErrorCode`, `RetryAfter`, `Service` | v1.41.0 | `## Error Types` — `### Typed Tags` |
| `WithTag<T>(TagKey<T>, T)` typed overload | Typed fluent tag builder on all `Reason<T>` subclasses | v1.41.0 | `## Error Types` — `### Typed Tags` |
| `ReasonTagExtensions` | `TryGet<T>` + `Has<T>` — typed tag reads on any `IReason` | v1.41.0 | `## Error Types` — `### Typed Tags` |
| `IErrorFactory<TSelf>` | C# 11 static abstract interface — `static abstract TSelf Create(string message)` | v1.41.0 | `## Error Types` — `### Typed Tags` |
| Built-in errors implement `IErrorFactory<TSelf>` | `Error`, `NotFoundError`, `ConflictError`, `ValidationError`, `ForbiddenError`, `UnauthorizedError` | v1.41.0 | `## Error Types` — `### Typed Tags` |
| `Result.Fail<TError>(string)` | Typed factory overload; `where TError : IError, IErrorFactory<TError>` | v1.41.0 | `## Error Types` — `### Typed Tags` |
| `DomainTags.CorrelationId` | `TagKey<string>` — context → error tag injection key | v1.42.0 | — |
| `DomainTags.OperationName` | `TagKey<string>` — context → error tag injection key | v1.42.0 | — |
| `DomainTags.TenantId` | `TagKey<string>` — context → error tag injection key | v1.42.0 | — |

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
| SmartEndpoints — `ProducesResponseType` for `ErrorsOf<T1..Tn>` | When method returns `Result<T, ErrorsOf<T1..Tn>>`, generator emits `.Produces<Ti>(status)` per error type in OpenAPI metadata | v1.43.0 | `## 🚀 SmartEndpoints` — `### OpenAPI Auto-Generation` table |

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

!!! warning "| RESL1009 | Replace `try/catch` with `Result<T>.Try()` — Info + Code Fix | v1.38.0 | `## 🛡️ Safety Analyzers` — `### RESL1009` |"

!!! warning "| RESL2001 | Unsafe `OneOf.AsT*` without `IsT*` guard — Warning + Code Fix | v1.14.2 | `## 🛡️ Safety Analyzers` — `### RESL2001` |"

!!! warning "| Code Fix: RESL1001 | Wrap in `if (IsSuccess)` guard or replace with `.Match()` | v1.14.2 | `## 🛡️ Safety Analyzers` — `### RESL1001` |"

!!! warning "| Code Fix: RESL1004 | Add `await` keyword | v1.19.0 | `## 🛡️ Safety Analyzers` — `### RESL1004` |"

!!! warning "| Code Fix: RESL1009 | Replace try/catch with `Result<T>.Try(() => ...)` or `Result<T>.Try(() => ..., ex => ...)` | v1.38.0 | `## 🛡️ Safety Analyzers` — `### RESL1009` |"

!!! warning "| Code Fix: RESL2001 | Replace `.AsT*` with exhaustive `.Match()` | v1.14.2 | `## 🛡️ Safety Analyzers` — `### RESL2001` |"

| `[DomainBoundary]` attribute | In `REslava.Result` core; marks methods/constructors as domain boundary entry points — companion to RESL1030 | v1.43.0 | `### 18.12. RESL1030` |
| RESL1030 | Typed error `Result<T, TError>` passed directly to a `[DomainBoundary]` method without `.MapError()` — Warning | v1.43.0 | `### 18.12. RESL1030` |

---

## 9. Http Extensions — `REslava.Result.Http`

| Feature | Short Description | Version | Docs |
|---------|------------------|---------|------|
| `GetResult<T>(string)` / `GetResult<T>(Uri)` | GET → `Result<T>`; auto-deserializes on 2xx, maps 4xx/5xx to typed errors | v1.33.0 | — |
| `PostResult<TBody, TResponse>` | POST with JSON body → `Result<TResponse>` | v1.33.0 | — |
| `PutResult<TBody, TResponse>` | PUT with JSON body → `Result<TResponse>` | v1.33.0 | — |
| `DeleteResult(string)` | DELETE → `Result` (non-generic); 2xx = `Result.Ok()` | v1.33.0 | — |
| `DeleteResult<T>(string)` | DELETE → `Result<T>` with deserialized body | v1.33.0 | — |
| `HttpResultOptions` | Configurable JSON options + complete status-code-to-error override | v1.33.0 | — |

---

## 10. Source Generators — ResultFlow

| Feature | Short Description | Version | Docs |
|---------|------------------|---------|------|
| `[ResultFlow]` attribute | Method-level: marks a method for automatic Mermaid pipeline diagram generation | v1.35.0 | `## 🗺️ Pipeline Visualization` — `### [ResultFlow]` |
| Fluent chain extraction | Walks `InvocationExpressionSyntax` tree; classifies each call by `NodeKind` (9 kinds) | v1.35.0 | `## 🗺️ Pipeline Visualization` — `### How it works` |
| Mermaid `flowchart LR` rendering | Converts `NodeKind` semantics to Mermaid nodes; pastel classDef colors, `\|fail\|` edges for Gatekeeper/TransformWithRisk, no outbound edge for Terminal | v1.35.0 | `## 🗺️ Pipeline Visualization` — `### Diagram Example` |
| `Generated.ResultFlow.{Class}_Flows` | One `public const string {MethodName}` per `[ResultFlow]` method — zero runtime overhead | v1.35.0 | `## 🗺️ Pipeline Visualization` — `### Generated Output` |
| REF001 diagnostic | Info diagnostic when `[ResultFlow]` method body cannot be parsed as a fluent chain | v1.35.0 | `## 🗺️ Pipeline Visualization` — `### REF001` |
| REF002 diagnostic + Code Action | Warning when diagram comment is missing; one-click code fix inserts the Mermaid diagram above the method body | v1.36.0 | `## 🗺️ Pipeline Visualization` — `### REF002 & Code Action` |
| Multi-library convention dictionary | Built-in support for ErrorOr (`Then`/`Switch`) and LanguageExt (`Filter`/`Do`/`DoLeft`) alongside REslava.Result | v1.36.0 | `## 🗺️ Pipeline Visualization` — `### Supported Libraries` |
| `resultflow.json` custom classification | AdditionalFile escape hatch — override or extend the built-in dictionary for any library | v1.36.0 | `## 🗺️ Pipeline Visualization` — `### resultflow.json` |
| REF003 diagnostic | Warning when `resultflow.json` is malformed — falls back to built-in convention | v1.36.0 | `## 🗺️ Pipeline Visualization` — `### REF003` |
| `REslava.ResultFlow` standalone package | Independent NuGet package — works with any Result library | v1.36.0 | `## 🗺️ Pipeline Visualization` — `### Installation` |
| Async step annotation (`⚡`) | `*Async` nodes automatically get a `⚡` label suffix — identifies async I/O steps at a glance | v1.38.0 | `## 🗺️ Pipeline Visualization` — `### Async Annotation` |
| `REslava.ResultFlow` — success type travel | Library-agnostic type travel: first generic type arg of each step's return type rendered as `"Bind<br/>User"` or `"Map<br/>User → UserDto"` labels; falls back to method name for non-generic types | v1.38.0 | `## 🗺️ Pipeline Visualization` — `### Type Travel` |
| `REslava.Result.Flow` — success type travel | New native package: `[ResultFlow]` infers `T` in `Result<T>` at each step via IResultBase; renders `"Bind<br/>User"` or `"Map<br/>User → UserDto"` labels | v1.38.0 | `## 🗺️ Pipeline Visualization` — `### Type Travel` |
| `REslava.Result.Flow` — error type surface | Scans step method bodies for `new XxxError(...)` where `XxxError implements IError`; renders typed `-->|DatabaseError| FAIL` edges with shared FAIL terminal | v1.38.0 | `## 🗺️ Pipeline Visualization` — `### Error Surface` |
| `IInvocationOperation` chain walker | Syntax-walk + per-node `GetOperation()` chain extraction (fixed in v1.38.1 to handle static roots + async extension methods); `IsPipelineRoot` prevents duplicate processing | v1.38.0 | — |
| `REslava.Result.Flow` — REF002 + Code Action | REslava.Result-native counterpart: REF002 info diagnostic on every `[ResultFlow]` method; one-click code fix inserts full-fidelity diagram (type travel + typed error edges) as a `mermaid` fence comment | v1.38.1 | `## 🗺️ Pipeline Visualization` — `### REF002 & Code Action` |
| `mermaid` fence comment format | Both `REslava.ResultFlow` and `REslava.Result.Flow` code actions now wrap the inserted diagram in a `\`\`\`mermaid … \`\`\`` fence — renders inline in VS Code, GitHub, Rider | v1.38.1 | — |
| `Result.Flow` — type-read mode | When method returns `Result<T, TError>`, failure edges show exact error type (reads `TypeArguments[1]`); e.g. `fail: ErrorsOf<ValidationError, InventoryError>`; zero body scanning; HTML-escaped angle brackets for Mermaid | v1.39.0 | `## 🗺️ Pipeline Visualization` — `### Type-Read Mode` |
| Gap 1: lambda body method name | `.Bind(x => SaveUser(x))` → step label `"SaveUser"` | v1.41.0 | — |
| Gap 3: variable initializer resolution | `var r = FindUser(); return r.Bind(...)` correctly seeds chain root | v1.41.0 | — |
| `PipelineNode.NodeId` | Stable node identifier (`"N{i}_{MethodName}"`) for runtime→diagram correlation via `ReasonMetadata.NodeId` | v1.41.0 | — |
| Mermaid node correlation block | `%% --- Node correlation ---` block at end of every diagram | v1.41.0 | — |
| `WithContext` — Invisible node | `.WithContext(...)` classified as `NodeKind.Invisible` in both extractors — not rendered as a pipeline step | v1.42.0 | — |
| `TryExtractContextHints` + diagram footer | `REslava.Result.Flow`: extracts `operationName`/`correlationId` literal args; emits `%%` footer comment block in Mermaid output | v1.42.0 | — |
| `PipelineNode.SourceFile` / `SourceLine` | Source location per pipeline step from `SyntaxNode.GetLocation()`; null for in-memory compilations | v1.43.0 | `### 3.10. Clickable Mermaid Nodes` |
| `ResultFlowLinkMode` — clickable nodes | MSBuild property / `resultflow.json` key; `vscode` mode emits `click` directives with `vscode://file/` URI per node | v1.43.0 | `### 3.10. Clickable Mermaid Nodes` |
| `{MethodName}_Sidecar` constant | Always-generated companion constant — Mermaid diagram wrapped in fenced `# Pipeline — {name}` markdown block | v1.43.0 | `### 3.11. Sidecar Markdown Constant` |
| Cross-method pipeline tracing (`_Cross`) | `REslava.Result.Flow`: follows `[ResultFlow]` chains across method boundaries; emits a `{MethodName}_Cross` constant stitching together all called sub-methods into one unified Mermaid diagram | v1.45.0 | `## 🗺️ Pipeline Visualization` — `### Cross-Method Tracing` |
| `_LayerView` constant | Layer-segregated subgraph Mermaid diagram — groups pipeline nodes by architectural layer (Domain, Application, Infrastructure, Presentation, CrossCutting); one subgraph per layer; layer colors match classDef palette | v1.45.0 | `## 🗺️ Pipeline Visualization` — `### Domain Boundary Diagrams` |
| `_Stats` constant | Pipeline statistics Mermaid diagram — node count, async step ratio, gatekeeper/bind ratios; rendered as a `%%` metadata block + info table | v1.45.0 | `## 🗺️ Pipeline Visualization` — `### Domain Boundary Diagrams` |
| `_ErrorSurface` constant | Error surface Mermaid diagram (`flowchart LR`) — highlights which steps can produce failures and what error types surface from each gatekeeper | v1.45.0 | `## 🗺️ Pipeline Visualization` — `### Domain Boundary Diagrams` |
| `_ErrorPropagation` constant | Error propagation Mermaid diagram (`flowchart TD`) — traces how each failure kind propagates end-to-end through the pipeline toward the terminal exit | v1.45.0 | `## 🗺️ Pipeline Visualization` — `### Domain Boundary Diagrams` |
| `LayerDetector` | Internal component that auto-detects the architectural layer of each `PipelineNode` from its declaring type's namespace segments and `[DomainBoundary]` markers; drives `_LayerView` subgraph grouping | v1.45.0 | — |
| Layer subgraph coloring | Each detected layer in `_LayerView` gets a distinct Mermaid subgraph with its own border/background color — Domain (purple), Application (blue), Infrastructure (orange), Presentation (green), CrossCutting (grey) | v1.45.0 | — |
| Match hexagon + ok/fail edges | `Match`/`MatchAsync` now renders as a Mermaid hexagon `{{"Match"}}:::terminal` with explicit `-->|ok| SUCCESS` and `-->|fail| FAIL` exits — replaces the dead-end rectangle; applies to both `REslava.Result.Flow` and `REslava.ResultFlow` | v1.46.0 | `## 🗺️ Pipeline Visualization` — `### Match — Multi-Branch Fan-Out` |
| `PipelineNode.MatchBranchLabels` | `IReadOnlyList<string>?` — when `Match` is called with N fail-branch lambdas (typed `ErrorsOf<T1..Tn>`), extracts each lambda's explicit parameter type name; drives N typed fail edges in the renderer | v1.46.0 | `## 🗺️ Pipeline Visualization` — `### Match — Multi-Branch Fan-Out` |
| Typed N-branch Match fan-out | `REslava.Result.Flow` only: when `MatchBranchLabels.Count > 0`, emits N distinct `-->|TypeName| FAIL` edges per error type instead of the generic `-->|fail| FAIL`; semantic model resolution with raw-syntax fallback | v1.46.0 | `## 🗺️ Pipeline Visualization` — `### Match — Multi-Branch Fan-Out` |
| Subgraph entry arrow | Cross-method subgraph blocks open with `ENTRY_{nodeId}[ ]:::entry ==>` (invisible thick arrow) to the first inner node; makes execution entry point visible in expanded pipelines; flat pipelines unchanged; both packages | v1.46.3 | — |
| `generate_flow_catalog.py` | Post-build script that scans `obj/Generated/**/*_Flows.g.cs` and publishes all diagram constants as a browsable MkDocs catalog page; groups by class → method → view type; `--project` / `--output` flags | v1.47.0 | — |
| Demo Project MkDocs section | `mkdocs/demo/` — dedicated docs section for `REslava.Result.Flow.Demo`; includes feature overview and live flow catalog; CI auto-rebuilds on every MkDocs deploy | v1.47.0 | — |
| Diagram frontmatter title | Every `[ResultFlow]` diagram opens with `---\ntitle: MethodName\n---`; renders as a native heading above the diagram in VS Code preview, GitHub, and all Mermaid viewers; both packages | v1.47.1 | — |
| `ENTRY_ROOT` root entry node | Chain seed call (e.g. `FindUser`) rendered as amber `ENTRY_ROOT["Method<br/>→ Type"]:::operation ==>` before the first pipeline step; async calls get `⚡`; both packages | v1.47.1 | — |
| Code action: insert/refresh parity | `REslava.ResultFlow` code action now replaces existing diagram comment in-place (was inserting duplicates); shows "Refresh" title when block exists; CRLF-normalised content; both packages now fully equivalent | v1.47.1 | — |
| Async suffix strip | Node labels and frontmatter title strip `Async` suffix — `⚡` already signals async; `FindProductAsync ⚡` → `FindProduct⚡`; both packages | v1.47.2 | — |
| `:::bind` / `:::map` classDef split | `Bind`/`Or`/`MapError` → `:::bind` (thick dark-green border); `Map` → `:::map` (plain green); subgraph borders keep `:::transform`; both packages | v1.47.2 | — |
| Title type annotation | Frontmatter title includes output type: `PlaceOrder⚡ → ⟨Order⟩`; non-generic: `→ ⟨⟩`; `Result<T,TError>` shows T only; both packages | v1.47.2 | — |
| `Legend` constant | One `Legend` Mermaid mini-diagram per `*_Flows` class; horizontal row showing all 9 node types with shapes and colors; access via `OrderService_Flows.Legend`; both packages | v1.47.2 | `### 🗺️ Node Type Legend — Legend constant` |
| `PipelineNode.PredicateText` + Gatekeeper tooltip | `string?` property set when `Ensure`/`Filter` first arg is a lambda; renderer wraps label in `<span title='p.Stock > 0'>Ensure</span>`; visible on hover in VS Code, silently dropped on GitHub; both packages | v1.47.2 | — |
| Legend Guard tooltip + note update | `Legend` constant Guard node uses `<span title='hover shows condition'>Guard</span>`; note updated to `⚡ = async \| Guard: condition shown on hover`; makes tooltip self-documenting in the legend; both packages | v1.47.3 | `### 3.11. Node Type Legend` |
| `scripts/svg.sh` — SVG pipeline orchestrator | New local script: build Demo → export `.mmd` → convert to `.svg`; re-run whenever generator rendering changes; `images/*.mmd` transient (gitignored); local only (mmdc/Puppeteer not in CI) | v1.47.3 | — |
| `generate_flow_catalog.py --export-mmd DIR` | New export mode: `{ClassName}_{ConstantName}.mmd` naming avoids collisions; `Legend.mmd` exported once; auto TD→450/LR→900 width via `SVGO_WIDTH` env var; Stats/Sidecar skipped | v1.47.3 | — |
| `ResultFlowTheme` enum | New enum: `Light = 0` (default), `Dark = 1`; emitted by the attribute generator in both packages; attribute usage: `[ResultFlow(Theme = ResultFlowTheme.Dark)]` | v1.47.4 | — |
| `ResultFlowThemes.cs` — Single Source of Truth | New internal static class in both generator packages; contains `MermaidInit`, `MermaidInitDark`, `Light` classDef block, `Dark` classDef block, `Layer0_Style`…`Layer4_Style` per theme — renderers emit zero hardcoded colour strings | v1.47.4 | — |
| `MermaidInitDark` — dark Mermaid init | Dark diagrams emit `%%{init: {'themeVariables': {'primaryTextColor': '#fff', 'titleColor': '#fff', 'edgeLabelBackground': '#2a2a2a'}}}%%`; fixes black title text and subgraph labels invisible on dark backgrounds; both packages | v1.47.4 | — |
| Dark classDef palette | Full dark colour set for all 9 node kinds matching MkDocs slate scheme; e.g. `operation` → `#3a2b1f`/`#f2c2a0`, `bind` → `#1f3a2d`/`#9fe0c0`, `failure` → `#3a1f1f`/`#f2b8b8`; both packages | v1.47.4 | — |
| Depth-indexed layer subgraphs | `_LayerView` and `_ErrorPropagation` renderers emit `subgraph Layer{depth}["ActualLayerName"]` IDs; `class Layer{depth} Layer{depth}_Style` directives for theming; no more hardcoded layer colour strings in renderers; both packages | v1.47.4 | — |
| `Layer{n}_Style` classDefs (0–4) | 5 depth-indexed layer classDefs in both Light and Dark themes; alternating 2-colour: even = blue-lavender, odd = mint; applied via `class Layer{depth} Layer{depth}_Style` after subgraph emit | v1.47.4 | — |
| `FulfillmentService` dark demo | New demo class in `REslava.Result.Flow.Demo`: `[DomainBoundary("Application")]` + `[ResultFlow(MaxDepth=2, Theme=Dark)]`; calls `WarehouseService.ReserveStock` (Domain) — Application→Domain cross-layer dark theme showcase | v1.47.4 | — |
| `ResultFlowDefaultTheme` MSBuild property | `<ResultFlowDefaultTheme>Dark</ResultFlowDefaultTheme>` in `Directory.Build.props`; solution-wide default; method-level `Theme` attribute wins; case-insensitive; both packages | v1.48.0 | [§3.17](../../README.md#317--solution-wide-default-theme--directorybuildprops-v1480) |
| CodeLens `▶ Open diagram preview` | VS Code extension `reslava.reslava-result-extensions` (Marketplace v1.0.0); 4-step fallback chain; orange R gutter icon on `[ResultFlow]` lines | v1.48.0 | [§3.16](../../README.md#316-️-codelens--diagram-preview-v1480) |
| NuGet README images → transparent PNGs | `mermaid-to-svg.sh` emits `.png` with `--backgroundColor transparent`; 27 PNGs at `images/*.png`; NuGet READMEs use `raw.githubusercontent.com` URLs | v1.48.0 | — |
| VSIX v1.1.0 — WebviewPanel renderer | Replaces sidecar `.md` preview; custom `vscode.WebviewPanel` with bundled Mermaid v10.9.5 (offline); one-panel-per-method; vscode:// node-click navigation; theme-aware background | v1.49.0 | — |
| VSIX v1.1.0 — toolbar | Source panel (Mermaid DSL + Copy); Legend panel (node colour swatches + hints); SVG export; PNG export (2× canvas) | v1.49.0 | — |

---

## 11. OpenTelemetry Integration — `REslava.Result.OpenTelemetry`

| Feature | Short Description | Version | Docs |
|---------|------------------|---------|------|
| `REslava.Result.OpenTelemetry` package | New NuGet package — zero-cost OTel integration; no-op when no active span | v1.42.0 | — |
| `.WithOpenTelemetry()` | Seeds `ResultContext.CorrelationId` from `Activity.Current.TraceId` and `OperationName` from `DisplayName`; 3 overloads: `Result<T>`, `Result`, `Result<T,TError>` | v1.42.0 | — |
| `.WriteErrorTagsToSpan()` | On failure, writes all error tags as span attributes on `Activity.Current`; passes through on success or no span; 3 overloads | v1.42.0 | — |

---

## 12. ResultContext — `REslava.Result`

| Feature | Short Description | Version | Docs |
|---------|------------------|---------|------|
| `ResultContext` sealed record | Pipeline context carrier: `Entity`, `EntityId`, `CorrelationId`, `OperationName`, `TenantId` (all nullable strings) | v1.42.0 | — |
| `Result<T>.Context` / `Result<T,TError>.Context` | Auto-seeded from `typeof(T).Name` on `Ok()`/`Fail()`; public readable, internal settable | v1.42.0 | — |
| `Result.Context` (non-generic) | Null by default; set explicitly via `.WithContext(entity: ...)` | v1.42.0 | — |
| `WithContext(...)` | Fluent merge of `entityId`, `correlationId`, `operationName`, `tenantId` into existing `Context` | v1.42.0 | — |
| Context propagation — parent-wins | All pipeline operators copy incoming `Context` to outgoing; `Map` additionally updates `Entity = typeof(TOut).Name` | v1.42.0 | — |
| Error auto-enrichment | On failure, injects `ResultContext` fields as error tags — non-overwriting (factory-set tags win) | v1.42.0 | — |

---

## Summary

!!! new "**v1.49.0** — 218 features across 15 categories."


| Category | Total Features |
|----------|---------------|
| Core Library | 42 |
| Error Types | 23 |
| SmartEndpoints | 19 |
| Result/OneOf → IResult | 14 |
| Result/OneOf → ActionResult | 8 |
| Validate | 5 |
| Problem Details | 5 |
| Analyzers | 14 |
| OneOf (incl. OneOf5/6/7/8 + chains + ErrorsOf) | 10 |
| Validation DSL | 1 |
| FluentValidation Bridge | 2 |
| Http Extensions | 6 |
| ResultFlow | 60 |
| OpenTelemetry | 3 |
| ResultContext | 6 |
| **Total** | **216** |

---

**See also**
- [API Documentation](../api-doc/index.md)
- [API Reference](../api/index.html)