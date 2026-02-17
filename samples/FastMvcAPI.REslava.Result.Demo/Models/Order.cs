namespace FastMvcAPI.REslava.Result.Demo.Models;

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

public enum OrderStatus
{
    Pending,
    Processing,
    Completed,
    Cancelled
}

public record CreateOrderRequest(int UserId, List<OrderItemRequest> Items);
public record OrderItemRequest(int ProductId, int Quantity);
public record OrderResponse(int Id, int UserId, string UserName, List<OrderItemResponse> Items, decimal TotalAmount, string Status, DateTime CreatedAt);
public record OrderItemResponse(int ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal Subtotal);
