using System.ComponentModel.DataAnnotations;
using REslava.Result.SourceGenerators;

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

/// <summary>
/// [Validate] triggers the source generator to emit a .Validate() → Result&lt;CreateOrderRequest&gt; extension.
/// The MVC controller calls req.Validate() explicitly before the service call.
/// </summary>
[Validate]
public record CreateOrderRequest(
    [property: Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer")]
    int UserId,

    [property: Required(ErrorMessage = "At least one order item is required")]
    List<OrderItemRequest> Items);

public record OrderItemRequest(
    [property: Range(1, int.MaxValue, ErrorMessage = "ProductId must be a positive integer")]
    int ProductId,

    [property: Range(1, 1_000, ErrorMessage = "Quantity must be between 1 and 1,000")]
    int Quantity);

public record OrderResponse(int Id, int UserId, string UserName, List<OrderItemResponse> Items, decimal TotalAmount, string Status, DateTime CreatedAt);
public record OrderItemResponse(int ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal Subtotal);
