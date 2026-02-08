using FastMinimalAPI.REslava.Result.Demo.Data;
using FastMinimalAPI.REslava.Result.Demo.Errors;
using FastMinimalAPI.REslava.Result.Demo.Models;
using Microsoft.EntityFrameworkCore;
using REslava.Result;
using REslava.Result.AdvancedPatterns;

namespace FastMinimalAPI.REslava.Result.Demo.Services;

/// <summary>
/// OrderService demonstrating complex business logic with multiple validation steps
/// Showcases OneOf4 patterns for handling multiple error types in order workflows
/// </summary>
public class OrderService
{
    private readonly DemoDbContext _context;

    public OrderService(DemoDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all orders with their items and product details
    /// Demonstrates Result&lt;T&gt; with complex entity navigation
    /// </summary>
    public async Task<Result<List<OrderResponse>>> GetAllOrdersAsync()
    {
        var orders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
            .ToListAsync();

        var response = orders.Select(o => new OrderResponse(
            o.Id,
            o.UserId,
            o.User.Name,
            o.Items.Select(oi => new OrderItemResponse(
                oi.ProductId,
                oi.Product.Name,
                oi.Quantity,
                oi.UnitPrice,
                oi.Subtotal
            )).ToList(),
            o.TotalAmount,
            o.Status.ToString(),
            o.CreatedAt
        )).ToList();

        return Result<List<OrderResponse>>.Ok(response);
    }

    /// <summary>
    /// Get order by ID
    /// Demonstrates OneOf&lt;TError, TSuccess&gt; for simple not-found scenarios
    /// </summary>
    public async Task<OneOf<OrderNotFoundError, OrderResponse>> GetOrderByIdAsync(int id)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return new OrderNotFoundError(id);
        }

        var response = new OrderResponse(
            order.Id,
            order.UserId,
            order.User.Name,
            order.Items.Select(oi => new OrderItemResponse(
                oi.ProductId,
                oi.Product.Name,
                oi.Quantity,
                oi.UnitPrice,
                oi.Subtotal
            )).ToList(),
            order.TotalAmount,
            order.Status.ToString(),
            order.CreatedAt
        );

        return response;
    }

    /// <summary>
    /// Get all orders for a specific user
    /// Demonstrates OneOf&lt;TError, TSuccess&gt; with list results
    /// </summary>
    public async Task<OneOf<UserNotFoundError, List<OrderResponse>>> GetOrdersByUserIdAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return new UserNotFoundError(userId);
        }

        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
            .ToListAsync();

        var response = orders.Select(o => new OrderResponse(
            o.Id,
            o.UserId,
            o.User.Name,
            o.Items.Select(oi => new OrderItemResponse(
                oi.ProductId,
                oi.Product.Name,
                oi.Quantity,
                oi.UnitPrice,
                oi.Subtotal
            )).ToList(),
            o.TotalAmount,
            o.Status.ToString(),
            o.CreatedAt
        )).ToList();

        return response;
    }

    /// <summary>
    /// Create a new order with complex validation
    /// ⭐ SHOWCASE: OneOf4 demonstrating multiple error types in complex business logic
    /// Error Scenarios:
    /// 1. UserNotFoundError (404) - User doesn't exist
    /// 2. InsufficientStockError (409) - Not enough inventory
    /// 3. ValidationError (400) - Invalid input (e.g., empty order, inactive user)
    /// Success: OrderResponse with created order details
    /// </summary>
    public async Task<OneOf<UserNotFoundError, InsufficientStockError, ValidationError, OrderResponse>>
        CreateOrderAsync(CreateOrderRequest request)
    {
        // Validation 1: Check if order has items
        if (request.Items == null || !request.Items.Any())
        {
            var error = new ValidationError("Items", "Order must contain at least one item");
            error.WithTag("Reason", "EmptyOrder");
            return error;
        }

        // Validation 2: Check if user exists
        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null)
        {
            return new UserNotFoundError(request.UserId);
        }

        // Validation 3: Check if user is active
        if (!user.IsActive)
        {
            var error = new ValidationError("User", $"User account is inactive: {user.Email}", user.Email);
            error.WithTag("UserId", user.Id).WithTag("Reason", "InactiveAccount");
            return error;
        }

        // Validation 4: Check stock availability for all products
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        // Check if all products exist
        if (products.Count != productIds.Distinct().Count())
        {
            var missingIds = productIds.Except(products.Select(p => p.Id));
            return new InsufficientStockError(
                missingIds.First(), 
                "Product not found", 
                0, 
                0
            );
        }

        // Check stock levels and calculate total
        decimal totalAmount = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in request.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);

            // Check if product is available
            if (!product.IsAvailable)
            {
                var stockError = new InsufficientStockError(
                    product.Id,
                    product.Name,
                    0,
                    item.Quantity
                );
                stockError.WithTag("Reason", "Product is currently unavailable");
                return stockError;
            }

            // Check stock level
            if (product.StockQuantity < item.Quantity)
            {
                return new InsufficientStockError(
                    product.Id,
                    product.Name,
                    product.StockQuantity,
                    item.Quantity
                );
            }

            // Reduce stock
            product.StockQuantity -= item.Quantity;
            if (product.StockQuantity == 0)
            {
                product.IsAvailable = false;
            }

            // Calculate subtotal
            var subtotal = product.Price * item.Quantity;
            totalAmount += subtotal;

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });
        }

        // Create the order
        var order = new Order
        {
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            TotalAmount = totalAmount,
            Items = orderItems
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Load navigation properties for response
        await _context.Entry(order).Reference(o => o.User).LoadAsync();
        foreach (var item in order.Items)
        {
            await _context.Entry(item).Reference(oi => oi.Product).LoadAsync();
        }

        var response = new OrderResponse(
            order.Id,
            order.UserId,
            order.User.Name,
            order.Items.Select(oi => new OrderItemResponse(
                oi.ProductId,
                oi.Product.Name,
                oi.Quantity,
                oi.UnitPrice,
                oi.Subtotal
            )).ToList(),
            order.TotalAmount,
            order.Status.ToString(),
            order.CreatedAt
        );

        return response;
    }

    /// <summary>
    /// Update order status
    /// Demonstrates OneOf3 for status update validation
    /// </summary>
    public async Task<OneOf<OrderNotFoundError, ValidationError, OrderResponse>> 
        UpdateOrderStatusAsync(int id, string status)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return new OrderNotFoundError(id);
        }

        // Validate status
        if (!Enum.TryParse<OrderStatus>(status, true, out var newStatus))
        {
            return new ValidationError("Status", $"Invalid order status. Valid values are: {string.Join(", ", Enum.GetNames<OrderStatus>())}", status);
        }

        order.Status = newStatus;
        await _context.SaveChangesAsync();

        var response = new OrderResponse(
            order.Id,
            order.UserId,
            order.User.Name,
            order.Items.Select(oi => new OrderItemResponse(
                oi.ProductId,
                oi.Product.Name,
                oi.Quantity,
                oi.UnitPrice,
                oi.Subtotal
            )).ToList(),
            order.TotalAmount,
            order.Status.ToString(),
            order.CreatedAt
        );

        return response;
    }

    /// <summary>
    /// Cancel an order and restore product stock
    /// Demonstrates OneOf3 with business rule validation
    /// </summary>
    public async Task<OneOf<OrderNotFoundError, ValidationError, OrderResponse>> CancelOrderAsync(int id)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return new OrderNotFoundError(id);
        }

        // Business rule: Can only cancel pending or processing orders
        if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
        {
            var error = new ValidationError("OrderStatus", "Cannot cancel order with this status", order.Status.ToString());
            error.WithTag("OrderId", id).WithTag("CurrentStatus", order.Status.ToString()).WithTag("Reason", "Order already completed or cancelled");
            return error;
        }

        // Restore product stock
        foreach (var item in order.Items)
        {
            item.Product.StockQuantity += item.Quantity;
            item.Product.IsAvailable = true; // Make available again
        }

        order.Status = OrderStatus.Cancelled;
        await _context.SaveChangesAsync();

        var response = new OrderResponse(
            order.Id,
            order.UserId,
            order.User.Name,
            order.Items.Select(oi => new OrderItemResponse(
                oi.ProductId,
                oi.Product.Name,
                oi.Quantity,
                oi.UnitPrice,
                oi.Subtotal
            )).ToList(),
            order.TotalAmount,
            order.Status.ToString(),
            order.CreatedAt
        );

        return response;
    }

    /// <summary>
    /// Get order statistics for a user
    /// Demonstrates Result&lt;T&gt; with calculated data
    /// </summary>
    public async Task<Result<OrderStatistics>> GetUserOrderStatisticsAsync(int userId)
    {
        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .ToListAsync();

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

/// <summary>
/// Order statistics DTO
/// </summary>
public record OrderStatistics(
    int TotalOrders,
    int CompletedOrders,
    int PendingOrders,
    int CancelledOrders,
    decimal TotalSpent,
    decimal AverageOrderValue
);
