namespace MinimalApi.Net10.Reference.Models;

public record Product
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public decimal Price { get; init; }
    public string Description { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public bool IsActive { get; init; } = true;
}
