# MinimalAPI .NET 10 Reference

A reference implementation showcasing pure .NET 10 Minimal API features without external libraries.

## 🎯 Purpose

This project demonstrates how to build clean, modern APIs using only .NET 10 built-in features. It serves as a baseline reference for comparing against the REslava.Result library with source generators.

## ✨ .NET 10 Features Demonstrated

### 🔧 Built-in Request Validation
```csharp
public record CreateProductRequest(
    [Required, MinLength(3)] string Name,
    [Range(0.01, 10000.00)] decimal Price,
    [Required, MinLength(10)] string Description
);
```

### 🚨 Enhanced Error Handling
- Automatic `ValidationProblem` responses
- RFC 7807 compliant error details
- Custom error extensions for better debugging

### 📚 OpenAPI Integration
- Enhanced Swagger documentation
- Custom document transformers
- Rich endpoint metadata

### 💎 Modern C# 14 Features
- Primary constructors for records
- Collection expressions
- Raw string literals
- Enhanced pattern matching

## 🏗️ Architecture

```
MinimalApi.Net10.Reference/
├── Models/                 # Data models and DTOs
│   ├── Product.cs         # Product entity
│   ├── Order.cs           # Order entity
│   ├── ProductRequests.cs # Product DTOs with validation
│   └── OrderRequests.cs   # Order DTOs with validation
├── Endpoints/             # API endpoint definitions
│   ├── ProductEndpoints.cs
│   └── OrderEndpoints.cs
├── Services/              # Business logic
│   ├── ProductService.cs
│   └── OrderService.cs
├── Data/                  # Data access
│   └── InMemoryDatabase.cs
└── Program.cs             # Application configuration
```

## 🚀 Getting Started

### Prerequisites
- .NET 10.0 SDK
- Visual Studio 2022 or VS Code

### Running the Application
```bash
cd MinimalApi.Net10.Reference
dotnet run
```

### Access Points
- **Swagger UI**: `https://localhost:xxxx/swagger`
- **API Root**: `https://localhost:xxxx/`
- **Health Check**: `https://localhost:xxxx/health`

## 📊 API Endpoints

### Products
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update existing product
- `DELETE /api/products/{id}` - Delete product

### Orders
- `GET /api/orders` - Get all orders
- `GET /api/orders/{id}` - Get order by ID
- `POST /api/orders` - Create simple order
- `POST /api/orders/advanced` - Create advanced order (complex validation)
- `PATCH /api/orders/{id}/status` - Update order status
- `DELETE /api/orders/{id}` - Delete order

## 🔍 Validation Examples

### Simple Validation
```json
POST /api/products
{
  "name": "Laptop",
  "price": 999.99,
  "description": "High-performance laptop",
  "stockQuantity": 10
}
```

### Complex Validation
```json
POST /api/orders/advanced
{
  "customerEmail": "user@example.com",
  "shippingAddress": "123 Main St, City, State",
  "items": [
    { "productId": 1, "quantity": 2 }
  ],
  "maxTotalValue": 5000.00,
  "requestedDeliveryDate": "2024-12-31"
}
```

## 🆚 Comparison with REslava.Result

| Feature | Pure .NET 10 | REslava.Result + Generators |
|---------|---------------|----------------------------|
| **Validation** | Manual ModelState checks | Automatic Result<T> conversion |
| **Error Handling** | ValidationProblem() | Built-in HTTP status mapping |
| **Boilerplate** | ~30 lines per endpoint | ~5 lines per endpoint |
| **Type Safety** | Runtime validation | Compile-time generation |
| **Performance** | Runtime overhead | Zero runtime overhead |

## 🎯 Key Takeaways

### What .NET 10 Does Well
- ✅ Built-in validation attributes
- ✅ Automatic OpenAPI generation
- ✅ Clean Minimal API syntax
- ✅ Enhanced error handling

### Where Boilerplate Remains
- ❌ Manual ModelState validation
- ❌ Repetitive error handling patterns
- ❌ Manual HTTP status mapping
- ❌ Verbosity in endpoint definitions

## 📝 Sample Usage

### Creating a Product (Pure .NET 10)
```csharp
app.MapPost("/api/products", async (CreateProductRequest request, ProductService service) =>
{
    if (!ModelState.IsValid)
        return Results.ValidationProblem(ModelState);
    
    var product = service.CreateProduct(request);
    return Results.Created($"/api/products/{product.Id}", product);
});
```

### Same with REslava.Result Generators
```csharp
app.MapPost("/api/products", (CreateProductRequest request, ProductService service) =>
    service.CreateProduct(request)); // Automatic conversion!
```

## 🔧 Development Notes

### Database
- Uses in-memory database for simplicity
- Seeded with sample data on startup
- No external dependencies required

### Validation
- Leverages .NET 10 built-in validation attributes
- Custom validation attributes for complex scenarios
- Enhanced error responses with detailed information

### Documentation
- Auto-generated Swagger/OpenAPI documentation
- Rich endpoint metadata and descriptions
- Interactive API testing interface

---

**This project serves as a reference for understanding the benefits that source generators and the REslava.Result library bring to .NET Minimal API development.**
