# OneOf\<T1, T2, T3> API Reference

## Overview

`OneOf<T1, T2, T3>` represents a value that can be one of three different types. It's ideal for complex state machines, API responses with multiple error types, or scenarios requiring more than two distinct states.

## Constructor Methods

### FromT1(T1 value)
Creates a `OneOf<T1, T2, T3>` containing a T1 value.

```csharp
OneOf<Success, ClientError, ServerError> result = OneOf<Success, ClientError, ServerError>.FromT1(new Success("Operation completed"));
```

**Parameters:**
- `value` (T1): The T1 value to wrap

**Returns:** `OneOf<T1, T2, T3>` containing the T1 value

---

### FromT2(T2 value)
Creates a `OneOf<T1, T2, T3>` containing a T2 value.

```csharp
OneOf<Success, ClientError, ServerError> result = OneOf<Success, ClientError, ServerError>.FromT2(new ClientError("Invalid input", 400));
```

**Parameters:**
- `value` (T2): The T2 value to wrap

**Returns:** `OneOf<T1, T2, T3>` containing the T2 value

---

### FromT3(T3 value)
Creates a `OneOf<T1, T2, T3>` containing a T3 value.

```csharp
OneOf<Success, ClientError, ServerError> result = OneOf<Success, ClientError, ServerError>.FromT3(new ServerError("Database timeout", 500));
```

**Parameters:**
- `value` (T3): The T3 value to wrap

**Returns:** `OneOf<T1, T2, T3>` containing the T3 value

---

## Pattern Matching Methods

### Match<TResult>(Func<T1, TResult> case1, Func<T2, TResult> case2, Func<T3, TResult> case3)
Three-way pattern matching that executes the appropriate function based on the contained type.

```csharp
string message = apiResult.Match(
    case1: success => $"Success: {success.Message}",
    case2: clientError => $"Client Error: {clientError.Message}",
    case3: serverError => $"Server Error: {serverError.Message}"
);
```

**Parameters:**
- `case1` (Func<T1, TResult>): Function to execute when containing T1
- `case2` (Func<T2, TResult>): Function to execute when containing T2
- `case3` (Func<T3, TResult>): Function to execute when containing T3

**Returns:** Result of executing the appropriate function

**Throws:** `ArgumentNullException` if any parameter is null

---

### Switch(Action<T1> case1, Action<T2> case2, Action<T3> case3)
Executes side effects based on the contained type.

```csharp
apiResult.Switch(
    case1: success => LogSuccess(success),
    case2: clientError => LogClientError(clientError),
    case3: serverError => LogServerError(serverError)
);
```

**Parameters:**
- `case1` (Action<T1>): Action to execute when containing T1
- `case2` (Action<T2>): Action to execute when containing T2
- `case3` (Action<T3>): Action to execute when containing T3

**Throws:** `ArgumentNullException` if any parameter is null

---

## Transformation Methods

### MapT2<TResult>(Func<T2, TResult> mapper)
Transforms the T2 value while preserving T1 and T3 values unchanged.

```csharp
OneOf<Success, ClientError, ServerError> result = CallApi();
OneOf<Success, FriendlyError, ServerError> mapped = result.MapT2(error => error.ToFriendlyError());
```

**Parameters:**
- `mapper` (Func<T2, TResult>): Function to transform T2 to TResult

**Returns:** `OneOf<T1, TResult, T3>` with transformed T2 or original T1/T3

**Throws:** `ArgumentNullException` if mapper is null

---

### MapT3<TResult>(Func<T3, TResult> mapper)
Transforms the T3 value while preserving T1 and T2 values unchanged.

```csharp
OneOf<Success, ClientError, ServerError> result = CallApi();
OneOf<Success, ClientError, RetryableError> mapped = result.MapT3(error => error.ToRetryableError());
```

**Parameters:**
- `mapper` (Func<T3, TResult>): Function to transform T3 to TResult

**Returns:** `OneOf<T1, T2, TResult>` with transformed T3 or original T1/T2

**Throws:** `ArgumentNullException` if mapper is null

---

### BindT2<TResult>(Func<T2, OneOf<T1, T2, TResult>> binder)
Chains OneOf operations, flattening nested OneOf types for T2.

```csharp
OneOf<Success, ClientError, ServerError> result = CallApi();
OneOf<Success, ValidatedClientError, ServerError> validated = result.BindT2(error => ValidateClientError(error));
```

**Parameters:**
- `binder` (Func<T2, OneOf<T1, T2, TResult>>): Function returning OneOf<T1, T2, TResult>

**Returns:** `OneOf<T1, TResult, T3>` from binder or original T1/T3

**Throws:** `ArgumentNullException` if binder is null

---

### BindT3<TResult>(Func<T3, OneOf<T1, T2, TResult>> binder)
Chains OneOf operations, flattening nested OneOf types for T3.

```csharp
OneOf<Success, ClientError, ServerError> result = CallApi();
OneOf<Success, ClientError, ProcessedServerError> processed = result.BindT3(error => ProcessServerError(error));
```

**Parameters:**
- `binder` (Func<T3, OneOf<T1, T2, TResult>>): Function returning OneOf<T1, T2, TResult>

**Returns:** `OneOf<T1, T2, TResult>` from binder or original T1/T2

**Throws:** `ArgumentNullException` if binder is null

---

## Properties

### IsT1
Gets whether the OneOf contains a T1 value.

```csharp
if (result.IsT1)
{
    var success = result.AsT1;
    Console.WriteLine($"Success: {success.Message}");
}
```

**Returns:** `true` if containing T1, `false` otherwise

---

### IsT2
Gets whether the OneOf contains a T2 value.

```csharp
if (result.IsT2)
{
    var clientError = result.AsT2;
    Console.WriteLine($"Client Error: {clientError.Message}");
}
```

**Returns:** `true` if containing T2, `false` otherwise

---

### IsT3
Gets whether the OneOf contains a T3 value.

```csharp
if (result.IsT3)
{
    var serverError = result.AsT3;
    Console.WriteLine($"Server Error: {serverError.Message}");
}
```

**Returns:** `true` if containing T3, `false` otherwise

---

### AsT1
Gets the value as T1. Throws if containing T2 or T3.

```csharp
if (result.IsT1)
{
    Success success = result.AsT1; // Safe to access
}
```

**Returns:** The contained T1 value

**Throws:** `InvalidOperationException` if containing T2 or T3

---

### AsT2
Gets the value as T2. Throws if containing T1 or T3.

```csharp
if (result.IsT2)
{
    ClientError clientError = result.AsT2; // Safe to access
}
```

**Returns:** The contained T2 value

**Throws:** `InvalidOperationException` if containing T1 or T3

---

### AsT3
Gets the value as T3. Throws if containing T1 or T2.

```csharp
if (result.IsT3)
{
    ServerError serverError = result.AsT3; // Safe to access
}
```

**Returns:** The contained T3 value

**Throws:** `InvalidOperationException` if containing T1 or T2

---

## Conversion Methods

### ToTwoWay<T3>(Func<T3, T1> mapT3ToT1, Func<T3, T2> mapT3ToT2)
Converts to a 2-way OneOf by mapping T3 to either T1 or T2.

```csharp
OneOf<Success, ClientError, ServerError> threeWay = CallApi();
OneOf<Success, ClientError> twoWay = threeWay.ToTwoWay(
    mapT3ToT1: serverError => new Success($"Retry: {serverError.Message}"),
    mapT3ToT2: serverError => new ClientError(serverError.Message, 503)
);
```

**Parameters:**
- `mapT3ToT1` (Func<T3, T1>): Function to map T3 to T1
- `mapT3ToT2` (Func<T3, T2>): Function to map T3 to T2

**Returns:** `OneOf<T1, T2>` with mapped T3 or original T1/T2

**Throws:** `ArgumentNullException` if any parameter is null

---

### ToTwoWayWithFallback<T1, T2>(T1 fallbackT1, T2 fallbackT2)
Converts to a 2-way OneOf by providing fallback values for T3.

```csharp
OneOf<Success, ClientError, ServerError> threeWay = CallApi();
OneOf<Success, ClientError> twoWay = threeWay.ToTwoWayWithFallback(
    fallbackT1: new Success("Default success"),
    fallbackT2: new ClientError("Default error", 500)
);
```

**Parameters:**
- `fallbackT1` (T1): Fallback T1 value when original is T3
- `fallbackT2` (T2): Fallback T2 value when original is T3

**Returns:** `OneOf<T1, T2>` with fallback values or original T1/T2

---

## Extension Methods

### ToThreeWay<T3>(this OneOf<T1, T2> oneOf, T3 fallbackT3)
Converts a 2-way OneOf to 3-way by adding a T3 fallback.

```csharp
OneOf<Error, User> twoWay = GetUser(id);
OneOf<Error, User, SystemError> threeWay = twoWay.ToThreeWay(new SystemError("System unavailable"));
```

**Parameters:**
- `oneOf` (OneOf<T1, T2>): The 2-way OneOf to convert
- `fallbackT3` (T3): The T3 value to use when converting

**Returns:** `OneOf<T1, T2, T3>` with original value or T3 fallback

---

## Equality and Comparison

### Equals(object? obj)
Compares OneOf values for equality.

```csharp
OneOf<Success, ClientError, ServerError> a = OneOf<Success, ClientError, ServerError>.FromT1(new Success("OK"));
OneOf<Success, ClientError, ServerError> b = OneOf<Success, ClientError, ServerError>.FromT1(new Success("OK"));
bool equal = a.Equals(b); // true
```

**Parameters:**
- `obj` (object?): Object to compare with

**Returns:** `true` if equal, `false` otherwise

---

### Equals(OneOf<T1, T2, T3> other)
Compares with another OneOf of the same types.

```csharp
OneOf<Success, ClientError, ServerError> a = GetApiResult();
OneOf<Success, ClientError, ServerError> b = GetApiResult();
bool equal = a.Equals(b); // true if same result
```

**Parameters:**
- `other` (OneOf<T1, T2, T3>): Another OneOf to compare with

**Returns:** `true` if equal, `false` otherwise

---

### GetHashCode()
Gets hash code for the OneOf value.

```csharp
int hash = result.GetHashCode();
```

**Returns:** Hash code based on contained value and type

---

### ToString()
String representation of the OneOf.

```csharp
OneOf<Success, ClientError, ServerError> result = OneOf<Success, ClientError, ServerError>.FromT1(new Success("OK"));
Console.WriteLine(result.ToString()); // "OneOf<Success, ClientError, ServerError>(T1: Success { Message = OK })"
```

**Returns:** String representation showing type and value

---

## LINQ Integration

OneOf<T1, T2, T3> supports LINQ query syntax through SelectMany:

```csharp
OneOf<Error, int, string> a = OneOf<Error, int, string>.FromT2(2);
OneOf<Error, int, string> b = OneOf<Error, int, string>.FromT2(3);

OneOf<Error, int, string> result = from x in a
                                  from y in b
                                  select x + y; // T2: 5
```

---

## Examples

### API Response with Three States
```csharp
public OneOf<Success, ClientError, ServerError> CallApi(string endpoint)
{
    try
    {
        var response = _httpClient.GetAsync(endpoint).Result;
        return response.StatusCode switch
        {
            HttpStatusCode.OK => OneOf<Success, ClientError, ServerError>.FromT1(new Success("Request successful")),
            HttpStatusCode.BadRequest => OneOf<Success, ClientError, ServerError>.FromT2(new ClientError("Bad request", 400)),
            HttpStatusCode.NotFound => OneOf<Success, ClientError, ServerError>.FromT2(new ClientError("Not found", 404)),
            _ => OneOf<Success, ClientError, ServerError>.FromT3(new ServerError("Server error", (int)response.StatusCode))
        };
    }
    catch (HttpRequestException ex)
    {
        return OneOf<Success, ClientError, ServerError>.FromT3(new ServerError(ex.Message, 500));
    }
}

public string ProcessApiCall(string endpoint)
{
    return CallApi(endpoint).Match(
        case1: success => $"✅ {success.Message}",
        case2: clientError => $"⚠️ Client error: {clientError.Message}",
        case3: serverError => $"❌ Server error: {serverError.Message}"
    );
}
```

### Configuration Parsing with Multiple Types
```csharp
public OneOf<ParseError, int, bool> ParseConfigValue(string key, string value)
{
    return key.ToLowerInvariant() switch
    {
        "timeout" when int.TryParse(value, out var timeout) && timeout > 0 => 
            OneOf<ParseError, int, bool>.FromT2(timeout),
        "enabled" when bool.TryParse(value, out var enabled) => 
            OneOf<ParseError, int, bool>.FromT3(enabled),
        "timeout" => OneOf<ParseError, int, bool>.FromT1(new ParseError("Invalid timeout value")),
        "enabled" => OneOf<ParseError, int, bool>.FromT1(new ParseError("Invalid enabled value")),
        _ => OneOf<ParseError, int, bool>.FromT1(new ParseError($"Unknown config key: {key}"))
    };
}

public void ApplyConfiguration()
{
    var timeout = ParseConfigValue("timeout", _config["timeout"]);
    var enabled = ParseConfigValue("enabled", _config["enabled"]);

    timeout.Match(
        case1: error => Console.WriteLine($"Timeout error: {error.Message}"),
        case2: value => _settings.Timeout = value,
        case3: _ => Console.WriteLine("Timeout config is boolean, expected integer")
    );

    enabled.Match(
        case1: error => Console.WriteLine($"Enabled error: {error.Message}"),
        case2: _ => Console.WriteLine("Enabled config is integer, expected boolean"),
        case3: value => _settings.Enabled = value
    );
}
```

### Database Operation States
```csharp
public OneOf<Created, Updated, NotFound> SaveUser(User user)
{
    var existing = _database.FindUser(user.Id);
    
    if (existing == null)
    {
        _database.Insert(user);
        return OneOf<Created, Updated, NotFound>.FromT1(new Created(user.Id));
    }
    else if (existing.Version < user.Version)
    {
        _database.Update(user);
        return OneOf<Created, Updated, NotFound>.FromT2(new Updated(user.Id));
    }
    else
    {
        return OneOf<Created, Updated, NotFound>.FromT3(new NotFound(user.Id));
    }
}

public string HandleUserSave(User user)
{
    return SaveUser(user).Match(
        case1: created => $"✅ User created with ID: {created.Id}",
        case2: updated => $"📝 User updated with ID: {updated.Id}",
        case3: notFound => $"❌ User not found or version conflict: {notFound.Id}"
    );
}
```

### State Machine Implementation
```csharp
public OneOf<Idle, Processing, Completed> ProcessWorkflow(WorkflowState state)
{
    return state.Status switch
    {
        WorkflowStatus.Pending => OneOf<Idle, Processing, Completed>.FromT2(new Processing(state.Id)),
        WorkflowStatus.Processing => state.IsComplete 
            ? OneOf<Idle, Processing, Completed>.FromT3(new Completed(state.Id))
            : OneOf<Idle, Processing, Completed>.FromT2(new Processing(state.Id)),
        WorkflowStatus.Completed => OneOf<Idle, Processing, Completed>.FromT3(new Completed(state.Id)),
        _ => OneOf<Idle, Processing, Completed>.FromT1(new Idle())
    };
}

public void UpdateWorkflow(WorkflowState state)
{
    ProcessWorkflow(state).Switch(
        case1: idle => Console.WriteLine("Workflow is idle"),
        case2: processing => Console.WriteLine($"Workflow {processing.Id} is processing"),
        case3: completed => Console.WriteLine($"Workflow {completed.Id} is completed")
    );
}
```

### Conversion Between 2-way and 3-way
```csharp
public OneOf<Error, User> GetUserSimple(int id)
{
    var result = GetUserDetailed(id);
    
    // Convert 3-way to 2-way by mapping server errors to regular errors
    return result.ToTwoWay(
        mapT3ToT1: serverError => new Error($"Server error: {serverError.Message}"),
        mapT3ToT2: serverError => new User("Fallback")
    );
}

public OneOf<Success, ClientError, ServerError> GetUserDetailed(int id)
{
    var simpleResult = GetUserSimple(id);
    
    // Convert 2-way to 3-way by adding server error fallback
    return simpleResult.ToThreeWay(new ServerError("Service unavailable", 503));
}
```

---

## Performance Notes

- **Memory Allocation**: Single allocation for the entire OneOf instance
- **Type Safety**: Compile-time guarantees about contained types
- **Pattern Matching**: Optimized for common T1/T2/T3 patterns
- **Discriminator**: Small overhead for tracking which type is contained

---

## Best Practices

### ✅ Do Use
- **Descriptive type names** that clearly indicate the relationship
- **Specific error types** for different error categories
- **Pattern matching** for handling all cases explicitly
- **Conversions** between 2-way and 3-way when needed

### ❌ Avoid
- **Using OneOf<T1, T2, T3>** when OneOf<T1, T2> would suffice
- **Magic strings or objects** as generic types
- **Nested OneOf** without proper flattening
- **Forgetting to handle all cases** in pattern matching

---

## When to Use vs OneOf<T1, T2>

| Situation | Use OneOf<T1, T2> | Use OneOf<T1, T2, T3> |
|-----------|-------------------|------------------------|
| Binary success/error | ✅ | ❌ |
| Multiple error types | ❌ | ✅ |
| Three distinct states | ❌ | ✅ |
| Simple API responses | ✅ | ❌ |
| Complex state machines | ❌ | ✅ |

---

## See Also

- [Advanced Patterns Guide](Advanced-Patterns.md) - Usage patterns and best practices
- [OneOf API](Oneof.md) - 2-way discriminated unions
- [Integration Extensions](Integration-Extensions.md) - Result integration methods
