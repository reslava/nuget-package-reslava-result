using FastMvcAPI.REslava.Result.Demo.Data;
using FastMvcAPI.REslava.Result.Demo.Models;
using Microsoft.EntityFrameworkCore;
using REslava.Result;
using REslava.Result.AdvancedPatterns;

namespace FastMvcAPI.REslava.Result.Demo.Services;

public class OrderService
{
    private readonly DemoDbContext _context;

    public OrderService(DemoDbContext context) => _context = context;

    public async Task<Result<List<OrderResponse>>> GetAllOrdersAsync()
    {
        var orders = await _context.Orders
            .Include(o => o.User).Include(o => o.Items).ThenInclude(oi => oi.Product)
            .ToListAsync();

        var response = orders.Select(o => new OrderResponse(
            o.Id, o.UserId, o.User.Name,
            o.Items.Select(oi => new OrderItemResponse(oi.ProductId, oi.Product.Name, oi.Quantity, oi.UnitPrice, oi.Subtotal)).ToList(),
            o.TotalAmount, o.Status.ToString(), o.CreatedAt
        )).ToList();

        return Result<List<OrderResponse>>.Ok(response);
    }

    public async Task<OneOf<NotFoundError, OrderResponse>> GetOrderByIdAsync(int id)
    {
        var order = await _context.Orders
            .Include(o => o.User).Include(o => o.Items).ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return new NotFoundError("Order", id);

        return new OrderResponse(
            order.Id, order.UserId, order.User.Name,
            order.Items.Select(oi => new OrderItemResponse(oi.ProductId, oi.Product.Name, oi.Quantity, oi.UnitPrice, oi.Subtotal)).ToList(),
            order.TotalAmount, order.Status.ToString(), order.CreatedAt);
    }

    public async Task<OneOf<NotFoundError, List<OrderResponse>>> GetOrdersByUserIdAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return new NotFoundError("User", userId);

        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.User).Include(o => o.Items).ThenInclude(oi => oi.Product)
            .ToListAsync();

        return orders.Select(o => new OrderResponse(
            o.Id, o.UserId, o.User.Name,
            o.Items.Select(oi => new OrderItemResponse(oi.ProductId, oi.Product.Name, oi.Quantity, oi.UnitPrice, oi.Subtotal)).ToList(),
            o.TotalAmount, o.Status.ToString(), o.CreatedAt
        )).ToList();
    }

    public async Task<OneOf<NotFoundError, ConflictError, ValidationError, OrderResponse>> CreateOrderAsync(CreateOrderRequest request)
    {
        if (request.Items == null || !request.Items.Any())
            return new ValidationError("Items", "Order must contain at least one item");

        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null)
            return new NotFoundError("User", request.UserId);

        if (!user.IsActive)
            return new ValidationError("User", $"User account is inactive: {user.Email}");

        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

        if (products.Count != productIds.Distinct().Count())
        {
            var missingIds = productIds.Except(products.Select(p => p.Id));
            return new NotFoundError("Product", missingIds.First());
        }

        decimal totalAmount = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in request.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);

            if (!product.IsAvailable)
                return new ConflictError($"Product '{product.Name}' is currently unavailable");

            if (product.StockQuantity < item.Quantity)
                return new ConflictError($"Insufficient stock for product '{product.Name}'. Requested: {item.Quantity}, Available: {product.StockQuantity}");

            product.StockQuantity -= item.Quantity;
            if (product.StockQuantity == 0)
                product.IsAvailable = false;

            totalAmount += product.Price * item.Quantity;
            orderItems.Add(new OrderItem { ProductId = product.Id, Quantity = item.Quantity, UnitPrice = product.Price });
        }

        var order = new Order
        {
            UserId = request.UserId, CreatedAt = DateTime.UtcNow,
            Status = OrderStatus.Pending, TotalAmount = totalAmount, Items = orderItems
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        await _context.Entry(order).Reference(o => o.User).LoadAsync();
        foreach (var item in order.Items)
            await _context.Entry(item).Reference(oi => oi.Product).LoadAsync();

        return new OrderResponse(
            order.Id, order.UserId, order.User.Name,
            order.Items.Select(oi => new OrderItemResponse(oi.ProductId, oi.Product.Name, oi.Quantity, oi.UnitPrice, oi.Subtotal)).ToList(),
            order.TotalAmount, order.Status.ToString(), order.CreatedAt);
    }

    public async Task<OneOf<NotFoundError, ValidationError, OrderResponse>> UpdateOrderStatusAsync(int id, string status)
    {
        var order = await _context.Orders
            .Include(o => o.User).Include(o => o.Items).ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return new NotFoundError("Order", id);

        if (!Enum.TryParse<OrderStatus>(status, true, out var newStatus))
            return new ValidationError("Status", $"Invalid order status. Valid values are: {string.Join(", ", Enum.GetNames<OrderStatus>())}");

        order.Status = newStatus;
        await _context.SaveChangesAsync();

        return new OrderResponse(
            order.Id, order.UserId, order.User.Name,
            order.Items.Select(oi => new OrderItemResponse(oi.ProductId, oi.Product.Name, oi.Quantity, oi.UnitPrice, oi.Subtotal)).ToList(),
            order.TotalAmount, order.Status.ToString(), order.CreatedAt);
    }

    public async Task<OneOf<NotFoundError, ValidationError, OrderResponse>> CancelOrderAsync(int id)
    {
        var order = await _context.Orders
            .Include(o => o.User).Include(o => o.Items).ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return new NotFoundError("Order", id);

        if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
            return new ValidationError("OrderStatus", $"Cannot cancel order in '{order.Status}' status");

        foreach (var item in order.Items)
        {
            item.Product.StockQuantity += item.Quantity;
            item.Product.IsAvailable = true;
        }

        order.Status = OrderStatus.Cancelled;
        await _context.SaveChangesAsync();

        return new OrderResponse(
            order.Id, order.UserId, order.User.Name,
            order.Items.Select(oi => new OrderItemResponse(oi.ProductId, oi.Product.Name, oi.Quantity, oi.UnitPrice, oi.Subtotal)).ToList(),
            order.TotalAmount, order.Status.ToString(), order.CreatedAt);
    }

    public async Task<Result<OrderStatistics>> GetUserOrderStatisticsAsync(int userId)
    {
        var orders = await _context.Orders.Where(o => o.UserId == userId).ToListAsync();
        var stats = new OrderStatistics(
            TotalOrders: orders.Count,
            CompletedOrders: orders.Count(o => o.Status == OrderStatus.Completed),
            PendingOrders: orders.Count(o => o.Status == OrderStatus.Pending),
            CancelledOrders: orders.Count(o => o.Status == OrderStatus.Cancelled),
            TotalSpent: orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount),
            AverageOrderValue: orders.Any(o => o.Status == OrderStatus.Completed)
                ? orders.Where(o => o.Status == OrderStatus.Completed).Average(o => o.TotalAmount)
                : 0
        );
        return Result<OrderStatistics>.Ok(stats);
    }
}

public record OrderStatistics(int TotalOrders, int CompletedOrders, int PendingOrders, int CancelledOrders, decimal TotalSpent, decimal AverageOrderValue);
