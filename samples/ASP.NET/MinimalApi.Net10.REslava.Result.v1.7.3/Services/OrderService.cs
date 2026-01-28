namespace MinimalApi.Net10.Reference.Services;

public class OrderService
{
    private readonly Data.InMemoryDatabase _database;

    public OrderService(Data.InMemoryDatabase database)
    {
        _database = database;
    }

    public IEnumerable<Models.Order> GetAllOrders() => _database.GetOrders();

    public Models.Order? GetOrderById(int id) => _database.GetOrder(id);

    public Models.Order? CreateOrder(Models.CreateOrderRequest request)
    {
        // Validate all products exist and have sufficient stock
        foreach (var item in request.Items)
        {
            if (!_database.ProductExists(item.ProductId))
                return null;

            if (!_database.HasSufficientStock(item.ProductId, item.Quantity))
                return null;
        }

        // Create order items with product details
        var orderItems = new List<Models.OrderItem>();
        decimal totalAmount = 0;

        foreach (var item in request.Items)
        {
            var product = _database.GetProduct(item.ProductId);
            if (product == null)
                return null;

            var orderItem = new Models.OrderItem
            {
                ProductId = item.ProductId,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            };

            orderItems.Add(orderItem);
            totalAmount += orderItem.TotalPrice;

            // Update stock
            _database.UpdateStock(item.ProductId, item.Quantity);
        }

        var order = new Models.Order
        {
            CustomerEmail = request.CustomerEmail,
            Items = orderItems,
            TotalAmount = totalAmount,
            ShippingAddress = request.ShippingAddress
        };

        return _database.AddOrder(order);
    }

    public Models.Order? CreateAdvancedOrder(Models.CreateAdvancedOrderRequest request)
    {
        // Validate all products exist and have sufficient stock
        foreach (var item in request.Items)
        {
            if (!_database.ProductExists(item.ProductId))
                return null;

            if (!_database.HasSufficientStock(item.ProductId, item.Quantity))
                return null;
        }

        // Create order items with product details
        var orderItems = new List<Models.OrderItem>();
        decimal totalAmount = 0;

        foreach (var item in request.Items)
        {
            var product = _database.GetProduct(item.ProductId);
            if (product == null)
                return null;

            var orderItem = new Models.OrderItem
            {
                ProductId = item.ProductId,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            };

            orderItems.Add(orderItem);
            totalAmount += orderItem.TotalPrice;

            // Update stock
            _database.UpdateStock(item.ProductId, item.Quantity);
        }

        // Check max total value constraint
        if (request.MaxTotalValue.HasValue && totalAmount > request.MaxTotalValue.Value)
            return null;

        var order = new Models.Order
        {
            CustomerEmail = request.CustomerEmail,
            Items = orderItems,
            TotalAmount = totalAmount,
            ShippingAddress = request.ShippingAddress,
            Status = Models.OrderStatus.Pending // Advanced orders start as pending
        };

        return _database.AddOrder(order);
    }

    public Models.Order? UpdateOrderStatus(int id, Models.OrderStatus status)
    {
        var existingOrder = _database.GetOrder(id);
        if (existingOrder == null)
            return null;

        var updatedOrder = existingOrder with { Status = status };
        return _database.UpdateOrder(id, updatedOrder);
    }

    public bool DeleteOrder(int id) => _database.DeleteOrder(id);
}
