# Integration Extensions API Reference

## Overview

Integration extensions provide seamless interoperability between Result and OneOf patterns, enabling mixed workflows, gradual migration, and flexible architecture designs.

## Result ↔ OneOf Conversions

### ResultOneOfExtensions

#### ToOneOf<TError, T>(this Result<T> result, Func<IReason, TError> errorMapper)
Converts a Result to OneOf using custom error mapping.

```csharp
Result<User> result = GetUserFromDatabase(id);
OneOf<ValidationError, User> oneOf = result.ToOneOf(reason => new ValidationError(reason.Message));
```

**Parameters:**
- `result` (Result<T>): The Result to convert
- `errorMapper` (Func<IReason, TError>): Function to map Result errors to TError type

**Returns:** `OneOf<TError, T>` containing TError on failure or T on success

**Throws:** `ArgumentNullException` if errorMapper is null

---

#### ToResult<TError, T>(this OneOf<TError, T> oneOf, Func<TError, IError> errorMapper)
Converts a OneOf to Result using custom error mapping.

```csharp
OneOf<ApiError, User> oneOf = GetUserFromApi(id);
Result<User> result = oneOf.ToResult(error => new Error(error.Message));
```

**Parameters:**
- `oneOf` (OneOf<TError, T>): The OneOf to convert
- `errorMapper` (Func<TError, IError>): Function to map TError to IError

**Returns:** `Result<T>` containing T on success or IError on failure

**Throws:** `ArgumentNullException` if errorMapper is null

---

## Pipeline Extensions

### OneOfResultIntegrationExtensions

#### SelectToResult<T1, T2, TResult>(this OneOf<T1, T2> oneOf, Func<T2, TResult> selector, Func<T1, IError>? errorMapper = null)
Transforms the success value of a OneOf using a selector function, returning a Result.

```csharp
OneOf<string, User> oneOf = GetUser(id);
Result<UserDto> result = oneOf.SelectToResult(
    user => new UserDto(user.Name),
    error => new Error($"User error: {error}")
);
```

**Parameters:**
- `oneOf` (OneOf<T1, T2>): The OneOf to transform
- `selector` (Func<T2, TResult>): Function to transform the success value
- `errorMapper` (Func<T1, IError>?, optional): Function to map T1 to IError

**Returns:** `Result<TResult>` containing transformed success or mapped error

**Throws:** `ArgumentNullException` if selector is null

---

#### SelectToResult<T1, T2, TResult>(this OneOf<T1, T2> oneOf, Func<T2, TResult> selector) where T1 : IError
Transforms the success value when T1 is an IError type, using the error directly.

```csharp
OneOf<ApiError, User> oneOf = GetUserFromApi(id);
Result<UserDto> result = oneOf.SelectToResult(user => new UserDto(user.Name));
```

**Parameters:**
- `oneOf` (OneOf<T1, T2>): The OneOf to transform (T1 must be IError)
- `selector` (Func<T2, TResult>): Function to transform the success value

**Returns:** `Result<TResult>` containing transformed success or original IError

**Throws:** `ArgumentNullException` if selector is null

---

#### BindToResult<T1, T2, TResult>(this OneOf<T1, T2> oneOf, Func<T2, Result<TResult>> binder, Func<T1, IError>? errorMapper = null)
Binds the success value of a OneOf to a Result-producing function.

```csharp
OneOf<string, User> oneOf = GetUser(id);
Result<User> result = oneOf.BindToResult(
    user => ValidateUser(user),
    error => new Error($"Bind error: {error}")
);
```

**Parameters:**
- `oneOf` (OneOf<T1, T2>): The OneOf to bind
- `binder` (Func<T2, Result<TResult>>): Function returning Result<TResult>
- `errorMapper` (Func<T1, IError>?, optional): Function to map T1 to IError

**Returns:** `Result<TResult>` from binder or mapped error

**Throws:** `ArgumentNullException` if binder is null

---

#### BindToResult<T1, T2, TResult>(this OneOf<T1, T2> oneOf, Func<T2, Result<TResult>> binder) where T1 : IError
Binds the success value when T1 is an IError type, using the error directly.

```csharp
OneOf<ApiError, User> oneOf = GetUserFromApi(id);
Result<User> result = oneOf.BindToResult(user => ValidateUser(user));
```

**Parameters:**
- `oneOf` (OneOf<T1, T2>): The OneOf to bind (T1 must be IError)
- `binder` (Func<T2, Result<TResult>>): Function returning Result<TResult>

**Returns:** `Result<TResult>` from binder or original IError

**Throws:** `ArgumentNullException` if binder is null

---

#### Filter<T1, T2>(this OneOf<T1, T2> oneOf, Func<T2, bool> predicate)
Filters a OneOf based on a predicate, returning the original OneOf if the predicate passes.

```csharp
OneOf<Error, User> oneOf = GetUser(id);
OneOf<Error, User> active = oneOf.Filter(user => user.IsActive);
```

**Parameters:**
- `oneOf` (OneOf<T1, T2>): The OneOf to filter
- `predicate` (Func<T2, bool>): Function to test the success value

**Returns:** `OneOf<T1, T2>` if predicate passes, throws InvalidOperationException otherwise

**Throws:** `ArgumentNullException` if predicate is null
**Throws:** `InvalidOperationException` if predicate returns false

---

#### ToOneOfCustom<T1, T2>(this Result<T2> result, Func<IReason, T1> errorMapper)
Converts a Result to OneOf using custom error mapping (alternative to basic ToOneOf).

```csharp
Result<User> result = GetUserFromDatabase(id);
OneOf<ValidationError, User> oneOf = result.ToOneOfCustom(reason => new ValidationError(reason.Message));
```

**Parameters:**
- `result` (Result<T2>): The Result to convert
- `errorMapper` (Func<IReason, T1>): Function to map Result errors to T1

**Returns:** `OneOf<T1, T2>` containing T1 on failure or T2 on success

**Throws:** `ArgumentNullException` if errorMapper is null

---

## Examples

### API Layer Integration
```csharp
public class UserService
{
    // API layer returns OneOf
    public OneOf<ApiError, User> GetUserFromApi(int id)
    {
        try
        {
            var response = _httpClient.GetAsync($"/api/users/{id}").Result;
            return response.IsSuccessStatusCode
                ? JsonSerializer.Deserialize<User>(response.Content.ReadAsStringAsync().Result)
                : new ApiError("User not found", 404);
        }
        catch (Exception ex)
        {
            return new ApiError(ex.Message, 500);
        }
    }

    // Business layer uses Result
    public Result<UserDto> GetUserDto(int id)
    {
        return GetUserFromApi(id)
            .SelectToResult(user => new UserDto(user.Name, user.Email));
    }

    // Database layer expects OneOf
    public OneOf<ValidationError, UserDto> GetUserForDatabase(int id)
    {
        return GetUserDto(id)
            .ToOneOfCustom(reason => new ValidationError(reason.Message));
    }
}
```

### Mixed Pipeline Scenarios
```csharp
public class OrderProcessor
{
    public OneOf<BusinessError, ProcessedOrder> ProcessOrder(OrderRequest request)
    {
        // Step 1: Validate request (returns OneOf)
        OneOf<ValidationError, ValidatedRequest> validated = ValidateRequest(request);
        
        // Step 2: Convert to Result for business logic
        Result<ValidatedRequest> businessResult = validated.SelectToResult(req => req);
        
        // Step 3: Process with Result workflow
        Result<Order> orderResult = businessResult.BindToResult(req => CreateOrder(req));
        
        // Step 4: Convert back to OneOf for response
        return orderResult.ToOneOfCustom(reason => new BusinessError(reason.Message))
            .BindToResult(order => ProcessOrder(order));
    }

    private OneOf<ValidationError, ValidatedRequest> ValidateRequest(OrderRequest request)
    {
        return request.IsValid()
            ? OneOf<ValidationError, ValidatedRequest>.FromT2(new ValidatedRequest(request))
            : OneOf<ValidationError, ValidatedRequest>.FromT1(new ValidationError("Invalid request"));
    }

    private Result<Order> CreateOrder(ValidatedRequest request)
    {
        try
        {
            var order = new Order(request);
            _database.Save(order);
            return Result<Order>.Ok(order);
        }
        catch (Exception ex)
        {
            return Result<Order>.Fail($"Failed to create order: {ex.Message}");
        }
    }

    private OneOf<BusinessError, ProcessedOrder> ProcessOrder(Order order)
    {
        try
        {
            var processed = _orderProcessor.Process(order);
            return OneOf<BusinessError, ProcessedOrder>.FromT2(processed);
        }
        catch (Exception ex)
        {
            return OneOf<BusinessError, ProcessedOrder>.FromT1(new BusinessError(ex.Message));
        }
    }
}
```

### Error Type Transformation
```csharp
public class ErrorTransformer
{
    // Transform between different error types
    public OneOf<DomainError, User> TransformApiUser(OneOf<ApiError, User> apiUser)
    {
        return apiUser.SelectToResult(
            user => user,
            apiError => new Error($"API Error: {apiError.Message}")
        ).ToOneOf(error => new DomainError(error.Message));
    }

    // Filter and transform in pipeline
    public OneOf<ValidationError, ActiveUser> GetActiveUser(int id)
    {
        return GetUser(id)
            .Filter(user => user.IsActive)
            .BindToResult(user => ValidateActiveUser(user))
            .ToOneOf(reason => new ValidationError(reason.Message));
    }

    private OneOf<DatabaseError, User> GetUser(int id)
    {
        var user = _database.FindUser(id);
        return user != null
            ? OneOf<DatabaseError, User>.FromT2(user)
            : OneOf<DatabaseError, User>.FromT1(new DatabaseError("User not found"));
    }

    private Result<ActiveUser> ValidateActiveUser(User user)
    {
        return user.IsValid()
            ? Result<ActiveUser>.Ok(new ActiveUser(user))
            : Result<ActiveUser>.Fail("User is not valid for activation");
    }
}
```

### Migration Scenarios
```csharp
public class MigrationExample
{
    // Legacy code using Result
    public Result<User> GetUserLegacy(int id)
    {
        try
        {
            var user = _database.FindUser(id);
            return user != null
                ? Result<User>.Ok(user)
                : Result<User>.Fail("User not found");
        }
        catch (Exception ex)
        {
            return Result<User>.Fail(ex.Message);
        }
    }

    // New code using OneOf
    public OneOf<NotFoundError, User> GetUserNew(int id)
    {
        try
        {
            var user = _database.FindUser(id);
            return user != null
                ? OneOf<NotFoundError, User>.FromT2(user)
                : OneOf<NotFoundError, User>.FromT1(new NotFoundError(id));
        }
        catch (Exception ex)
        {
            return OneOf<NotFoundError, User>.FromT1(new NotFoundError(id, ex.Message));
        }
    }

    // Bridge between old and new
    public OneOf<NotFoundError, User> GetUserWithMigration(int id, bool useNewImplementation)
    {
        if (useNewImplementation)
        {
            return GetUserNew(id);
        }
        else
        {
            // Convert legacy Result to new OneOf
            return GetUserLegacy(id).ToOneOf(reason => new NotFoundError(id, reason.Message));
        }
    }

    // Gradual migration - can work with both patterns
    public string ProcessUser(int id)
    {
        var legacyResult = GetUserLegacy(id);
        var newResult = GetUserNew(id);

        // Can process both types
        string legacyMessage = legacyResult.Match(
            error => $"Legacy error: {error.Message}",
            user => $"Legacy user: {user.Name}"
        );

        string newMessage = newResult.Match(
            error => $"New error: {error.Message}",
            user => $"New user: {user.Name}"
        );

        return $"{legacyMessage} | {newMessage}";
    }
}
```

### Configuration and Settings
```csharp
public class ConfigurationManager
{
    // Parse configuration with multiple result types
    public OneOf<ConfigError, int> GetTimeoutSetting()
    {
        var timeoutString = _configuration["timeout"];
        
        return int.TryParse(timeoutString, out var timeout)
            ? timeout > 0
                ? OneOf<ConfigError, int>.FromT2(timeout)
                : OneOf<ConfigError, int>.FromT1(new ConfigError("Timeout must be positive"))
            : OneOf<ConfigError, int>.FromT1(new ConfigError("Invalid timeout format"));
    }

    // Use in Result workflow
    public Result<ValidatedConfig> ValidateConfiguration()
    {
        return GetTimeoutSetting()
            .SelectToResult(timeout => new ValidatedConfig(timeout))
            .BindToResult(config => ValidateConfig(config));
    }

    // Convert to OneOf for API response
    public OneOf<ValidationError, ValidatedConfig> GetConfigForApi()
    {
        return ValidateConfiguration()
            .ToOneOfCustom(reason => new ValidationError(reason.Message));
    }

    private Result<ValidatedConfig> ValidateConfig(ValidatedConfig config)
    {
        return config.Timeout <= 300
            ? Result<ValidatedConfig>.Ok(config)
            : Result<ValidatedConfig>.Fail("Timeout cannot exceed 300 seconds");
    }
}
```

---

## Performance Considerations

### Method Selection Guidelines

| Scenario | Recommended Method | Reason |
|----------|-------------------|---------|
| Simple error mapping | `ToResult`/`ToOneOf` with IError constraint | Direct usage, no mapping overhead |
| Complex error transformation | `SelectToResult`/`BindToResult` with custom mapper | Full control over error mapping |
| Filtering operations | `Filter` | Built-in predicate support |
| Migration scenarios | `ToOneOfCustom` | Alternative mapping approach |

### Memory Allocation

- **Direct IError methods**: Zero additional allocation for error mapping
- **Custom mapper methods**: One allocation for mapped error object
- **Filter method**: No allocation on success, throws on failure

### Performance Tips

1. **Use IError overloads** when T1 implements IError for best performance
2. **Avoid complex mappers** in hot paths - consider pre-mapped error types
3. **Use Filter** for simple predicates instead of Bind with conditional logic
4. **Cache mapper functions** if they're complex and reused frequently

---

## Best Practices

### ✅ Do Use

- **IError overloads** for common error/success patterns
- **Custom mappers** for error type transformations
- **Filter** for conditional processing
- **Mixed pipelines** when different layers use different patterns

### ❌ Avoid

- **Complex mappers** that throw exceptions
- **Nested conversions** without clear purpose
- **Magic strings** in error mapping
- **Forgetting null checks** in custom mappers

---

## Migration Strategies

### From Result to OneOf

```csharp
// Before: Result pattern
Result<User> result = GetUser(id);
if (result.IsSuccess)
{
    return ProcessUser(result.Value);
}
else
{
    return HandleError(result.Errors.First());
}

// After: OneOf pattern
OneOf<Error, User> oneOf = result.ToOneOf(error => new Error(error.Message));
return oneOf.Match(
    error => HandleError(error),
    user => ProcessUser(user)
);
```

### From OneOf to Result

```csharp
// Before: OneOf pattern
OneOf<Error, User> oneOf = GetUser(id);
return oneOf.Match(
    error => Result<User>.Fail(error.Message),
    user => Result<User>.Ok(user)
);

// After: Result pattern
Result<User> result = oneOf.ToResult(error => new Error(error.Message));
return result;
```

### Gradual Migration

```csharp
// Layer 1: API (OneOf)
public OneOf<ApiError, User> GetApiUser(int id) { /* ... */ }

// Layer 2: Business (Result) - can work with both
public Result<UserDto> GetUserDto(int id)
{
    return GetApiUser(id).SelectToResult(user => user.ToDto());
}

// Layer 3: Database (OneOf) - can work with both
public OneOf<DbError, UserDto> SaveUser(UserDto dto)
{
    return ValidateUser(dto)
        .ToOneOfCustom(reason => new DbError(reason.Message))
        .BindToResult(user => _database.Save(user))
        .ToOneOf(error => new DbError(error.Message));
}
```

---

## See Also

- [Advanced Patterns Guide](Advanced-Patterns.md) - Usage patterns and integration scenarios
- [Maybe API](Maybe.md) - Optional value patterns
- [OneOf API](Oneof.md) - Discriminated union patterns
- [OneOf3 API](Oneof3.md) - Three-way discriminated unions
