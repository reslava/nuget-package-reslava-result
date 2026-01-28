using REslava.Result;
using System.Collections.Immutable;

namespace MinimalApi.Net10.Reference.Services;

public class ProductService
{
    private readonly Data.InMemoryDatabase _database;

    public ProductService(Data.InMemoryDatabase database)
    {
        _database = database;
    }

    public Result<IEnumerable<Models.Product>> GetAllProducts() 
        => new Result<IEnumerable<Models.Product>>(_database.GetProducts(), ImmutableList<IReason>.Empty);

    public Result<Models.Product> GetProductById(int id)
    {
        var product = _database.GetProduct(id);
        return product is null 
            ? new Result<Models.Product>(null, new Error($"Product with ID {id} not found"))
            : new Result<Models.Product>(product, ImmutableList<IReason>.Empty);
    }

    public Result<Models.Product> CreateProduct(Models.CreateProductRequest request)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3)
            return new Result<Models.Product>(null, new Error("Product name must be at least 3 characters"));
        
        if (request.Price <= 0 || request.Price > 10000)
            return new Result<Models.Product>(null, new Error("Price must be between $0.01 and $10,000"));
        
        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 10)
            return new Result<Models.Product>(null, new Error("Description must be at least 10 characters"));

        var product = new Models.Product
        {
            Name = request.Name,
            Price = request.Price,
            Description = request.Description,
            StockQuantity = request.StockQuantity
        };

        var createdProduct = _database.AddProduct(product);
        return new Result<Models.Product>(createdProduct, ImmutableList<IReason>.Empty);
    }

    public Result<Models.Product> UpdateProduct(int id, Models.UpdateProductRequest request)
    {
        var existingProduct = _database.GetProduct(id);
        if (existingProduct == null)
            return new Result<Models.Product>(null, new Error($"Product with ID {id} not found"));

        // Basic validation
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3)
            return new Result<Models.Product>(null, new Error("Product name must be at least 3 characters"));
        
        if (request.Price <= 0 || request.Price > 10000)
            return new Result<Models.Product>(null, new Error("Price must be between $0.01 and $10,000"));
        
        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 10)
            return new Result<Models.Product>(null, new Error("Description must be at least 10 characters"));

        var updatedProduct = new Models.Product
        {
            Name = request.Name,
            Price = request.Price,
            Description = request.Description,
            StockQuantity = request.StockQuantity,
            IsActive = request.IsActive,
            CreatedAt = existingProduct.CreatedAt // Preserve original creation date
        };

        var result = _database.UpdateProduct(id, updatedProduct);
        return new Result<Models.Product>(result, ImmutableList<IReason>.Empty);
    }

    public Result<bool> DeleteProduct(int id)
    {
        var deleted = _database.DeleteProduct(id);
        return deleted 
            ? new Result<bool>(true, ImmutableList<IReason>.Empty)
            : new Result<bool>(false, new Error($"Product with ID {id} not found"));
    }

    public bool ProductExists(int id) => _database.ProductExists(id);
}
