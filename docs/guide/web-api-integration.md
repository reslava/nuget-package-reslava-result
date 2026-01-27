# Web API Integration Guide

**🚀 Eliminate 70-90% of boilerplate code in your ASP.NET Core APIs!**

This guide shows you how to integrate REslava.Result with ASP.NET Core to create clean, maintainable APIs with automatic HTTP response conversion.

---

## 🎯 The Problem

### Traditional Web API Code

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetUserAsync(id);
            
            if (user == null)
                return NotFound("User not found");
                
            if (!user.IsActive)
                return BadRequest("User is inactive");
                
            return Ok(new UserDto(user));
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user");
            return StatusCode(500, "Internal server error");
        }
    }
}
```

**Problems:**
- ❌ Exception-based error handling
- ❌ Manual HTTP status code mapping
- ❌ Inconsistent error response format
- ❌ Complex nested try-catch blocks
- ❌ Hard to test both success and failure paths

---

## ✅ The REslava.Result Solution

### With Source Generator

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<Result<UserDto>> GetUser(int id)
    {
        return await _userService.GetUserAsync(id)
            .Ensure(u => u != null, new NotFoundError("User", id))
            .Ensure(u => u.IsActive, new BusinessError("UserInactive", "User is inactive"))
            .Map(u => new UserDto(u));
    }
}
```

**That's it!** The source generator automatically converts `Result<UserDto>` to the appropriate HTTP response.

---

## 📦 Installation

### 1. Core Packages

```bash
dotnet add package REslava.Result
dotnet add package REslava.Result.SourceGenerators
```

### 2. Enable the Generator

Add this to your `Program.cs`:

```csharp
using REslava.Result.SourceGenerators;

[assembly: GenerateResultExtensions]

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();
```

---

## ⚡ Quick Start

### Step 1: Update Your Services

```csharp
public class UserService
{
    public async Task<Result<User>> GetUserAsync(int id)
    {
        return await Result<int>.Ok(id)
            .Ensure(i => i > 0, "Invalid user ID")
            .BindAsync(async i => await _repository.FindAsync(i))
            .Ensure(u => u != null, new NotFoundError("User", id))
            .Ensure(u => u.IsActive, new BusinessError("UserInactive", "User is inactive"));
    }
}
```

### Step 2: Simplify Your Controllers

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
            .Map(u => new UserDto(u));
    }
}
```

### Step 3: Automatic HTTP Responses

The source generator automatically handles:

| Result Type | HTTP Response | Status Code |
|-------------|----------------|-------------|
| `Result<T>.Ok(value)` | `Ok<T>(value)` | 200 |
| `Result.Ok()` | `Ok()` | 200 |
| `Result<T>.Fail(NotFoundError)` | `ProblemDetails` | 404 |
| `Result<T>.Fail(ValidationError)` | `ProblemDetails` | 422 |
| `Result<T>.Fail(UnauthorizedError)` | `ProblemDetails` | 401 |
| `Result<T>.Fail(ForbiddenError)` | `ProblemDetails` | 403 |
| `Result<T>.Fail(DuplicateError)` | `ProblemDetails` | 409 |
| `Result<T>.Fail(other)` | `ProblemDetails` | 400 |

---

## 🎯 HTTP Method Extensions

The source generator also provides HTTP method-specific extensions:

### POST with Location Header

```csharp
[HttpPost]
public async Task<Result<UserDto>> CreateUser([FromBody] CreateUserRequest request)
{
    return await _userService.CreateUserAsync(request)
        .ToPostResult(user => $"/api/users/{user.Id}"); // Returns 201 Created with Location header
}
```

### DELETE with No Content

```csharp
[HttpDelete("{id}")]
public async Task<Result> DeleteUser(int id)
{
    return await _userService.DeleteUserAsync(id)
        .ToDeleteResult(); // Returns 204 No Content
}
```

### GET with Cache Headers

```csharp
[HttpGet("{id}")]
public async Task<Result<UserDto>> GetUser(int id)
{
    return await _userService.GetUserAsync(id)
        .Map(u => new UserDto(u))
        .ToGetResult(); // Returns 200 OK with cache-friendly headers
}
```

---

## 🛡️ Error Handling

### Smart Error Mapping

The source generator automatically maps error types to appropriate HTTP status codes:

```csharp
// These error types are automatically mapped:
new NotFoundError("User", id)           // → 404 Not Found
new ValidationError("Email", "Invalid") // → 422 Unprocessable Entity  
new UnauthorizedError("Not logged in")  // → 401 Unauthorized
new ForbiddenError("Access denied")     // → 403 Forbidden
new DuplicateError("Email exists")      // → 409 Conflict
new Error("Generic error")               // → 400 Bad Request
```

### Custom Error Mapping

Configure custom mappings in the generator attribute:

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

### Rich Error Context

Error tags are automatically included in ProblemDetails:

```csharp
var error = new ValidationError("Email", "Invalid format")
    .WithTag("Field", "Email")
    .WithTag("Value", email)
    .WithTag("ErrorCode", "VAL_001");

Result<User>.Fail(error);
```

**Generated ProblemDetails:**
```json
{
  "type": "https://httpstatuses.io/422",
  "title": "Unprocessable Entity",
  "status": 422,
  "detail": "Email: Invalid format",
  "extensions": {
    "errors": [
      {
        "message": "Email: Invalid format",
        "tags": {
          "Field": "Email",
          "Value": "invalid-email",
          "ErrorCode": "VAL_001"
        }
      }
    ]
  }
}
```

---

## 🌐 Minimal APIs

### Perfect for Minimal APIs

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

### Validation in Minimal APIs

```csharp
app.MapPost("/users", async (CreateUserRequest request, IUserService service) =>
{
    return Result<CreateUserRequest>.Ok(request)
        .Ensure(r => !string.IsNullOrWhiteSpace(r.Email), "Email is required")
        .Ensure(r => IsValidEmail(r.Email), "Invalid email format")
        .Ensure(r => r.Age >= 18, "Must be 18 or older")
        .BindAsync(async r => await service.CreateUserAsync(r))
        .ToPostResult(user => $"/users/{user.Id}");
});
```

---

## 📊 Real-World Example

### Complete User Management API

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

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
            .ToPostResult(user => $"/api/users/{user.Id}");
    }

    [HttpPut("{id}")]
    public async Task<Result<UserDto>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        return await _userService.UpdateUserAsync(id, request)
            .ToPutResult(); // Returns 200 OK
    }

    [HttpDelete("{id}")]
    public async Task<Result> DeleteUser(int id)
    {
        return await _userService.DeleteUserAsync(id)
            .ToDeleteResult(); // Returns 204 No Content
    }

    [HttpGet]
    public async Task<Result<PagedResult<UserDto>>> GetUsers([FromQuery] UserQuery query)
    {
        return await _userService.GetUsersAsync(query)
            .ToGetResult(); // Returns 200 OK with cache headers
    }
}
```

### Supporting Service

```csharp
public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IEmailService _emailService;

    public async Task<Result<User>> CreateUserAsync(CreateUserRequest request)
    {
        return await Result<CreateUserRequest>.Ok(request)
            // Validate input
            .Ensure(r => !string.IsNullOrWhiteSpace(r.Email), "Email is required")
            .Ensure(r => IsValidEmail(r.Email), "Invalid email format")
            .Ensure(r => r.Age >= 18, "Must be 18 or older")
            
            // Check business rules
            .EnsureAsync(async r => !await EmailExistsAsync(r.Email), 
                new DuplicateError("Email", r.Email))
            
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
            .BindAsync(async user => await _repository.SaveAsync(user))
            
            // Send welcome email
            .TapAsync(user => _emailService.SendWelcomeEmailAsync(user.Email))
            
            // Add success tracking
            .WithSuccess("User created successfully");
    }

    public async Task<Result<User>> GetUserAsync(int id)
    {
        return await Result<int>.Ok(id)
            .Ensure(i => i > 0, "Invalid user ID")
            .BindAsync(async i => await _repository.FindAsync(i))
            .Ensure(u => u != null, new NotFoundError("User", id))
            .Ensure(u => u.IsActive, new BusinessError("UserInactive", "User is inactive"));
    }

    public async Task<Result<User>> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        return await GetUserAsync(id)
            .Bind(user => Result<User>.Ok(user)
                .Ensure(u => !string.IsNullOrWhiteSpace(request.Name), "Name is required")
                .Map(u => 
                {
                    u.Name = request.Name;
                    u.UpdatedAt = DateTime.UtcNow;
                    return u;
                })
                .BindAsync(async u => await _repository.UpdateAsync(u))
                .WithSuccess("User updated successfully"));
    }

    public async Task<Result> DeleteUserAsync(int id)
    {
        return await GetUserAsync(id)
            .BindAsync(async user => await _repository.DeleteAsync(user.Id))
            .WithSuccess("User deleted successfully");
    }

    public async Task<Result<PagedResult<User>>> GetUsersAsync(UserQuery query)
    {
        return await Result<UserQuery>.Ok(query)
            .Ensure(q => q.Page >= 1, "Page must be >= 1")
            .Ensure(q => q.PageSize >= 1 && q.PageSize <= 100, "Page size must be between 1 and 100")
            .BindAsync(async q => await _repository.GetPagedAsync(q))
            .WithSuccess("Users retrieved successfully");
    }

    private async Task<bool> EmailExistsAsync(string email)
    {
        return await _repository.EmailExistsAsync(email);
    }

    private bool IsValidEmail(string email)
    {
        return email.Contains("@") && email.Contains(".");
    }
}
```

---

## 🧪 Testing Web APIs

### Unit Tests Made Simple

```csharp
[Test]
public async Task GetUser_ValidId_ReturnsUser()
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
public async Task GetUser_InvalidId_ReturnsNotFound()
{
    // Arrange
    var userId = -1;
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

## 🎯 Best Practices

### ✅ Do's

- **Use Result for all business logic** - Keep controllers thin
- **Create custom error types** - Make errors domain-specific
- **Use HTTP method extensions** - Leverage REST conventions
- **Add error tags** - Provide rich debugging context
- **Test both success and failure paths** - Ensure comprehensive coverage

### ❌ Don'ts

- **Don't use exceptions for expected failures** - Use Result instead
- **Don't put business logic in controllers** - Keep it in services
- **Don't ignore error context** - Use tags for debugging
- **Don't mix Result with exception handling** - Be consistent

---

## 📚 Next Steps

- **⚡ [Source Generator Guide](source-generator.md)** - Learn how the magic works
- **🧠 [Advanced Patterns](advanced-patterns.md)** - Explore Maybe, OneOf, and more
- **📚 [API Reference](../api/)** - Complete technical documentation

---

## 🎉 You're Ready!

You now have everything you need to build clean, maintainable Web APIs with REslava.Result. 

**Key benefits achieved:**
- ✅ 70-90% less boilerplate code
- ✅ Automatic HTTP response conversion
- ✅ Consistent error handling
- ✅ RFC 7807 compliant responses
- ✅ Type-safe error handling
- ✅ Easy testing

**Start building amazing APIs today!** 🚀
