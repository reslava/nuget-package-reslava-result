# OneOf\<T1, T2> API Reference

## Overview

`OneOf<T1, T2>` represents a value that can be one of two different types. It's a type-safe discriminated union that eliminates magic strings and enables expressive error handling.

## Constructor Methods

### FromT1(T1 value)
Creates a `OneOf<T1, T2>` containing a T1 value.

```csharp
OneOf<Error, User> result = OneOf<Error, User>.FromT1(new Error("User not found"));
```

**Parameters:**
- `value` (T1): The T1 value to wrap

**Returns:** `OneOf<T1, T2>` containing the T1 value

---

### FromT2(T2 value)
Creates a `OneOf<T1, T2>` containing a T2 value.

```csharp
OneOf<Error, User> result = OneOf<Error, User>.FromT2(new User("Alice"));
```

**Parameters:**
- `value` (T2): The T2 value to wrap

**Returns:** `OneOf<T1, T2>` containing the T2 value

---

## Pattern Matching Methods

### Match<TResult>(Func<T1, TResult> case1, Func<T2, TResult> case2)
Pattern matching that executes the appropriate function based on the contained type.

```csharp
string message = userResult.Match(
    case1: error => $"Error: {error.Message}",
    case2: user => $"User: {user.Name}"
);
```

**Parameters:**
- `case1` (Func<T1, TResult>): Function to execute when containing T1
- `case2` (Func<T2, TResult>): Function to execute when containing T2

**Returns:** Result of executing the appropriate function

**Throws:** `ArgumentNullException` if any parameter is null

---

### Switch(Action<T1> case1, Action<T2> case2)
Executes side effects based on the contained type.

```csharp
userResult.Switch(
    case1: error => LogError(error),
    case2: user => AuditUser(user)
);
```

**Parameters:**
- `case1` (Action<T1>): Action to execute when containing T1
- `case2` (Action<T2>): Action to execute when containing T2

**Throws:** `ArgumentNullException` if any parameter is null

---

## Transformation Methods

### MapT2<TResult>(Func<T2, TResult> mapper)
Transforms the T2 value while preserving T1 values unchanged.

```csharp
OneOf<Error, User> result = GetUser(id);
OneOf<Error, UserDto> dto = result.MapT2(user => user.ToDto());
```

**Parameters:**
- `mapper` (Func<T2, TResult>): Function to transform T2 to TResult

**Returns:** `OneOf<T1, TResult>` with transformed T2 or original T1

**Throws:** `ArgumentNullException` if mapper is null

---

### BindT2<TResult>(Func<T2, OneOf<T1, TResult>> binder)
Chains OneOf operations, flattening nested OneOf types.

```csharp
OneOf<Error, User> result = GetUser(id);
OneOf<Error, ValidatedUser> validated = result.BindT2(user => ValidateUser(user));
```

**Parameters:**
- `binder` (Func<T2, OneOf<T1, TResult>>): Function returning OneOf<T1, TResult>

**Returns:** `OneOf<T1, TResult>` from binder or original T1

**Throws:** `ArgumentNullException` if binder is null

---

## Properties

### IsT1
Gets whether the OneOf contains a T1 value.

```csharp
if (result.IsT1)
{
    var error = result.AsT1;
    Console.WriteLine($"Error: {error.Message}");
}
```

**Returns:** `true` if containing T1, `false` otherwise

---

### IsT2
Gets whether the OneOf contains a T2 value.

```csharp
if (result.IsT2)
{
    var user = result.AsT2;
    Console.WriteLine($"User: {user.Name}");
}
```

**Returns:** `true` if containing T2, `false` otherwise

---

### AsT1
Gets the value as T1. Throws if containing T2.

```csharp
if (result.IsT1)
{
    Error error = result.AsT1; // Safe to access
}
```

**Returns:** The contained T1 value

**Throws:** `InvalidOperationException` if containing T2

---

### AsT2
Gets the value as T2. Throws if containing T1.

```csharp
if (result.IsT2)
{
    User user = result.AsT2; // Safe to access
}
```

**Returns:** The contained T2 value

**Throws:** `InvalidOperationException` if containing T1

---

## Conversion Methods

### ToOneOf<T3>(T3 fallbackT3)
Converts to a 3-way OneOf by adding a T3 fallback value.

```csharp
OneOf<Error, User> twoWay = GetUser(id);
OneOf<Error, User, SystemError> threeWay = twoWay.ToOneOf(new SystemError("Timeout"));
```

**Parameters:**
- `fallbackT3` (T3): The T3 value to use when converting

**Returns:** `OneOf<T1, T2, T3>` with original value or T3 fallback

---

### ToThreeWay<T3>(T3 fallbackT3)
Extension method alias for ToOneOf<T3>.

```csharp
OneOf<Error, User> twoWay = GetUser(id);
OneOf<Error, User, SystemError> threeWay = twoWay.ToThreeWay(new SystemError("Timeout"));
```

**Parameters:**
- `fallbackT3` (T3): The T3 value to use when converting

**Returns:** `OneOf<T1, T2, T3>` with original value or T3 fallback

---

## Equality and Comparison

### Equals(object? obj)
Compares OneOf values for equality.

```csharp
OneOf<Error, User> a = OneOf<Error, User>.FromT2(new User("Alice"));
OneOf<Error, User> b = OneOf<Error, User>.FromT2(new User("Alice"));
bool equal = a.Equals(b); // true
```

**Parameters:**
- `obj` (object?): Object to compare with

**Returns:** `true` if equal, `false` otherwise

---

### Equals(OneOf<T1, T2> other)
Compares with another OneOf of the same types.

```csharp
OneOf<Error, User> a = GetUser(id);
OneOf<Error, User> b = GetUser(id);
bool equal = a.Equals(b); // true if same user/error
```

**Parameters:**
- `other` (OneOf<T1, T2>): Another OneOf to compare with

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
OneOf<Error, User> user = OneOf<Error, User>.FromT2(new User("Alice"));
Console.WriteLine(user.ToString()); // "OneOf<Error, User>(T2: User { Name = Alice })"

OneOf<Error, User> error = OneOf<Error, User>.FromT1(new Error("Not found"));
Console.WriteLine(error.ToString()); // "OneOf<Error, User>(T1: Error { Message = Not found })"
```

**Returns:** String representation showing type and value

---

## LINQ Integration

OneOf<T1, T2> supports LINQ query syntax through SelectMany:

```csharp
OneOf<Error, int> a = OneOf<Error, int>.FromT2(2);
OneOf<Error, int> b = OneOf<Error, int>.FromT2(3);

OneOf<Error, int> result = from x in a
                          from y in b
                          select x + y; // T2: 5
```

---

## Examples

### API Response Pattern
```csharp
public OneOf<ApiError, User> GetUserFromApi(int id)
{
    try
    {
        var response = _httpClient.GetAsync($"/api/users/{id}").Result;
        if (response.IsSuccessStatusCode)
        {
            var user = JsonSerializer.Deserialize<User>(response.Content.ReadAsStringAsync().Result);
            return OneOf<ApiError, User>.FromT2(user);
        }
        else
        {
            return OneOf<ApiError, User>.FromT1(new ApiError("User not found", 404));
        }
    }
    catch (Exception ex)
    {
        return OneOf<ApiError, User>.FromT1(new ApiError(ex.Message, 500));
    }
}

public string ProcessUser(int id)
{
    return GetUserFromApi(id).Match(
        case1: error => $"Failed: {error.Message}",
        case2: user => $"Success: {user.Name}"
    );
}
```

### Configuration Parsing
```csharp
public OneOf<ParseError, int> ParseTimeout(string configValue)
{
    if (int.TryParse(configValue, out var timeout))
    {
        return timeout > 0 
            ? OneOf<ParseError, int>.FromT2(timeout)
            : OneOf<ParseError, int>.FromT1(new ParseError("Timeout must be positive"));
    }
    else
    {
        return OneOf<ParseError, int>.FromT1(new ParseError("Invalid timeout format"));
    }
}

public int GetTimeout()
{
    var config = _configuration["timeout"];
    return ParseTimeout(config).Match(
        case1: error => 30, // Default timeout
        case2: timeout => timeout
    );
}
```

### Validation Chain
```csharp
public OneOf<ValidationError, ValidatedUser> ValidateUser(User user)
{
    return OneOf<ValidationError, User>.FromT2(user)
        .Filter(u => !string.IsNullOrEmpty(u.Name), new ValidationError("Name is required"))
        .Filter(u => u.Age >= 18, new ValidationError("User must be 18 or older"))
        .MapT2(u => new ValidatedUser(u));
}

private OneOf<ValidationError, User> Filter(this OneOf<ValidationError, User> oneOf, Func<User, bool> predicate, ValidationError error)
{
    return oneOf.Match(
        case1: error => OneOf<ValidationError, User>.FromT1(error),
        case2: user => predicate(user) 
            ? OneOf<ValidationError, User>.FromT2(user)
            : OneOf<ValidationError, User>.FromT1(error)
    );
}
```

### Error Handling Transformation
```csharp
public OneOf<BusinessError, UserDto> GetUserDto(int id)
{
    return GetUserFromApi(id)
        .MapT2(user => user.ToDto())
        .BindT2(dto => ValidateDto(dto))
        .MapT1(apiError => new BusinessError(apiError.Message));
}

private OneOf<BusinessError, UserDto> ValidateDto(UserDto dto)
{
    return !string.IsNullOrEmpty(dto.Email)
        ? OneOf<BusinessError, UserDto>.FromT2(dto)
        : OneOf<BusinessError, UserDto>.FromT1(new BusinessError("Invalid email"));
}
```

---

## Performance Notes

- **Memory Allocation**: Single allocation for the entire OneOf instance
- **Type Safety**: Compile-time guarantees about contained types
- **Pattern Matching**: Optimized for common T1/T2 patterns
- **Inlining**: Most methods are small and can be inlined by JIT

---

## Best Practices

### ✅ Do Use
- **Specific error types** instead of strings for T1
- **Descriptive type names** that indicate the relationship
- **Pattern matching** for handling different cases
- **Functional transformations** with MapT2 and BindT2

### ❌ Avoid
- **Using string/object** as generic types when possible
- **Nested OneOf** without flattening with Bind
- **Exception throwing** in pattern matching functions
- **Magic strings** in error types

---

## See Also

- [Advanced Patterns Guide](Advanced-Patterns.md) - Usage patterns and best practices
- [OneOf3 API](Oneof3.md) - 3-way discriminated unions
- [Integration Extensions](Integration-Extensions.md) - Result integration methods
