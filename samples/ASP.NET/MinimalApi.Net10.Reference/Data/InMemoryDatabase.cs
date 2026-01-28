namespace MinimalApi.Net10.Reference.Data;

public class InMemoryDatabase
{
    private readonly Dictionary<int, Models.Product> _products = new();
    private readonly Dictionary<int, Models.Order> _orders = new();
    private int _nextProductId = 1;
    private int _nextOrderId = 1;

    public InMemoryDatabase()
    {
        // Seed with sample data
        SeedData();
    }

    // Products
    public IEnumerable<Models.Product> GetProducts() => _products.Values;

    public Models.Product? GetProduct(int id) => _products.TryGetValue(id, out var product) ? product : null;

    public Models.Product AddProduct(Models.Product product)
    {
        var newProduct = product with { Id = _nextProductId++ };
        _products[newProduct.Id] = newProduct;
        return newProduct;
    }

    public Models.Product? UpdateProduct(int id, Models.Product product)
    {
        if (!_products.ContainsKey(id))
            return null;

        var updatedProduct = product with { Id = id };
        _products[id] = updatedProduct;
        return updatedProduct;
    }

    public bool DeleteProduct(int id) => _products.Remove(id);

    // Orders
    public IEnumerable<Models.Order> GetOrders() => _orders.Values;

    public Models.Order? GetOrder(int id) => _orders.TryGetValue(id, out var order) ? order : null;

    public Models.Order AddOrder(Models.Order order)
    {
        var newOrder = order with { Id = _nextOrderId++ };
        _orders[newOrder.Id] = newOrder;
        return newOrder;
    }

    public Models.Order? UpdateOrder(int id, Models.Order order)
    {
        if (!_orders.ContainsKey(id))
            return null;

        var updatedOrder = order with { Id = id };
        _orders[id] = updatedOrder;
        return updatedOrder;
    }

    public bool DeleteOrder(int id) => _orders.Remove(id);

    // Helper methods
    public bool ProductExists(int id) => _products.ContainsKey(id);

    public bool HasSufficientStock(int productId, int quantity)
    {
        var product = GetProduct(productId);
        return product?.StockQuantity >= quantity;
    }

    public void UpdateStock(int productId, int quantity)
    {
        var product = GetProduct(productId);
        if (product != null)
        {
            var updatedProduct = product with { StockQuantity = product.StockQuantity - quantity };
            _products[productId] = updatedProduct;
        }
    }

    private void SeedData()
    {
        // Add sample products
        var products = new[]
        {
            new Models.Product
            {
                Name = "Laptop Pro 15",
                Price = 1299.99m,
                Description = "High-performance laptop with 15-inch display, 16GB RAM, and 512GB SSD",
                StockQuantity = 25,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Models.Product
            {
                Name = "Wireless Mouse",
                Price = 29.99m,
                Description = "Ergonomic wireless mouse with precision tracking and long battery life",
                StockQuantity = 150,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Models.Product
            {
                Name = "Mechanical Keyboard",
                Price = 89.99m,
                Description = "RGB mechanical keyboard with blue switches and programmable keys",
                StockQuantity = 75,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Models.Product
            {
                Name = "USB-C Hub",
                Price = 49.99m,
                Description = "7-in-1 USB-C hub with HDMI, USB 3.0, and SD card reader",
                StockQuantity = 100,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Models.Product
            {
                Name = "Monitor Stand",
                Price = 39.99m,
                Description = "Adjustable monitor stand with built-in storage drawer",
                StockQuantity = 50,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            }
        };

        foreach (var product in products)
        {
            AddProduct(product);
        }

        // Add sample orders
        var orders = new[]
        {
            new Models.Order
            {
                CustomerEmail = "john.doe@example.com",
                Items = new List<Models.OrderItem>
                {
                    new() { ProductId = 1, ProductName = "Laptop Pro 15", Quantity = 1, UnitPrice = 1299.99m },
                    new() { ProductId = 2, ProductName = "Wireless Mouse", Quantity = 1, UnitPrice = 29.99m }
                },
                TotalAmount = 1329.98m,
                Status = Models.OrderStatus.Confirmed,
                ShippingAddress = "123 Main St, New York, NY 10001"
            },
            new Models.Order
            {
                CustomerEmail = "jane.smith@example.com",
                Items = new List<Models.OrderItem>
                {
                    new() { ProductId = 3, ProductName = "Mechanical Keyboard", Quantity = 2, UnitPrice = 89.99m }
                },
                TotalAmount = 179.98m,
                Status = Models.OrderStatus.Shipped,
                ShippingAddress = "456 Oak Ave, Los Angeles, CA 90001"
            }
        };

        foreach (var order in orders)
        {
            AddOrder(order);
        }
    }
}
