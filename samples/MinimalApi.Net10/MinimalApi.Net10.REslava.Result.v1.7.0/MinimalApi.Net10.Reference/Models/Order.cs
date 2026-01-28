namespace MinimalApi.Net10.Reference.Models;

public record Order
{
    public int Id { get; init; }
    public required string CustomerEmail { get; init; }
    public List<OrderItem> Items { get; init; } = new();
    public decimal TotalAmount { get; init; }
    public DateTime OrderDate { get; init; } = DateTime.UtcNow;
    public OrderStatus Status { get; init; } = OrderStatus.Pending;
    public string ShippingAddress { get; init; } = string.Empty;
}

public record OrderItem
{
    public int ProductId { get; init; }
    public required string ProductName { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TotalPrice => Quantity * UnitPrice;
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}
