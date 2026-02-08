namespace FastMinimalAPI.REslava.Result.Demo.Models;

/// <summary>
/// Product entity representing items available for purchase
/// </summary>
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

/// <summary>
/// DTO for creating a new product
/// </summary>
public record CreateProductRequest(
    string Name, 
    string Description, 
    decimal Price, 
    int StockQuantity, 
    string Category);

/// <summary>
/// DTO for updating an existing product
/// </summary>
public record UpdateProductRequest(
    string? Name = null,
    string? Description = null,
    decimal? Price = null,
    int? StockQuantity = null,
    string? Category = null,
    bool? IsAvailable = null);

/// <summary>
/// DTO for product response
/// </summary>
public record ProductResponse(
    int Id, 
    string Name, 
    string Description, 
    decimal Price, 
    int StockQuantity, 
    string Category, 
    bool IsAvailable, 
    DateTime CreatedAt);
