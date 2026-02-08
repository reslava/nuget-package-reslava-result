# Fast Minimal API - REslava.Result Demo

**Production-ready demonstration of type-safe error handling in ASP.NET Core Minimal APIs using REslava.Result library v1.12.0**

---

## 🎯 What This Demo Showcases

This demo application demonstrates **real-world patterns** for building robust Minimal APIs with:
- ✅ **Type-safe error handling** using `Result<T>` and `OneOf<T1,T2,T3,T4>` patterns
- ✅ **Zero exception-based control flow** - errors are values, not exceptions
- ✅ **Railway-oriented programming** - clean separation of success/failure paths
- ✅ **Production-ready error responses** with proper HTTP status codes
- ✅ **Complete CRUD operations** for Users, Products, and Orders
- ✅ **Complex business logic** with multi-step validation

---

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 or higher
- REslava.Result v1.12.0 (automatically installed via NuGet)

### Run the Demo
```bash
dotnet restore
dotnet run
```

The application starts at: **http://localhost:5000** (or https://localhost:5001)
Swagger UI is available at the root: **http://localhost:5000/**

---

## 📚 Learning Path: From Simple to Advanced

### 🟢 Level 1: Basic Result<T> Pattern

**UserEndpoints.cs** - Simple CRUD with `Result<T>`

```csharp
// GET /api/users/{id}
var result = await userService.GetUserByIdAsync(id);

return result.Match(
    case1: notFound => Results.NotFound(new { error = notFound.Message }),
    case2: user => Results.Ok(new { data = user })
);
```

**Pattern**: `OneOf<UserNotFoundError, User>` (2 types)
- **Error case** (T1): 404 Not Found
- **Success case** (T2): 200 OK with user data

### 🟡 Level 2: Multi-Error OneOf3 Pattern

**ProductEndpoints.cs** - Complex validation with `OneOf<T1,T2,T3>`

```csharp
// POST /api/products
var result = await productService.CreateProductAsync(request);

return result.Match(
    case1: validation => Results.BadRequest(new { error = validation.Message }),
    case2: duplicate => Results.Conflict(new { error = duplicate.Message }),
    case3: product => Results.Created($"/api/products/{product.Id}", new { data = product })
);
```

**Pattern**: `OneOf<ValidationError, DuplicateProductError, Product>` (3 types)
- **Error case 1** (T1): 400 Bad Request - Invalid input
- **Error case 2** (T2): 409 Conflict - Product already exists
- **Success case** (T3): 201 Created with product data

### 🔴 Level 3: Advanced OneOf4 Pattern

**OrderEndpoints.cs** - Complex business logic with `OneOf<T1,T2,T3,T4>`

```csharp
// POST /api/orders
var result = await orderService.CreateOrderAsync(request);

return result.Match(
    case1: userNotFound => Results.NotFound(new { error = userNotFound.Message }),
    case2: insufficientStock => Results.StatusCode(409, new { error = insufficientStock.Message }),
    case3: validation => Results.BadRequest(new { error = validation.Message }),
    case4: order => Results.Created($"/api/orders/{order.Id}", new { data = order })
);
```

**Pattern**: `OneOf<UserNotFoundError, InsufficientStockError, ValidationError, OrderResponse>` (4 types)
- **Error case 1** (T1): 404 Not Found - User doesn't exist
- **Error case 2** (T2): 409 Conflict - Insufficient stock
- **Error case 3** (T3): 400 Bad Request - Validation errors (empty order, inactive user, etc.)
- **Success case** (T4): 201 Created with order data

**Why OneOf4?** REslava.Result supports `OneOf<T1,T2>`, `OneOf<T1,T2,T3>`, and `OneOf<T1,T2,T3,T4>`. The OrderService uses OneOf4 to handle **3 error types** + **1 success type** = 4 cases total. User inactive validation is consolidated into `ValidationError` for consistency.

---

## 🏗️ Architecture

### Project Structure
```
FastMinimalAPI.REslava.Result.Demo/
├── Data/
│   └── DemoDbContext.cs              # EF Core In-Memory Database
├── Models/
│   ├── User.cs                        # Domain: User entity
│   ├── Product.cs                     # Domain: Product entity
│   └── Order.cs                       # Domain: Order entity + DTOs
├── Errors/
│   ├── NotFoundErrors.cs              # 404 errors (UserNotFoundError, etc.)
│   ├── BusinessErrors.cs              # 403/409 errors (UserInactiveError, InsufficientStockError)
│   └── ValidationErrors.cs            # 400 errors (ValidationError, DuplicateError)
├── Services/
│   ├── UserService.cs                 # Business logic: User operations
│   ├── ProductService.cs              # Business logic: Product operations
│   └── OrderService.cs                # Business logic: Order operations (complex)
├── Endpoints/
│   ├── UserEndpoints.cs               # API: User endpoints (OneOf2)
│   ├── ProductEndpoints.cs            # API: Product endpoints (OneOf3)
│   └── OrderEndpoints.cs              # API: Order endpoints (OneOf4)
└── Program.cs                         # Application entry point
```

### Custom Error Types

**NotFoundErrors.cs** (404)
```csharp
public class UserNotFoundError : Error { }
public class ProductNotFoundError : Error { }
public class OrderNotFoundError : Error { }
```

**BusinessErrors.cs** (403, 409)
```csharp
public class UserInactiveError : Error { }              // 403 Forbidden
public class InsufficientStockError : Error { }         // 409 Conflict
public class DuplicateProductError : Error { }          // 409 Conflict
```

**ValidationErrors.cs** (400, 422)
```csharp
public class ValidationError : Error { }                // 400 Bad Request
```

---

## 📖 API Reference

### Users API (OneOf2 Pattern)

| Endpoint | Method | Pattern | Response |
|----------|--------|---------|----------|
| `/api/users` | GET | `Result<List<User>>` | 200 OK / 500 Error |
| `/api/users/{id}` | GET | `OneOf<NotFound, User>` | 200 OK / 404 Not Found |
| `/api/users` | POST | `OneOf<Validation, User>` | 201 Created / 400 Bad Request |
| `/api/users/{id}` | PUT | `OneOf<NotFound, Validation, User>` | 200 OK / 400 / 404 |
| `/api/users/{id}` | DELETE | `OneOf<NotFound, User>` | 200 OK / 404 Not Found |

### Products API (OneOf3 Pattern)

| Endpoint | Method | Pattern | Response |
|----------|--------|---------|----------|
| `/api/products` | GET | `Result<List<Product>>` | 200 OK / 500 Error |
| `/api/products/{id}` | GET | `OneOf<NotFound, Product>` | 200 OK / 404 Not Found |
| `/api/products` | POST | `OneOf<Validation, Duplicate, Product>` | 201 Created / 400 / 409 |
| `/api/products/{id}` | PUT | `OneOf<NotFound, Validation, Product>` | 200 OK / 400 / 404 |
| `/api/products/{id}/stock` | PATCH | `OneOf<NotFound, Validation, Product>` | 200 OK / 400 / 404 |

### Orders API (OneOf4 Pattern - Advanced!)

| Endpoint | Method | Pattern | Response |
|----------|--------|---------|----------|
| `/api/orders` | GET | `Result<List<OrderResponse>>` | 200 OK / 500 Error |
| `/api/orders/{id}` | GET | `OneOf<NotFound, OrderResponse>` | 200 OK / 404 Not Found |
| `/api/orders/user/{userId}` | GET | `OneOf<UserNotFound, List<OrderResponse>>` | 200 OK / 404 |
| `/api/orders` | POST | `OneOf<UserNotFound, InsufficientStock, Validation, OrderResponse>` | 201 / 400 / 404 / 409 |
| `/api/orders/{id}/status` | PATCH | `OneOf<NotFound, Validation, OrderResponse>` | 200 OK / 400 / 404 |
| `/api/orders/{id}/cancel` | DELETE | `OneOf<NotFound, Validation, OrderResponse>` | 200 OK / 400 / 404 |

---

## 🎓 Code Examples

### Example 1: Create User (Simple OneOf2)

**Request**:
```bash
POST /api/users
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john@example.com",
  "isActive": true
}
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "id": 4,
    "name": "John Doe",
    "email": "john@example.com",
    "isActive": true,
    "createdAt": "2024-01-15T10:30:00Z"
  },
  "message": "User created successfully"
}
```

**Validation Error** (400 Bad Request):
```json
{
  "success": false,
  "error": "Email is already in use",
  "errorType": "ValidationError",
  "field": "Email"
}
```

---

### Example 2: Create Product (OneOf3 Pattern)

**Request**:
```bash
POST /api/products
Content-Type: application/json

{
  "name": "Gaming Mouse",
  "price": 79.99,
  "stock": 50,
  "category": "Electronics"
}
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "id": 4,
    "name": "Gaming Mouse",
    "price": 79.99,
    "stock": 50,
    "category": "Electronics",
    "createdAt": "2024-01-15T10:35:00Z"
  },
  "message": "Product created successfully"
}
```

**Duplicate Error** (409 Conflict):
```json
{
  "success": false,
  "error": "Product with name 'Gaming Mouse' already exists",
  "errorType": "DuplicateProduct"
}
```

---

### Example 3: Create Order (Advanced OneOf4 Pattern)

**Request**:
```bash
POST /api/orders
Content-Type: application/json

{
  "userId": 1,
  "items": [
    { "productId": 1, "quantity": 2 },
    { "productId": 2, "quantity": 1 }
  ]
}
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "id": 1,
    "userId": 1,
    "userName": "Alice Johnson",
    "items": [
      {
        "productId": 1,
        "productName": "Laptop Pro 15",
        "quantity": 2,
        "unitPrice": 1299.99,
        "totalPrice": 2599.98
      },
      {
        "productId": 2,
        "productName": "Wireless Mouse",
        "quantity": 1,
        "unitPrice": 29.99,
        "totalPrice": 29.99
      }
    ],
    "totalAmount": 2629.97,
    "status": "Pending",
    "createdAt": "2024-01-15T10:40:00Z"
  },
  "message": "Order created successfully"
}
```

**Error Cases**:

1. **User Not Found** (404):
```json
{
  "success": false,
  "error": "User with id 999 not found",
  "errorType": "UserNotFound",
  "field": "UserId"
}
```

2. **Insufficient Stock** (409 Conflict):
```json
{
  "success": false,
  "error": "Insufficient stock for product Laptop Pro 15. Requested: 100, Available: 50",
  "errorType": "InsufficientStock",
  "productId": 1,
  "requested": 100,
  "available": 50
}
```

3. **Validation Errors** (400 Bad Request):
```json
// Empty Order
{
  "success": false,
  "error": "Order must contain at least one item",
  "errorType": "ValidationError",
  "field": "Items",
  "reason": "EmptyOrder"
}

// Inactive User Account
{
  "success": false,
  "error": "Validation failed for 'User': User account is inactive",
  "errorType": "ValidationError",
  "field": "User",
  "userId": 3,
  "reason": "InactiveAccount"
}
```

---

## 🔍 Key Patterns Demonstrated

### 1. Railway-Oriented Programming
```csharp
// Service layer returns OneOf
public async Task<OneOf<UserNotFoundError, InsufficientStockError, ValidationError, OrderResponse>>
    CreateOrderAsync(CreateOrderRequest request)
{
    // Validate user exists
    var user = await _context.Users.FindAsync(request.UserId);
    if (user == null)
        return new UserNotFoundError($"User with id {request.UserId} not found");

    // Validate user is active (consolidated into ValidationError)
    if (!user.IsActive)
    {
        var error = new ValidationError("User", "User account is inactive", user.Email);
        return error.WithTag("UserId", user.Id).WithTag("Reason", "InactiveAccount");
    }

    // Validate stock for each item
    // ... more validations ...

    // Success path - create order
    return new OrderResponse { /* ... */ };
}
```

### 2. Type-Safe Error Handling
```csharp
// Endpoint handles all cases explicitly
return result.Match(
    case1: userNotFound => Results.NotFound(/* ... */),    // 404
    case2: insufficientStock => Results.StatusCode(409, /* ... */),  // 409
    case3: validation => Results.BadRequest(/* ... */),    // 400
    case4: order => Results.Created(/* ... */)             // 201
);
```

### 3. Rich Error Context with Tags
```csharp
return new InsufficientStockError($"Insufficient stock for product {product.Name}")
    .WithTag("ProductId", product.Id)
    .WithTag("ProductName", product.Name)
    .WithTag("RequestedQuantity", item.Quantity)
    .WithTag("AvailableStock", product.Stock);
```

---

## 🧪 Testing the API

### Using cURL

**Get all users**:
```bash
curl http://localhost:5000/api/users
```

**Create a new order**:
```bash
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 1,
    "items": [
      {"productId": 1, "quantity": 2},
      {"productId": 2, "quantity": 1}
    ]
  }'
```

**Test error cases**:
```bash
# User not found (404)
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{"userId": 999, "items": [{"productId": 1, "quantity": 1}]}'

# Empty order (400)
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{"userId": 1, "items": []}'

# Insufficient stock (409)
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{"userId": 1, "items": [{"productId": 1, "quantity": 1000}]}'
```

---

## 📦 Seed Data

The demo includes pre-populated data:

**Users** (3):
- Alice Johnson (Active)
- Bob Smith (Active)
- Charlie Brown (Inactive - for testing 403 errors)

**Products** (3):
- Laptop Pro 15 ($1299.99, Stock: 50)
- Wireless Mouse ($29.99, Stock: 200)
- USB-C Cable ($12.99, Stock: 500)

---

## 🎯 Why REslava.Result?

### Traditional Exception-Based Approach ❌
```csharp
public async Task<User> GetUserAsync(int id)
{
    var user = await _context.Users.FindAsync(id);
    if (user == null)
        throw new NotFoundException($"User {id} not found");  // 💥 Exception!
    
    return user;
}

// Endpoint - no type safety
app.MapGet("/users/{id}", async (int id) =>
{
    try
    {
        var user = await service.GetUserAsync(id);
        return Results.Ok(user);
    }
    catch (NotFoundException ex)  // Runtime error handling
    {
        return Results.NotFound(ex.Message);
    }
});
```

### REslava.Result Approach ✅
```csharp
public async Task<OneOf<UserNotFoundError, User>> GetUserAsync(int id)
{
    var user = await _context.Users.FindAsync(id);
    if (user == null)
        return new UserNotFoundError($"User {id} not found");  // ✅ Error is a value
    
    return user;
}

// Endpoint - compile-time type safety!
app.MapGet("/users/{id}", async (int id) =>
{
    var result = await service.GetUserAsync(id);
    
    return result.Match(
        case1: notFound => Results.NotFound(new { error = notFound.Message }),
        case2: user => Results.Ok(new { data = user })
    );  // ✅ All cases handled at compile time
});
```

**Benefits**:
- ✅ **Type-safe** - Compiler ensures all error cases are handled
- ✅ **No exceptions** - Errors are values, not control flow
- ✅ **Self-documenting** - Return type shows all possible outcomes
- ✅ **Railway-oriented** - Clean separation of success/failure paths
- ✅ **Production-ready** - Proper HTTP status codes automatically

---

## 🛠️ Technologies

- **ASP.NET Core 8.0** - Minimal APIs
- **Entity Framework Core** - In-Memory Database
- **REslava.Result v1.12.0** - Type-safe error handling
- **Swagger/OpenAPI** - API documentation

---

## 📚 Learn More

- **REslava.Result Documentation**: https://github.com/reslava/nuget-package-reslava-result
- **Source Code**: This demo project
- **NuGet Package**: `dotnet add package REslava.Result`

---

## 🤝 Contributing

Found a bug or have a suggestion? Open an issue at:
https://github.com/reslava/nuget-package-reslava-result/issues

---

## 📄 License

MIT License - See LICENSE file for details

---

**Built with ❤️ using REslava.Result v1.12.0**

*Demonstrating production-ready patterns for type-safe error handling in ASP.NET Core Minimal APIs*
