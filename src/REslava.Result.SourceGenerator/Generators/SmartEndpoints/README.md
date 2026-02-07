# SmartEndpoints Generator

**Automatically generates ASP.NET Core Minimal API endpoints from methods returning `Result<T>` or `OneOf<...>` types.**

---

## ✨ Features

- **🎯 Zero Boilerplate**: Write business logic, get HTTP endpoints automatically
- **🔄 Intelligent Mapping**: Auto-detects HTTP methods from naming conventions (Get*, Create*, Update*, Delete*)
- **🚦 Smart HTTP Codes**: Automatically maps error types to appropriate HTTP status codes (404, 409, 401, etc.)
- **📝 Type-Safe**: Leverages C# source generators for compile-time safety
- **🎨 Flexible**: Use attributes for fine-grained control or conventions for rapid development
- **📚 OpenAPI Ready**: Generates endpoints compatible with Swagger/OpenAPI

---

## 📦 Installation

```bash
dotnet add package REslava.Result.SourceGenerators
```

---

## 🚀 Quick Start

### **Option 1: Class-Level Auto-Generation**

```csharp
using REslava.Result.SourceGenerators.SmartEndpoints;

[AutoGenerateEndpoints(RoutePrefix = "/api/users")]
public class UserController
{
    // Auto-generates: GET /api/users/{id}
    // Returns 200 OK with User or 404 Not Found
    public OneOf<UserNotFoundError, User> GetUser(int id)
    {
        var user = _database.FindUser(id);
        return user ?? new UserNotFoundError(id);
    }

    // Auto-generates: POST /api/users
    // Returns 201 Created or 409 Conflict or 400 Bad Request
    public OneOf<ValidationError, ConflictError, User> CreateUser(CreateUserRequest request)
    {
        // Validation
        if (string.IsNullOrEmpty(request.Email))
            return new ValidationError("Email is required");

        // Duplicate check
        if (_database.EmailExists(request.Email))
            return new ConflictError($"Email {request.Email} already exists");

        var user = _database.CreateUser(request);
        return user;
    }

    // Auto-generates: PUT /api/users/{id}
    // Returns 200 OK or 404 Not Found or 400 Bad Request
    public OneOf<ValidationError, UserNotFoundError, User> UpdateUser(int id, UpdateUserRequest request)
    {
        var user = _database.FindUser(id);
        if (user == null)
            return new UserNotFoundError(id);

        if (string.IsNullOrEmpty(request.Name))
            return new ValidationError("Name is required");

        _database.UpdateUser(id, request);
        return user;
    }

    // Auto-generates: DELETE /api/users/{id}
    // Returns 204 No Content or 404 Not Found
    public OneOf<UserNotFoundError, DeleteResult> DeleteUser(int id)
    {
        var user = _database.FindUser(id);
        if (user == null)
            return new UserNotFoundError(id);

        _database.DeleteUser(id);
        return new DeleteResult();
    }
}
```

### **Option 2: Method-Level Explicit Mapping**

```csharp
public class ProductService
{
    [AutoMapEndpoint("/api/products/{id}", HttpMethod = "GET")]
    public Result<Product> GetProduct(int id)
    {
        var product = _repository.Find(id);
        return product != null 
            ? Result<Product>.Ok(product)
            : Result<Product>.Fail("Product not found");
    }

    [AutoMapEndpoint("/api/products/search", HttpMethod = "GET")]
    public Result<List<Product>> SearchProducts(string query, int page = 1)
    {
        var results = _repository.Search(query, page);
        return Result<List<Product>>.Ok(results);
    }
}
```

---

## 🛠️ Setup in `Program.cs`

```csharp
using Generated.SmartEndpoints;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ✅ ONE LINE - Maps all SmartEndpoints
app.MapSmartEndpoints();

app.Run();
```

That's it! All your endpoints are now registered.

---

## 🎯 HTTP Method Detection

SmartEndpoints automatically infers HTTP methods from method names:

| Method Name Pattern | HTTP Verb | Route Pattern |
|---------------------|-----------|---------------|
| `GetUser(int id)` | GET | `/prefix/{id}` |
| `GetUsers()` | GET | `/prefix` |
| `CreateUser(...)` | POST | `/prefix` |
| `UpdateUser(int id, ...)` | PUT | `/prefix/{id}` |
| `PatchUser(int id, ...)` | PATCH | `/prefix/{id}` |
| `DeleteUser(int id)` | DELETE | `/prefix/{id}` |
| `SearchProducts(...)` | GET | `/prefix/search` (custom) |

---

## 🚦 Error Type → HTTP Status Code Mapping

SmartEndpoints intelligently maps error types to HTTP status codes:

| Error Type | HTTP Status | Example |
|-----------|-------------|---------|
| `ValidationError` | 400 Bad Request | Invalid input |
| `UserNotFoundError` | 404 Not Found | Resource doesn't exist |
| `ConflictError` | 409 Conflict | Duplicate email |
| `UnauthorizedError` | 401 Unauthorized | Not logged in |
| `ForbiddenError` | 403 Forbidden | No permission |
| `DatabaseError` | 500 Internal Server Error | System failure |

### **Custom Mappings**

```csharp
// Explicitly control HTTP status
[MapToProblemDetails(StatusCode = 402, Title = "Payment Required")]
public class InsufficientCreditsError : Error
{
    public InsufficientCreditsError(int required, int available)
        : base($"Need {required} credits, but only have {available}")
    {
    }
}
```

---

## 🎨 Advanced Configuration

### **Authentication & Authorization**

```csharp
[AutoGenerateEndpoints(
    RoutePrefix = "/api/admin",
    RequiresAuth = true,
    Policies = new[] { "AdminOnly" }
)]
public class AdminController
{
    // All endpoints require authentication + AdminOnly policy
    public OneOf<ValidationError, User> CreateAdmin(CreateAdminRequest request) { }
}
```

### **OpenAPI Tags**

```csharp
[AutoGenerateEndpoints(
    RoutePrefix = "/api/orders",
    Tags = new[] { "Orders", "E-Commerce" }
)]
public class OrderController
{
    // Swagger groups these under "Orders" and "E-Commerce"
}
```

### **Custom Routes**

```csharp
[AutoMapEndpoint("/api/v2/users/{userId}/orders/{orderId}", HttpMethod = "GET")]
public OneOf<OrderNotFoundError, Order> GetUserOrder(int userId, int orderId)
{
    // Full control over route template
}
```

---

## 🔄 Works with Result<T>

```csharp
[AutoGenerateEndpoints]
public class LegacyController
{
    // Also works with Result<T>
    public Result<User> GetUser(int id)
    {
        return _service.GetUser(id);
    }

    public Result<List<User>> GetAllUsers()
    {
        return _service.GetAllUsers();
    }
}
```

---

## 📊 Generated Code Example

**Your Code:**

```csharp
[AutoGenerateEndpoints(RoutePrefix = "/api/users")]
public class UserController
{
    public OneOf<UserNotFoundError, User> GetUser(int id) { }
}
```

**Generated Code:**

```csharp
public static class SmartEndpointExtensions
{
    public static IEndpointRouteBuilder MapSmartEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapUserControllerEndpoints();
        return endpoints;
    }

    public static IEndpointRouteBuilder MapUserControllerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/users/{id}", (int id) =>
        {
            var controller = new UserController();
            var result = controller.GetUser(id);
            return result.ToIResult(); // Uses OneOf→IResult extension
        });
        
        return endpoints;
    }
}
```

---

## 🎯 When to Use SmartEndpoints

### ✅ **Perfect For:**
- **CRUD APIs**: Rapid development of standard REST APIs
- **Microservices**: Clean separation of business logic and HTTP concerns
- **Prototyping**: Quickly expose domain logic as HTTP endpoints
- **Minimal APIs**: You're using ASP.NET Core Minimal APIs

### ❌ **Not Ideal For:**
- **Complex Routing**: Custom route constraints, regex patterns
- **Legacy MVC**: You're using traditional Controllers with views
- **Non-HTTP**: SignalR, gRPC, or other non-REST protocols

---

## 🧩 Architecture Benefits

1. **Separation of Concerns**: Business logic stays pure, HTTP is generated
2. **Type Safety**: Compile-time errors if you change return types
3. **Consistency**: All endpoints follow the same error mapping conventions
4. **Testability**: Test business logic without HTTP concerns
5. **Maintainability**: Change one method, endpoint updates automatically

---

## 🚀 Migration Path

**Before (Manual):**

```csharp
app.MapGet("/users/{id}", async (int id, UserService service) =>
{
    var result = await service.GetUserAsync(id);
    
    if (result.IsFailed)
    {
        var error = result.Errors.First();
        if (error is UserNotFoundError)
            return Results.NotFound(error.Message);
        return Results.BadRequest(error.Message);
    }
    
    return Results.Ok(result.Value);
});
```

**After (SmartEndpoints):**

```csharp
[AutoGenerateEndpoints]
public class UserController
{
    public OneOf<UserNotFoundError, User> GetUser(int id)
    {
        return _service.GetUser(id);
    }
}
```

---

## 📝 Complete Example

```csharp
// Domain Errors
public class UserNotFoundError : Error
{
    public UserNotFoundError(int id) : base($"User {id} not found") { }
}

public class ConflictError : Error
{
    public ConflictError(string message) : base(message) { }
}

// Controller
[AutoGenerateEndpoints(RoutePrefix = "/api/users")]
public class UserController
{
    private readonly IUserRepository _repository;

    public UserController(IUserRepository repository)
    {
        _repository = repository;
    }

    public OneOf<UserNotFoundError, User> GetUser(int id)
    {
        var user = _repository.Find(id);
        return user ?? new UserNotFoundError(id);
    }

    public OneOf<ValidationError, ConflictError, User> CreateUser(CreateUserRequest request)
    {
        if (string.IsNullOrEmpty(request.Email))
            return new ValidationError("Email required");

        if (_repository.EmailExists(request.Email))
            return new ConflictError($"Email {request.Email} exists");

        return _repository.Create(request);
    }
}

// Program.cs
var app = builder.Build();
app.MapSmartEndpoints();
app.Run();
```

---

## 🔗 See Also

- [ResultToIResult Generator](../ResultToIResult/README.md)
- [OneOf2ToIResult Generator](../OneOf2ToIResult/README.md)
- [REslava.Result Documentation](https://github.com/reslava/nuget-package-reslava-result)

---

## 📄 License

MIT © REslava 2025
