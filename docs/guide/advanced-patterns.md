# Advanced Patterns Guide

**🧠 Level up your functional programming with Maybe, OneOf, and advanced Result patterns!**

This guide explores the powerful advanced patterns in REslava.Result that enable sophisticated error handling, null safety, and discriminated unions.

---

## 🎯 When to Use Advanced Patterns

| Pattern | Best For | When to Avoid |
|---------|-----------|---------------|
| **Maybe\<T>** | Optional values, cache lookups, nullable operations | You need error details |
| **OneOf\<T1, T2>** | Error/success alternatives, binary states | You have >2 states |
| **OneOf\<T1, T2, T3>** | Complex state machines, API responses | You have >3 states |
| **Result + LINQ** | Complex data pipelines | Simple operations |
| **Pipeline Integration** | Mixed architectures | Single pattern projects |

---

## 📖 Maybe\<T> - Safe Null Handling

### 🎯 The Problem

```csharp
// ❌ Traditional null checking
User? user = GetUserFromCache(id);
if (user != null)
{
    var email = user.Email;
    if (email != null && email.Contains("@"))
    {
        return email.ToUpper();
    }
}
return "no-reply@example.com";

// ❌ Null reference exceptions waiting to happen
var email = GetUserFromCache(id).Email.ToUpper(); // Crash!
```

### ✅ The Maybe Solution

```csharp
// ✅ Safe, fluent, and expressive
Maybe<User> user = GetUserFromCache(id);
var email = user
    .Select(u => u.Email)
    .Filter(email => email.Contains("@"))
    .Map(email => email.ToUpper())
    .ValueOrDefault("no-reply@example.com");
```

### 🚀 Basic Usage

#### Creation

```csharp
// From existing value
Maybe<User> user = Maybe<User>.FromValue(database.GetUser(id));

// Empty Maybe
Maybe<User> empty = Maybe<User>.None;

// From nullable
Maybe<User> fromNullable = user?.ToMaybe() ?? Maybe<User>.None;
```

#### Transformation

```csharp
Maybe<string> email = user
    .Select(u => u.Email)                    // Transform User -> string
    .Filter(e => e.Contains("@"))            // Filter valid emails
    .Map(e => e.ToUpper());                  // Transform string -> string
```

#### Pattern Matching

```csharp
string message = user.Match(
    some: u => $"User found: {u.Name}",
    none: () => "User not found"
);
```

### 🎯 Real-World Example

#### Configuration Management

```csharp
public class ConfigurationService
{
    public Maybe<string> GetSetting(string key)
    {
        return _configuration[key]?.ToMaybe() ?? Maybe<string>.None;
    }

    public Maybe<int> GetTimeout()
    {
        return GetSetting("Timeout")
            .Select(int.Parse)
            .Filter(timeout => timeout > 0);
    }

    public Maybe<Uri> GetApiUrl()
    {
        return GetSetting("ApiUrl")
            .Filter(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .Map(url => new Uri(url));
    }
}

// Usage
var timeout = configService.GetTimeout().ValueOrDefault(30);
var apiUrl = configService.GetApiUrl().ValueOrDefault(new Uri("https://api.example.com"));
```

#### Cache Operations

```csharp
public class CacheService
{
    public Maybe<User> GetUserFromCache(int id)
    {
        return _cache.Get<User>($"user:{id}")?.ToMaybe() ?? Maybe<User>.None;
    }

    public async Task<Result<User>> GetUserWithFallback(int id)
    {
        return await GetUserFromCache(id)
            .ToResult(() => new NotFoundError("User", id))
            .Recover(async error => await _database.GetUserAsync(id));
    }
}
```

---

## 🎯 OneOf\<T1, T2> - Type-Safe Alternatives

### 🎯 The Problem

```csharp
// ❌ Using exceptions for flow control
try
{
    var user = await GetUserFromApi(id);
    return ProcessUser(user);
}
catch (ApiException ex)
{
    return HandleApiError(ex);
}

// ❌ Using null/flags for state
object result = GetUserFromApi(id);
if (result is Exception error)
{
    // Handle error
}
else if (result is User user)
{
    // Process user
}
```

### ✅ The OneOf Solution

```csharp
// ✅ Type-safe, explicit alternatives
OneOf<ApiError, User> result = await GetUserFromApi(id);

return result.Match(
    case1: error => HandleApiError(error),
    case2: user => ProcessUser(user)
);
```

### 🚀 Basic Usage

#### Creation

```csharp
// From T1 value
OneOf<Error, User> result = OneOf<Error, User>.FromT2(new User { Name = "John" });

// From T2 value  
OneOf<Error, User> result = OneOf<Error, User>.FromT1(new Error("API failed"));

// From Result
OneOf<ValidationError, User> oneOf = userResult.ToOneOf(
    error => new ValidationError(error.Message)
);
```

#### Pattern Matching

```csharp
string message = apiResult.Match(
    case1: error => $"API Error: {error.Message}",
    case2: user => $"User: {user.Name}"
);
```

#### Functional Operations

```csharp
OneOf<Error, UserDto> dto = apiResult
    .MapT2(user => user.ToDto())
    .BindT2(dto => ValidateDto(dto));
```

### 🎯 Real-World Example

#### API Client

```csharp
public class UserApiClient
{
    public async Task<OneOf<ApiError, User>> GetUserAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/users/{id}");
            
            if (response.StatusCode == HttpStatusCode.NotFound)
                return OneOf<ApiError, User>.FromT1(new NotFoundError("User", id));
                
            if (!response.IsSuccessStatusCode)
                return OneOf<ApiError, User>.FromT1(new ApiError(response));
                
            var user = await response.Content.ReadFromJsonAsync<User>();
            return OneOf<ApiError, User>.FromT2(user!);
        }
        catch (HttpRequestException ex)
        {
            return OneOf<ApiError, User>.FromT1(new NetworkError(ex.Message));
        }
    }

    public async Task<OneOf<ValidationError, User>> CreateUserAsync(CreateUserRequest request)
    {
        // Validate first
        var validationResult = ValidateRequest(request);
        if (validationResult.IsFailed)
            return OneOf<ValidationError, User>.FromT1(
                new ValidationError(validationResult.Errors));

        // Call API
        var apiResult = await PostToApiAsync("/users", request);
        
        return apiResult.Match(
            case1: error => OneOf<ValidationError, User>.FromT1(
                new ValidationError(error.Message)),
            case2: user => OneOf<ValidationError, User>.FromT2(user)
        );
    }
}
```

#### Business Logic

```csharp
public class OrderService
{
    public OneOf<InsufficientStockError, Order> CreateOrder(CreateOrderRequest request)
    {
        // Check inventory
        var inventory = GetInventory(request.ProductId);
        if (inventory.Stock < request.Quantity)
            return OneOf<InsufficientStockError, Order>.FromT1(
                new InsufficientStockError(request.ProductId, inventory.Stock));

        // Calculate pricing
        var pricing = CalculatePricing(request);
        if (pricing.IsFailed)
            return OneOf<InsufficientStockError, Order>.FromT1(
                new InsufficientStockError("Pricing calculation failed"));

        // Create order
        var order = new Order
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            Price = pricing.Value,
            CreatedAt = DateTime.UtcNow
        };

        return OneOf<InsufficientStockError, Order>.FromT2(order);
    }
}
```

---

## 🎲 OneOf\<T1, T2, T3> - Complex State Handling

### 🎯 The Problem

```csharp
// ❌ Complex nested conditions
if (response.IsSuccess)
{
    // Success path
}
else if (response.StatusCode == 400)
{
    // Client error path  
}
else if (response.StatusCode == 500)
{
    // Server error path
}
else
{
    // Unknown error path
}
```

### ✅ The Three-Way Solution

```csharp
// ✅ Explicit, type-safe three-way matching
OneOf<Success, ClientError, ServerError> result = await CallApi(endpoint);

return result.Match(
    case1: success => HandleSuccess(success),
    case2: clientError => HandleClientError(clientError), 
    case3: serverError => HandleServerError(serverError)
);
```

### 🚀 Real-World Example

#### API Response Handler

```csharp
public class ApiResponseHandler
{
    public async Task<OneOf<Success, ClientError, ServerError>> CallApiAsync(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            
            return response.StatusCode switch
            {
                HttpStatusCode.OK => OneOf<Success, ClientError, ServerError>.FromT1(
                    new Success(await response.Content.ReadAsStringAsync())),
                    
                HttpStatusCode.BadRequest => OneOf<Success, ClientError, ServerError>.FromT2(
                    new ClientError("Bad request", await response.Content.ReadAsStringAsync())),
                    
                HttpStatusCode.Unauthorized => OneOf<Success, ClientError, ServerError>.FromT2(
                    new ClientError("Unauthorized", await response.Content.ReadAsStringAsync())),
                    
                HttpStatusCode.NotFound => OneOf<Success, ClientError, ServerError>.FromT2(
                    new ClientError("Not found", await response.Content.ReadAsStringAsync())),
                    
                HttpStatusCode.InternalServerError => OneOf<Success, ClientError, ServerError>.FromT3(
                    new ServerError("Internal server error", await response.Content.ReadAsStringAsync())),
                    
                HttpStatusCode.ServiceUnavailable => OneOf<Success, ClientError, ServerError>.FromT3(
                    new ServerError("Service unavailable", await response.Content.ReadAsStringAsync())),
                    
                _ => OneOf<Success, ClientError, ServerError>.FromT3(
                    new ServerError($"Unexpected status: {response.StatusCode}", 
                        await response.Content.ReadAsStringAsync()))
            };
        }
        catch (HttpRequestException ex)
        {
            return OneOf<Success, ClientError, ServerError>.FromT3(
                new ServerError("Network error", ex.Message));
        }
    }

    public async Task<IActionResult> HandleApiResponse(string endpoint)
    {
        var result = await CallApiAsync(endpoint);
        
        return result.Match(
            case1: success => Ok(new { data = success.Data }),
            case2: clientError => BadRequest(new { error = clientError.Message }),
            case3: serverError => StatusCode(500, new { error = serverError.Message })
        );
    }
}
```

---

## 🔄 Pattern Integration

### Result ↔ OneOf Conversion

```csharp
// Result → OneOf
Result<User> result = await GetUserAsync(id);
OneOf<ValidationError, User> oneOf = result.ToOneOf(
    error => new ValidationError(error.Message)
);

// OneOf → Result
OneOf<ApiError, User> apiResult = await GetUserFromApiAsync(id);
Result<User> result = apiResult.ToResult(
    error => new Error(error.Message)
);
```

### Pipeline Integration

```csharp
// Mixed workflow: API → Business → Database
OneOf<ApiError, User> apiResult = await GetUserFromApiAsync(1);

// Transform to Result for business logic
Result<UserDto> businessResult = apiResult.SelectToResult(
    user => user.ToDto(),
    error => new Error($"API Error: {error.Message}")
);

// Transform back to OneOf for database layer
OneOf<DbError, UserDto> dbResult = businessResult.ToOneOfCustom(
    reason => new DbError(reason.Message)
);
```

### LINQ Integration

```csharp
// Complex data pipeline with LINQ
var result = from user in GetUserAsync(id)
            from profile in GetProfileAsync(user.Id)
            where profile.IsActive
            from orders in GetOrdersAsync(user.Id)
            select new UserProfile(user, profile, orders);

// Equivalent to:
var result = await GetUserAsync(id)
    .BindAsync(user => GetProfileAsync(user.Id))
    .WhereAsync(profile => profile.IsActive)
    .BindAsync(profile => GetOrdersAsync(profile.UserId))
    .SelectAsync((user, profile, orders) => new UserProfile(user, profile, orders));
```

---

## 🎯 Advanced Use Cases

### State Machine

```csharp
public class OrderStateMachine
{
    public OneOf<DraftOrder, PendingOrder, ConfirmedOrder> CreateOrder(CreateOrderRequest request)
    {
        return OneOf<DraftOrder, PendingOrder, ConfirmedOrder>.FromT1(
            new DraftOrder(request));
    }

    public OneOf<DraftOrder, PendingOrder, ConfirmedOrder> SubmitDraft(DraftOrder draft)
    {
        if (!ValidateDraft(draft))
            return OneOf<DraftOrder, PendingOrder, ConfirmedOrder>.FromT1(draft);

        return OneOf<DraftOrder, PendingOrder, ConfirmedOrder>.FromT2(
            new PendingOrder(draft));
    }

    public OneOf<DraftOrder, PendingOrder, ConfirmedOrder> ConfirmPending(PendingOrder pending)
    {
        if (!ProcessPayment(pending))
            return OneOf<DraftOrder, PendingOrder, ConfirmedOrder>.FromT2(pending);

        return OneOf<DraftOrder, PendingOrder, ConfirmedOrder>.FromT3(
            new ConfirmedOrder(pending));
    }

    public string GetStatus(OneOf<DraftOrder, PendingOrder, ConfirmedOrder> order)
    {
        return order.Match(
            case1: draft => "Draft",
            case2: pending => "Pending",
            case3: confirmed => "Confirmed"
        );
    }
}
```

### Validation Pipeline

```csharp
public class ValidationPipeline
{
    public OneOf<ValidationError, ValidatedUser> ValidateUser(CreateUserRequest request)
    {
        return OneOf<ValidationError, ValidatedUser>.FromT2(request)
            // Email validation
            .BindT2(req => ValidateEmail(req.Email)
                ? OneOf<ValidationError, ValidatedUser>.FromT2(req)
                : OneOf<ValidationError, ValidatedUser>.FromT1(
                    new ValidationError("Email", "Invalid format")))
            
            // Age validation
            .BindT2(req => req.Age >= 18
                ? OneOf<ValidationError, ValidatedUser>.FromT2(req)
                : OneOf<ValidationError, ValidatedUser>.FromT1(
                    new ValidationError("Age", "Must be 18 or older")))
            
            // Name validation
            .BindT2(req => !string.IsNullOrWhiteSpace(req.Name)
                ? OneOf<ValidationError, ValidatedUser>.FromT2(new ValidatedUser(req))
                : OneOf<ValidationError, ValidatedUser>.FromT1(
                    new ValidationError("Name", "Required")));
    }
}
```

---

## 🧪 Testing Advanced Patterns

### Testing Maybe

```csharp
[Test]
public void GetUserFromCache_ExistingUser_ReturnsUser()
{
    // Arrange
    var userId = 1;
    var expectedUser = new User { Id = userId, Name = "John" };
    _cache.Setup(x => x.Get<User>($"user:{userId}")).Returns(expectedUser);

    // Act
    Maybe<User> result = _cacheService.GetUserFromCache(userId);

    // Assert
    Assert.IsTrue(result.IsSome);
    Assert.AreEqual(expectedUser.Name, result.Value.Name);
}

[Test]
public void GetUserFromCache_NotInCache_ReturnsNone()
{
    // Arrange
    var userId = 999;
    _cache.Setup(x => x.Get<User>($"user:{userId}")).Returns((User)null);

    // Act
    Maybe<User> result = _cacheService.GetUserFromCache(userId);

    // Assert
    Assert.IsTrue(result.IsNone);
}
```

### Testing OneOf

```csharp
[Test]
public async Task GetUser_ApiSuccess_ReturnsUser()
{
    // Arrange
    var userId = 1;
    var expectedUser = new User { Id = userId, Name = "John" };
    _httpClientMock.Setup(x => x.GetAsync($"/users/{userId}"))
        .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedUser)
        });

    // Act
    OneOf<ApiError, User> result = await _apiClient.GetUserAsync(userId);

    // Assert
    Assert.IsTrue(result.IsT2);
    Assert.AreEqual(expectedUser.Name, result.AsT2.Name);
}

[Test]
public async Task GetUser_ApiNotFound_ReturnsError()
{
    // Arrange
    var userId = 999;
    _httpClientMock.Setup(x => x.GetAsync($"/users/{userId}"))
        .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

    // Act
    OneOf<ApiError, User> result = await _apiClient.GetUserAsync(userId);

    // Assert
    Assert.IsTrue(result.IsT1);
    Assert.IsInstanceOfType(result.AsT1, typeof(NotFoundError));
}
```

---

## 🎯 Best Practices

### ✅ Do's

- **Choose the right pattern** for your specific use case
- **Use Maybe for optional values** and cache operations
- **Use OneOf for discriminated unions** and state machines
- **Test all paths** in your pattern matches
- **Keep patterns simple** - avoid deeply nested structures

### ❌ Don'ts

- **Don't use Maybe when you need error details**
- **Don't create OneOf with too many types** - consider state pattern instead
- **Don't mix patterns unnecessarily** - be consistent
- **Don't forget to handle all cases** in pattern matching
- **Don't over-engineer simple scenarios**

---

## 📚 Next Steps

- **📖 [Getting Started](getting-started.md)** - Back to basics
- **🌐 [Web API Integration](web-api-integration.md)** - Apply patterns in APIs
- **⚡ [Source Generator](source-generator.md)** - Auto-conversion magic
- **📚 [API Reference](../api/)** - Complete technical documentation

---

## 🎉 You're Now an Advanced Patterns Expert!

You have the tools to write sophisticated, type-safe, and expressive code with REslava.Result.

**Key achievements:**
- ✅ Safe null handling with Maybe\<T>
- ✅ Type-safe alternatives with OneOf
- ✅ Complex state management with OneOf\<T1, T2, T3>
- ✅ Seamless pattern integration
- ✅ Functional programming techniques

**Start building amazing applications today!** 🚀
