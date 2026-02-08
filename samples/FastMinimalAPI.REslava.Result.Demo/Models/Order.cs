namespace FastMinimalAPI.REslava.Result.Demo.Models;

/// <summary>
/// Order entity representing user purchases
/// </summary>
public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// Order item representing individual products in an order
/// </summary>
public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
}

/// <summary>
/// Order status enumeration
/// </summary>
public enum OrderStatus
{
    Pending,
    Processing,
    Completed,
    Cancelled
}

/// <summary>
/// DTO for creating a new order
/// </summary>
public record CreateOrderRequest(int UserId, List<OrderItemRequest> Items);

/// <summary>
/// DTO for order item in create request
/// </summary>
public record OrderItemRequest(int ProductId, int Quantity);

/// <summary>
/// DTO for order response
/// </summary>
public record OrderResponse(
    int Id,
    int UserId,
    string UserName,
    List<OrderItemResponse> Items,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt);

/// <summary>
/// DTO for order item response
/// </summary>
public record OrderItemResponse(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal);
