using System.ComponentModel.DataAnnotations;

namespace MinimalApi.Net10.Reference.Models;

public record CreateProductRequest
{
    [Required, MinLength(3, ErrorMessage = "Product name must be at least 3 characters")]
    [MaxLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
    public required string Name { get; init; }

    [Range(0.01, 10000.00, ErrorMessage = "Price must be between $0.01 and $10,000")]
    public required decimal Price { get; init; }

    [Required, MinLength(10, ErrorMessage = "Description must be at least 10 characters")]
    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public required string Description { get; init; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
    public int StockQuantity { get; init; } = 0;
}

public record UpdateProductRequest
{
    [Required, MinLength(3, ErrorMessage = "Product name must be at least 3 characters")]
    [MaxLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
    public required string Name { get; init; }

    [Range(0.01, 10000.00, ErrorMessage = "Price must be between $0.01 and $10,000")]
    public required decimal Price { get; init; }

    [Required, MinLength(10, ErrorMessage = "Description must be at least 10 characters")]
    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public required string Description { get; init; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
    public int StockQuantity { get; init; }

    public bool IsActive { get; init; } = true;
}
