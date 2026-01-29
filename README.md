# REslava.Result

<div align="center">

![.NET](https://img.shields.io/badge/.NET-512BD4?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?&logo=csharp&logoColor=white)
![NuGet Version](https://img.shields.io/nuget/v/REslava.Result?style=flat&logo=nuget)
![License](https://img.shields.io/badge/license-MIT-green)
[![GitHub contributors](https://img.shields.io/github/contributors/reslava/REslava.Result)](https://GitHub.com/reslava/REslava.Result/graphs/contributors/) 
[![GitHub Stars](https://img.shields.io/github/stars/reslava/REslava.Result)](https://github.com/reslava/REslava.Result/stargazers) 
[![NuGet Downloads](https://img.shields.io/nuget/dt/REslava.Result)](https://www.nuget.org/packages/REslava.Result)

**🚀 Production-Ready Result Pattern + Auto-Conversion for ASP.NET Core**

</div>

---

## 🎯 Why Developers Love REslava.Result?

**Stop fighting exceptions. Start writing predictable, maintainable code.**

- **⚡ 70-90% Less Boilerplate** - Auto-convert `Result<T>` to HTTP responses
- **🛡️ Type-Safe Error Handling** - No more hidden exceptions in production
- **📊 Rich Error Context** - Built-in tagging and metadata for debugging
- **🔧 Zero Dependencies** - Clean, secure, and fast
- **🧠 Functional Programming** - Expressive, composable code

---

## ⚡ Quick Start (30 seconds)

### 📦 Installation

```bash
dotnet add package REslava.Result
dotnet add package REslava.Result.SourceGenerators
```

### 🚀 Enable Auto-Conversion

```csharp
// Add this to your Program.cs
using REslava.Result.SourceGenerators;
[assembly: GenerateResultExtensions]

var builder = WebApplication.CreateBuilder(args);
// ... rest of your setup
```

### 🎯 **Want to see it in action?**
Check out our **[ASP.NET Integration Samples](samples/ASP.NET/README.md)** to compare pure .NET 10 vs REslava.Result implementations!

### 🏗️ Architecture Evolution v1.8.0

### 🧠 **Enhanced Source Generator Architecture**

The v1.8.0 release introduces a revolutionary **metadata discovery system** that transforms how error types are mapped to HTTP responses:

#### **🔍 Three-Tier Error Mapping Priority**
1. **🎯 Explicit Attributes** - `[MapToProblemDetails(StatusCode = 404)]`
2. **⚙️ Custom Mappings** - Configuration-based error mappings  
3. **🧠 Convention-Based** - Smart pattern matching (NotFoundError → 404)

#### **🚀 Enhanced Capabilities**
- **📊 10+ HTTP Status Patterns** - NotFound, Validation, Conflict, etc.
- **🏷️ Rich Metadata** - Error tags, types, and custom properties
- **🔧 RFC 7807 Compliance** - Standardized ProblemDetails responses
- **⚡ Zero Runtime Overhead** - All processing at compile-time

#### **📈 Architecture Comparison**

| 🏗️ **Architecture** | **v1.7.3** | **v1.8.0 (Enhanced)** |
|-------------------|------------|----------------------|
| Error Mapping | Simple switch statements | **Metadata discovery system** |
| Custom Types | Not supported | **Full custom error type support** |
| HTTP Status | Basic patterns | **10+ intelligent patterns** |
| Configuration | Limited | **Three-tier priority system** |
| Extensibility | Fixed | **Highly extensible** |
| Performance | Good | **Optimized compile-time** |

### ✨ Enhanced v1.8.0 Features

#### **🏷️ Custom Error Types with Metadata**
```csharp
[MapToProblemDetails(
    StatusCode = 402,
    Type = "https://api.example.com/payment-required",
    Title = "Payment Required")]
public class PaymentRequiredError : Error
{
    public decimal Amount { get; }
    
    public PaymentRequiredError(decimal amount, string message) : base(message)
    {
        Amount = amount;
        this.WithTag("Amount", amount);
    }
}
```

#### **🧠 Smart Convention Matching**
```csharp
// These automatically map to correct HTTP status codes:
NotFoundError → 404 Not Found
ValidationError → 422 Unprocessable Entity  
ConflictError → 409 Conflict
UnauthorizedError → 401 Unauthorized
PaymentRequiredError → 402 Payment Required
RateLimitError → 429 Too Many Requests
TimeoutError → 408 Request Timeout
ServerError → 500 Internal Server Error
ServiceUnavailableError → 503 Service Unavailable
// ... and more patterns
```

#### **⚙️ Advanced Configuration**
```csharp
[assembly: GenerateResultExtensions(
    Namespace = "Generated.ResultExtensions",
    IncludeErrorTags = true,
    GenerateHttpMethodExtensions = true,
    CustomErrorMappings = new[] { 
        "PaymentRequiredError:402",
        "CustomBusinessError:418"
    })]
```

### ✨ Magic Happens

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

---

## 🚀 Key Features

| 📦 **Core Library** | 🚀 **Enhanced Source Generator v1.8.0** | 🧠 **Advanced Patterns** |
|-------------------|--------------------------------------|-------------------------|
| Type-safe Result pattern | **Metadata discovery system** | `Maybe<T>` for null safety |
| Fluent chaining | **Three-tier error mapping** | `OneOf` for discriminated unions |
| Rich error context | **10+ intelligent HTTP patterns** | LINQ query syntax |
| Zero dependencies | **RFC 7807 ProblemDetails** | Performance optimized |
| Railway-oriented programming | **Custom error type support** | Async/await support |
| **🆕 Enhanced error tags** | **AOT & NativeAOT compatible** | **🆕 Validation rules** |

---

## 🎯 Real-World Impact

### 🏢 **For Enterprise Teams**
- **Explicit failure tracking** replaces hidden exception flows
- **Rich error context** with tags for debugging and monitoring
- **Better observability** with structured error information

### 🧪 **For Test-Driven Development**
- **Predictable patterns** make unit tests simple and reliable
- **No complex exception setups** - just assert on Result values
- **Faster test writing** with deterministic results

### 👥 **For Team Collaboration**
- **Clear contracts** between services and components
- **Consistent patterns** across the entire codebase
- **Improved onboarding** for new team members

---

## 📚 Deep Dive Documentation

### 🎯 **Choose Your Path**

| I'm building a... | 📖 Start Here | 🎯 What You'll Learn |
|------------------|---------------|---------------------|
| **Web API** | [🌐 Web API Integration](docs/guide/web-api-integration.md) | Auto-conversion, error mapping, best practices |
| **Library/Service** | [📖 Getting Started](docs/guide/getting-started.md) | Core Result pattern, validation, error handling |
| **Advanced App** | [🧠 Advanced Patterns](docs/guide/advanced-patterns.md) | Maybe, OneOf, functional programming |
| **Curious About Magic** | [⚡ Source Generator](docs/guide/source-generator.md) | How auto-conversion works, configuration options |

### 📚 **Complete Reference**

- **📖 [Getting Started Guide](docs/guide/getting-started.md)** - Learn the basics
- **🌐 [Web API Integration](docs/guide/web-api-integration.md)** - ASP.NET Core setup
- **⚡ [Source Generator](docs/guide/source-generator.md)** - Auto-conversion magic
- **🧠 [Advanced Patterns](docs/guide/advanced-patterns.md)** - Maybe, OneOf, and more
- **📚 [API Reference](docs/api/)** - Complete technical documentation
- **🏗️ [Architecture & Design](docs/api/Overview.md)** - Design decisions and patterns

### 🎯 **Hands-On Samples**

- **🚀 [ASP.NET Integration Samples](samples/ASP.NET/README.md)** - Compare pure .NET 10 vs REslava.Result with source generators
  - **MinimalApi.Net10.Reference** - Pure .NET 10 implementation (baseline)
  - **MinimalApi.Net10.REslava.Result.v1.7.3** - REslava.Result + source generators (70-90% less code)

---

## 🎯 Quick Examples

### 📦 **Core Library - Type-Safe Error Handling**

```csharp
// Fluent, chainable operations
var result = await Result<string>.Ok(email)
    .Ensure(e => IsValidEmail(e), "Invalid email format")
    .EnsureAsync(async e => !await EmailExistsAsync(e), "Email already registered")
    .BindAsync(async e => await CreateUserAsync(e))
    .WithSuccess("User created successfully");

// Pattern matching
return result.Match(
    onSuccess: user => CreatedAtAction(nameof(GetUser), new { id = user.Id }, user),
    onFailure: errors => BadRequest(new { errors })
);
```

### 🚀 **Enhanced Source Generator v1.8.0 - Zero Boilerplate**

```csharp
// 🏷️ Define custom error types with metadata
[MapToProblemDetails(StatusCode = 404, Title = "User Not Found")]
public class UserNotFoundError : Error
{
    public int UserId { get; }
    public UserNotFoundError(int userId) : base($"User {userId} not found")
    {
        UserId = userId;
        this.WithTag("UserId", userId);
    }
}

// Your service returns Result<T> with rich error context
public async Task<Result<User>> GetUserAsync(int id)
{
    return await Result<int>.Ok(id)
        .Ensure(i => i > 0, "Invalid user ID")
        .BindAsync(async i => await _repository.FindAsync(i))
        .Ensure(u => u != null, new UserNotFoundError(id));
}

// 🎯 Your controller just returns the Result - auto-converted!
app.MapGet("/users/{id}", async (int id) => 
    await _userService.GetUserAsync(id));

// 🚀 Enhanced HTTP responses are automatically generated:
// 200 OK with User data
// 404 Not Found with ProblemDetails + custom metadata
// 400 Bad Request with validation errors
// ...and 10+ more intelligent patterns
```

### 🧠 **Advanced Patterns - Functional Programming**

```csharp
// Maybe<T> for safe null handling
Maybe<User> user = GetUserFromCache(id);
var email = user
    .Select(u => u.Email)
    .Filter(email => email.Contains("@"))
    .ValueOrDefault("no-reply@example.com");

// OneOf for discriminated unions
OneOf<ValidationError, User> result = ValidateAndCreateUser(request);
return result.Match(
    case1: error => BadRequest(error),
    case2: user => Ok(user)
);
```

---

## 📈 Production Benefits

| 🎯 **Challenge** | 🚀 **REslava.Result Solution** |
|------------------|------------------------------|
| **Hidden exceptions** | Explicit error contracts in method signatures |
| **Complex error handling** | Fluent, chainable operations |
| **Hard to debug failures** | Rich error context with tags |
| **Inconsistent error responses** | Automatic RFC 7807 compliance |
| **Slow development** | 70-90% less boilerplate code |

---

## 🏆 Why Choose REslava.Result?

### ✅ **Zero Dependencies**
- **No external packages** - Reduces security vulnerabilities
- **Small footprint** - Only ~50KB compiled
- **Fast compilation** - No complex dependency chains

### ✅ **Production-Ready**
- **95%+ code coverage** - Reliable in production
- **Comprehensive testing** - Unit, integration, and performance tests
- **Memory efficient** - Immutable design, predictable allocations
- **🆕 v1.8.0 Enhanced Architecture** - Metadata discovery with zero runtime overhead

### ✅ **Developer Experience**
- **Rich IntelliSense** - Extensive XML documentation
- **Modern C#** - Supports .NET 8, 9, and 10
- **AOT compatible** - Works with NativeAOT and trimming
- **🆕 Enhanced Error Context** - Rich metadata and custom error types

---

## 🤝 Contributing

We welcome contributions! Please see our [contributing guidelines](CONTRIBUTING.md) for details on how to submit issues, pull requests, and documentation improvements.

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🎉 Ready to Transform Your Error Handling?

**📖 [Start with the Getting Started Guide](docs/guide/getting-started.md)**

---

<div align="center">

**⭐ If REslava.Result makes your code more predictable, give us a star! ⭐**

</div>
