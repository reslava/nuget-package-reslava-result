# REslava.Result.SourceGenerators

Source generators for the REslava.Result library that automatically convert `Result<T>` to `IResult` for ASP.NET Core Minimal APIs.

## 🚀 Features

- ✅ **Eliminates 70-90% of boilerplate** - No more manual `.Match()` calls in every endpoint
- ✅ **RFC 7807 compliant** - Automatic ProblemDetails responses
- ✅ **Smart error mapping** - Automatically maps errors to appropriate HTTP status codes
- ✅ **Preserves error tags** - Rich context included in ProblemDetails
- ✅ **Type-safe** - All code generated at compile-time
- ✅ **AOT compatible** - Works with NativeAOT
- ✅ **Async support** - Works seamlessly with `async`/`await`
- ✅ **Extensive XML documentation** - Full IntelliSense support

## 📦 Installation

```bash
dotnet add package REslava.Result.SourceGenerators
```

## 🎯 Quick Start

### Step 1: Enable the Generator

Add the assembly attribute to your `Program.cs`:

```csharp
using REslava.Result.SourceGenerators;

[assembly: GenerateResultExtensions]

var builder = WebApplication.CreateBuilder(args);
// ... rest of your code
```

### Step 2: Use Direct Returns

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

// ✅ AFTER: Direct return - auto-converted!
app.MapGet("/users/{id}", async (int id, IUserService service) =>
    await service.GetUserAsync(id));
```

That's it! The generator automatically:
- Converts `Result<T>` success to `200 OK` with the value
- Maps errors to appropriate HTTP status codes (404, 409, 422, etc.)
- Creates RFC 7807 ProblemDetails for failures
- Includes error tags in the response context

## ⚙️ Configuration

Customize the generator with attribute properties:

```csharp
[assembly: GenerateResultExtensions(
    Namespace = "MyApp.Extensions",              // Custom namespace
    IncludeErrorTags = true,                     // Include tags in ProblemDetails
    GenerateHttpMethodExtensions = true,         // Generate ToGetResult(), ToPostResult(), etc.
    CustomErrorMappings = new[]                  // Map custom error types to status codes
    {
        "ValidationError:422",
        "NotFoundError:404",
        "DuplicateError:409",
        "UnauthorizedError:401"
    }
)]
```

### Configuration Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Namespace` | `string` | `"Generated.ResultExtensions"` | Namespace for generated code |
| `IncludeSuccessReasons` | `bool` | `false` | Include success reasons in responses |
| `UseImplicitConversions` | `bool` | `true` | Enable implicit `Result` → `IResult` conversions |
| `IncludeErrorTags` | `bool` | `true` | Include error tags in ProblemDetails |
| `LogErrors` | `bool` | `false` | Automatically log errors (requires ILogger) |
| `CustomErrorMappings` | `string[]` | `[]` | Custom error type to status code mappings |
| `GenerateHttpMethodExtensions` | `bool` | `true` | Generate method-specific extensions |
| `DefaultErrorStatusCode` | `int` | `400` | Default status for unrecognized errors |

## 🎨 HTTP Method-Specific Extensions

When `GenerateHttpMethodExtensions = true`, you get optimized methods for each HTTP verb:

```csharp
// GET - Returns 200 OK or 404 Not Found
app.MapGet("/users/{id}", async (int id, IUserService service) =>
    (await service.GetUserAsync(id)).ToGetResult());

// POST - Returns 201 Created with Location header
app.MapPost("/users", async (CreateUserRequest request, IUserService service) =>
{
    var result = await service.CreateUserAsync(request);
    return result.ToPostResult(user => $"/users/{user.Id}");
});

// PUT - Returns 200 OK or 404 Not Found
app.MapPut("/users/{id}", async (int id, UpdateUserRequest request, IUserService service) =>
    (await service.UpdateUserAsync(id, request)).ToPutResult());

// DELETE - Returns 204 No Content or 404 Not Found
app.MapDelete("/users/{id}", async (int id, IUserService service) =>
    (await service.DeleteUserAsync(id)).ToDeleteResult());

// PATCH - Returns 200 OK, 404, or 422
app.MapPatch("/users/{id}", async (int id, PatchUserRequest request, IUserService service) =>
    (await service.PatchUserAsync(id, request)).ToPatchResult());
```

## 🏷️ Error Tags & Rich Context

Error tags are automatically included in ProblemDetails:

```csharp
// In your service
public Result<User> CreateUser(CreateUserRequest request)
{
    if (_db.Users.Any(u => u.Email == request.Email))
    {
        var error = new DuplicateError("Email already exists")
            .WithTag("Email", request.Email)
            .WithTag("AttemptedAt", DateTime.UtcNow)
            .WithTag("UserId", existingUser.Id);
        
        return Result<User>.Fail(error);
    }
    
    // ...
}

// Generated response:
// {
//   "type": "https://httpstatuses.io/409",
//   "title": "Conflict",
//   "status": 409,
//   "detail": "Email already exists",
//   "context": {
//     "Email": "john@example.com",
//     "AttemptedAt": "2026-01-27T10:30:00Z",
//     "UserId": 123
//   }
// }
```

## 🎯 Custom Error Types

Map your custom error types to specific HTTP status codes:

```csharp
// Define custom error types
public class ValidationError : Error
{
    public ValidationError(string message) : base(message) { }
}

public class NotFoundError : Error
{
    public NotFoundError(string message) : base(message) { }
}

// Configure mappings
[assembly: GenerateResultExtensions(
    CustomErrorMappings = new[]
    {
        "ValidationError:422",
        "NotFoundError:404",
        "DuplicateError:409"
    }
)]

// Use in services
public Result<User> GetUser(int id)
{
    var user = _db.Users.Find(id);
    
    if (user == null)
    {
        return Result<User>.Fail(new NotFoundError($"User {id} not found"));
        // Automatically maps to 404 Not Found
    }
    
    return Result<User>.Ok(user);
}
```

## 🔍 Pattern-Based Error Detection

Even without custom mappings, the generator detects error patterns:

```csharp
// These error messages automatically map to appropriate status codes:

// → 404 Not Found
Result<User>.Fail("User not found");
Result<User>.Fail("Resource does not exist");

// → 409 Conflict
Result<User>.Fail("Email already exists");
Result<User>.Fail("Duplicate entry detected");

// → 422 Unprocessable Entity
Result<User>.Fail("Invalid email format");
Result<User>.Fail("Validation failed");

// → 401 Unauthorized
Result<User>.Fail("Not authorized");
Result<User>.Fail("Unauthorized access");

// → 403 Forbidden
Result<User>.Fail("Access denied");
Result<User>.Fail("Forbidden resource");

// → 400 Bad Request (default)
Result<User>.Fail("Something went wrong");
```

## 📝 Generated Code Example

Here's what the generator creates for you:

```csharp
namespace Generated.ResultExtensions
{
    /// <summary>
    /// Extension methods for converting Result<T> to IResult.
    /// Auto-generated by REslava.Result.SourceGenerators.
    /// </summary>
    public static class ResultToIResultExtensions
    {
        /// <summary>
        /// Converts a Result<T> to IResult for use in Minimal API endpoints.
        /// Success returns 200 OK with the value, failure returns appropriate error response.
        /// </summary>
        public static IResult ToIResult<T>(this Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Results.Ok(result.Value);
            }
            
            var statusCode = DetermineStatusCode(result.Errors);
            var problemDetails = CreateProblemDetails(result.Errors, statusCode, null);
            
            return Results.Problem(
                detail: problemDetails.Detail,
                instance: problemDetails.Instance,
                statusCode: problemDetails.Status,
                title: problemDetails.Title,
                type: problemDetails.Type,
                extensions: problemDetails.Extensions);
        }
        
        // ... additional helper methods
    }
}
```

## 🧪 Testing

The generated code is fully testable:

```csharp
[Fact]
public void ToIResult_Success_ReturnsOk()
{
    // Arrange
    var result = Result<User>.Ok(new User { Id = 1, Name = "John" });
    
    // Act
    var iresult = result.ToIResult();
    
    // Assert
    Assert.IsType<Ok<User>>(iresult);
}

[Fact]
public void ToIResult_NotFoundError_Returns404()
{
    // Arrange
    var result = Result<User>.Fail(new NotFoundError("User not found"));
    
    // Act
    var iresult = result.ToIResult();
    
    // Assert
    var problem = Assert.IsType<ProblemHttpResult>(iresult);
    Assert.Equal(404, problem.StatusCode);
}

[Fact]
public void ToIResult_WithTags_IncludesContext()
{
    // Arrange
    var error = new NotFoundError("User not found")
        .WithTag("UserId", 123)
        .WithTag("SearchedAt", DateTime.UtcNow);
    var result = Result<User>.Fail(error);
    
    // Act
    var iresult = result.ToIResult();
    
    // Assert
    var problem = Assert.IsType<ProblemHttpResult>(iresult);
    Assert.Contains("context", problem.ProblemDetails.Extensions.Keys);
}
```

## 🎓 Best Practices

### ✅ DO

1. **Use custom error types** for domain-specific errors
2. **Add error tags** for rich context and debugging
3. **Configure mappings** for your error types
4. **Use HTTP method extensions** for cleaner code
5. **Test generated code** like any other code

### ❌ DON'T

1. **Don't mix Result and exception-based error handling** in the same endpoint
2. **Don't create generic error messages** - be specific
3. **Don't forget to configure custom mappings** for your error types
4. **Don't over-use tags** - include only relevant context

## 🔧 Troubleshooting

### Generator not running?

1. Clean and rebuild: `dotnet clean && dotnet build`
2. Check that attribute is at assembly level
3. Verify package is correctly referenced
4. Enable source generator diagnostics in IDE

### Generated code not found?

1. Check the namespace matches your configuration
2. Add `using Generated.ResultExtensions;` (or your custom namespace)
3. Restart IDE for IntelliSense refresh

### Compilation errors?

1. Ensure REslava.Result package is referenced
2. Check .NET version compatibility (requires .NET 6+)
3. Verify all required namespaces are imported

## 📊 Performance

- **Zero runtime overhead** - All code generated at compile-time
- **AOT compatible** - Works with NativeAOT compilation
- **No reflection** - Type-safe code generation
- **Incremental** - Only regenerates when configuration changes

## 🤝 Contributing

Contributions welcome! Please:
1. Follow existing code style
2. Add tests for new features
3. Update documentation
4. Ensure AOT compatibility

## 📄 License

MIT License - see LICENSE file for details

## 🔗 Related

- [REslava.Result](https://github.com/reslava/Result) - The core Result library
- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [RFC 7807 - Problem Details](https://tools.ietf.org/html/rfc7807)

---

**Made with ❤️ for functional error handling in C#**
