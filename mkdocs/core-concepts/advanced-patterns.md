---
title: Advanced Patterns
---

**Take your functional programming skills to the next level with these powerful patterns:**

### 🎲 Maybe<T> - Safe Null Handling
**Eliminate null reference exceptions permanently:**
```csharp
// ❌ Traditional null checking
string email = user?.Email?.ToLower();
if (string.IsNullOrEmpty(email))
{
    email = "no-reply@example.com";
}

// ✅ Maybe<T> functional approach
Maybe<User> maybeUser = GetUserFromCache(id);
string email = maybeUser
    .Select(u => u.Email)
    .Filter(e => !string.IsNullOrWhiteSpace(e))
    .Map(e => e.ToLower())
    .ValueOrDefault("no-reply@example.com");

// Chaining operations safely
var result = maybeUser
    .Filter(u => u.IsActive)
    .Select(u => u.Profile)
    .Select(p => p.Settings)
    .Select(s => s.Theme)
    .ValueOrDefault("default-theme");
```

### 🔀 OneOf - Discriminated Unions
**Express multiple possible outcomes with type safety:**
```csharp
// Internal OneOf implementation
OneOf<ValidationError, NotFoundError, User> result = ValidateAndCreateUser(request);

// Pattern matching with exhaustive checking
return result.Match(
    case1: validationError => BadRequest(new { errors = validationError.Errors }),
    case2: notFoundError => NotFound(new { message = notFoundError.Message }),
    case3: user => CreatedAtAction(nameof(GetUser), new { id = user.Id }, user)
);

// 🆕 v1.12.0: OneOf4 for complex scenarios
OneOf<ValidationError, NotFoundError, ConflictError, User> complexResult = 
    ValidateCreateUserWithConflictCheck(request);

return complexResult.Match(
    case1: validationError => BadRequest(new { errors = validationError.Errors }),
    case2: notFoundError => NotFound(new { message = notFoundError.Message }),
    case3: conflictError => Conflict(new { error = conflictError.Message }),
    case4: user => CreatedAtAction(nameof(GetUser), new { id = user.Id }, user)
);

// Conversion to Result for chaining
var userResult = result.ToResult(); // Convert OneOf to Result

// REslava.Result internal OneOf support (v1.12.0)
using REslava.Result.AdvancedPatterns.OneOf;
OneOf<ValidationError, User> internalResult = ValidateUser(request);
return internalResult.ToIResult(); // Auto-converts to HTTP response!
```

### ✅ Validation Framework
**Declarative validation with rich error context:**
```csharp
// Built-in validation rules
var validator = Validator.Create<User>()
    .Rule(u => u.Email, email => email.Contains("@"), "Invalid email format")
    .Rule(u => u.Name, name => !string.IsNullOrWhiteSpace(name), "Name is required")
    .Rule(u => u.Age, age => age >= 18, "Must be 18 or older")
    .Rule(u => u.Email, async email => !await EmailExistsAsync(email), "Email already exists");

// Execute validation
var validationResult = await validator.ValidateAsync(user);

// Chain with Result operations
var result = validationResult
    .Bind(validUser => CreateUserAsync(validUser))
    .WithSuccess("User created successfully");

// Custom validation rules
public class UniqueEmailRule : IValidationRule<User>
{
    public ValidationResult Validate(User user)
    {
        return EmailExistsAsync(user.Email).GetAwaiter().GetResult()
            ? ValidationResult.Fail("Email already exists")
            : ValidationResult.Success();
    }
}
```

### 🔄 Functional Composition
**Build complex operations from simple functions:**
```csharp
// Function composition
Func<CreateUserRequest, Result<User>> createUserPipeline = Compose(
    ValidateRequest,
    MapToUser,
    ValidateUser,
    SaveUser,
    SendWelcomeEmail
);

// Use the composed function
var result = createUserPipeline(request);

// Higher-order functions with Result
var results = users
    .Where(u => u.IsActive)
    .Select(u => ProcessUser(u))
    .Sequence(); // Turns IEnumerable<Result<T>> into Result<IEnumerable<T>>

// Async traverse operations
var results = await userIds
    .Traverse(id => GetUserAsync(id)); // Async version of Sequence

// Error aggregation
var aggregatedResult = results
    .Map(users => users.ToList())
    .Tap(users => LogInfo($"Processed {users.Count} users"));
```

### 🏷️ Domain Error Hierarchy (v1.20.0)
**Built-in domain errors with HTTP semantics — no more reinventing error types:**
```csharp
// 5 built-in domain error types
Result<User>.Fail(new NotFoundError("User", userId));          // 404
Result<User>.Fail(new ValidationError("Email", "Required"));   // 422
Result<User>.Fail(new ConflictError("User", "email", email));  // 409
Result<User>.Fail(new UnauthorizedError());                    // 401
Result<User>.Fail(new ForbiddenError("delete", "admin-panel"));// 403

// Each carries HttpStatusCode tag — auto-mapped by ToIResult()
var result = await _service.GetUserAsync(id);
return result.ToIResult(); // NotFoundError → 404, ValidationError → 422

// Pattern matching on error types
result.Match(
    onSuccess: user => Ok(user),
    onFailure: errors => errors.First() switch
    {
        NotFoundError nf => NotFound(nf.Message),
        ValidationError ve => UnprocessableEntity(new { field = ve.FieldName, error = ve.Message }),
        ConflictError => Conflict(),
        _ => Problem()
    });

// Fluent chaining preserved via CRTP
var error = new NotFoundError("User", 42)
    .WithTag("RequestId", requestId)
    .WithMessage("Custom message");  // Returns NotFoundError, not base type
```

### 🏷️ Rich Error Context
**Add structured metadata for debugging and monitoring:**
```csharp
// Error with tags and metadata
var error = new NotFoundError("User", userId)
    .WithTag("RequestId", requestId)
    .WithTag("Timestamp", DateTime.UtcNow);

// Result with rich context
var result = Result<User>.Fail(error);

// Extract context for logging
if (result.IsFailure)
{
    var firstError = result.Errors.First();
    var entity = firstError.Tags.GetValueOrDefault("EntityName");
    var requestId = firstError.Tags.GetValueOrDefault("RequestId");

    logger.LogWarning("{Entity} not found for request {RequestId}", entity, requestId);
}
```

### 🚀 Performance Patterns
**Optimize for high-performance scenarios:**
```csharp
// Value objects for reduced allocations
public readonly record struct UserEmail(string Value)
{
    public static Result<UserEmail> Create(string email) =>
        string.IsNullOrWhiteSpace(email)
            ? Result<UserEmail>.Fail("Email required")
            : email.Contains("@")
                ? Result<UserEmail>.Ok(new UserEmail(email))
                : Result<UserEmail>.Fail("Invalid email format");
}

// Array pooling for high-throughput scenarios
using System.Buffers;

var result = Result<string[]>.Ok(ArrayPool<string>.Shared.Rent(1000))
    .Ensure(arr => arr.Length >= 1000, "Array too small")
    .Tap(arr => ArrayPool<string>.Shared.Return(arr));

// Memory-efficient validation
public ref struct ValidationSpan(ReadOnlySpan<char> input)
{
    public bool IsValid => !input.IsEmpty && input.Contains('@');
    public Result<ReadOnlySpan<char>> AsResult() =>
        IsValid ? Result<ReadOnlySpan<char>>.Ok(input) 
                : Result<ReadOnlySpan<char>>.Fail("Invalid email");
}
```

---