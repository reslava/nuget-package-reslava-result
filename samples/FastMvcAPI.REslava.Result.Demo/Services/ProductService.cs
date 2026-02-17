using Microsoft.EntityFrameworkCore;
using REslava.Result;
using REslava.Result.AdvancedPatterns;
using FastMvcAPI.REslava.Result.Demo.Data;
using FastMvcAPI.REslava.Result.Demo.Models;

namespace FastMvcAPI.REslava.Result.Demo.Services;

public class ProductService
{
    private readonly DemoDbContext _context;

    public ProductService(DemoDbContext context) => _context = context;

    public async Task<Result<List<ProductResponse>>> GetAllProductsAsync()
    {
        var products = await _context.Products.ToListAsync();
        var response = products.Select(p => new ProductResponse(
            p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.Category, p.IsAvailable, p.CreatedAt
        )).ToList();
        return Result<List<ProductResponse>>.Ok(response);
    }

    public async Task<OneOf<NotFoundError, ProductResponse>> GetProductByIdAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return new NotFoundError("Product", id);
        return new ProductResponse(product.Id, product.Name, product.Description, product.Price, product.StockQuantity, product.Category, product.IsAvailable, product.CreatedAt);
    }

    public async Task<OneOf<ValidationError, ProductResponse>> CreateProductAsync(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return new ValidationError("Name", "This field is required");
        if (string.IsNullOrWhiteSpace(request.Category))
            return new ValidationError("Category", "This field is required");
        if (request.Price <= 0)
            return new ValidationError("Price", "Price must be greater than 0");
        if (request.StockQuantity < 0)
            return new ValidationError("StockQuantity", "Stock quantity cannot be negative");

        var product = new Product
        {
            Name = request.Name, Description = request.Description,
            Price = request.Price, StockQuantity = request.StockQuantity,
            Category = request.Category, IsAvailable = request.StockQuantity > 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return new ProductResponse(product.Id, product.Name, product.Description, product.Price, product.StockQuantity, product.Category, product.IsAvailable, product.CreatedAt);
    }

    public async Task<OneOf<ValidationError, NotFoundError, ProductResponse>> UpdateProductAsync(int id, UpdateProductRequest request)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return new NotFoundError("Product", id);

        if (request.Price.HasValue && request.Price.Value <= 0)
            return new ValidationError("Price", "Price must be greater than 0");
        if (request.StockQuantity.HasValue && request.StockQuantity.Value < 0)
            return new ValidationError("StockQuantity", "Stock quantity cannot be negative");

        if (request.Name != null) product.Name = request.Name;
        if (request.Description != null) product.Description = request.Description;
        if (request.Price.HasValue) product.Price = request.Price.Value;
        if (request.StockQuantity.HasValue)
        {
            product.StockQuantity = request.StockQuantity.Value;
            product.IsAvailable = request.StockQuantity.Value > 0;
        }
        if (request.Category != null) product.Category = request.Category;
        if (request.IsAvailable.HasValue) product.IsAvailable = request.IsAvailable.Value;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return new ProductResponse(product.Id, product.Name, product.Description, product.Price, product.StockQuantity, product.Category, product.IsAvailable, product.CreatedAt);
    }

    public async Task<Result<bool>> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return Result<bool>.Fail(new NotFoundError("Product", id));
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return Result<bool>.Ok(true);
    }

    public async Task<Result<List<ProductResponse>>> GetProductsByCategoryAsync(string category)
    {
        var products = await _context.Products.Where(p => p.Category == category).ToListAsync();
        var response = products.Select(p => new ProductResponse(
            p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.Category, p.IsAvailable, p.CreatedAt
        )).ToList();
        return Result<List<ProductResponse>>.Ok(response);
    }
}
