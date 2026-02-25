using System.ComponentModel.DataAnnotations;
using REslava.Result.SourceGenerators;

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

/// <summary>
/// [Validate] triggers the source generator to emit a .Validate() → Result&lt;CreateProductRequest&gt; extension.
/// The MVC controller calls req.Validate() explicitly before the service call.
/// </summary>
[Validate]
public record CreateProductRequest(
    [property: Required(ErrorMessage = "Name is required")]
    [property: StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be 2–100 characters")]
    string Name,

    [property: StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    string Description,

    [property: Range(0.01, 1_000_000, ErrorMessage = "Price must be between 0.01 and 1,000,000")]
    decimal Price,

    [property: Range(0, 10_000, ErrorMessage = "StockQuantity must be 0–10,000")]
    int StockQuantity,

    [property: Required(ErrorMessage = "Category is required")]
    [property: StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
    string Category);

public record UpdateProductRequest(
    string? Name = null,
    string? Description = null,
    decimal? Price = null,
    int? StockQuantity = null,
    string? Category = null,
    bool? IsAvailable = null);

public record ProductResponse(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string Category,
    bool IsAvailable,
    DateTime CreatedAt);
