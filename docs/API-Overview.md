# REslava.Result API Overview

This document provides a comprehensive overview of all available API methods in the REslava.Result library, organized by functional categories.

## Quick Reference

| Category | Document | Description |
|-----------|----------|-------------|
| **Reasons** | [API-Reasons.md](API-Reasons.md) | All reason types (Success, Error, ExceptionError, ConversionError) and their methods |
| **Result Methods** | [API-Result-Methods.md](API-Result-Methods.md) | Core instance methods on Result classes (Map, Tap, Match, Bind, Conversions) |
| **Result Factories** | [API-Result-Factories.md](API-Result-Factories.md) | Static factory methods for creating Results (Ok, Fail, Combine, Conditional, Try) |
| **Result Extensions** | [API-Result-Extensions.md](API-Result-Extensions.md) | Extension methods for enhanced functionality (LINQ, Validation, Async operations) |

## Key Concepts

### Result Types
- **Result**: Non-generic result for operations without return values
- **Result<TValue>**: Generic result for operations with return values
- **IReason**: Base interface for all reasons (successes and errors)
- **ISuccess**: Interface for success reasons
- **IError**: Interface for error reasons

### Reason Types
- **Success**: Represents a successful operation
- **Error**: Represents a general error
- **ExceptionError**: Wraps exceptions with automatic tag generation
- **ConversionError**: Used for implicit conversion failures

### Core Patterns

#### 1. Creation
```csharp
// Success cases
var success = Result.Ok();
var successWithValue = Result<User>.Ok(user);
var successWithMessage = Result.Ok("Operation completed");

// Failure cases
var failure = Result.Fail("Something went wrong");
var failureWithError = Result<User>.Fail(new ValidationError("Email", "Invalid format"));

// Tags are immediately available with safe access:
var field = failureWithError.Errors[0].GetTagString("Field"); // "Email"
var errorType = failureWithError.Errors[0].GetTagString("ErrorType"); // "Validation"
```

#### 2. Transformation
```csharp
// Map - transform value
var name = userResult.Map(u => u.Name);

// Bind - chain operations
var order = userResult.Bind(u => CreateOrder(u));

// LINQ syntax
var result = from user in GetUser(id)
            from order in CreateOrder(user)
            select order.Total;
```

#### 3. Pattern Matching
```csharp
var message = result.Match(
    onSuccess: () => "Success!",
    onFailure: errors => $"Failed: {errors[0].Message}"
);
```

#### 4. Side Effects
```csharp
var logged = result.Tap(user => logger.LogInformation($"User: {user.Name}"));
var validated = result.Ensure(user => user.Email != null, "Email required");
```

#### 5. Combination
```csharp
var combined = Result.Combine(
    ValidateEmail(email),
    ValidateAge(age),
    ValidateName(name)
);
```

## Method Categories

### Factory Methods
- **Ok()**: Create successful results
- **Fail()**: Create failed results  
- **Try()**: Wrap operations in exception handling
- **Combine/Merge**: Combine multiple results
- **OkIf/FailIf**: Conditional creation

### Instance Methods
- **Map()**: Transform values
- **Bind()**: Chain operations
- **Tap()**: Execute side effects
- **Match()**: Pattern matching
- **Ensure()**: Validation
- **WithReason/WithError/WithSuccess()**: Add reasons

### Extension Methods
- **LINQ**: Query syntax support
- **Validation**: Ensure conditions
- **Async**: Task-based operations
- **Tap**: Side effects on failures

## Thread Safety

All Result types are **immutable** and therefore thread-safe. All operations return new instances rather than modifying existing ones.

## Error Handling Philosophy

- **No exceptions for expected failures**: Use Result pattern instead of throwing
- **Rich error context**: Errors can contain tags and metadata
- **Composable**: Results can be chained and combined
- **Explicit**: Success/failure must be explicitly handled

## Performance Considerations

- **Immutable design**: Creates new instances, but enables safe sharing
- **Lazy evaluation**: Many factory methods support lazy predicates
- **Efficient combining**: Optimized for multiple result operations
- **Minimal allocations**: Uses immutable collections efficiently

## Async Support

Most methods have async counterparts:
- `Map()` → `MapAsync()`
- `Bind()` → `BindAsync()`
- `Tap()` → `TapAsync()`
- `Match()` → `MatchAsync()`
- Extensions for `Task<Result<T>`> types

## LINQ Integration

Full LINQ query syntax support enables expressive functional composition:

```csharp
var result = from user in GetUserAsync(id)
            from profile in GetProfileAsync(user.Id)
            where profile.IsActive
            select new UserProfile(user, profile);
```

This is equivalent to:

```csharp
var result = await GetUserAsync(id)
    .BindAsync(user => GetProfileAsync(user.Id))
    .WhereAsync(profile => profile.IsActive)
    .SelectAsync((user, profile) => new UserProfile(user, profile));
```
