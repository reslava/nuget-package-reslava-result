namespace FastMvcAPI.REslava.Result.Demo.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public record CreateProductRequest(string Name, string Description, decimal Price, int StockQuantity, string Category);
public record UpdateProductRequest(string? Name = null, string? Description = null, decimal? Price = null, int? StockQuantity = null, string? Category = null, bool? IsAvailable = null);
public record ProductResponse(int Id, string Name, string Description, decimal Price, int StockQuantity, string Category, bool IsAvailable, DateTime CreatedAt);
