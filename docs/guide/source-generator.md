# Source Generator Guide

**🪄 Discover the magic behind automatic Result<T> to HTTP response conversion!**

The REslava.Result Source Generator eliminates boilerplate code by generating extension methods at compile-time. This guide shows you how it works and how to configure it for your needs.

---

## 🎯 What Problem Does It Solve?

### Before: Manual Conversion Everywhere

```csharp
[ApiController]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _userService.GetUserAsync(id);
        
        return result.Match(
            onSuccess: user => Ok(new UserDto(user)),
            onFailure: errors => {
                var statusCode = errors[0] switch {
                    NotFoundError => 404,
                    ValidationError => 422,
                    UnauthorizedError => 401,
                    _ => 400
                };
                
                return StatusCode(statusCode, new ProblemDetails {
                    Type = $"https://httpstatuses.io/{statusCode}",
                    Title = GetStatusTitle(statusCode),
                    Status = statusCode,
                    Detail = string.Join(", ", errors.Select(e => e.Message)),
                    Extensions = new Dictionary<string, object> {
                        ["errors"] = errors.Select(e => new {
                            message = e.Message,
                            tags = e.Tags
                        })
                    }
                });
            }
        );
    }
}
```

**Problems:**
- ❌ 20+ lines of boilerplate per endpoint
- ❌ Manual HTTP status code mapping
- ❌ Inconsistent error response format
- ❌ Easy to make mistakes in error handling
- ❌ Hard to maintain across many endpoints

### After: Zero Boilerplate

```csharp
[ApiController]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<Result<UserDto>> GetUser(int id)
    {
        return await _userService.GetUserAsync(id)
            .Map(u => new UserDto(u));
    }
}
```

**That's it!** The source generator automatically handles all the conversion logic.

---

## ⚡ How It Works

### 1. Enable the Generator

```csharp
using REslava.Result.SourceGenerators;

[assembly: GenerateResultExtensions]

var builder = WebApplication.CreateBuilder(args);
// ... rest of Program.cs
```

### 2. Generated Code

The source generator creates extension methods like this:

```csharp
// Generated code (you don't write this)
public static class ResultExtensions
{
    public static Microsoft.AspNetCore.Http.IResult ToIResult<T>(
        this REslava.Result.Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }
        
        var statusCode = GetStatusCode(result.Errors);
        var problemDetails = new ProblemDetails
        {
            Type = $"https://httpstatuses.io/{statusCode}",
            Title = GetStatusTitle(statusCode),
            Status = statusCode,
            Detail = GetErrorDetail(result.Errors),
            Extensions = new Dictionary<string, object>
            {
                ["errors"] = result.Errors.Select(e => new {
                    message = e.Message,
                    tags = e.Tags
                }).ToArray()
            }
        };
        
        return Results.Problem(problemDetails);
    }
}
```

### 3. Automatic Usage

Your controller methods automatically use the generated extensions:

```csharp
// Your code
public async Task<Result<UserDto>> GetUser(int id)
{
    return await _userService.GetUserAsync(id)
        .Map(u => new UserDto(u));
}

// What the compiler sees (simplified)
public async Task<Microsoft.AspNetCore.Http.IResult> GetUser(int id)
{
    var result = await _userService.GetUserAsync(id)
        .Map(u => new UserDto(u));
    
    return result.ToIResult(); // Generated extension method
}
```

---

## 🎯 Generated Extension Methods

### Core Conversion Methods

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

## 🛠️ Configuration Options

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
        "ServiceUnavailable:503",
        "CustomBusinessError:418"
    }
)]
```

### Advanced Configuration

```csharp
[assembly: GenerateResultExtensions(
    Namespace = "MyApp.Web.Extensions",
    IncludeErrorTags = true,
    LogErrors = true,  // Enable error logging during conversion
    GenerateHttpMethodExtensions = true,
    DefaultErrorStatusCode = 422,
    CustomErrorMappings = new[] 
    {
        "InsufficientFunds:402",
        "AccountLocked:423",
        "MaintenanceMode:503"
    }
)]
```

---

## 🎯 Error Type to Status Code Mapping

### Built-in Mappings

| Error Type Pattern | HTTP Status | Example |
|-------------------|-------------|---------|
| `*NotFound*` | 404 | `NotFoundError`, `UserNotFound` |
| `*DoesNotExist*` | 404 | `UserDoesNotExist`, `RecordDoesNotExist` |
| `*ValidationError*` | 422 | `ValidationError`, `EmailValidationError` |
| `*Unauthorized*` | 401 | `UnauthorizedError`, `AccessUnauthorized` |
| `*Forbidden*` | 403 | `ForbiddenError`, `AccessForbidden` |
| `*Duplicate*` | 409 | `DuplicateError`, `EmailDuplicate` |
| Default | 400 | All other errors |

### Custom Error Types

```csharp
public class PaymentRequiredError : Error
{
    public PaymentRequiredError(string message) : base(message) { }
}

public class RateLimitExceededError : Error
{
    public RateLimitExceededError(string message) : base(message) { }
}

// Configure mapping
[assembly: GenerateResultExtensions(
    CustomErrorMappings = new[] 
    {
        "PaymentRequiredError:402",
        "RateLimitExceededError:429"
    }
)]
```

---

## 📊 Generated ProblemDetails Structure

### Standard ProblemDetails

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
        "tags": {
          "Field": "Email",
          "ErrorCode": "REQUIRED"
        }
      },
      {
        "message": "Password must be at least 8 characters",
        "tags": {
          "Field": "Password", 
          "MinLength": 8,
          "ErrorCode": "MIN_LENGTH"
        }
      }
    ]
  }
}
```

---

## 🌐 Real-World Examples

### E-commerce API

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpPost]
    public async Task<Result<ProductDto>> CreateProduct([FromBody] CreateProductRequest request)
    {
        return await _productService.CreateProductAsync(request)
            .ToPostResult(p => $"/api/products/{p.Id}"); // 201 Created with Location
    }

    [HttpGet("{id}")]
    public async Task<Result<ProductDto>> GetProduct(int id)
    {
        return await _productService.GetProductAsync(id)
            .ToGetResult(); // 200 OK with cache headers
    }

    [HttpPut("{id}")]
    public async Task<Result<ProductDto>> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        return await _productService.UpdateProductAsync(id, request)
            .ToPutResult(); // 200 OK
    }

    [HttpDelete("{id}")]
    public async Task<Result> DeleteProduct(int id)
    {
        return await _productService.DeleteProductAsync(id)
            .ToDeleteResult(); // 204 No Content
    }
}
```

### Supporting Service

```csharp
public class ProductService
{
    public async Task<Result<Product>> CreateProductAsync(CreateProductRequest request)
    {
        return await Result<CreateProductRequest>.Ok(request)
            // Validate input
            .Ensure(r => !string.IsNullOrWhiteSpace(r.Name), "Product name is required")
            .Ensure(r => r.Price > 0, "Price must be greater than 0")
            .Ensure(r => r.Stock >= 0, "Stock cannot be negative")
            
            // Check business rules
            .EnsureAsync(async r => !await ProductExistsAsync(r.Name), 
                new DuplicateError("Product", r.Name))
            .EnsureAsync(async r => await CategoryExistsAsync(r.CategoryId), 
                new NotFoundError("Category", r.CategoryId))
            
            // Create product
            .MapAsync(async r => new Product 
            { 
                Id = Guid.NewGuid().ToString(),
                Name = r.Name,
                Price = r.Price,
                CategoryId = r.CategoryId,
                Stock = r.Stock,
                CreatedAt = DateTime.UtcNow
            })
            
            // Save to database
            .BindAsync(async p => await _repository.SaveAsync(p))
            
            // Add success tracking
            .WithSuccess("Product created successfully");
    }

    public async Task<Result<Product>> GetProductAsync(int id)
    {
        return await Result<int>.Ok(id)
            .Ensure(i => i > 0, "Invalid product ID")
            .BindAsync(async i => await _repository.FindAsync(i))
            .Ensure(p => p != null, new NotFoundError("Product", id))
            .Ensure(p => p.IsActive, new BusinessError("ProductInactive", "Product is not active"));
    }

    private async Task<bool> ProductExistsAsync(string name)
    {
        return await _repository.ExistsByNameAsync(name);
    }

    private async Task<bool> CategoryExistsAsync(string categoryId)
    {
        return await _categoryRepository.ExistsAsync(categoryId);
    }
}
```

---

## 🧪 Testing Generated Code

### Unit Testing Controllers

```csharp
[Test]
public async Task CreateProduct_ValidRequest_ReturnsCreated()
{
    // Arrange
    var request = new CreateProductRequest { Name = "Test Product", Price = 10.99m };
    var expectedProduct = new Product { Id = "123", Name = request.Name };
    
    _productService.Setup(x => x.CreateProductAsync(request))
        .ReturnsAsync(Result<Product>.Ok(expectedProduct));

    // Act
    var result = await _controller.CreateProduct(request);

    // Assert
    var createdResult = result as CreatedAtActionResult;
    Assert.IsNotNull(createdResult);
    Assert.AreEqual(201, createdResult.StatusCode);
    Assert.AreEqual("/api/products/123", createdResult.Location);
    
    var productDto = createdResult.Value as ProductDto;
    Assert.IsNotNull(productDto);
    Assert.AreEqual(expectedProduct.Name, productDto.Name);
}

[Test]
public async Task GetProduct_NotFound_ReturnsNotFound()
{
    // Arrange
    var productId = 999;
    _productService.Setup(x => x.GetProductAsync(productId))
        .ReturnsAsync(Result<Product>.Fail(new NotFoundError("Product", productId)));

    // Act
    var result = await _controller.GetProduct(productId);

    // Assert
    var notFoundResult = result as NotFoundObjectResult;
    Assert.IsNotNull(notFoundResult);
    Assert.AreEqual(404, notFoundResult.StatusCode);
    
    var problemDetails = notFoundResult.Value as ProblemDetails;
    Assert.IsNotNull(problemDetails);
    Assert.AreEqual("Product with id '999' not found", problemDetails.Detail);
}
```

### Integration Testing

```csharp
[Test]
public async Task CreateProduct_EndToEnd_ReturnsCorrectResponse()
{
    // Arrange
    var request = new CreateProductRequest { Name = "Test Product", Price = 10.99m };
    
    // Act
    var response = await _client.PostAsJsonAsync("/api/products", request);
    
    // Assert
    Assert.AreEqual(201, response.StatusCode);
    
    var location = response.Headers.Location?.ToString();
    Assert.IsNotNull(location);
    Assert.IsTrue(location.StartsWith("/api/products/"));
    
    var productDto = await response.Content.ReadFromJsonAsync<ProductDto>();
    Assert.IsNotNull(productDto);
    Assert.AreEqual(request.Name, productDto.Name);
}
```

---

## 🔧 Advanced Features

### Error Logging

Enable error logging to debug conversion issues:

```csharp
[assembly: GenerateResultExtensions(
    LogErrors = true  // Logs errors during conversion
)]
```

### Conditional Generation

Control which methods are generated:

```csharp
[assembly: GenerateResultExtensions(
    GenerateHttpMethodExtensions = false  // Only generate ToIResult()
)]
```

### Custom Namespace

Organize generated code in your own namespace:

```csharp
[assembly: GenerateResultExtensions(
    Namespace = "MyApp.Web.Generated"
)]
```

---

## 🎯 Best Practices

### ✅ Do's

- **Use descriptive error types** - Make status code mapping intuitive
- **Add error tags** - Provide rich context for debugging
- **Test both success and failure paths** - Ensure correct HTTP responses
- **Use HTTP method extensions** - Follow REST conventions
- **Configure custom mappings** - Match your domain's error types

### ❌ Don'ts

- **Don't rely on default mappings** for domain-specific errors
- **Don't ignore error tags** - They provide valuable debugging information
- **Don't mix Result with IActionResult** - Be consistent in your controllers
- **Don't forget the assembly attribute** - The generator won't run without it

---

## 🚀 Performance Considerations

### Compile-Time Generation

- **Zero runtime overhead** - All code generated at compile-time
- **No reflection** - Direct method calls
- **AOT compatible** - Works with NativeAOT and trimming
- **Minimal allocations** - Efficient generated code

### Memory Efficiency

- **Immutable structures** - No state to manage
- **Static extension methods** - No per-request overhead
- **Efficient error handling** - Minimal object creation

---

## 📚 Next Steps

- **🌐 [Web API Integration](web-api-integration.md)** - Complete API setup guide
- **🧠 [Advanced Patterns](advanced-patterns.md)** - Explore Maybe, OneOf, and more
- **📚 [API Reference](../api/)** - Complete technical documentation

---

## 🎉 You're Now a Source Generator Expert!

You understand how the magic works and can configure it for your specific needs.

**Key takeaways:**
- ✅ Zero boilerplate code in controllers
- ✅ Automatic HTTP response conversion
- ✅ RFC 7807 compliant ProblemDetails
- ✅ Configurable error mappings
- ✅ Compile-time generation with zero runtime overhead

**Start building clean APIs today!** 🚀
