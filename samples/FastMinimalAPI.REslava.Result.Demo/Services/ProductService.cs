using Microsoft.EntityFrameworkCore;
using REslava.Result;
using REslava.Result.AdvancedPatterns;
using FastMinimalAPI.REslava.Result.Demo.Data;
using FastMinimalAPI.REslava.Result.Demo.Models;
using FastMinimalAPI.REslava.Result.Demo.Errors;

namespace FastMinimalAPI.REslava.Result.Demo.Services;

/// <summary>
/// Product service demonstrating inventory management with Result pattern
/// </summary>
public class ProductService
{
    private readonly DemoDbContext _context;

    public ProductService(DemoDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    public async Task<Result<List<ProductResponse>>> GetAllProductsAsync()
    {
        var products = await _context.Products.ToListAsync();

        var response = products.Select(p => new ProductResponse(
            p.Id, p.Name, p.Description, p.Price, p.StockQuantity, 
            p.Category, p.IsAvailable, p.CreatedAt
        )).ToList();

        return Result<List<ProductResponse>>.Ok(response);
    }

    /// <summary>
    /// Get product by ID - OneOf with NotFound
    /// </summary>
    public async Task<OneOf<ProductNotFoundError, ProductResponse>> GetProductByIdAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return new ProductNotFoundError(id);

        var response = new ProductResponse(
            product.Id, product.Name, product.Description, product.Price,
            product.StockQuantity, product.Category, product.IsAvailable, product.CreatedAt
        );

        return response;
    }

    /// <summary>
    /// Create product - OneOf3 with validation errors
    /// </summary>
    public async Task<OneOf<ValidationError, InvalidPriceError, ProductResponse>> CreateProductAsync(
        CreateProductRequest request)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(request.Name))
            return new RequiredFieldError("Name");

        if (string.IsNullOrWhiteSpace(request.Category))
            return new RequiredFieldError("Category");

        // Validate price
        if (request.Price <= 0)
            return new InvalidPriceError(request.Price);

        // Validate stock
        if (request.StockQuantity < 0)
            return new InvalidStockError(request.StockQuantity);

        // Create product
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            Category = request.Category,
            IsAvailable = request.StockQuantity > 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var response = new ProductResponse(
            product.Id, product.Name, product.Description, product.Price,
            product.StockQuantity, product.Category, product.IsAvailable, product.CreatedAt
        );

        return response;
    }

    /// <summary>
    /// Update product - OneOf4 demonstrating all 4 error types
    /// </summary>
    public async Task<OneOf<ValidationError, ProductNotFoundError, InvalidPriceError, ProductResponse>> UpdateProductAsync(
        int id, UpdateProductRequest request)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return new ProductNotFoundError(id);

        // Validate price if provided
        if (request.Price.HasValue && request.Price.Value <= 0)
            return new InvalidPriceError(request.Price.Value);

        // Validate stock if provided
        if (request.StockQuantity.HasValue && request.StockQuantity.Value < 0)
            return new InvalidStockError(request.StockQuantity.Value);

        // Update fields
        if (request.Name != null)
            product.Name = request.Name;

        if (request.Description != null)
            product.Description = request.Description;

        if (request.Price.HasValue)
            product.Price = request.Price.Value;

        if (request.StockQuantity.HasValue)
        {
            product.StockQuantity = request.StockQuantity.Value;
            product.IsAvailable = request.StockQuantity.Value > 0;
        }

        if (request.Category != null)
            product.Category = request.Category;

        if (request.IsAvailable.HasValue)
            product.IsAvailable = request.IsAvailable.Value;

        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var response = new ProductResponse(
            product.Id, product.Name, product.Description, product.Price,
            product.StockQuantity, product.Category, product.IsAvailable, product.CreatedAt
        );

        return response;
    }

    /// <summary>
    /// Delete product
    /// </summary>
    public async Task<Result<bool>> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return Result<bool>.Fail(new ProductNotFoundError(id));

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    public async Task<Result<List<ProductResponse>>> GetProductsByCategoryAsync(string category)
    {
        var products = await _context.Products
            .Where(p => p.Category == category)
            .ToListAsync();

        var response = products.Select(p => new ProductResponse(
            p.Id, p.Name, p.Description, p.Price, p.StockQuantity,
            p.Category, p.IsAvailable, p.CreatedAt
        )).ToList();

        return Result<List<ProductResponse>>.Ok(response);
    }
}
