# Quick Start Guide - REslava.Result v1.10.0

Welcome! This guide gets you up and running with REslava.Result v1.10.0 and its revolutionary OneOf integration.

## 🚀 Quick Start (30 seconds)

### 📦 Installation

```bash
# Core functional programming library
dotnet add package REslava.Result

# ASP.NET integration + OneOf extensions (v1.10.0 unified package)
dotnet add package REslava.Result.SourceGenerators
```

### 🔧 Project Setup

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  </PropertyGroup>

  <ItemGroup>
    <!-- REslava.Result packages -->
    <PackageReference Include="REslava.Result" Version="1.10.0" />
    <PackageReference Include="REslava.Result.SourceGenerators" Version="1.10.0" />
  </ItemGroup>
</Project>
```

### ✨ Magic Happens

```csharp
// Add this to your Program.cs
using REslava.Result.SourceGenerators;

// 🆕 v1.10.0: OneOf extensions work automatically with smart auto-detection!
// No additional setup required for external OneOf library
```

### 🎯 **Want to see it in action?**
Check out our **[Main README](README.md)** for complete examples and **[samples/ASP.NET](samples/ASP.NET/README.md)** for live comparisons!

---

## 🌐 Web API Integration

### ❌ BEFORE: Manual conversion everywhere
```csharp
app.MapGet("/users/{id}", async (int id, IUserService service) =>
{
    var result = await service.GetUserAsync(id);
    return result.Match(
        onSuccess: user => Results.Ok(user),
        onFailure: errors => Results.Problem(...)
    );
});
```

### ✅ AFTER: Return Result<T> directly!
```csharp
app.MapGet("/users/{id}", async (int id, IUserService service) =>
{
    return await service.GetUserAsync(id); // Auto-converts to HTTP response!
});
```

### 🆕 v1.10.0: OneOf Extensions Also Work!
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

## 🧠 Core Library Usage

### Result Pattern Basics
```csharp
// Success cases
Result<string>.Ok("hello")
Result<int>.Ok(42)

// Failure cases  
Result<string>.Fail("Something went wrong")
Result<User>.Fail(new UserNotFoundError(id))

// Chaining operations
var result = await Result<CreateUserRequest>.Ok(request)
    .Ensure(r => !string.IsNullOrWhiteSpace(r.Email), "Email required")
    .EnsureAsync(async r => !await EmailExistsAsync(r.Email), "Email already exists")
    .BindAsync(async r => await CreateUserAsync(r))
    .WithSuccess("User created successfully");
```

### Advanced Patterns
```csharp
// Maybe<T> for safe null handling
Maybe<User> user = GetUserFromCache(id);
string email = user
    .Select(u => u.Email)
    .Filter(e => e.Contains("@"))
    .ValueOrDefault("no-reply@example.com");

// OneOf discriminated unions
OneOf<ValidationError, User> result = ValidateAndCreateUser(request);
return result.Match(
    case1: error => BadRequest(error),
    case2: user => Ok(user)
);
```

---

## 🎯 Smart Auto-Detection (v1.10.0)

**Zero Configuration Required - It Just Works!**

### ✅ What Gets Detected Automatically
- **REslava.Result** types → ResultToIResult extensions
- **External OneOf** types → OneOf2ToIResult extensions  
- **Three-way OneOf** types → OneOf3ToIResult extensions
- **Your existing code** → No changes needed

### 🔧 Conflict Prevention
- **Setup Detection**: Automatically detects your OneOf setup
- **Namespace Isolation**: Different extension classes prevent conflicts
- **Zero Compilation Errors**: Perfect coexistence guaranteed

---

## 📚 Next Steps

### 🎯 Choose Your Learning Path

| I want to... | 📖 Start Here | 🎯 What You'll Learn |
|-------------|---------------|---------------------|
| **Build Web APIs** | [Main README - ASP.NET Integration](README.md#-the-transformation-70-90-less-code) | Auto-conversion, OneOf extensions |
| **Learn Functional Programming** | [Main README - Core Library](README.md#-reslavaresult-core-library) | Result pattern, Maybe, OneOf |
| **Create Custom Generators** | [Custom Generator Guide](docs/how-to-create-custom-generator.md) | Build your own generators |
| **See Live Examples** | [ASP.NET Samples](samples/ASP.NET/README.md) | Compare pure .NET vs REslava.Result |
| **Run Tests** | [Main README - Testing](README.md#-testing--quality-assurance) | 1902+ tests, CI/CD |

### 🧪 Test Your Setup

```bash
# Clone and run samples
git clone https://github.com/reslava/REslava.Result.git
cd REslava.Result/samples/OneOfTest.Api
dotnet run

# Run the test suite
cd ../../tests/REslava.Result.SourceGenerators.Tests
dotnet test --verbosity normal
```

---

## 🏆 What Makes v1.10.0 Special?

### 🚀 **OneOf Integration Breakthrough**
- **External OneOf Library Support** - Works with OneOf package v3.0.26
- **Three-Type OneOf Support** - OneOf<T1,T2,T3> with intelligent HTTP mapping
- **Smart Auto-Detection** - Zero configuration required

### 🧠 **Advanced Patterns**
- **Maybe<T>** - Safe null handling
- **Validation Framework** - Declarative validation with rich context
- **Functional Composition** - Build complex operations from simple functions

### 📊 **Quality Assurance**
- **1902+ Tests Passing** - Comprehensive test coverage
- **95%+ Code Coverage** - Production-ready reliability
- **SOLID Architecture** - Clean, maintainable code

---

## 🎉 You're Ready!

**🚀 Start building with 70-90% less boilerplate code!**

- **Web APIs**: Return Result<T> and OneOf<T> directly
- **Libraries**: Use Result pattern for type-safe error handling  
- **Advanced Apps**: Leverage Maybe, OneOf, and functional composition
- **Custom Generators**: Extend the platform with your own generators

**Welcome to the future of .NET development!** ✨
