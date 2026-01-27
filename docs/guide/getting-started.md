# Getting Started with REslava.Result

**🚀 Your journey to predictable, type-safe error handling starts here!**

This guide will take you from zero to productive with REslava.Result in about 10 minutes.

---

## 📦 Installation

### Core Library

```bash
# .NET CLI
dotnet add package REslava.Result

# Package Manager Console
Install-Package REslava.Result
```

### Optional: Source Generator (for ASP.NET Core)

```bash
# .NET CLI
dotnet add package REslava.Result.SourceGenerators

# Package Manager Console
Install-Package REslava.Result.SourceGenerators
```

---

## 🎯 Core Concepts

### What is a Result?

A `Result<T>` represents an operation that can either succeed with a value or fail with errors:

```csharp
// Success case
Result<User> success = Result<User>.Ok(new User { Name = "John" });

// Failure case  
Result<User> failure = Result<User>.Fail("User not found");
```

### Why Use Result Instead of Exceptions?

| ❌ **Exceptions** | ✅ **Result Pattern** |
|------------------|----------------------|
| Hidden in method signatures | Explicit in return type |
| Runtime surprises | Compile-time guarantees |
| Complex try-catch blocks | Simple pattern matching |
| Hard to test | Easy to test both paths |

---

## ⚡ Basic Usage

### 1. Creating Results

```csharp
using REslava.Result;

// Success cases
var success = Result<int>.Ok(42);
var successWithMessage = Result<string>.Ok("Hello", "Operation completed");

// Failure cases
var failure = Result<int>.Fail("Something went wrong");
var failureWithError = Result<User>.Fail(new ValidationError("Email", "Invalid format"));
```

### 2. Pattern Matching

```csharp
var result = await userService.GetUserAsync(id);

// With return value
var response = result.Match(
    onSuccess: user => Ok(new UserDto(user)),
    onFailure: errors => BadRequest(new { errors })
);

// Side effects only
result.Match(
    onSuccess: user => Console.WriteLine($"Welcome {user.Name}"),
    onFailure: errors => Console.WriteLine($"Failed: {string.Join(", ", errors.Select(e => e.Message))}")
);
```

### 3. Checking Results

```csharp
if (result.IsSuccess)
{
    var user = result.Value; // Safe to access
    Console.WriteLine($"User: {user.Name}");
}
else
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Error: {error.Message}");
    }
}
```

---

## 🔗 Fluent Operations

### Map - Transform Values

```csharp
Result<int>.Ok(42)
    .Map(x => x * 2)                    // Result<int> with value 84
    .Map(x => x.ToString())             // Result<string> with value "84"
    .Map(x => $"Value: {x}");           // Result<string> with value "Value: 84"
```

### Bind - Chain Operations

```csharp
Result<string>.Ok("user@example.com")
    .Bind(email => ValidateEmail(email))      // Returns Result<Email>
    .Bind(email => FindUser(email))           // Returns Result<User>
    .Bind(user => AuthenticateUser(user));    // Returns Result<Session>
```

### Ensure - Validation

```csharp
Result<string>.Ok(password)
    .Ensure(p => p.Length >= 8, "Password must be at least 8 characters")
    .Ensure(p => p.Any(char.IsDigit), "Password must contain a digit")
    .Ensure(p => p.Any(char.IsUpper), "Password must contain uppercase");
```

### Tap - Side Effects

```csharp
Result<Order>.Ok(order)
    .Tap(o => _logger.LogInfo($"Processing order {o.Id}"))
    .Tap(o => _metrics.RecordOrder(o))
    .Bind(o => ProcessPayment(o));
```

---

## 🛡️ Error Handling

### Rich Error Context

```csharp
var error = new ValidationError("Email", "Invalid format")
    .WithTag("Field", "Email")
    .WithTag("Value", email)
    .WithTag("ErrorCode", "VAL_001");

Result<User>.Fail(error);

// Safe tag access
if (result.IsFailed)
{
    var field = result.Errors[0].GetTagString("Field");        // "Email"
    var errorCode = result.Errors[0].GetTagString("ErrorCode"); // "VAL_001"
}
```

### Multiple Errors

```csharp
Result<string>.Ok(input)
    .Ensure(
        (s => s.Length >= 8, new ValidationError("Password", "Min 8 characters")),
        (s => s.Any(char.IsDigit), new ValidationError("Password", "Requires digit")),
        (s => s.Any(char.IsUpper), new ValidationError("Password", "Requires uppercase"))
    );
// If validation fails: Result with ALL three errors
```

### Custom Error Types

```csharp
public class NotFoundError : Error
{
    public NotFoundError(string entityType, string id) 
        : base($"{entityType} with id '{id}' not found")
    {
        WithTag("EntityType", entityType);
        WithTag("EntityId", id);
        WithTag("StatusCode", 404);
    }
}

// Usage
return Result<User>.Fail(new NotFoundError("User", userId));
```

---

## 🧪 Testing Results

### Unit Tests Made Simple

```csharp
[Test]
public void ValidateEmail_ValidEmail_ReturnsSuccess()
{
    // Arrange
    var email = "user@example.com";
    
    // Act
    var result = EmailService.Validate(email);
    
    // Assert
    Assert.IsTrue(result.IsSuccess);
    Assert.AreEqual(email, result.Value);
}

[Test]
public void ValidateEmail_InvalidEmail_ReturnsError()
{
    // Arrange
    var email = "invalid";
    
    // Act
    var result = EmailService.Validate(email);
    
    // Assert
    Assert.IsTrue(result.IsFailed);
    Assert.AreEqual("Invalid email format", result.Errors[0].Message);
}
```

---

## 🎯 Real-World Example

### User Registration Service

```csharp
public class UserService
{
    public async Task<Result<User>> RegisterUserAsync(RegisterRequest request)
    {
        return await Result<RegisterRequest>.Ok(request)
            // Validate input
            .Ensure(r => !string.IsNullOrWhiteSpace(r.Email), "Email is required")
            .Ensure(r => IsValidEmail(r.Email), "Invalid email format")
            .Ensure(r => r.Age >= 18, "Must be 18 or older")
            
            // Check business rules
            .EnsureAsync(async r => !await EmailExistsAsync(r.Email), "Email already registered")
            
            // Create user
            .MapAsync(async r => new User 
            { 
                Id = Guid.NewGuid().ToString(),
                Email = r.Email, 
                Name = r.Name,
                Age = r.Age,
                CreatedAt = DateTime.UtcNow
            })
            
            // Save to database
            .BindAsync(async user => await _userRepository.SaveAsync(user))
            
            // Add success tracking
            .WithSuccess("User account created successfully")
            .TapAsync(user => _emailService.SendWelcomeEmailAsync(user.Email));
    }
}
```

### Controller Usage

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        return await _userService.RegisterUserAsync(request)
            .Match(
                onSuccess: user => CreatedAtAction(nameof(GetUser), new { id = user.Id }, new UserDto(user)),
                onFailure: errors => BadRequest(new { errors })
            );
    }
}
```

---

## 🔄 Async Operations

Most methods have async counterparts:

```csharp
// MapAsync - Transform asynchronously
var result = await Result<int>.Ok(userId)
    .MapAsync(async id => await _db.Users.FindAsync(id))
    .MapAsync(async user => await EnrichUserData(user));

// BindAsync - Chain async operations
var result = await Result<string>.Ok(email)
    .BindAsync(async e => await ValidateEmailAsync(e))
    .BindAsync(async e => await CreateUserAsync(e));

// TryAsync - Catch exceptions in async operations
var result = await Result<Data>.TryAsync(async () => 
    await _api.GetDataAsync(id));
```

---

## 🎯 Best Practices

### ✅ Do's

- **Use Result for business logic** - Expected failures should return Results
- **Be specific with errors** - Create custom error types for your domain
- **Use tags for context** - Add debugging information to errors
- **Chain operations** - Use Bind for sequential operations
- **Test both paths** - Write tests for success and failure cases

### ❌ Don'ts

- **Don't use Result for truly exceptional cases** - Use exceptions for unexpected failures
- **Don't ignore errors** - Always handle the failure case
- **Don't create deeply nested chains** - Break complex logic into smaller functions
- **Don't mix patterns** - Be consistent with Result usage in your codebase

---

## 📚 Next Steps

Now that you understand the basics, choose your path:

- **🌐 [Web API Integration](web-api-integration.md)** - Learn how to auto-convert Results to HTTP responses
- **⚡ [Source Generator](source-generator.md)** - Discover the magic behind auto-conversion
- **🧠 [Advanced Patterns](advanced-patterns.md)** - Explore Maybe, OneOf, and functional programming
- **📚 [API Reference](../api/)** - Complete technical documentation

---

## 🎉 You're Ready!

You now have the foundation to write predictable, type-safe code with REslava.Result. 

**Next recommendation:** If you're building Web APIs, check out the [Web API Integration guide](web-api-integration.md) to eliminate 70-90% of your boilerplate code!
