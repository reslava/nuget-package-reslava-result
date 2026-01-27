# REslava.Result.SourceGenerators

**🪄 Automatic Result<T> to HTTP Response Conversion for ASP.NET Core**

This source generator eliminates 70-90% of boilerplate code in your Web APIs by automatically converting `Result<T>` objects to appropriate HTTP responses.

---

## 🎯 Quick Overview

### The Magic

```csharp
// ❌ BEFORE: Manual conversion everywhere
app.MapGet("/users/{id}", async (int id, IUserService service) =>
{
    var result = await service.GetUserAsync(id);
    
    return result.Match(
        onSuccess: user => Results.Ok(user),
        onFailure: errors => Results.Problem(...)
    );
});

// ✅ AFTER: Return Result<T> directly!
app.MapGet("/users/{id}", async (int id, IUserService service) =>
{
    return await service.GetUserAsync(id); // Auto-converts to HTTP response!
});
```

### What Gets Generated

- **✅ Automatic HTTP status mapping** - Errors → correct status codes
- **✅ RFC 7807 ProblemDetails** - Standardized error responses
- **✅ Error tag preservation** - Rich debugging context
- **✅ HTTP method extensions** - POST with Location, DELETE with 204, etc.
- **✅ Zero runtime overhead** - All code generated at compile-time

---

## 📦 Installation

```bash
dotnet add package REslava.Result.SourceGenerators
```

---

## ⚡ Quick Start

### 1. Enable the Generator

Add this to your `Program.cs`:

```csharp
using REslava.Result.SourceGenerators;

[assembly: GenerateResultExtensions]

var builder = WebApplication.CreateBuilder(args);
// ... rest of your setup
```

### 2. Update Your Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<Result<UserDto>> GetUser(int id)
    {
        return await _userService.GetUserAsync(id)
            .Map(u => new UserDto(u));
    }

    [HttpPost]
    public async Task<Result<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        return await _userService.CreateUserAsync(request)
            .ToPostResult(user => $"/api/users/{user.Id}"); // 201 Created with Location
    }
}
```

That's it! The source generator handles all the HTTP response conversion automatically.

---

## 🛠️ Configuration

### Basic Configuration

```csharp
[assembly: GenerateResultExtensions(
    Namespace = "MyApp.Generated",           // Default: "Generated.ResultExtensions"
    IncludeErrorTags = true,                  // Default: true
    LogErrors = false,                        // Default: false
    GenerateHttpMethodExtensions = true,     // Default: true
    DefaultErrorStatusCode = 400              // Default: 400
)]
```

### Custom Error Mappings

```csharp
[assembly: GenerateResultExtensions(
    CustomErrorMappings = new[] 
    {
        "PaymentRequired:402",
        "RateLimitExceeded:429",
        "ServiceUnavailable:503"
    }
)]
```

---

## 🎯 Generated Extensions

### Core Conversion

| Method | Returns | Use Case |
|--------|---------|----------|
| `ToIResult()` | `IResult` | General HTTP response conversion |
| `ToIResult<T>()` | `IResult` | Generic Result conversion |

### HTTP Method Extensions

| Method | HTTP Status | Use Case |
|--------|-------------|----------|
| `ToPostResult()` | 201 Created | POST operations |
| `ToPostResult(Func<T, string>)` | 201 Created + Location | POST with location header |
| `ToGetResult()` | 200 OK | GET operations |
| `ToPutResult()` | 200 OK | PUT operations |
| `ToPatchResult()` | 200 OK | PATCH operations |
| `ToDeleteResult()` | 204 No Content | DELETE operations |

---

## 🛡️ Error Mapping

### Built-in Mappings

| Error Type Pattern | HTTP Status |
|-------------------|-------------|
| `*NotFound*` | 404 |
| `*DoesNotExist*` | 404 |
| `*ValidationError*` | 422 |
| `*Unauthorized*` | 401 |
| `*Forbidden*` | 403 |
| `*Duplicate*` | 409 |
| Default | 400 |

### Example Error Types

```csharp
// These are automatically mapped:
new NotFoundError("User", id)           // → 404 Not Found
new ValidationError("Email", "Invalid") // → 422 Unprocessable Entity  
new UnauthorizedError("Not logged in")  // → 401 Unauthorized
new ForbiddenError("Access denied")     // → 403 Forbidden
new DuplicateError("Email exists")      // → 409 Conflict
```

---

## 📊 Generated ProblemDetails

### Single Error

```json
{
  "type": "https://httpstatuses.io/404",
  "title": "Not Found",
  "status": 404,
  "detail": "User with id '123' not found",
  "extensions": {
    "errors": [
      {
        "message": "User with id '123' not found",
        "tags": {
          "EntityType": "User",
          "EntityId": "123",
          "StatusCode": 404
        }
      }
    ]
  }
}
```

### Multiple Errors

```json
{
  "type": "https://httpstatuses.io/400",
  "title": "Bad Request",
  "status": 400,
  "detail": "2 errors occurred",
  "extensions": {
    "errors": [
      {
        "message": "Email is required",
        "tags": { "Field": "Email", "ErrorCode": "REQUIRED" }
      },
      {
        "message": "Password must be at least 8 characters",
        "tags": { "Field": "Password", "MinLength": 8 }
      }
    ]
  }
}
```

---

## 🌐 Minimal APIs

Perfect for minimal APIs:

```csharp
app.MapGet("/users/{id}", async (int id, IUserService service) =>
    await service.GetUserAsync(id)
        .Map(u => new UserDto(u)));

app.MapPost("/users", async (CreateUserRequest request, IUserService service) =>
    await service.CreateUserAsync(request)
        .ToPostResult(user => $"/users/{user.Id}"));

app.MapDelete("/users/{id}", async (int id, IUserService service) =>
    await service.DeleteUserAsync(id)
        .ToDeleteResult());
```

---

## 🧪 Testing

### Unit Tests

```csharp
[Test]
public async Task GetUser_ValidId_ReturnsOk()
{
    // Arrange
    var userId = 1;
    var expectedUser = new User { Id = userId, Name = "John" };
    _userService.Setup(x => x.GetUserAsync(userId))
        .ReturnsAsync(Result<User>.Ok(expectedUser));

    // Act
    var result = await _controller.GetUser(userId);

    // Assert
    var okResult = result as Ok<UserDto>;
    Assert.IsNotNull(okResult);
    Assert.AreEqual(expectedUser.Name, okResult.Value.Name);
}

[Test]
public async Task GetUser_NotFound_ReturnsNotFound()
{
    // Arrange
    var userId = 999;
    _userService.Setup(x => x.GetUserAsync(userId))
        .ReturnsAsync(Result<User>.Fail(new NotFoundError("User", userId)));

    // Act
    var result = await _controller.GetUser(userId);

    // Assert
    var notFoundResult = result as NotFoundObjectResult;
    Assert.IsNotNull(notFoundResult);
    Assert.AreEqual(404, notFoundResult.StatusCode);
}
```

---

## 🚀 Performance

- **✅ Zero runtime overhead** - All code generated at compile-time
- **✅ No reflection** - Direct method calls
- **✅ AOT compatible** - Works with NativeAOT and trimming
- **✅ Minimal allocations** - Efficient generated code

---

## 📚 Complete Documentation

For comprehensive guides and examples:

### 🎯 **Choose Your Path**

| I want to... | 📖 Read This | 🎯 What You'll Learn |
|-------------|--------------|---------------------|
| **Set up a Web API** | [🌐 Web API Integration](../docs/guide/web-api-integration.md) | Complete setup, best practices |
| **Learn the basics** | [📖 Getting Started](../docs/guide/getting-started.md) | Core Result pattern fundamentals |
| **Understand the magic** | [⚡ Source Generator](../docs/guide/source-generator.md) | How it works, configuration |
| **Explore advanced features** | [🧠 Advanced Patterns](../docs/guide/advanced-patterns.md) | Maybe, OneOf, functional programming |

### 📚 **Reference Documentation**

- **[📖 Getting Started](../docs/guide/getting-started.md)** - Learn the basics
- **[🌐 Web API Integration](../docs/guide/web-api-integration.md)** - Complete API setup
- **[⚡ Source Generator Deep Dive](../docs/guide/source-generator.md)** - How the magic works
- **[🧠 Advanced Patterns](../docs/guide/advanced-patterns.md)** - Maybe, OneOf, and more
- **[📚 API Reference](../docs/api/)** - Complete technical documentation

---

## 🎯 Best Practices

### ✅ Do's

- **Use descriptive error types** - Make status code mapping intuitive
- **Add error tags** - Provide rich context for debugging
- **Use HTTP method extensions** - Follow REST conventions
- **Test both paths** - Ensure correct HTTP responses

### ❌ Don'ts

- **Don't forget the assembly attribute** - Generator won't run without it
- **Don't ignore error tags** - They provide valuable debugging information
- **Don't mix Result with IActionResult** - Be consistent

---

## 🎉 Ready to Eliminate Boilerplate?

**📖 [Start with the Web API Integration Guide](../docs/guide/web-api-integration.md)**

---

<div align="center">

**⚡ Transform your Web APIs in minutes, not hours!**

</div>
