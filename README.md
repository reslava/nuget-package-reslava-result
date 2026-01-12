# REslava.Result

<div align="center">

![.NET](https://img.shields.io/badge/.NET-512BD4?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?&logo=csharp&logoColor=white)
[![GitHub contributors](https://img.shields.io/github/contributors/reslava/REslava.Result)](https://GitHub.com/reslava/REslava.Result/graphs/contributors/) 
[![GitHub Stars](https://img.shields.io/github/stars/reslava/REslava.Result)](https://github.com/reslava/REslava.Result/stargazers) 
[![GitHub license](https://img.shields.io/github/license/reslava/REslava.Result)](https://github.com/reslava/REslava.Result/blob/master/LICENSE.txt)

**A clean, fluent, and type-safe approach to error handling in C#**

[Features](#-features) • [Quick Start](#-quick-start) • [Documentation](#-documentation) • [Design](#-architecture-and-design) • [Contributing](#-contributing)

</div>

---

## 🎯 Why REslava.Result?

Stop throwing exceptions for expected failures. **REslava.Result** brings functional error handling to .NET with a beautifully simple, fluent API that makes your code more predictable, maintainable, and expressive.

```csharp
// Before: Exception-based error handling ❌
public User CreateUser(string email, int age)
{
    if (age < 18) throw new ValidationException("Must be 18+");
    if (!IsValidEmail(email)) throw new ValidationException("Invalid email");
    return new User(email, age);
}

// After: Result pattern ✅
public Result<User> CreateUser(string email, int age)
{
    return Result<string>.Ok(email)
        .EnsureNotNull("Email is required")
        .Ensure(e => IsValidEmail(e), "Invalid email format")
        .Bind(e => Result<int>.Ok(age)
            .Ensure(a => a >= 18, "Must be 18 or older")
            .Map(a => new User(e, a)));
}
```

## ✨ Features

### 🎨 Fluent & Expressive API

Chain operations naturally with a symmetrical, intuitive API:

```csharp
var result = Result<User>.Ok(user)
    .WithSuccess("User validated")
    .Tap(u => _logger.LogInfo($"Processing user {u.Id}"))
    .Bind(u => SaveToDatabase(u))
    .Map(u => new UserDto(u))
    .Match(
        onSuccess: dto => Ok(dto),
        onFailure: errors => BadRequest(errors)
    );
```

### 🔄 Powerful Transformations

**Map** - Transform success values:
```csharp
Result<int>.Ok(42)
    .Map(x => x * 2)           // Result<int> with value 84
    .Map(x => x.ToString())    // Result<string> with value "84"
    .Map(x => $"Value: {x}");  // Result<string> with value "Value: 84"
```

**Bind** - Chain operations that return Results (preserves success reasons):
```csharp
Result<string>.Ok("user@example.com")
    .WithSuccess("Email received")
    .Bind(email => ValidateEmail(email))      // Returns Result<Email>
    .Bind(email => FindUser(email))           // Returns Result<User>
    .Bind(user => AuthenticateUser(user));    // Returns Result<Session>
// All success reasons preserved through the chain!
```

**Tap** - Execute side effects without breaking the chain:
```csharp
Result<Order>.Ok(order)
    .Tap(o => _logger.LogInfo($"Processing order {o.Id}"))
    .Tap(o => _metrics.RecordOrder(o))
    .Bind(o => ProcessPayment(o))
    .Tap(o => SendConfirmationEmail(o));
```

### ✅ Comprehensive Validation

**Single validations:**
```csharp
Result<int>.Ok(age)
    .Ensure(a => a >= 18, "Must be 18 or older")
    .Ensure(a => a <= 120, "Age seems unrealistic");
```

**Multiple validations** (collects all failures):
```csharp
Result<string>.Ok(password)
    .Ensure(
        (p => p.Length >= 8, new Error("Min 8 characters")),
        (p => p.Any(char.IsDigit), new Error("Requires digit")),
        (p => p.Any(char.IsUpper), new Error("Requires uppercase"))
    );
// If validation fails: Result with ALL three errors
```

**Null safety:**
```csharp
Result<User>.Ok(user)
    .EnsureNotNull("User cannot be null");
```

### 🛡️ Safe Exception Handling

Convert exceptions into Results automatically:

```csharp
// Synchronous
var result = Result<User>.Try(() => 
    JsonSerializer.Deserialize<User>(json)
);

// Asynchronous
var result = await Result<User>.TryAsync(async () => 
    await _httpClient.GetFromJsonAsync<User>(url)
);

// With custom error handling
var result = Result.Try(
    () => File.Delete(path),
    ex => new Error($"Failed to delete: {ex.Message}")
        .WithTags("FilePath", path)
        .WithTags("ErrorCode", "FILE_DELETE_FAILED")
);
```

### 🏷️ Rich Error Context

Add context to errors with tags and metadata:

```csharp
var error = new Error("Validation failed")
    .WithTags("Field", "Email")
    .WithTags("Value", email)
    .WithTags("Timestamp", DateTime.UtcNow)
    .WithTags("UserId", userId);

Result<User>.Fail(error);

// Access later
if (result.IsFailed)
{
    var field = result.Errors[0].Tags["Field"];     // "Email"
    var userId = result.Errors[0].Tags["UserId"];   // userId value
}
```

### 🎭 Pattern Matching

Handle success and failure cases elegantly:

```csharp
// With return value
var response = result.Match(
    onSuccess: user => Ok(new UserDto(user)),
    onFailure: errors => BadRequest(new { errors })
);

// Side effects only
result.Match(
    onSuccess: user => Console.WriteLine($"Welcome {user.Name}"),
    onFailure: errors => Console.WriteLine($"Failed: {string.Join(", ", errors)}")
);
```

### 🔀 Implicit Conversions

Natural, type-safe conversions:

```csharp
// From value to Result
Result<int> result = 42;  // Implicit conversion

// From error to Result
Result<User> result = new Error("Not found");

// From multiple errors
Result<Data> result = new[] {
    new Error("Error 1"),
    new Error("Error 2")
};

// In return statements
public Result<int> GetAge(User user)
{
    if (user == null) return new Error("User not found");
    if (user.Age < 0) return new Error("Invalid age");
    return user.Age;  // Clean and readable!
}
```

### 🎨 Custom Error Types

#### Simple Approach (Recommended for Most Cases)

Inherit from `Error` to add domain-specific constructors and default tags:

```csharp
public class ValidationError : Error
{
    public ValidationError(string field, string message) 
        : base($"{field}: {message}")
    {
        WithTags("Field", field)
            .WithTags("ErrorType", "Validation")
            .WithTags("Severity", "Warning");
    }
}

public class NotFoundError : Error
{
    public NotFoundError(string entityType, string id) 
        : base($"{entityType} with id '{id}' not found")
    {
        WithTags("EntityType", entityType)
            .WithTags("EntityId", id)
            .WithTags("ErrorType", "NotFound")
            .WithTags("StatusCode", 404);
    }
}

// Usage
return Result<User>.Fail(new NotFoundError("User", userId));
return Result<User>.Fail(new ValidationError("Email", "Invalid format"));
```
#### Advanced Approach (For Custom Fluent Methods)

Use direct CRTP inheritance when you need error-type-specific fluent methods:

```csharp
public class DatabaseError : Reason<DatabaseError>, IError
{
    public DatabaseError() : base("Database error occurred") { }

    // Custom fluent method specific to DatabaseError
    public DatabaseError WithQuery(string query)
    {
        WithTags("Query", query);
        return this; // Returns DatabaseError, not Error
    }

    // Custom fluent method
    public DatabaseError WithRetryCount(int count)
    {
        WithTags("RetryCount", count);
        return this;
    }
}

// Usage with custom fluent API
var error = new DatabaseError()
    .WithQuery("SELECT * FROM Users")
    .WithRetryCount(3)
    .WithTags("Server", "localhost");
```
**Key Difference:**

- `ValidationError : Error` → Inherits `WithTags()` that returns `Error`
- `DatabaseError : Reason<DatabaseError>` → Gets `WithTags()` that returns `DatabaseError`

**Which to choose?**

- ✅ **Inherit from `Error`** (Simple) - 95% of cases
- ✅ **Inherit from `Reason<T>`** (Advanced) - Only when you need custom fluent methods

## 📦 Quick Start

### Installation

```bash
dotnet add package REslava.Result
```

### Basic Usage

```csharp
using REslava.Result;

// Success case
var success = Result<int>.Ok(42);
Console.WriteLine(success.Value); // 42

// Failure case
var failure = Result<int>.Fail("Something went wrong");
Console.WriteLine(failure.IsFailed); // true
Console.WriteLine(failure.Errors[0].Message); // "Something went wrong"
```

### Real-World Example: User Registration

```csharp
public async Task<Result<UserDto>> RegisterUser(RegisterRequest request)
{
    return await Result<string>.Ok(request.Email)
        .EnsureNotNull("Email is required")
        .Ensure(e => IsValidEmail(e), "Invalid email format")
        .Ensure(e => !await _db.Users.AnyAsync(u => u.Email == e), 
            "Email already registered")
        .BindAsync(async email => await Result<string>.Ok(request.Password)
            .Ensure(
                (p => p.Length >= 8, new Error("Min 8 characters")),
                (p => p.Any(char.IsDigit), new Error("Requires digit")),
                (p => p.Any(char.IsUpper), new Error("Requires uppercase"))
            )
            .MapAsync(async password => new User 
            { 
                Email = email, 
                PasswordHash = await _hasher.Hash(password) 
            }))
        .TapAsync(async user => await _db.Users.AddAsync(user))
        .TapAsync(async _ => await _db.SaveChangesAsync())
        .Tap(user => _logger.LogInfo($"User registered: {user.Email}"))
        .MapAsync(async user => await SendWelcomeEmail(user))
        .Map(user => new UserDto(user));
}
```

### API Controller Example

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        return await _userService.GetUserById(id)
            .Match(
                onSuccess: user => Ok(user),
                onFailure: errors => errors[0].Tags.ContainsKey("StatusCode") 
                    ? StatusCode((int)errors[0].Tags["StatusCode"], new { errors })
                    : BadRequest(new { errors })
            );
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        return await _userService.CreateUser(request)
            .Match(
                onSuccess: user => CreatedAtAction(nameof(GetUser), 
                    new { id = user.Id }, user),
                onFailure: errors => BadRequest(new { errors })
            );
    }
}
```

## 📐 Architecture and Design 

### CRTP (Curiously Recurring Template Pattern)

REslava.Result leverages CRTP to achieve type-safe fluent interfaces without code duplication:

```csharp
public abstract class Reason<TReason> : Reason
    where TReason : Reason<TReason>
{
    public TReason WithMessage(string message) 
    { 
        Message = message; 
        return (TReason)this; 
    }
    
    public TReason WithTags(string key, object value) 
    { 
        Tags.Add(key, value); 
        return (TReason)this; 
    }
}

public class Error : Reason<Error>, IError { }
public class Success : Reason<Success>, ISuccess { }
```

**Benefits of CRTP:**
- ✅ **Type-safe fluent chaining**: Each method returns the correct derived type
- ✅ **Zero code duplication**: Shared behavior in base class
- ✅ **Compile-time safety**: No runtime type checking needed
- ✅ **Natural API**: `error.WithTags(...).WithMessage(...)` works perfectly

**Why this matters:**
```csharp
// Without CRTP (returns base type, breaks fluent chain)
Reason reason = error.WithMessage("test");  // Can't call Error-specific methods

// With CRTP (returns derived type, maintains fluency)
Error error = new Error().WithMessage("test").WithTags("Code", 404);  // ✅ Perfect!
```

### Design Principles

**1. Immutability by Default**
```csharp
// Result values are immutable
var result = Result<int>.Ok(42);
result.Map(x => x * 2);  // Returns NEW result, original unchanged
```

**2. Railway-Oriented Programming**
```csharp
// Success path continues, failure short-circuits
Result<string>.Ok(input)
    .Map(x => x.Trim())          // Executes
    .Ensure(x => x.Length > 0, "Empty")  // Executes
    .Bind(x => FindUser(x))      // If this fails...
    .Map(u => u.Email)           // ...this doesn't execute
    .Tap(e => SendEmail(e));     // ...nor does this
```

**3. Explicit Over Implicit**
```csharp
// Value access is explicit and safe
if (result.IsSuccess)
{
    var value = result.Value;  // ✅ Safe: throws if failed
    var value = result.ValueOrDefault;  // ✅ Safe: returns default if failed
}
```

**4. Composition Over Inheritance**

The library uses interface composition for flexibility:

```csharp
// Reason system is interface-based
public interface IReason { }
public interface IError : IReason { }
public interface ISuccess : IReason { }

// Base implementation using CRTP
public abstract class Reason<TReason> : Reason where TReason : Reason<TReason>
{
    public TReason WithMessage(string message) { /*...*/ }
    public TReason WithTags(string key, object value) { /*...*/ }
}

// Concrete implementations
public class Error : Reason<Error>, IError { }
public class Success : Reason<Success>, ISuccess { }

// Easy to extend
public class CustomError : Reason<CustomError>, IError { }

//This design allows:
//✅ Adding new reason types without modifying existing code
//✅ Type-safe fluent interfaces via CRTP
//✅ Interface-based polymorphism for reasoning
```


### UML Class Diagram

```
┌─────────────────┐
│   <<interface>> │
│     IReason     │
│─────────────────│
│ + Message       │
│ + Tags          │
└────────┬────────┘
         │
    ┌────┴─────┐
    │          │
┌───▼────┐ ┌──▼──────┐
│ IError │ │ISuccess │
└───┬────┘ └───┬─────┘
    │          │
┌───▼──────────▼───┐
│  Reason<TReason> │  ← CRTP Pattern
│──────────────────│
│ + WithMessage()  │
│ + WithTags()     │
└────┬─────────────┘
     │
  ┌──┴────┐
  │       │
┌─▼───┐ ┌─▼──────┐
│Error│ │Success │
└─────┘ └────────┘
```
 
[See full UML diagram](docs/UML.md) • [See simple version](docs/UML-simple.md) • [See simple in PNG format](images/UML-simple.png)

## 🎓 Advanced Usage

### Async Operations

```csharp
// MapAsync - Transform asynchronously
var result = await Result<int>.Ok(userId)
    .MapAsync(async id => await _db.Users.FindAsync(id))
    .MapAsync(async user => await EnrichUserData(user));

// BindAsync - Chain async operations
var result = await Result<string>.Ok(email)
    .BindAsync(async e => await ValidateEmailAsync(e))
    .BindAsync(async e => await CreateUserAsync(e));

// TryAsync - Catch exceptions in async operations
var result = await Result<Data>.TryAsync(
    async () => await _api.GetDataAsync(id)
);
```

### Complex Validation Pipelines

```csharp
public Result<Order> ValidateOrder(OrderRequest request)
{
    return Result<OrderRequest>.Ok(request)
        .EnsureNotNull("Request cannot be null")
        .Ensure(
            (r => r.Items?.Count > 0, 
                new Error("Order must have items")),
            (r => r.Total > 0, 
                new Error("Total must be positive")),
            (r => r.Total <= 10000, 
                new Error("Order exceeds maximum")),
            (r => !string.IsNullOrEmpty(r.CustomerId), 
                new Error("Customer ID required"))
        )
        .Bind(r => ValidateInventory(r))
        .Bind(r => CheckCustomerCredit(r))
        .Map(r => new Order(r));
}
```

### Working with Multiple Results

```csharp
// Aggregate results
public Result<Report> GenerateReport(List<int> dataIds)
{
    var results = dataIds
        .Select(id => Result<Data>.Try(() => LoadData(id)))
        .ToList();

    var failures = results.Where(r => r.IsFailed).ToList();
    if (failures.Any())
    {
        var allErrors = failures.SelectMany(f => f.Errors);
        return Result<Report>.Fail(allErrors);
    }

    var data = results.Select(r => r.Value).ToList();
    return Result<Report>.Ok(new Report(data));
}
```

### Custom Success Tracking

```csharp
var result = Result<User>.Ok(user)
    .WithSuccess("User validated")
    .WithSuccess("Email verified")
    .Tap(u => _db.Add(u))
    .WithSuccess("User saved to database")
    .Tap(u => _cache.Set(u))
    .WithSuccess("User cached");

// Audit trail
foreach (var success in result.Successes)
{
    _logger.LogInfo(success.Message);
}
```

## 🔮 Roadmap

Planned features for upcoming releases:

- [ ] **Result Aggregation**: Combine multiple results with smart error aggregation
- [ ] **Async LINQ Extensions**: `SelectAsync`, `WhereAsync` for IEnumerable<Result<T>>
- [ ] **Validation Rules Engine**: Declarative validation with fluent rule builder
- [ ] **Retry Policies**: Built-in retry mechanisms for transient failures
- [ ] **Circuit Breaker**: Fault tolerance patterns integration
- [ ] **Serialization Support**: JSON/XML serialization for Result types
- [ ] **Source Generators**: Compile-time code generation for common patterns
- [ ] **ASP.NET Core Integration**: Middleware and ActionFilters for Result-based APIs
- [ ] **FluentValidation Integration**: Seamless integration with FluentValidation
- [ ] **SignalR Support**: Result pattern for real-time communication

Want to contribute? Check out our [Contributing Guide](CONTRIBUTING.md)!

## 🤝 Contributing

Contributions are welcome! Please read our [Contributing Guidelines](CONTRIBUTING.md) and [Code of Conduct](CODE_OF_CONDUCT.md).

### Development Workflow

```bash
# Clone the repository
git clone https://github.com/reslava/nuget-package-reslava-result.git
cd nuget-package-reslava-result

# Install dependencies
npm install
dotnet restore

# Create feature branch
git checkout dev
git checkout -b feature/my-feature

# Make changes and commit
git add .
npm run commit  # Uses Commitizen for conventional commits

# Run tests
dotnet test

# See detailed workflow
```
📖 See [QUICK-START.md](QUICK-START.md) for complete development guide.

## 📚 Documentation

- [Quick Start Guide](QUICK-START.md)
- [Branching Strategy](BRANCHING-STRATEGY.md)
- [Full UML Diagram](docs/UML.md)
- [Simplified UML](docs/UML-simple.md)

## 📕 References & Inspiration

This library was inspired by excellent work in the .NET community:

- [ErrorOr](https://github.com/amantinband/error-or) - Discriminated unions for error handling
- [FluentResults](https://github.com/altmann/FluentResults) - Result object implementation
- [OneOf](https://github.com/mcintyre321/OneOf) - Discriminated unions in C#

Helpful articles:
- [The Result Pattern in C#](https://www.milanjovanovic.tech/blog/functional-error-handling-in-dotnet-with-the-result-pattern)
- [Working with the Result Pattern](https://andrewlock.net/series/working-with-the-result-pattern/)
- [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

---

<div align="center">

**⭐ Star this repository if you find it useful!**

Made with ❤️ by [Rafa Eslava](https://github.com/reslava) for developers

[Report Bug](https://github.com/reslava/nuget-package-reslava-result/issues) • [Request Feature](https://github.com/reslava/nuget-package-reslava-result/issues) • [Discussions](https://github.com/reslava/nuget-package-reslava-result/discussions)

</div>
