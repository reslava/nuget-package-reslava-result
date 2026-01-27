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

| 📦 **Core Library** | 🚀 **Source Generator** | 🧠 **Advanced Patterns** |
|-------------------|----------------------|-------------------------|
| Type-safe Result pattern | Auto `Result<T>` → HTTP responses | `Maybe<T>` for null safety |
| Fluent chaining | RFC 7807 ProblemDetails | `OneOf` for discriminated unions |
| Rich error context | Smart HTTP status mapping | LINQ query syntax |
| Zero dependencies | AOT compatible | Performance optimized |
| Railway-oriented programming | Error tag preservation | Async/await support |

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

### 🚀 **Source Generator - Zero Boilerplate**

```csharp
// Your service returns Result<T>
public async Task<Result<User>> GetUserAsync(int id)
{
    return await Result<int>.Ok(id)
        .Ensure(i => i > 0, "Invalid user ID")
        .BindAsync(async i => await _repository.FindAsync(i))
        .Ensure(u => u != null, new NotFoundError("User", id));
}

// Your controller just returns the Result - auto-converted!
app.MapGet("/users/{id}", async (int id) => 
    await _userService.GetUserAsync(id));

// HTTP responses are automatically generated:
// 200 OK with User data
// 404 Not Found with ProblemDetails
// 400 Bad Request with validation errors
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

### ✅ **Developer Experience**
- **Rich IntelliSense** - Extensive XML documentation
- **Modern C#** - Supports .NET 8, 9, and 10
- **AOT compatible** - Works with NativeAOT and trimming

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
