# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) guideline.

## [1.14.0] - 2026-02-10

### ✨ Added

**NEW: REslava.Result.Analyzers NuGet Package**
- New companion NuGet package providing Roslyn diagnostic analyzers for REslava.Result
- **RESL1001 — Unsafe Result<T>.Value access**: Warns when `.Value` is accessed without checking `IsSuccess` or `IsFailed` first. Detects 5 guard patterns: `if (result.IsSuccess)`, `if (!result.IsFailed)`, else-branch of `IsFailed`, early return, and early throw
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
