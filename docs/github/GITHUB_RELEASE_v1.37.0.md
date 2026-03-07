# REslava.Result v1.37.0

Three new fluent methods that fill genuine gaps in the pipeline API: `Switch` for void dispatch, `MapError` for error-path transformation, and `Or`/`OrElse` for concise fallback handling.

---

## 🔀 `Switch` / `SwitchAsync` — Void Dispatch

Route success and failure to two actions without returning a value. The primary new value is the `Task` extensions — filling a gap that `void Match` extensions do not cover:

```csharp
// Sync
result.Switch(
    onSuccess: user   => _cache.Set(user.Id, user),
    onFailure: errors => _metrics.Increment("fetch.error"));

// Task extension — end-of-chain after async pipeline
await GetUserAsync(id)
    .Switch(
        onSuccess: user   => _cache.Set(user.Id, user),
        onFailure: errors => _metrics.Increment("fetch.error"));

// Async actions
await CreateOrderAsync(dto)
    .SwitchAsync(
        onSuccess: async order  => await PublishAsync(order),
        onFailure: async errors => await AlertAsync(errors[0]));
```

Available on `Result`, `Result<T>`, `Task<Result>`, `Task<Result<T>>`.

---

## 🗺️ `MapError` / `MapErrorAsync` — Error Path Transform

Transform errors in the failure path. The symmetric counterpart to `Map`: success passes through unchanged, result state never changes.

```csharp
// Enrich errors with service context
Result<User> result = await userRepository.GetAsync(id)
    .MapError(errors => errors
        .Select(e => (IError)new NotFoundError($"[UserService] {e.Message}"))
        .ToImmutableList());

// Async mapper
Result<Order> result = await orderTask
    .MapErrorAsync(async errors =>
    {
        await _audit.LogAsync(errors);
        return errors.Select(e => (IError)new Error($"[OrderSvc] {e.Message}")).ToImmutableList();
    });
```

Available on `Result`, `Result<T>`, `Task<Result>`, `Task<Result<T>>`. Distinct from `Recover`: `MapError` always remains a failure; `Recover` can produce success.

---

## 🔄 `Or` / `OrElse` / `OrElseAsync` — Fallback on Failure

Provide a fallback result when failure occurs. Simpler API than `Recover` for the common case:

```csharp
// Or — eager fallback (pre-built result)
Result<User> result = TryGetUser(id).Or(Result<User>.Ok(GuestUser.Instance));

// OrElse — lazy fallback (factory only called on failure, receives errors)
Result<User> result = TryGetUser(id)
    .OrElse(errors => _cache.Get(id));

// Fallback can itself fail
Result<User> result = TryPrimary(id)
    .OrElse(errors => TrySecondary(id));

// Task extension + async factory
Result<User> result = await TryGetUserAsync(id)
    .OrElseAsync(async errors => await FetchFromCacheAsync(id));
```

Available on `Result`, `Result<T>`, `Task<Result>`, `Task<Result<T>>`.

---

## 📦 NuGet

| Package | Link |
|---------|------|
| REslava.Result | [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.37.0) |
| REslava.Result.AspNetCore | [View on NuGet](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.37.0) |
| REslava.Result.Analyzers | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.37.0) |
| REslava.Result.FluentValidation | [View on NuGet](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.37.0) |
| REslava.Result.Http | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Http/1.37.0) |
| REslava.ResultFlow | [View on NuGet](https://www.nuget.org/packages/REslava.ResultFlow/1.37.0) |

---

## Stats

- 3,960 tests passing across net8.0, net9.0, net10.0 (1,216×3) + generator (131) + ResultFlow (27) + analyzer (68) + FluentValidation bridge (26) + Http (20×3)
- 136 features across 13 categories
