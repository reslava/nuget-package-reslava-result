# MinimalAPI .NET 10 Samples

This directory contains sample projects demonstrating different approaches to building Minimal APIs with .NET 10.

## 📁 Project Structure

```
samples/ASP.NET/
├── MinimalApi.Net10.Reference/                    # Pure .NET 10 implementation (baseline)
└── MinimalApi.Net10.REslava.Result.v1.7.3/       # Using REslava.Result v1.7.3 with source generators
```

## 🎯 Purpose

These samples serve as **educational references** to compare different development approaches:

### **Pure .NET 10 (Reference)**
- ✅ Built-in validation features
- ✅ Manual error handling with ValidationProblem
- ✅ Standard Minimal API patterns
- ✅ OpenAPI/Swagger integration
- ❌ More boilerplate code
- ❌ Manual HTTP response management

### **REslava.Result v1.7.3 + Source Generators**
- ✅ **Automatic Result<T> to IResult conversion**
- ✅ **Smart error classification** (400, 404, 422, etc.)
- ✅ **Generated extension methods** (`ToIResult()`, `ToPostResult()`, etc.)
- ✅ **Clean, declarative code** - No manual `Results.Ok()` or `Results.Problem()`
- ✅ **Consistent error handling** across all endpoints
- ✅ **Type-safe Result pattern** - Railway-oriented programming
- ✅ **Source generator magic** - Zero runtime overhead

## 🚀 Quick Comparison

### **Before (Pure .NET 10):**
```csharp
// Manual validation and response handling
var errors = new Dictionary<string, string[]>();
if (string.IsNullOrWhiteSpace(request.Name))
    errors["Name"] = new[] { "Name required" };

if (errors.Any())
    return Results.ValidationProblem(errors);

var product = productService.CreateProduct(request);
return Results.Created($"/api/products/{product.Id}", product);
```

### **After (REslava.Result + Source Generator):**
```csharp
// Clean, declarative Result pattern
if (string.IsNullOrWhiteSpace(request.Name))
    return Result<Product>.Fail("Name required").ToIResult();

var product = productService.CreateProduct(request);
return Result<Product>.Ok(product).ToPostResult();
```

## 🎯 Generated Extension Methods

The source generator automatically creates these extension methods:

| Method | HTTP Status | Use Case |
|--------|-------------|----------|
| `ToIResult()` | 200/400/404/500 | Standard CRUD operations |
| `ToPostResult()` | 201/400 | Resource creation |
| `ToPutResult()` | 200/400/404 | Resource updates |
| `ToDeleteResult()` | 200/400/404 | Resource deletion |
| `ToPatchResult()` | 200/400/404 | Partial updates |

## 🧠 Smart Error Classification

The source generator automatically classifies errors:

- **"not found"** → 404 Not Found
- **"invalid"** → 422 Unprocessable Entity  
- **"unauthorized"** → 401 Unauthorized
- **"forbidden"** → 403 Forbidden
- **"conflict"** → 409 Conflict
- **Default** → 500 Internal Server Error

## 🔧 Setup Instructions

### **For REslava.Result v1.7.3 Sample:**

1. **Add project references:**
```xml
<ProjectReference Include="../../../SourceGenerator/REslava.Result.SourceGenerators.csproj" 
                     ReferenceOutputAssembly="false" 
                     OutputItemType="Analyzer" />
<ProjectReference Include="../../../SourceGenerator/REslava.Result.SourceGenerators.csproj" 
                     ReferenceOutputAssembly="true" />
<ProjectReference Include="../../../src/REslava.Result.csproj" />
```

2. **Enable source generator:**
```csharp
using REslava.Result.SourceGenerators;
[assembly: GenerateResultExtensions]
```

3. **Use generated extensions:**
```csharp
using Generated.ResultExtensions;

app.MapGet("/api/products/{id}", (int id) =>
{
    if (id <= 0)
        return Result<Product>.Fail("Invalid ID").ToIResult();
        
    var product = productService.GetProductById(id);
    return product is null 
        ? Result<Product>.Fail("Not found").ToIResult()
        : Result<Product>.Ok(product).ToIResult();
});
```

## 🎯 Learning Path

1. **Start with `MinimalApi.Net10.Reference`** - Understand the baseline .NET 10 approach
2. **Compare with `MinimalApi.Net10.REslava.Result.v1.7.3`** - See the source generator benefits
3. **Run both samples** - Experience the difference in code clarity and maintainability
4. **Examine generated files** - Look in `obj/Debug/net10.0/generated/` to see what the generator creates

## 📚 Additional Resources

- [REslava.Result Documentation](../../README.md)
- [Source Generator Architecture](../../SourceGenerator/README.md)
- [Result Pattern Best Practices](../../docs/result-pattern.md)

## 🤝 Contributing

These samples are part of the REslava.Result library. Feel free to submit issues and enhancement requests!
