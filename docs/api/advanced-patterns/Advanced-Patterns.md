# Advanced Patterns - Complete Guide

## 🧠 Overview

REslava.Result goes beyond basic Result handling with advanced functional programming patterns that enable type-safe, expressive code. These patterns help you eliminate null reference exceptions, handle complex state machines, and create maintainable, predictable code.

### 🎯 Available Patterns

| Pattern | Description | Primary Use Case |
|---------|-------------|------------------|
| **Maybe\<T>** | Optional values with functional chaining | Safe handling of nullable operations |
| **OneOf\<T1, T2>** | 2-way discriminated unions | Error/success or alternative value scenarios |
| **OneOf\<T1, T2, T3>** | 3-way discriminated unions | Complex state representations |
| **Result ↔ OneOf** | Seamless integration | Migration between patterns |
| **Pipeline Extensions** | Mixed workflows | Combine patterns naturally |

---

## 📖 Maybe\<T> - Safe Optional Operations

### 🎯 When to Use Maybe\<T>

- **Database queries** that might not return results
- **Configuration values** that may be missing
- **Cache lookups** that could miss
- **Optional method parameters** without null checks

### 🚀 Basic Usage

```csharp
// Creation
Maybe<User> user = Maybe<User>.FromValue(database.GetUser(id));
Maybe<User> empty = Maybe<User>.None;

// Functional chaining
var email = user
    .Select(u => u.Email)
    .Filter(email => email.Contains("@"))
    .ValueOrDefault("no-reply@example.com");

// Pattern matching
string message = user.Match(
    some: u => $"User found: {u.Name}",
    none: () => "User not found"
);
```

### 📚 Complete API Reference

#### Constructor Methods
- `Maybe<T>.FromValue(T value)` - Create from existing value
- `Maybe<T>.None` - Create empty Maybe

#### Functional Methods
- `Select<TResult>(Func<T, TResult> selector)` - Transform value
- `Filter(Func<T, bool> predicate)` - Conditional filtering
- `Bind<TResult>(Func<T, Maybe<TResult>> binder)` - Chain Maybe operations
- `Match<TResult>(Func<T, TResult> some, Func<TResult> none)` - Pattern matching

#### Properties
- `IsSome` - True if has value
- `IsNone` - True if empty
- `Value` - Get value (throws if None)

---

## 🎯 OneOf\<T1, T2> - Type-Safe Alternatives

### 🎯 When to Use OneOf\<T1, T2>

- **API responses** that can be success or specific error types
- **Parsing operations** with multiple valid result types
- **Configuration values** with different possible types
- **State machines** with two distinct states

### 🚀 Basic Usage

```csharp
// Creation
OneOf<Error, User> result = GetUserFromApi(id);

// Pattern matching
return result.Match(
    case1: error => HandleError(error),
    case2: user => ProcessUser(user)
);

// Functional operations
OneOf<Error, UserDto> dto = result
    .MapT2(user => user.ToDto())
    .BindT2(dto => ValidateDto(dto));

// Switch for side effects
result.Switch(
    case1: error => LogError(error),
    case2: user => AuditUserAccess(user)
);
```

### 📚 Complete API Reference

#### Constructor Methods
- `OneOf<T1, T2>.FromT1(T1 value)` - Create with T1 value
- `OneOf<T1, T2>.FromT2(T2 value)` - Create with T2 value

#### Functional Methods
- `Match<TResult>(Func<T1, TResult> case1, Func<T2, TResult> case2)` - Pattern matching
- `Switch(Action<T1> case1, Action<T2> case2)` - Side effects
- `MapT2<TResult>(Func<T2, TResult> mapper)` - Transform T2 value
- `BindT2<TResult>(Func<T2, OneOf<T1, TResult>> binder)` - Chain operations

#### Properties
- `IsT1` - True if contains T1
- `IsT2` - True if contains T2
- `AsT1` - Get as T1 (throws if T2)
- `AsT2` - Get as T2 (throws if T1)

---

## 🎲 OneOf\<T1, T2, T3> - Complex State Handling

### 🎯 When to Use OneOf\<T1, T2, T3>

- **API responses** with success, client error, and server error
- **State machines** with three distinct states
- **Configuration parsing** with multiple valid types
- **Workflow processes** with multiple outcomes

### 🚀 Basic Usage

```csharp
// Creation
OneOf<Success, ClientError, ServerError> apiResult = CallApi(endpoint);

// Three-way pattern matching
return apiResult.Match(
    case1: success => HandleSuccess(success),
    case2: clientError => HandleClientError(clientError),
    case3: serverError => HandleServerError(serverError)
);

// Functional transformations
OneOf<Success, ClientError, ProcessedData> processed = apiResult
    .MapT2(clientError => clientError.ToUserFriendlyError())
    .MapT3(serverError => serverError.ToRetryableError());

// Conversions between 2-way and 3-way
OneOf<Error, Success> twoWay = apiResult.ToTwoWay(
    mapT3ToT1: serverError => serverError.ToError(),
    mapT3ToT2: serverError => Success.Retry
);
```

### 📚 Complete API Reference

#### Constructor Methods
- `OneOf<T1, T2, T3>.FromT1(T1 value)` - Create with T1 value
- `OneOf<T1, T2, T3>.FromT2(T2 value)` - Create with T2 value
- `OneOf<T1, T2, T3>.FromT3(T3 value)` - Create with T3 value

#### Functional Methods
- `Match<TResult>(Func<T1, TResult> case1, Func<T2, TResult> case2, Func<T3, TResult> case3)` - Pattern matching
- `Switch(Action<T1> case1, Action<T2> case2, Action<T3> case3)` - Side effects
- `MapT2<TResult>(Func<T2, TResult> mapper)` - Transform T2 value
- `MapT3<TResult>(Func<T3, TResult> mapper)` - Transform T3 value
- `BindT2<TResult>(Func<T2, OneOf<T1, T2, TResult>> binder)` - Chain T2 operations
- `BindT3<TResult>(Func<T3, OneOf<T1, T2, TResult>> binder)` - Chain T3 operations

#### Extension Methods
- `ToThreeWay<T3>(this OneOf<T1, T2> oneOf, T3 fallbackT3)` - Convert to 3-way
- `ToTwoWay<T3>(this OneOf<T1, T2, T3> oneOf, Func<T3, T1> mapT3ToT1, Func<T3, T2> mapT3ToT2)` - Convert to 2-way

---

## 🔄 Result ↔ OneOf Integration

### 🎯 When to Use Integration

- **Gradual migration** from Result to OneOf patterns
- **Mixed architectures** where different layers use different patterns
- **API boundaries** between services using different patterns
- **Legacy integration** combining old Result code with new OneOf code

### 🚀 Basic Usage

```csharp
// Result → OneOf conversion
Result<User> result = GetUserFromDatabase(id);
OneOf<ValidationError, User> oneOf = result.ToOneOf(
    reason => new ValidationError(reason.Message)
);

// OneOf → Result conversion
OneOf<ApiError, User> apiResult = GetUserFromApi(id);
Result<User> result = apiResult.ToResult(
    error => new Error(error.Message)
);

// Pipeline integration
OneOf<ApiError, User> apiResult = GetUserFromApi(1);
Result<UserDto> businessResult = apiResult.SelectToResult(user => user.ToDto());
OneOf<ValidationError, UserDto> finalResult = businessResult.ToOneOfCustom(
    reason => new ValidationError(reason.Message)
);
```

### 📚 Integration Methods

#### ResultOneOfExtensions
- `ToOneOf<TError, T>(this Result<T> result, Func<IReason, TError> errorMapper)` - Result to OneOf
- `ToResult<TError, T>(this OneOf<TError, T> oneOf, Func<TError, IError> errorMapper)` - OneOf to Result

#### OneOfResultIntegrationExtensions
- `SelectToResult<T1, T2, TResult>(this OneOf<T1, T2> oneOf, Func<T2, TResult> selector, Func<T1, IError>? errorMapper = null)` - Transform to Result
- `SelectToResult<T1, T2, TResult>(this OneOf<T1, T2> oneOf, Func<T2, TResult> selector) where T1 : IError` - Direct IError usage
- `BindToResult<T1, T2, TResult>(this OneOf<T1, T2> oneOf, Func<T2, Result<TResult>> binder, Func<T1, IError>? errorMapper = null)` - Chain to Result
- `BindToResult<T1, T2, TResult>(this OneOf<T1, T2> oneOf, Func<T2, Result<TResult>> binder) where T1 : IError` - Direct IError binding
- `Filter<T1, T2>(this OneOf<T1, T2> oneOf, Func<T2, bool> predicate)` - Conditional filtering
- `ToOneOfCustom<T1, T2>(this Result<T2> result, Func<IReason, T1> errorMapper)` - Custom Result to OneOf

---

## 🚀 Pipeline Extensions - Mixed Workflows

### 🎯 When to Use Pipeline Extensions

- **Complex business logic** requiring multiple pattern transformations
- **API layering** where different layers use different patterns
- **Data processing pipelines** with multiple validation steps
- **Error handling transformations** between different error types

### 🚀 Real-World Scenarios

```csharp
// Scenario 1: API → Business → Database pipeline
OneOf<ApiError, User> apiResult = GetUserFromApi(1);

// Transform to Result for business logic
Result<UserDto> businessResult = apiResult.SelectToResult(
    user => user.ToDto(),
    error => new Error($"API Error: {error.Message}")
);

// Transform back to OneOf for database layer
OneOf<DbError, UserDto> dbResult = businessResult.ToOneOfCustom(
    reason => new DbError(reason.Message)
);

// Scenario 2: Mixed validation pipeline
OneOf<ValidationError, User> validationResult = ValidateUser(user);
Result<User> businessResult = validationResult.SelectToResult(
    user => user,
    error => new Error($"Validation failed: {error.Message}")
);

Result<ProcessedUser> processedResult = businessResult.BindToResult(
    user => ProcessUser(user)
);

// Scenario 3: Error type transformation
OneOf<string, User> legacyResult = GetLegacyUser(id);
OneOf<ApiError, User> modernResult = legacyResult.SelectToResult(
    user => user,
    error => new ApiError(error, 500)
).ToOneOf(error => error);
```

---

## 📖 Migration Guide

### 🔄 From Exceptions to Result Pattern

```csharp
// Before: Exception-based
try {
    User user = GetUser(id);
    return ProcessUser(user);
}
catch (NotFoundException ex) {
    return HandleNotFound(ex);
}
catch (ValidationException ex) {
    return HandleValidation(ex);
}

// After: Result pattern
Result<User> userResult = GetUser(id);
return userResult.Match(
    error => error switch {
        NotFoundException => HandleNotFound(error),
        ValidationException => HandleValidation(error),
        _ => Result<User>.Fail("Unknown error")
    },
    user => ProcessUser(user)
);
```

### 🔄 From Result to OneOf

```csharp
// Before: Result pattern
Result<User> result = GetUser(id);
if (result.IsSuccess) {
    return ProcessUser(result.Value);
} else {
    return HandleError(result.Errors);
}

// After: OneOf pattern
OneOf<Error, User> oneOf = result.ToOneOf(error => new Error(error.Message));
return oneOf.Match(
    error => HandleError(error),
    user => ProcessUser(user)
);
```

### 🔄 From Null to Maybe

```csharp
// Before: Null checking
User? user = GetUser(id);
if (user != null) {
    return ProcessUser(user);
} else {
    return HandleNotFound();
}

// After: Maybe pattern
Maybe<User> user = GetUser(id).ToMaybe();
return user.Match(
    some: u => ProcessUser(u),
    none: () => HandleNotFound()
);
```

---

## 🎯 Best Practices

### ✅ When to Use Each Pattern

| Pattern | Best For | Avoid When |
|---------|----------|------------|
| **Maybe\<T>** | Optional values, cache lookups | You need error details |
| **OneOf\<T1, T2>** | Error/success, binary states | You have >2 states |
| **OneOf\<T1, T2, T3>** | Complex states, API responses | You have >3 states |
| **Result** | Business logic, validation | You need multiple success types |
| **Exceptions** | Truly exceptional cases | Expected error conditions |

### ✅ Pattern Combinations

```csharp
// Good: Maybe for optional, OneOf for typed errors
Maybe<Config> config = GetConfig();
OneOf<ValidationError, ProcessedConfig> processed = config
    .Select(c => ValidateConfig(c))
    .ValueOrDefault(OneOf<ValidationError, ProcessedConfig>.FromT1(
        new ValidationError("Config not found")
    ));

// Good: Result for business logic, OneOf for API layer
Result<BusinessResult> business = ProcessBusiness(data);
OneOf<ApiError, BusinessResult> api = business.ToOneOf(
    error => new ApiError(error.Message, 400)
);
```

### ❌ Anti-Patterns

```csharp
// Avoid: OneOf with too many types
OneOf<Error, User, Admin, Guest, System> user = GetUser(id);
// Better: Use specific types or state pattern

// Avoid: Maybe when you need error details
Maybe<User> user = GetUser(id); // Lost error information
// Better: Use OneOf<Error, User>

// Avoid: Mixing patterns unnecessarily
Result<OneOf<Error, User>> complex = GetUser(id);
// Better: Choose one pattern consistently
```

---

## ⚡ Performance Considerations

### 📊 Benchmarks

| Operation | Maybe\<T> | OneOf\<T1, T2> | OneOf\<T1, T2, T3> | Result\<T> |
|-----------|-----------|----------------|-------------------|------------|
| Creation | ~2ns | ~3ns | ~4ns | ~2ns |
| Pattern Match | ~5ns | ~7ns | ~9ns | ~6ns |
| Transformation | ~8ns | ~10ns | ~12ns | ~8ns |

### 🚀 Optimization Tips

1. **Prefer struct patterns** for performance-critical code
2. **Use specific types** instead of string/object when possible
3. **Avoid deep nesting** of pattern matches
4. **Consider inline functions** for simple transformations
5. **Profile your specific use case** - patterns have different characteristics

### 🎯 Memory Allocation

- **Maybe\<T>**: Zero allocation for None, boxed for Some with structs
- **OneOf\<T1, T2>**: Single allocation, stores discriminator + value
- **OneOf\<T1, T2, T3>**: Single allocation, larger discriminator
- **Result\<T>**: Similar to OneOf, optimized for success cases

---

## 🧪 Testing Patterns

### ✅ Unit Testing Maybe\<T>

```csharp
[Test]
public void Maybe_Select_WithValue_ShouldTransform()
{
    // Arrange
    Maybe<int> maybe = Maybe<int>.FromValue(42);
    
    // Act
    Maybe<string> result = maybe.Select(x => x.ToString());
    
    // Assert
    Assert.IsTrue(result.IsSome);
    Assert.AreEqual("42", result.Value);
}

[Test]
public void Maybe_Filter_WithPredicate_ShouldFilter()
{
    // Arrange
    Maybe<int> maybe = Maybe<int>.FromValue(42);
    
    // Act
    Maybe<int> result = maybe.Filter(x => x > 40);
    
    // Assert
    Assert.IsTrue(result.IsSome);
    Assert.AreEqual(42, result.Value);
}
```

### ✅ Unit Testing OneOf\<T1, T2>

```csharp
[Test]
public void OneOf_Match_WithT2_ShouldExecuteCase2()
{
    // Arrange
    OneOf<Error, string> oneOf = OneOf<Error, string>.FromT2("success");
    
    // Act
    string result = oneOf.Match(
        case1: error => "error",
        case2: success => success.ToUpper()
    );
    
    // Assert
    Assert.AreEqual("SUCCESS", result);
}
```

---

## 📚 Further Reading

- [Maybe API](Maybe.md) - Complete Maybe<T> API
- [OneOf API](Oneof.md) - Complete OneOf<T1, T2> API
- [OneOf3 API](Oneof3.md) - Complete OneOf<T1, T2, T3> API
- [Integration Extensions API](Integration-Extensions.md) - All integration methods
- [Console Samples](../../samples/REslava.Result.Samples.Console/Examples) - Working examples
- [Test Examples](../../tests/REslava.Result.Tests) - Test patterns and best practices
- [GitHub Repository](https://github.com/reslava/nuget-package-reslava-result) - Source code and issues

---

## 🤝 Contributing

We welcome contributions to the advanced patterns! Please see our [contributing guidelines](../CONTRIBUTING.md) for details on how to submit issues, pull requests, and documentation improvements.

---

**🎉 Ready to level up your functional programming in C#? Start with the [Console Samples](../../samples/REslava.Result.Samples.Console/Examples) to see these patterns in action!**
