# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) guideline.

## [1.36.0] - 2026-03-02

### ✨ Added
- **`REslava.ResultFlow` package** — standalone library-agnostic pipeline visualizer, independent of `REslava.Result`. Contains `[ResultFlow]` source generator, `REF002` analyzer, and "Insert diagram as comment" code action. Works with any fluent Result library. Target: `using REslava.ResultFlow;`.
- **`REF002` analyzer + Code Action** — Info diagnostic on every `[ResultFlow]` method with a detectable chain. Single-click code action inserts the Mermaid pipeline diagram as a `/* ... */` block comment directly above the method, respecting existing indentation. No build required.
- **Convention dictionary expansion** — built-in support for **ErrorOr** (`Then`/`ThenAsync`, `Switch`/`SwitchAsync`) and **LanguageExt** (`Filter`, `Do`/`DoAsync`, `DoLeft`/`DoLeftAsync`) alongside REslava.Result — zero configuration.
- **`resultflow.json` AdditionalFile config** — escape hatch for custom or third-party libraries. Config entries **override** the built-in dictionary. Supported keys: `bind`, `map`, `tap`, `tapOnFailure`, `gatekeeper`, `terminal`. REF003 Warning emitted on malformed JSON (fallback to convention).

### ⚠️ Breaking Changes
- **`[ResultFlow]` attribute namespace changed**: `REslava.Result.SourceGenerators` → `REslava.ResultFlow`. Update `using REslava.Result.SourceGenerators;` to `using REslava.ResultFlow;` and replace the `REslava.Result.SourceGenerators` package reference with `REslava.ResultFlow`.
- **`REslava.Result.SourceGenerators` package renamed** to `REslava.Result.AspNetCore`. No stub package — update your reference directly.

### 📚 Documentation
- NuGet README for `REslava.ResultFlow` expanded with Installation, Supported Libraries table, `resultflow.json` config, Code Action description, Diagnostics
- `mkdocs/resultflow/` new top-level section with `index.md`
- `mkdocs/reference/features/index.md`: ResultFlow section expanded to 10 features (total 133 across 13 categories)
- README: section 16.4 expanded with Standalone Package, Code Action, Supported Libraries, `resultflow.json` subsections
- Acknowledgments: ErrorOr (Amanti Band) and LanguageExt (Paul Louth) added

### Stats
- 3,795 tests passing across net8.0, net9.0, net10.0 (1,157×3) + generator (143) + ResultFlow (27) + analyzer (68) + FluentValidation bridge (26) + Http (20×3)
- 133 features across 13 categories

---

## [1.35.0] - 2026-03-02

### ✨ Added
- **`[ResultFlow]` source generator** — decorate any fluent `Result<T>` pipeline method with `[ResultFlow]`; a Mermaid `flowchart LR` diagram is emitted as a `public const string` in `Generated.ResultFlow.{ClassName}_Flows` at compile time — zero runtime overhead, zero manual maintenance. Supports 9 operation semantics: `Ensure`/`EnsureAsync` (gatekeeper with `|fail|` edge), `Bind`/`BindAsync` (transform with risk), `Map`/`MapAsync` (pure transform), `Tap`/`TapAsync` (side effect — success), `TapOnFailure`/`TapOnFailureAsync` (side effect — failure), `TapBoth` (side effect — both), `Match` (terminal, no outbound edges), `WithSuccess`/`WithError` (invisible — traversed, not rendered). REF001 info diagnostic emitted when method body cannot be parsed as a fluent chain.

### 📚 Documentation
- README: `### 🗺️ Pipeline Visualization — [ResultFlow]` section with Mermaid diagram, color legend, and generated output example
- `mkdocs/index.md`: comparison table row + teaser admonition
- `mkdocs/reference/features/index.md`: section 10 — ResultFlow (5 features, total 128 across 13 categories)

### Stats
- 3,768 tests passing across net8.0, net9.0, net10.0 (1,157×3) + generator (143) + analyzer (68) + FluentValidation bridge (26) + Http (20×3)
- 128 features across 13 categories

---

## [1.34.0] - 2026-03-02

### ⚠️ Breaking Changes
- **`IResultResponse<T>` renamed to `IResultBase<T>`** — `IResultResponse` carried an unintended HTTP connotation; `IResultBase` is the semantically correct name — it is the base contract for all `Result` types, not an HTTP response. If you reference `IResultResponse<T>` directly in your code, update to `IResultBase<T>`.

### 📚 Documentation
- Fill documentation gaps: Http extensions usage, generator setup guide, `ConversionError` documented in error type table

### Stats
- 3,756 tests passing (unchanged from v1.33.0)
- 123 features across 12 categories (unchanged)

---

## [1.33.0] - 2026-03-01

### ✨ Added
- **`REslava.Result.Http`** — new 5th NuGet package; wraps `HttpClient` calls so every HTTP response and network failure becomes a typed `Result<T>`; public API: `GetResult<T>(string/Uri)`, `PostResult<TBody, TResponse>`, `PutResult<TBody, TResponse>`, `DeleteResult`, `DeleteResult<T>`; configurable via `HttpResultOptions` (custom `JsonSerializerOptions`, custom `StatusCodeMapper`); default mapper: `NotFoundError`, `UnauthorizedError`, `ForbiddenError`, `ConflictError`, `ValidationError`, generic `Error` for all other 4xx/5xx; network failures (`HttpRequestException`) and timeouts (`OperationCanceledException`) wrapped in `ExceptionError`; user-initiated cancellation re-thrown; no extra NuGet dependencies on net8+

### 📚 Documentation
- README/TOC full restructure — 26 sections with improved logical hierarchy and sub-section grouping
- MkDocs site restructure — sub-folder grid-card navigation, `architecture/source-generators/` sub-folder, orphan detection script (`scripts/find_orphans.py`), pipeline script fixes

### Stats
- 3,756 tests passing across net8.0, net9.0, net10.0 (1,157×3) + generator (131) + analyzer (68) + FluentValidation bridge (26) + Http (20×3)
- 123 features across 12 categories

---

## [1.32.0] - 2026-02-28

### ✨ Added
- **`Result.Validate(r1, r2, ..., mapper)`** — applicative validation; runs 2/3/4 independent `Result<T>` validations simultaneously; accumulates ALL errors without short-circuiting; maps heterogeneous success values to `Result<TResult>` via typed mapper func; mapper is only invoked when all inputs succeed
- **`Result<T>.Deconstruct()`** — C# 8+ tuple unpacking; `var (value, errors) = result` (2-component) and `var (isSuccess, value, errors) = result` (3-component) for `Result<T>`; `var (isSuccess, errors) = result` for non-generic `Result`; value is `default` when `IsFailure`
- **`Maybe<T>` ↔ `Result<T>` interop** — `maybe.ToResult(Func<IError>)`, `maybe.ToResult(IError)`, `maybe.ToResult(string)` bridge `None` to typed failure; `result.ToMaybe()` converts success to `Some(value)` and failure to `None` (error info intentionally discarded)

### Stats
- 3,696 tests passing across net8.0, net9.0, net10.0 (1,157×3) + generator (131) + analyzer (68) + FluentValidation bridge (26)

---

## [1.31.0] - 2026-02-28

### ✨ Added
- **`Result.WithLogger(ILogger, string)`** / **`LogOnFailure(ILogger, string)`** — Tap-style ILogger integration; logs Debug on success, Warning on domain failure (non-`ExceptionError`), Error on `ExceptionError`; structured log properties: `result.outcome`, `result.error.type`, `result.error.message`; `Task<Result<T>>` and `Task<Result>` extensions with `CancellationToken`
- **`Result.Recover()`** / **`RecoverAsync()`** — railway recovery; transforms any failure into a new `Result<T>` (success or failure) via `Func<ImmutableList<IError>, Result<T>>`; error list passed to recovery func enables context-aware branching (e.g. skip recovery on `ForbiddenError`); non-generic `Result` variant included; `Task<Result<T>>` extensions with `CancellationToken`
- **`Result.Filter()`** / **`FilterAsync()`** — converts success to failure when a predicate fails; `Func<T, IError>` error factory enables value-dependent contextual messages (e.g. `"User 'John' is not active"`); 3 sync overloads: factory / static `IError` / string message; async predicate variant (`Func<T, Task<bool>>`); `Task<Result<T>>` extensions; predicate exceptions wrapped in `ExceptionError`

### Stats
- 3,591 tests passing across net8.0, net9.0, net10.0 (1,122×3) + generator (131) + analyzer (68) + FluentValidation bridge (26)

---

## [1.30.0] - 2026-02-27

### ✨ Added
- **`Result.Catch<TException>()`** / **`CatchAsync<TException>()`** — inline typed exception handler in the railway pipeline; if the result contains an `ExceptionError` wrapping `TException`, replaces it with the error returned by the handler (in-place, preserving position in the reasons list); `Task<Result<T>>` extension also catches `TException` thrown directly from the source task
- **`Result.WithActivity(Activity?)`** — Tap-style extension that enriches an existing OTel `Activity` span with result outcome metadata: `result.outcome` (`"success"` / `"failure"`), `result.error.type`, `result.error.message`, `result.error.count` (when > 1 error); sets `ActivityStatusCode.Ok` / `ActivityStatusCode.Error`; null-safe (no-op when activity is null); no new NuGet dependency — uses BCL `System.Diagnostics.Activity`

### Stats
- 3,432 tests passing across net8.0, net9.0, net10.0 (1,069×3) + generator (131) + analyzer (68) + FluentValidation bridge (26)

---

## [1.29.0] - 2026-02-25

### ⚠️ Breaking Changes
- **`IsFailed` renamed to `IsFailure`** — `IsSuccess` / `IsFailure` is the correct symmetric pair. `IsFailed` was past-tense verb form (semantically incorrect for a boolean property). No alias or `[Obsolete]` shim — update call sites directly.

### ✨ Added
- **Console samples — 3 new examples** covering v1.27.0–v1.28.0 features:
  - `14_ValidationDSL.cs` — all 19 native DSL rules with real-world order validator
  - `15_OneOf5_OneOf6.cs` — 5/6-way unions, chain extensions (up/down conversions), checkout pipeline
  - `16_AsyncPatterns_Advanced.cs` — WhenAll, Retry (backoff), Timeout, TapOnFailure, OkIf/FailIf, Try/TryAsync
- **FastMinimalAPI validation showcase** — side-by-side comparison of all three validation approaches:
  - `/api/smart/validation` — DataAnnotations + `[Validate]` auto-guard vs. native Validation DSL
  - `/api/smart/fluent-validation` — `[FluentValidate]` migration bridge demo (optional)
- **FastMvcAPI validation parity** — explicit `request.Validate()` guard + `CancellationToken` in MVC controllers
- **Feature Reference page** — `mkdocs/reference/features/index.md` — 109 features across 11 categories

### 🔧 Fixed
- **release.yml** — `REslava.Result.FluentValidation` added to Build and Pack steps (was missing; caused v1.28.0 NuGet package to lack its embedded README)

### Stats
- 3,339 tests passing across net8.0, net9.0, net10.0 + generator (131) + analyzer (68) + FluentValidation bridge (26)

---

## [1.28.0] - 2026-02-25

### ✨ Added
- **REslava.Result.FluentValidation** — new 4th NuGet package (generator-only, `DevelopmentDependency=true`); `[FluentValidate]` attribute emits `.Validate(IValidator<T> validator)` + `.ValidateAsync(IValidator<T>, CancellationToken)` extension methods per decorated type; bridge for teams migrating from FluentValidation who want `Result<T>` and SmartEndpoints integration without rewriting existing validators
- **SmartEndpoints FluentValidation injection** — when a POST/PUT body parameter type is decorated with `[FluentValidate]`, the generated lambda automatically adds `IValidator<T>` as a DI-injected parameter and emits the `.Validate(validator)` guard block; adds `using FluentValidation;` and `using Generated.FluentValidationExtensions;` conditionally
- **RESL1006 analyzer** — compile-error diagnostic when both `[Validate]` and `[FluentValidate]` are applied to the same type; prevents conflicting `.Validate()` extension method signatures

### Stats
- 3,339 tests passing across net8.0, net9.0, net10.0 + generator (131) + analyzer (68) + FluentValidation bridge (26)

---

## [1.27.0] - 2026-02-25

### ✨ Added
- **CancellationToken Support in SmartEndpoints** — generated endpoint lambdas detect `CancellationToken cancellationToken = default` in service method signatures and inject it as an endpoint parameter; service methods remain opt-in; fully backward-compatible
- **OneOf5 / OneOf6** — new `OneOf<T1..T5>` and `OneOf<T1..T6>` readonly structs with full `Match`, `Switch`, `MapT*`, `BindT*`, equality, `GetHashCode`, `ToString`, and implicit conversions; matching the same API surface as OneOf2–OneOf4
- **OneOf chain extensions** — complete arity chain in `OneOfExtensions`: `ToFourWay`, `ToFiveWay`, `ToSixWay` up-conversions (anchors the new type via a `defaultValue` parameter) plus nullable, mapper, and `WithFallback` down-conversions across the full 2↔3↔4↔5↔6 chain
- **Native Validation DSL** — 19 fluent extension methods on `ValidatorRuleBuilder<T>` via `ValidatorRuleBuilderExtensions`; `Expression<Func<T, TProperty>>` selectors auto-infer property names for default error messages:
  - **String**: `NotEmpty`, `NotWhiteSpace`, `MinLength`, `MaxLength`, `Length`, `EmailAddress`, `Matches`, `StartsWith`, `EndsWith`, `Contains`
  - **Numeric** (generic `where TNum : struct, IComparable<TNum>`): `GreaterThan`, `LessThan`, `Range`, `Positive`, `NonNegative` — work for `int`, `long`, `double`, `decimal`, etc.
  - **Collection**: `NotEmpty<T,TItem>`, `MinCount`, `MaxCount`
  - **Reference**: `NotNull`
- **DocFX API Reference** — all public types, members, and XML documentation now fully surfaced in the hosted API reference at `/reference/api/`

### 🔧 Fixed
- **OneOf4 bug fixes** — 10+ edge-case fixes across `AsT*` guard behaviour, `MapT*` propagation, `BindT*` null handling
- **OneOf5/6 source generators** — added `OneOf5ToIResultGenerator`, `OneOf6ToIResultGenerator`, `OneOf5ToActionResultGenerator`, `OneOf6ToActionResultGenerator`

### Stats
- 3,313 tests passing across net8.0, net9.0, net10.0 + generator (131) + analyzer (68)

---

## [1.26.0] - 2026-02-24

### ✨ Added
- **RESL1005 analyzer** — Info-level diagnostic that suggests a domain-specific error type (`NotFoundError`, `ConflictError`, `UnauthorizedError`, `ForbiddenError`, `ValidationError`) when `new Error("...")` is used with a message that implies a well-known HTTP error category. Helps developers discover domain errors that carry automatic HTTP status context and integrate with `ToIResult()`.
- **SmartEndpoints: Auto-Validation** — when a method's body parameter type is decorated with `[Validate]`, the generated endpoint lambda automatically calls `.Validate()` and returns early with the validation result before invoking the service. Requires no attribute on the method — decoration on the type is the only signal. Adds `using Generated.ValidationExtensions;` conditionally.

### Stats
- 2,862 tests passing across net8.0, net9.0, net10.0 + generator (106) + analyzer (68) tests

## [1.25.0] - 2026-02-24

### ✨ Added
- **Documentation website** — MkDocs Material site auto-generated from `README.md` on every push; 8 nav sections, dark/light mode, search, social cards, git revision dates
- **DocFX API reference landing page** — Bootstrap card grid with namespace cards, Core Types at a Glance, and quick-links to docs/GitHub/NuGet; deployed at `/reference/api/`
- **CI path filtering** — CI workflow now uses allowlist (`src/**`, `tests/**`) instead of denylist; docs-only commits no longer trigger the test suite

### 🔧 Fixed
- `organize_docs.py`: `reference/api-docs` path typo corrected to `reference/api-doc`
- Docs workflow trigger: added `docfx/**` path and corrected self-reference from `mkdocs.yml` to `mkdocs-docfx.yml`
- MkDocs Reference index: replaced copy-pasted placeholder descriptions with accurate content per card

### Stats
- 2,843 tests passing across net8.0, net9.0, net10.0 + generator + analyzer tests

## [1.24.0] - 2026-02-23

### ✨ Added
- `[Validate]` source generator — decorate any record/class to get a `.Validate()` extension method returning `Result<T>`; delegates to `Validator.TryValidateObject` so all 20+ `DataAnnotations` types work automatically; invalid fields surface as `ValidationError` with `FieldName` populated; composable with `.Bind()` / `.ToIResult()` / `.ToActionResult()`

### Stats
- 2,843 tests passing across net8.0, net9.0, net10.0 + generator + analyzer tests

## [1.23.0] - 2026-02-23

### ✨ Added
- **SmartEndpoints: Endpoint Filters** — new `[SmartFilter(typeof(T))]` attribute (`AllowMultiple = true`) generates `.AddEndpointFilter<T>()` for each filter in declaration order
- **SmartEndpoints: Output Caching** — `CacheSeconds` property on `[AutoGenerateEndpoints]` (class default) and `[AutoMapEndpoint]` (method override); generates `.CacheOutput(x => x.Expire(...))` only for GET endpoints; `-1` = explicit opt-out
- **SmartEndpoints: Rate Limiting** — `RateLimitPolicy` property on both attribute levels; generates `.RequireRateLimiting("policy")`; `"none"` = explicit opt-out; method value overrides class default
- **FastMinimalAPI Demo: SmartCatalogController** — showcases all three features with `LoggingEndpointFilter`, output cache + rate limiter middleware registered in `Program.cs`
- **scripts/validate-release.sh** — pre-release validation checklist (9 checks: version, CHANGELOG, release notes file, README roadmap/history, tests, git state, TODO check, test count accuracy)
- **11 new source generator tests** — `SmartEndpoints_FiltersAndCachingTests.cs`

### Stats
- 2,836 tests passing across net8.0, net9.0, net10.0 + generator + analyzer tests

## [1.22.0] - 2026-02-18

### ✨ Added
- **OneOfToActionResult source generator** — `ToActionResult()` extension methods for `OneOf<T1,...,T4>` in MVC controllers
  - 3 thin generator wrappers (OneOf2, OneOf3, OneOf4) with shared orchestrator
  - IError.Tags-first mapping + type-name heuristic fallback, MVC result types
  - Generated into `namespace Generated.OneOfActionResultExtensions`
  - MVC demo controllers updated — all `OneOf.Match()` replaced with `.ToActionResult()` one-liners
- **12 new source generator tests** for OneOfToActionResult (2/3/4 arity)

### 🔧 Fixed
- **OneOfToIResult: tag-based error mapping** — `MapErrorToHttpResult` now checks `IError.Tags["HttpStatusCode"]` first, falls back to type-name heuristic only for non-IError types. Domain errors with custom `HttpStatusCode` tags now map correctly.
- **ValidationError → 422** — OneOfToIResult heuristic and SmartEndpoints OpenAPI both now map `ValidationError`/`Invalid` to 422 (was 400)

### 📝 Changed
- **SmartEndpoints OpenAPI: accurate error status codes** — `DetermineOpenApiStatusCode` maps `ValidationError` to 422 (was 400). `Result<T>` endpoints now declare `.Produces(400)`, `.Produces(404)`, `.Produces(409)`, `.Produces(422)` (was only 400).

### Stats
- 2,836 tests passing across net8.0, net9.0, net10.0 + generator + analyzer tests

## [1.21.0] - 2026-02-17

### ✨ Added
- **ResultToActionResult source generator** — `ToActionResult()` extension methods for ASP.NET MVC controllers
  - Convention-based: reads `HttpStatusCode` tag from domain errors, auto-maps to `IActionResult` types
  - Explicit overload: `ToActionResult(onSuccess, onFailure)` escape hatch for full control
  - HTTP verb variants: `ToPostActionResult()` (201), `ToPutActionResult()` (200), `ToPatchActionResult()` (200), `ToDeleteActionResult()` (204)
  - Private `MapErrorToActionResult` helper: 401→`UnauthorizedResult`, 403→`ForbidResult`, 404→`NotFoundObjectResult`, 409→`ConflictObjectResult`, default→`ObjectResult`
  - Generated into `namespace Generated.ActionResultExtensions`
  - Zero runtime dependency — MVC types emitted as string literals by the generator
- **FastMvcAPI demo app** (`samples/FastMvcAPI.REslava.Result.Demo`)
  - MVC equivalent of existing Minimal API demo — same domain (Users, Products, Orders)
  - Showcases `ToActionResult()` one-liners and `OneOf.Match()` with MVC result types
  - Runs on port 5001 (side-by-side with Minimal API demo on 5000)
  - Scalar UI at `/scalar/v1`
- **9 new source generator tests** for ResultToActionResult generator

### Stats
- 2,813 tests passing across net8.0, net9.0, net10.0 + generator + analyzer tests

## [1.20.0] - 2026-02-17

### ✨ Added

**Structured Error Hierarchy (Domain Errors)**
- `NotFoundError` — HTTP 404, with `(entityName, id)` constructor and `EntityName`/`EntityId` tags
- `ValidationError` — HTTP 422, with `FieldName` property, `(fieldName, message)` constructor
- `ConflictError` — HTTP 409, with `(entityName, conflictField, conflictValue)` constructor
- `UnauthorizedError` — HTTP 401, with default "Authentication required" message
- `ForbiddenError` — HTTP 403, with `(action, resource)` constructor
- All domain errors use CRTP pattern (`Reason<TSelf>, IError`), carry `HttpStatusCode` and `ErrorType` tags, and support fluent `WithTag`/`WithMessage` chaining
- 27 new domain error tests

**Test Coverage Hardening (123 new tests)**
- `ResultConditionalTests` — 39 tests covering all `OkIf`/`FailIf` overloads (non-generic, generic, lazy, async)
- `ResultTryTests` — 15 tests covering `Try`/`TryAsync` (success, exception, custom handler, null guards, cancellation)
- `ResultCombineTests` — 18 tests covering `Merge`/`Combine`/`CombineParallelAsync`
- `ResultTapExtensionsTests` — 30 tests covering `TapOnFailure`/`TapBoth`/`TapAsync` variants
- `ResultLINQTaskExtensionsTests` — 21 tests covering `Task<Result<S>>` LINQ extensions

### 🔧 Changed

**ResultToIResult Generator — Domain Error-Aware HTTP Mapping**
- `ToIResult`, `ToPostResult`, `ToPutResult`, `ToPatchResult`, `ToDeleteResult` now read the `HttpStatusCode` tag from domain errors instead of always returning 400
- Supports both `HttpStatusCode` (library convention) and `StatusCode` (legacy convention) tags
- Maps: 404→`NotFound`, 401→`Unauthorized`, 403→`Forbid`, 409→`Conflict`, others→`Problem(statusCode)`
- Extracted shared `MapErrorToIResult` helper (eliminated 5x duplicated error blocks)

**ValidationResult.Failure — Uses ValidationError**
- `ValidationResult<T>.Failure(string)` now creates `ValidationError` instead of generic `Error`
- Failures automatically carry `HttpStatusCode=422` and `ErrorType=Validation` tags

**Performance: Cached Computed Properties**
- `Result.Errors` and `Result.Successes` are now lazy-cached on first access
- `Result.IsFailure` uses `Errors.Count > 0` instead of re-enumerating `Reasons.OfType<IError>().Any()`
- Safe because `Result` is immutable (`Reasons` has `private init`)

**SmartEndpoints: Convention-Based Route Prefix**
- Default route prefix derived from class name (e.g., `UserService` → `/api/users`) instead of hard-coded `/api/test`
- Strips common suffixes: Service, Controller, Endpoints, Endpoint

**Result.ToString() Override**
- Base `Result` class now overrides `ToString()`: `Result: IsSuccess='True', Reasons=[...]`

**ExceptionError Namespace Fix**
- `ExceptionError` moved from global namespace to `REslava.Result` namespace (was polluting consumers' global scope)

**Result\<T\> Constructor Encapsulation**
- Two `public` constructors changed to `internal` — prevents construction of invalid states bypassing factory methods
- Added `InternalsVisibleTo("REslava.Result.Tests")`

### 🧹 Removed

**Source Generator Dead Code Cleanup**
- Deleted duplicate `HttpStatusCodeMapper` (2 files — static and instance versions)
- Deleted orphan `SmartEndpointExtensionGenerator.cs` (stale intermediate version)
- Deleted `Test1.cs` (empty placeholder) and `ConsoleTest.cs.disabled` (abandoned)
- Removed duplicate `ExtractStringArrayFromAttributeData` method in `SmartEndpointsOrchestrator`
- Removed marker comments from `Result.Combine.cs`, `Result.Conversions.cs`, `Result.Generic.cs`

**Demo App: Migrated to Library Domain Errors**
- Deleted 3 custom error files (`NotFoundErrors.cs`, `ValidationErrors.cs`, `BusinessErrors.cs`) — 12 custom error classes replaced by 5 library domain errors
- Simplified OneOf signatures (e.g., `OneOf<ValidationError, InvalidPriceError, ProductResponse>` → `OneOf<ValidationError, ProductResponse>`)
- Demo app now references local project instead of NuGet package (for latest domain errors)

### 📊 Stats

- **2,798 tests passing** (896 x 3 TFMs + 56 source generator + 54 analyzer)
- 150 new tests in this release
- 7 files deleted, 5 domain error types added

---

## [1.19.0] - 2026-02-16

### ✨ Added

**RESL1004 — Async Result Not Awaited Analyzer**
- Detects `Task<Result<T>>` assigned to `var` without `await` in async methods
- Code fix: automatically adds `await` keyword
- Skips: explicit `Task<...>` type declarations, non-async methods, returned tasks

**CancellationToken Support Throughout**
- Added `CancellationToken cancellationToken = default` to all async methods
- Instance methods: `TapAsync`, `BindAsync`, `MapAsync`, `MatchAsync`
- Factory methods: `TryAsync` (generic and non-generic)
- Extension methods: `BindAsync`, `MapAsync`, `TapAsync`, `TapOnFailureAsync`, `WithSuccessAsync`, `EnsureAsync`, `EnsureNotNullAsync`, `SelectManyAsync`, `SelectAsync`, `WhereAsync`, `MatchAsync`
- Source-compatible: existing code compiles without changes
- 13 new CancellationToken tests + 8 new analyzer tests

---

## [1.18.0] - 2026-02-16

### ✨ Added

**Task-Based Async Patterns (WhenAll, Retry, Timeout)**
- `Result.WhenAll()` — run 2/3/4 async Result operations concurrently, returning typed tuples with aggregated errors
- `Result.WhenAll(IEnumerable<Task<Result<T>>>)` — collection variant returning `Result<ImmutableList<T>>`
- `Result.Retry()` — retry async operations with configurable delay, exponential backoff, and CancellationToken support
- `.Timeout()` extension on `Task<Result<T>>` — enforce time limits with TimeoutTag metadata on timeout errors
- Exception-safe: faulted/cancelled tasks wrapped in `ExceptionError`, `OperationCanceledException` stops retries
- Non-generic overloads for `Result.Retry()` and `.Timeout()`
- 41 new async pattern tests (per TFM)

---

## [1.17.0] - 2026-02-16

### ✨ Added

**JSON Serialization Support (System.Text.Json)**
- `JsonConverter<Result<T>>` — serializes as `{ "isSuccess": true, "value": ..., "errors": [], "successes": [] }`
- `JsonConverter<OneOf<T1,T2>>`, `OneOf<T1,T2,T3>`, `OneOf<T1,T2,T3,T4>` — serializes as `{ "index": 0, "value": ... }`
- `JsonConverter<Maybe<T>>` — serializes as `{ "hasValue": true, "value": ... }`
- `JsonSerializerOptions.AddREslavaResultConverters()` extension method to register all converters
- Error/Success reasons serialized with type name, message, and tags metadata
- Zero new dependencies — uses built-in `System.Text.Json`
- All converters use hardcoded camelCase property names for predictable output
- 48 new serialization tests (16 per TFM)

---

## [1.16.0] - 2026-02-16

### 🔧 Changed

**NuGet Package READMEs — Discoverability**
- Created tailored NuGet README for `REslava.Result` — focused quick-start with before/after code comparison
- Created tailored NuGet README for `REslava.Result.SourceGenerators` — SmartEndpoints showcase with before/after
- Created tailored NuGet README for `REslava.Result.Analyzers` — diagnostic rules table with code fix examples
- Each package now has its own focused README (~60-75 lines) instead of sharing the full GitHub README (~800+ lines)
- NuGet READMEs stored in `docs/nuget/` directory

---

## [1.15.0] - 2026-02-15

### 🧹 Removed

**Project Cleanup — Node.js Toolchain & Legacy Files**
- Removed Node.js release toolchain (`package.json`, `package-lock.json`, `.versionrc.json`) — superseded by `Directory.Build.props` + GitHub Actions `release.yml`
- Removed Husky git hooks (`.husky/`) and commitlint (`commitlint.config.js`) — commit validation now handled by CI/CD pipeline
- Removed `scripts/` directory (5 files: `clean-before-test.ps1`, `quick-clean.ps1`, `update-github-release.sh`, `update-versions.js`, `CLEAN-BEFORE-TEST.md`) — superseded by CI/CD pipeline
- Removed `templates/` directory — incomplete, unpublished dotnet template
- Removed `samples/NuGetValidationTest/` — stale test project with v1.9.0 package references

### 🔧 Changed

**Documentation Refresh**
- Standardized emoji: replaced 🏗️ with 📐 across all 34 markdown files (fixed anchor link issues with variation selector)
- Updated README.md Roadmap section (v1.15.0 current, refreshed milestone descriptions)
- Removed speculative "Future Versions" section from README.md
- Updated test counts to 2,004+ throughout documentation
- Rewrote `samples/README.md` to reflect actual sample projects

---

## [1.14.2] - 2026-02-15

### ✨ Added

**New Analyzers & Code Fixes (Phase 2 + 3)**
- **RESL1003 — Prefer Match() over if-check**: Info-level suggestion when both `.Value` and `.Errors` are accessed in complementary `if`/`else` branches. Detects all 4 condition variants: `IsSuccess`, `IsFailure`, `!IsSuccess`, `!IsFailure`
- **RESL2001 — Unsafe OneOf.AsT* access**: Warning when `.AsT1`–`.AsT4` is accessed on `OneOf<T1,...>` without checking the corresponding `.IsT*` first. Supports guard detection via if-checks and early returns
- **RESL1001 Code Fix**: Two fix options — wrap in `if (result.IsSuccess) { ... }` guard, or replace with `.Match(v => v, e => default)`
- **RESL2001 Code Fix**: Replaces `.AsT*` with complete `.Match()` call, generating all arity lambdas with `NotImplementedException()` placeholders

**Infrastructure**
- Shared `GuardDetectionHelper` with parameterized `GuardConfig` — reusable guard detection for both Result and OneOf analyzers
- Generic `AnalyzerTestHelper` with `CreateAnalyzerTest<T>()` and `CreateCodeFixTest<T,F>()` methods
- `OneOfStubSource` test stubs for all 3 OneOf arities
- 28 new analyzer tests (46 total), 2,004 total project tests

### 🔧 Changed
- Refactored `UnsafeValueAccessAnalyzer` to use shared `GuardDetectionHelper` (247 → ~80 lines)

---

## [1.14.1] - 2026-02-10

### 🔧 Changed

**Source Generator Consolidation: OneOfToIResult**
- Consolidated `OneOf2ToIResult`, `OneOf3ToIResult`, `OneOf4ToIResult` into a single `OneOfToIResult` directory
- Replaced 15 near-identical files with 7 arity-parameterized shared implementations
- Single `OneOfToIResultOrchestrator` handles all arities (2, 3, 4) via constructor parameter
- 3 thin `[Generator]` wrappers remain (Roslyn requires separate classes per generator)
- Unified test file with 12 tests replacing 3 separate test files
- No API changes — generated output is identical

---

## [1.14.0] - 2026-02-10

### ✨ Added

**NEW: REslava.Result.Analyzers NuGet Package**
- New companion NuGet package providing Roslyn diagnostic analyzers for REslava.Result
- **RESL1001 — Unsafe Result<T>.Value access**: Warns when `.Value` is accessed without checking `IsSuccess` or `IsFailure` first. Detects 5 guard patterns: `if (result.IsSuccess)`, `if (!result.IsFailure)`, else-branch of `IsFailure`, early return, and early throw
- **RESL1002 — Discarded Result<T> return value**: Warns when a method returning `Result<T>` or `Task<Result<T>>` is called and the return value is ignored, silently swallowing errors
- 18 analyzer tests (10 for RESL1001, 8 for RESL1002)
- Zero-dependency analyzer — ships as `analyzers/dotnet/cs` in the NuGet package

**NuGet Package Improvements**
- Added package icon to REslava.Result.SourceGenerators and REslava.Result.Analyzers
- Added package README to REslava.Result.Analyzers
- Release pipeline now builds and publishes all 3 packages

### 🔧 Fixed

**CI/CD Pipeline**
- Release workflow now includes REslava.Result.Analyzers in build, pack, and publish steps

---

## [1.13.0] - 2026-02-10

### ✨ Added

**SmartEndpoints: Authorization & Policy Support**
- Class-level `RequiresAuth = true` on `[AutoGenerateEndpoints]` — all endpoints emit `.RequireAuthorization()`
- Class-level `Policies = new[] { "Admin" }` — emits `.RequireAuthorization("Admin")`
- Class-level `Roles = new[] { "Admin", "Manager" }` — emits `.RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Manager" })`
- Method-level `[SmartAllowAnonymous]` attribute — overrides class auth, emits `.AllowAnonymous()`
- Method-level `[AutoMapEndpoint(AllowAnonymous = true, Roles = ...)]` — fine-grained control
- Auth inheritance: class-level defaults propagate to all methods unless overridden
- Auto-adds `.Produces(401)` to OpenAPI metadata for auth-protected endpoints
- Conditional `using Microsoft.AspNetCore.Authorization;` only when Roles are used
- 12 new authorization tests (`SmartEndpoints_AuthorizationTests.cs`)

### 📚 Documented

**LINQ Query Syntax for Result<T>** (already implemented, now formally documented)
- `Select`, `SelectMany` (2-param + 3-param for query syntax), `Where` — all with async variants
- Enables: `from user in GetUser(id) from account in GetAccount(user.AccountId) select ...`
- 35 tests passing across net8.0, net9.0, net10.0
- Console sample: `05_LINQSyntax.cs` with 8 progressive examples
- Moved from "Next Up" to "Current" in roadmap

### ✨ Added

**Demo App: JWT Bearer Authentication Showcase**
- JWT Bearer auth configured for SmartEndpoints auth demo
- `/auth/token` endpoint generates test JWTs with optional role parameter
- `SmartOrderController` uses `RequiresAuth = true` with `[SmartAllowAnonymous]` on `GetOrders()`
- Side-by-side comparison: authenticated SmartOrders vs unauthenticated SmartProducts

**SmartEndpoints: OpenAPI Metadata Auto-Generation**
- Endpoints now emit full OpenAPI metadata from return type analysis at compile time
  - `.WithName("ControllerBase_MethodName")` — globally unique endpoint names
  - `.WithSummary("...")` — auto-generated from PascalCase method name or XML doc `<summary>`
  - `.WithTags("...")` — auto-generated from class name (strips Controller/Service suffix, splits PascalCase)
  - `.Produces<T>(200)` — typed success response from `Result<T>` or non-error OneOf type arguments
  - `.Produces(statusCode)` — error status codes inferred from error type names (NotFound→404, Conflict→409, Unauthorized→401, Forbidden→403, Database→500, Validation/default→400)
- Endpoints are grouped per controller using `MapGroup(prefix).WithTags(tag)` instead of flat registration
  - Relative routes within groups (e.g., `/{id}` instead of `/api/products/{id}`)
  - Controller-scoped variable names (e.g., `smartProductGroup`)
- Status code deduplication — two errors mapping to 400 produce a single `.Produces(400)`
- 21 new tests covering all OpenAPI metadata features (`SmartEndpoints_OpenApiMetadataTests.cs`)

---

## [1.12.2] - 2026-02-09

### 🔧 Fixed

**SmartEndpoints Source Generator**
- Fixed SmartEndpointsGenerator to delegate to orchestrator instead of emitting hardcoded stub endpoint
  - Removed inline placeholder code (`/api/simple/test`) that bypassed the real generator pipeline
  - Generator now follows the same SOLID pattern as all other generators (Generator → Orchestrator)
- Added dependency injection support in generated endpoints
  - Services are now injected via ASP.NET Minimal API parameter binding instead of `new ClassName()`
- Added async/await support in generated endpoints
  - Detects `Task<Result<T>>` and `Task<OneOf<...>>` return types
  - Generates proper `async`/`await` lambda syntax
- Added missing `using` directives in generated code
  - `using System.Threading.Tasks;`
  - `using Generated.ResultExtensions;`
  - `using Generated.OneOfExtensions;`

---

## [1.12.1] - 2026-02-08

### ✨ Added

**Sample Applications**
- **FastMinimalAPI.REslava.Result.Demo** - Production-ready ASP.NET Core Minimal API demonstration
  - Comprehensive README with learning path (Level 1, 2, 3 patterns)
  - 15 endpoints showcasing Result&lt;T&gt; and OneOf patterns
  - Real-world business scenarios (Users, Products, Orders)
  - Rich error handling with custom error types
  - In-memory database with seed data for testing
  - Full CRUD operations with validation

- **REslava.Result.Samples.Console** - Educational console application
  - 13 progressive examples from basic to advanced
  - Comprehensive feature coverage (20+ patterns)
  - LINQ syntax, async operations, validation pipelines
  - Maybe&lt;T&gt; and OneOf&lt;T1,T2,T3,T4&gt; functional patterns
  - Result↔OneOf conversions and integrations

### 🔧 Fixed

**Source Generators**
- Removed hardcoded namespace reference in SmartEndpointsGenerator
  - Fixed `using MinimalApi.Net10.REslavaResult.Models;` that caused build errors
  - Generator now works with any project namespace

**FastMinimalAPI Demo**
- Fixed OneOf5 → OneOf4 conversion in CreateOrder endpoint
  - Consolidated UserInactiveError into ValidationError
  - Updated all endpoint handlers to match new signature
  - Corrected parameter ordering in OrderResponse constructors
  - Fixed Product.StockQuantity property references

**Console Sample**
- Fixed XML documentation warnings in Maybe&lt;T&gt; examples
- Corrected project reference paths after directory restructuring

### 📚 Improved

**Documentation**
- Added comprehensive README for Console sample
- Updated FastMinimalAPI README to reflect actual implementation
- Clarified error handling patterns and use cases

### 📊 Stats

- **Sample Apps**: 2 new comprehensive demos
- **Example Files**: 13 progressive console examples
- **API Endpoints**: 15 web API endpoints demonstrating patterns
- **Lines of Code**: ~3,500 lines of sample code
- **Build Status**: ✅ All samples build and run successfully

---

## [1.12.0] - 2026-02-07

### ✨ Added
- **OneOf4ToIResult Generator** - 4-way discriminated unions with intelligent HTTP mapping
- **Enhanced SmartEndpoints** - Better OneOf4 support and automatic endpoint generation
- **Complete Generator Integration** - All generators working together seamlessly
- **Automated Testing Infrastructure** - 1,928 tests passing with bash script validation

### 🚀 Improved  
- **Fast APIs Development** - 10x faster development, 90% less code
- **Self-Explanatory Development** - Zero boilerplate, business logic only
- **Zero Manual Configuration** - Automatic route, error, and status mapping
- **Comprehensive Documentation** - Updated README, release notes, quick-start guides

### 🔧 Fixed
- Project reference paths after directory restructuring
- Package metadata paths for README and icon files
- Test project compilation issues
- Source generator test infrastructure

### 📊 Stats
- 1,928 tests passing (up from 1,902)
- 17 source generator tests passing
- 9 integration tests passing
- 95%+ code coverage maintained

## [1.11.0] - 2025-02-05

### 🎯 Added
- **SmartEndpoints Generator** - Complete Zero Boilerplate API generation
  - Automatic route generation with parameter awareness
  - Intelligent HTTP method detection (GET/POST/PUT/DELETE)
  - Route prefix support via `[AutoGenerateEndpoints(RoutePrefix = "...")]`
  - Full integration with existing OneOf2/OneOf3 extensions
  - Comprehensive error handling with automatic HTTP status mapping

### 🔄 Changed
- **Route Inference** - Enhanced to include `{id}` parameters when needed
- **OneOf Integration** - SmartEndpoints now uses existing OneOf extensions
- **Generated Code** - Cleaned up debug code and production-ready

### 🧪 Fixed
- **SmartEndpoints Warnings** - Resolved null reference warnings
- **Route Generation** - Fixed parameter-aware route inference
- **Test Coverage** - Added comprehensive MSTest suite for SmartEndpoints

### ⚠️ Breaking Changes
- **SmartEndpoints Route Inference** - Generated routes now properly include `{id}` parameters
  - Routes may change from `/api/User` to `/api/User/{id}` for methods with ID parameters
  - This improves route correctness and is a recommended update

### 📚 Documentation
- Updated README with comprehensive SmartEndpoints examples
- Added breaking changes documentation
- Enhanced troubleshooting section

---

## [1.10.3] - 2025-02-05

### 🎯 Added
- **OneOf2ToIResult Generator** - Two-type error handling
- **OneOf3ToIResult Generator** - Three-type error handling
- **Intelligent HTTP Mapping** - Automatic error type detection
- **Comprehensive Error Coverage** - All common error scenarios

### 🔄 Changed
- **Error Detection** - Smart error type identification
- **HTTP Status Mapping** - Automatic response code generation

---

## [1.10.2] - 2025-02-05

### 🎯 Added
- **ResultToIResult Generator** - Basic Result<T> conversion
- **HTTP Status Mapping** - Intelligent error response generation
- **ProblemDetails Support** - Structured error responses

### 🔄 Changed
- **Core Library** - Enhanced error handling capabilities

---

## [1.10.1] - 2025-02-05

### 🎯 Added
- **Initial Release** - Core Result types
- **Error Handling** - Basic error type definitions
- **HTTP Integration** - ASP.NET Core IResult support

### 🔄 Changed
- **Initial Setup** - Project structure and packaging

---

## [1.10.0] - 2025-02-05

### 🎯 Added
- **Framework Foundation** - Railway-oriented programming patterns
- **Result Types** - Success, Error, ValidationError types
- **Basic HTTP Integration** - IResult conversion methods

### 🔄 Changed
- **Initial Setup** - Project structure and packaging
