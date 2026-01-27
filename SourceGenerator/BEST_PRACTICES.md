# Best Practices for Result → IResult Source Generator

This guide covers best practices, patterns, and anti-patterns when using the Result to IResult source generator.

## Table of Contents
- [Design Principles](#design-principles)
- [Error Handling Patterns](#error-handling-patterns)
- [Performance Considerations](#performance-considerations)
- [Testing Strategies](#testing-strategies)
- [Common Pitfalls](#common-pitfalls)
- [Advanced Scenarios](#advanced-scenarios)

---

## Design Principles

### 1. Single Responsibility

**✅ DO:** Keep error types focused and specific

```csharp
// Good - specific error types
public class UserNotFoundError : Error
{
    public int UserId { get; }
    
    public UserNotFoundError(int userId) 
        : base($"User {userId} not found")
    {
        UserId = userId;
        this.WithTag("UserId", userId);
    }
}

public class EmailAlreadyExistsError : Error
{
    public string Email { get; }
    
    public EmailAlreadyExistsError(string email) 
        : base($"Email {email} is already registered")
    {
        Email = email;
        this.WithTag("Email", email);
        this.WithTag("ErrorCode", "EMAIL_DUPLICATE");
    }
}
```

**❌ DON'T:** Create generic "catch-all" errors

```csharp
// Bad - too generic
public class ApplicationError : Error
{
    public ApplicationError(string message) : base(message) { }
}

// Usage becomes unclear
return Result<User>.Fail(new ApplicationError("Something went wrong")); // What went wrong?
```

### 2. Fail Fast with Clear Messages

**✅ DO:** Provide clear, actionable error messages

```csharp
public Result<User> CreateUser(CreateUserRequest request)
{
    // Validate early with clear messages
    if (string.IsNullOrWhiteSpace(request.Email))
        return Result<User>.Fail(
            new ValidationError("Email is required and cannot be empty"));
    
    if (!IsValidEmailFormat(request.Email))
        return Result<User>.Fail(
            new ValidationError($"'{request.Email}' is not a valid email format"));
    
    // Business logic...
}
```

**❌ DON'T:** Use vague error messages

```csharp
// Bad - not actionable
if (string.IsNullOrWhiteSpace(request.Email))
    return Result<User>.Fail("Invalid input"); // What's invalid?

if (!IsValidEmailFormat(request.Email))
    return Result<User>.Fail("Error"); // What error?
```

### 3. Preserve Context with Tags

**✅ DO:** Add rich context via error tags

```csharp
public Result<Order> ProcessOrder(int orderId, ProcessOrderRequest request)
{
    var order = await _repository.GetOrderAsync(orderId);
    
    if (order == null)
    {
        return Result<Order>.Fail(
            new NotFoundError($"Order {orderId} not found")
                .WithTag("OrderId", orderId)
                .WithTag("UserId", request.UserId)
                .WithTag("RequestedAt", DateTime.UtcNow)
                .WithTag("RequestIp", httpContext.Connection.RemoteIpAddress?.ToString())
        );
    }
    
    // Process order...
}
```

**❌ DON'T:** Lose important context

```csharp
// Bad - no context for debugging
if (order == null)
    return Result<Order>.Fail("Order not found");
```

---

## Error Handling Patterns

### Pattern 1: Layered Error Handling

Structure your errors by layer:

```csharp
// Domain Layer - Business logic errors
public class InsufficientStockError : Error
{
    public int ProductId { get; }
    public int RequestedQuantity { get; }
    public int AvailableStock { get; }
    
    public InsufficientStockError(int productId, int requested, int available)
        : base($"Insufficient stock for product {productId}. Requested: {requested}, Available: {available}")
    {
        ProductId = productId;
        RequestedQuantity = requested;
        AvailableStock = available;
        
        this.WithTag("ProductId", productId)
            .WithTag("Requested", requested)
            .WithTag("Available", available);
    }
}

// Application Layer - Use case errors
public class OrderProcessingError : Error
{
    public OrderProcessingError(string message, IError innerError)
        : base(message)
    {
        if (innerError.Tags != null)
        {
            foreach (var tag in innerError.Tags)
                this.WithTag(tag.Key, tag.Value);
        }
    }
}

// API Layer - Maps automatically via source generator
app.MapPost("/orders", async (CreateOrderRequest request, IOrderService service) =>
{
    var result = await service.CreateOrderAsync(request);
    return result; // Auto-converts to appropriate IResult
});
```

### Pattern 2: Railway-Oriented Programming

Chain operations that return Results:

```csharp
public async Task<Result<Order>> CreateOrderAsync(CreateOrderRequest request)
{
    return await ValidateOrderRequest(request)
        .Bind(async req => await CheckProductAvailability(req))
        .Bind(async req => await CalculateTotalPrice(req))
        .Bind(async req => await SaveOrderToDatabase(req))
        .Tap(async order => await SendOrderConfirmationEmail(order));
}

// Each step returns Result<T> - generator handles final conversion
app.MapPost("/orders", async (CreateOrderRequest request, IOrderService service) =>
    await service.CreateOrderAsync(request)); // Auto-converted!
```

### Pattern 3: Aggregate Validation

Collect multiple validation errors:

```csharp
public Result<User> ValidateAndCreateUser(CreateUserRequest request)
{
    var errors = new List<IError>();
    
    if (string.IsNullOrWhiteSpace(request.Email))
        errors.Add(new ValidationError("Email is required")
            .WithTag("Field", "Email"));
    
    if (request.Age < 18)
        errors.Add(new ValidationError("Must be 18 or older")
            .WithTag("Field", "Age")
            .WithTag("MinAge", 18)
            .WithTag("ActualAge", request.Age));
    
    if (errors.Any())
        return Result<User>.Fail(errors.ToArray());
    
    // Create user...
    return Result<User>.Ok(newUser);
}

// Generator creates ProblemDetails with all errors in extensions
```

---

## Performance Considerations

### 1. Avoid Allocations in Hot Paths

**✅ DO:** Reuse error instances for common cases

```csharp
// Static cached errors for frequent cases
public static class CommonErrors
{
    public static readonly NotFoundError UserNotFound = 
        new NotFoundError("User not found");
    
    public static readonly ValidationError EmailRequired = 
        new ValidationError("Email is required");
    
    public static readonly UnauthorizedError Unauthorized = 
        new UnauthorizedError("Authentication required");
}

// Usage
public Result<User> GetUser(int id)
{
    var user = _cache.GetUser(id);
    return user != null 
        ? Result<User>.Ok(user) 
        : Result<User>.Fail(CommonErrors.UserNotFound);
}
```

**❌ DON'T:** Create new error instances unnecessarily

```csharp
// Bad - allocates new error object each time
public Result<User> GetUser(int id)
{
    // ... in a loop called 1000x/sec
    return Result<User>.Fail(new NotFoundError("User not found")); // New allocation!
}
```

### 2. Lazy Tag Evaluation

**✅ DO:** Only add expensive tags when needed

```csharp
public Result<Report> GenerateReport(ReportRequest request)
{
    var report = _generator.Generate(request);
    
    if (report == null)
    {
        var error = new ReportGenerationError("Report generation failed");
        
        // Only add expensive diagnostics in development
        if (_environment.IsDevelopment())
        {
            error.WithTag("RequestDetails", JsonSerializer.Serialize(request))
                 .WithTag("SystemMemory", GC.GetTotalMemory(false))
                 .WithTag("ThreadCount", Process.GetCurrentProcess().Threads.Count);
        }
        
        return Result<Report>.Fail(error);
    }
    
    return Result<Report>.Ok(report);
}
```

### 3. Minimize Async State Machine Overhead

**✅ DO:** Return synchronous Results when possible

```csharp
// Good - no async when not needed
public Result<User> GetCachedUser(int id)
{
    var user = _memoryCache.Get<User>(id);
    return user != null 
        ? Result<User>.Ok(user) 
        : Result<User>.Fail(CommonErrors.UserNotFound);
}

// Only use async when necessary
public async Task<Result<User>> GetUserFromDatabaseAsync(int id)
{
    var user = await _db.Users.FindAsync(id);
    return user != null 
        ? Result<User>.Ok(user) 
        : Result<User>.Fail(CommonErrors.UserNotFound);
}
```

---

## Testing Strategies

### 1. Test Generated Extensions

```csharp
[Fact]
public void ToIResult_WithNotFoundError_Returns404()
{
    // Arrange
    var error = new NotFoundError("User not found")
        .WithTag("UserId", 123);
    var result = Result<User>.Fail(error);

    // Act
    var iresult = result.ToIResult();

    // Assert
    var problem = Assert.IsType<ProblemHttpResult>(iresult);
    Assert.Equal(404, problem.StatusCode);
    Assert.Contains("123", problem.ProblemDetails.Extensions["context"]["UserId"]);
}
```

### 2. Integration Tests with TestServer

```csharp
[Fact]
public async Task GetUser_NotFound_Returns404WithProblemDetails()
{
    // Arrange
    using var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/users/999");

    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    
    var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
    Assert.NotNull(problemDetails);
    Assert.Equal("https://httpstatuses.io/404", problemDetails.Type);
    Assert.Contains("not found", problemDetails.Detail, StringComparison.OrdinalIgnoreCase);
}
```

### 3. Snapshot Testing for ProblemDetails

```csharp
[Fact]
public void ToIResult_ProblemDetails_MatchesSnapshot()
{
    // Arrange
    var error = new ValidationError("Email is required")
        .WithTag("Field", "Email")
        .WithTag("Validator", "Required");
    var result = Result<User>.Fail(error);

    // Act
    var iresult = result.ToIResult();
    var problem = ((ProblemHttpResult)iresult).ProblemDetails;

    // Assert
    var snapshot = JsonSerializer.Serialize(problem, new JsonSerializerOptions 
    { 
        WriteIndented = true 
    });
    
    // Compare with saved snapshot
    _snapshotManager.AssertMatchesSnapshot(snapshot, "ValidationError.json");
}
```

---

## Common Pitfalls

### Pitfall 1: Mixing Result and Exceptions

**❌ DON'T:** Mix Result pattern with exception-based error handling

```csharp
// Bad - inconsistent error handling
app.MapGet("/users/{id}", async (int id, IUserService service) =>
{
    try
    {
        var result = await service.GetUserAsync(id); // Returns Result<User>
        
        if (!result.IsSuccess)
            throw new NotFoundException(); // DON'T throw - defeats purpose of Result!
        
        return Results.Ok(result.Value);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});
```

**✅ DO:** Use Result pattern consistently

```csharp
// Good - consistent Result pattern
app.MapGet("/users/{id}", async (int id, IUserService service) =>
    await service.GetUserAsync(id)); // Generator handles everything
```

### Pitfall 2: Ignoring Error Tags

**❌ DON'T:** Add tags but never use them

```csharp
// Bad - tags added but not useful
return Result<User>.Fail(
    new NotFoundError("User not found")
        .WithTag("Timestamp", DateTime.UtcNow) // Not useful
        .WithTag("MachineName", Environment.MachineName) // Not relevant
        .WithTag("RandomGuid", Guid.NewGuid()) // Why?
);
```

**✅ DO:** Add meaningful, actionable tags

```csharp
// Good - useful debugging information
return Result<User>.Fail(
    new NotFoundError("User not found")
        .WithTag("UserId", userId)
        .WithTag("SearchedBy", currentUserId)
        .WithTag("CorrelationId", httpContext.TraceIdentifier)
);
```

### Pitfall 3: Over-Engineering Error Types

**❌ DON'T:** Create too many specialized error types

```csharp
// Bad - too many specific errors
public class UserNotFoundByIdError : Error { }
public class UserNotFoundByEmailError : Error { }
public class UserNotFoundByUsernameError : Error { }
public class UserNotFoundInCacheError : Error { }
// ... 50 more variations
```

**✅ DO:** Use tags for variations

```csharp
// Good - one type with tags for variations
public class UserNotFoundError : Error
{
    public static UserNotFoundError ById(int id) =>
        new UserNotFoundError($"User {id} not found")
            .WithTag("SearchType", "ById")
            .WithTag("UserId", id);
    
    public static UserNotFoundError ByEmail(string email) =>
        new UserNotFoundError($"User with email {email} not found")
            .WithTag("SearchType", "ByEmail")
            .WithTag("Email", email);
}
```

---

## Advanced Scenarios

### Scenario 1: Custom Status Code Mapping

```csharp
// Define custom error with specific HTTP semantics
[MapToHttpStatus(429)] // Rate limit exceeded
public class RateLimitExceededError : Error
{
    public int RetryAfterSeconds { get; }
    
    public RateLimitExceededError(int retryAfter) 
        : base($"Rate limit exceeded. Retry after {retryAfter} seconds")
    {
        RetryAfterSeconds = retryAfter;
        this.WithTag("RetryAfter", retryAfter)
            .WithTag("RateLimitType", "PerUser");
    }
}

// Configure in generator
[assembly: GenerateResultExtensions(
    CustomErrorMappings = new[] 
    { 
        "RateLimitExceededError:429" 
    }
)]
```

### Scenario 2: Conditional Error Details

```csharp
public class OrderService
{
    private readonly IConfiguration _config;
    
    public Result<Order> CreateOrder(CreateOrderRequest request)
    {
        if (!ValidateStock(request.Items))
        {
            var error = new InsufficientStockError("Insufficient stock");
            
            // Add sensitive details only in development
            if (_config.GetValue<bool>("ShowDetailedErrors"))
            {
                error.WithTag("RequestedItems", JsonSerializer.Serialize(request.Items))
                     .WithTag("CurrentStock", GetStockLevels());
            }
            
            return Result<Order>.Fail(error);
        }
        
        // ...
    }
}
```

### Scenario 3: Correlation IDs and Distributed Tracing

```csharp
public class TrackedError : Error
{
    public TrackedError(string message, HttpContext httpContext) 
        : base(message)
    {
        this.WithTag("TraceId", httpContext.TraceIdentifier)
            .WithTag("CorrelationId", httpContext.Request.Headers["X-Correlation-ID"].ToString())
            .WithTag("RequestPath", httpContext.Request.Path)
            .WithTag("Timestamp", DateTime.UtcNow.ToString("O"));
    }
}

// Usage
app.MapPost("/orders", async (CreateOrderRequest request, IOrderService service, HttpContext httpContext) =>
{
    var result = await service.CreateOrderAsync(request);
    
    if (!result.IsSuccess)
    {
        // Tags automatically included in ProblemDetails for tracing
        _logger.LogError(
            "Order creation failed. TraceId: {TraceId}", 
            httpContext.TraceIdentifier);
    }
    
    return result;
});
```

---

## Summary Checklist

### ✅ DO
- [ ] Use specific, actionable error messages
- [ ] Add meaningful tags for debugging
- [ ] Configure custom error mappings for your domain
- [ ] Test generated extensions thoroughly
- [ ] Use Result pattern consistently throughout your API
- [ ] Document your error types and their meanings
- [ ] Follow RFC 7807 ProblemDetails standards

### ❌ DON'T
- [ ] Mix Result pattern with exceptions in endpoints
- [ ] Create overly generic error types
- [ ] Add tags that aren't useful for debugging
- [ ] Forget to configure CustomErrorMappings
- [ ] Ignore error context in production issues
- [ ] Return vague error messages to users
- [ ] Over-engineer with too many error types

---

**Remember:** The goal is to make error handling:
1. **Consistent** - Same patterns everywhere
2. **Informative** - Rich context for debugging
3. **Type-safe** - Compile-time guarantees
4. **Maintainable** - Easy to understand and modify
5. **Performant** - Minimal overhead

Happy coding! 🚀
