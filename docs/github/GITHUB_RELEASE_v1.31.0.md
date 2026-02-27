# REslava.Result v1.31.0

Three new core library methods: ILogger integration, railway recovery, and predicate-based filtering.

---

## ✨ What's New

### `Result.WithLogger` / `LogOnFailure` — Structured ILogger Integration

Tap-style extension methods that log result outcomes without breaking the pipeline. No boilerplate `if (result.IsSuccess)` blocks.

```csharp
// Log every result — Debug on success, Warning/Error on failure
Result<User> user = await GetUserAsync(id)
    .WithLogger(logger, "GetUser");

// Log only failures
Result<Order> order = await CreateOrderAsync(dto)
    .LogOnFailure(logger, "CreateOrder");
```

**Log levels:**
- ✅ Success → `Debug` — `"{operationName} succeeded"`
- ⚠️ Domain failure (`Error`, `NotFoundError`, etc.) → `Warning` — `"{operationName} failed"`
- 💥 Exception failure (`ExceptionError`) → `Error` — `"{operationName} failed with exception"`

Structured properties on every log entry: `result.outcome`, `result.error.type`, `result.error.message`.

Both `Result` and `Result<T>` supported. `Task<Result<T>>` extensions with `CancellationToken`.

---

### `Result.Recover` / `RecoverAsync` — Railway Recovery

The counterpart to `Bind`: where `Bind` operates on the success path, `Recover` operates on the failure path. Transform any failure into a new `Result` — which can itself be success or failure.

```csharp
// Fallback to cache if primary DB call fails
Result<User> user = await userRepository.GetAsync(id)
    .Recover(errors => userCache.Get(id));

// Async fallback — secondary data source
Result<User> user = await userRepository.GetAsync(id)
    .RecoverAsync(errors => fallbackApi.GetUserAsync(id));

// Context-aware: only recover if not forbidden
Result<Document> doc = await fetchDocument(id)
    .Recover(errors => errors.Any(e => e is ForbiddenError)
        ? Result<Document>.Fail(errors)
        : localCache.Get(id));

// Non-generic Result — command recovery
Result result = await DeleteUser(id)
    .Recover(errors => ArchiveUser(id));
```

The recovery func receives the full `ImmutableList<IError>` — enabling context-aware branching.

**Distinct from `Catch<TException>`:** `Catch` targets only `ExceptionError` wrapping a specific exception type and always returns a failure (different error). `Recover` handles any failure and can return success.

Both `Result` and `Result<T>` supported. `Task<Result<T>>` extensions with `CancellationToken`.

---

### `Result.Filter` / `FilterAsync` — Predicate Filtering

Convert a success into a failure when a predicate on the value is not met. The error factory receives the value — enabling contextual error messages that `Ensure` cannot express.

```csharp
// Value-dependent error message — the Filter use case
Result<User> activeUser = userResult
    .Filter(u => u.IsActive, u => new Error($"User '{u.Name}' is not active."));

// Static error — convenience overload
Result<Order> pending = orderResult
    .Filter(o => o.Status == OrderStatus.Pending, new ConflictError("Order", "status"));

// String message — convenience overload
Result<Product> inStock = productResult
    .Filter(p => p.Stock > 0, "Product is out of stock.");

// Async predicate (e.g. external validation service)
Result<Order> valid = await orderResult
    .FilterAsync(async o => await validator.IsValidAsync(o),
                 o => new ValidationError("Order", o.Id.ToString(), "failed validation"));
```

**Distinct from `Ensure`:** `Ensure` takes a static `Error` or `string` fixed at the call site. `Filter` takes `Func<T, IError>` — the error is computed from the value, enabling messages like `"User 'John' is not active"`.

Three sync overloads: factory / static `IError` / string. Predicate exceptions wrapped in `ExceptionError`. `Task<Result<T>>` extensions with `CancellationToken`.

---

## 📦 NuGet Packages

| Package | Link |
|---------|------|
| REslava.Result | [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.31.0) |
| REslava.Result.SourceGenerators | [View on NuGet](https://www.nuget.org/packages/REslava.Result.SourceGenerators/1.31.0) |
| REslava.Result.Analyzers | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.31.0) |
| REslava.Result.FluentValidation | [View on NuGet](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.31.0) |

---

## 📊 Stats

- **114 features** across 11 categories
- 3,591 tests passing across net8.0, net9.0, net10.0 (1,122×3) + generator (131) + analyzer (68) + FluentValidation bridge (26)
