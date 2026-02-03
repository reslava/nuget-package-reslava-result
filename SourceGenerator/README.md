# REslava.Result.SourceGenerators

**🪄 Automatic Result<T> & OneOf<T> to HTTP Response Conversion for ASP.NET Core**

This source generator eliminates 70-90% of boilerplate code in your Web APIs by automatically converting `Result<T>` and `OneOf<T>` objects to appropriate HTTP responses.

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

### 🆕 v1.10.0: OneOf Integration Magic

```csharp
// External OneOf library support
using OneOf;

public OneOf<NotFoundError, User> GetUser(int id) { /* logic */ }

app.MapGet("/users/oneof/{id}", async (int id) =>
{
    return GetUser(id); // Auto-converts OneOf<T1,T2> too!
});

// Three-way OneOf support
public OneOf<ValidationError, NotFoundError, User> CreateUser(CreateUserRequest request) { /* logic */ }

app.MapPost("/users", async (CreateUserRequest request) =>
{
    return CreateUser(request); // Auto-converts OneOf<T1,T2,T3>!
});
```

---

## 🏗️ Generator Architecture

### 📦 Complete Generator Suite

**v1.10.0 includes three powerful generators:**

| Generator | Purpose | Supported Types | HTTP Mapping |
|-----------|---------|----------------|-------------|
| **ResultToIResult** | Convert REslava.Result to HTTP | `Result<T>`, `Result` | Smart error classification |
| **OneOf2ToIResult** | Convert 2-type OneOf to HTTP | `OneOf<T1,T2>` | T1→400, T2→200 |
| **OneOf3ToIResult** | Convert 3-type OneOf to HTTP | `OneOf<T1,T2,T3>` | T1→400, T2→400, T3→200 |

### 🎯 Smart Auto-Detection

**Zero Configuration Required - It Just Works!**

```csharp
// Your code - no changes needed
public Result<User> GetUser(int id) { /* ... */ }
public OneOf<Error, User> GetOneOfUser(int id) { /* ... */ }
public OneOf<ValidationError, NotFoundError, User> CreateUser() { /* ... */ }

// Generated automatically
// ResultToIResultExtensions.cs
// OneOf2Extensions.cs  
// OneOf3Extensions.cs
```

### 🔧 Conflict Prevention

- **Setup Detection**: Automatically detects your OneOf setup
- **Namespace Isolation**: Different extension classes prevent conflicts
- **Zero Compilation Errors**: Perfect coexistence guaranteed

---

## 🚀 HTTP Mapping Intelligence

### Result<T> Smart Classification
```csharp
// Error messages are automatically classified
Result<User>.Fail("User not found")           // → 404 Not Found
Result<User>.Fail("Validation failed")        // → 400 Bad Request  
Result<User>.Fail("Unauthorized access")      // → 401 Unauthorized
Result<User>.Fail("Access forbidden")         // → 403 Forbidden
Result<User>.Fail("User already exists")      // → 409 Conflict
Result<User>.Fail("Server error")             // → 500 Internal Server Error
```

### OneOf<T1,T2> Mapping
```csharp
// First type (typically error) → 400
// Second type (typically success) → 200
OneOf<ValidationError, User> result = ValidateUser(id);
return result.ToIResult(); // 400 or 200
```

### OneOf<T1,T2,T3> Mapping  
```csharp
// First two types (errors) → 400
// Third type (success) → 200
OneOf<ValidationError, NotFoundError, User> result = CreateUser(request);
return result.ToIResult(); // 400, 400, or 200
```

---

## 🧪 Generated Output Examples

### ResultToIResult Extensions
```csharp
// Generated: ResultToIResultExtensions.g.cs
namespace Generated.ResultExtensions
{
    public static class ResultExtensions
    {
        public static IResult ToIResult<T>(this Result<T> result)
        {
            return result.IsSuccess 
                ? Results.Ok(result.Value)
                : CreateProblemResult(result.Errors);
        }
    }
}
```

### OneOf2ToIResult Extensions
```csharp
// Generated: OneOf2ToIResultExtensions.g.cs
namespace Generated.OneOfExtensions
{
    public static class OneOf2Extensions
    {
        public static IResult ToIResult<T1, T2>(this OneOf<T1, T2> oneOf)
        {
            return oneOf.Match(
                t1 => Results.BadRequest(t1?.ToString() ?? "Error"),
                t2 => Results.Ok(t2)
            );
        }
    }
}
```

### OneOf3ToIResult Extensions
```csharp
// Generated: OneOf3ToIResultExtensions.g.cs
namespace Generated.OneOfExtensions
{
    public static class OneOf3Extensions
    {
        public static IResult ToIResult<T1, T2, T3>(this OneOf<T1, T2, T3> oneOf)
        {
            return oneOf.Match(
                t1 => Results.BadRequest(t1?.ToString() ?? "Error"),
                t2 => Results.BadRequest(t2?.ToString() ?? "Error"),
                t3 => Results.Ok(t3)
            );
        }
    }
}
```

---

## 🔧 Advanced Configuration

### Custom Error Classification
```csharp
// Custom error types with specific HTTP mappings
[MapToProblemDetails(StatusCode = 422, Title = "Validation Failed")]
public class ValidationError : Error { /* ... */ }

[MapToProblemDetails(StatusCode = 404, Title = "Resource Not Found")]
public class UserNotFoundError : Error { /* ... */ }
```

### Generated Attributes
```csharp
// Automatically generated attributes
[GenerateResultExtensions]
[GenerateOneOf2Extensions] 
[GenerateOneOf3Extensions]
[MapToProblemDetails]
```

---

## 📊 Quality & Testing

### 🧪 Comprehensive Test Suite
**1902+ Tests Passing - Production Ready**

| Test Suite | Tests | Coverage |
|------------|-------|----------|
| **ResultToIResult** | 6/6 ✅ | Extension generation, HTTP mapping |
| **OneOf2ToIResult** | 5/5 ✅ | Two-type OneOf, error handling |
| **OneOf3ToIResult** | 4/4 ✅ | Three-type OneOf, status mapping |
| **Core Library** | 1886/1886 ✅ | Base functionality, patterns |
| **Total** | **1902/1902** ✅ | **95%+ code coverage** |

### 🚀 CI/CD Pipeline
```yaml
# .github/workflows/ci.yml
- Build Solution: dotnet build --configuration Release
- Run Tests: dotnet test --configuration Release --no-build
- Total Tests: 1902+ passing
- Coverage: 95%+ code coverage
```

---

## 🎯 Real-World Usage

### Sample API Endpoints
```csharp
// Result<T> endpoints
app.MapGet("/users/{id}", async (int id) => await userService.GetUserAsync(id));
app.MapPost("/users", async (CreateUserRequest request) => await userService.CreateUserAsync(request));

// OneOf<T1,T2> endpoints  
app.MapGet("/external-users/{id}", async (int id) => GetExternalUser(id));

// OneOf<T1,T2,T3> endpoints
app.MapPost("/complex-users", async (CreateUserRequest request) => CreateUserWithValidation(request));
```

### Generated HTTP Responses
```json
// Success (200 OK)
{
  "id": 123,
  "name": "John Doe",
  "email": "john@example.com"
}

// Error (400 Bad Request)
{
  "type": "https://httpstatuses.com/400",
  "title": "Bad Request", 
  "status": 400,
  "detail": "User not found"
}

// Validation Error (422 Unprocessable Entity)
{
  "type": "https://httpstatuses.com/422",
  "title": "Validation Failed",
  "status": 422,
  "errors": {
    "Email": ["Email is required"],
    "Name": ["Name must be at least 2 characters"]
  }
}
```

---

## 📚 Documentation

### 🎯 Choose Your Path

| I want to... | 📖 Start Here | 🎯 What You'll Learn |
|-------------|---------------|---------------------|
| **Use in Web APIs** | [Main README - Integration](../README.md#-the-transformation-70-90-less-code) | Auto-conversion, OneOf extensions |
| **Understand Architecture** | [Main README - Architecture](../README.md#-complete-architecture) | SOLID design, generator pipeline |
| **Create Custom Generators** | [Custom Generator Guide](../docs/how-to-create-custom-generator.md) | Build your own generators |
| **Run Tests** | [Main README - Testing](../README.md#-testing--quality-assurance) | 1902+ tests, CI/CD |

### 🔗 Related Documentation
- **[Main Project README](../README.md)** - Complete project overview
- **[Quick Start Guide](../QUICK-START.md)** - 30-second setup
- **[Custom Generator Guide](../docs/how-to-create-custom-generator.md)** - Extending the platform
- **[ASP.NET Samples](../samples/ASP.NET/README.md)** - Live examples

---

## 🏆 Why REslava.Result.SourceGenerators?

### ✅ **Zero Dependencies**
- No external packages required
- Clean, secure compilation
- Small footprint (~50KB)

### ✅ **Production Ready**  
- 1902+ tests passing
- 95%+ code coverage
- SOLID architecture
- AOT & NativeAOT compatible

### ✅ **Developer Experience**
- Zero configuration required
- Smart auto-detection
- Rich IntelliSense
- Comprehensive error messages

### ✅ **Extensible Platform**
- Build custom generators
- SOLID interface design
- Comprehensive testing framework
- Production-grade examples

---

**🚀 Transform your Web APIs with 70-90% less boilerplate code!**

*Start with the [Quick Start Guide](../QUICK-START.md) and see the magic in action!* ✨

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
