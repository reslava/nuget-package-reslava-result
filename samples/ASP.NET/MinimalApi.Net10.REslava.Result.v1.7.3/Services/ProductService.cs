namespace MinimalApi.Net10.Reference.Services;

public class ProductService
{
    private readonly Data.InMemoryDatabase _database;

    public ProductService(Data.InMemoryDatabase database)
    {
        _database = database;
    }

    public IEnumerable<Models.Product> GetAllProducts() => _database.GetProducts();

    public Models.Product? GetProductById(int id) => _database.GetProduct(id);

    public Models.Product CreateProduct(Models.CreateProductRequest request)
    {
        var product = new Models.Product
        {
            Name = request.Name,
            Price = request.Price,
            Description = request.Description,
            StockQuantity = request.StockQuantity
        };

        return _database.AddProduct(product);
    }

    public Models.Product? UpdateProduct(int id, Models.UpdateProductRequest request)
    {
        var existingProduct = _database.GetProduct(id);
        if (existingProduct == null)
            return null;

        var updatedProduct = new Models.Product
        {
            Name = request.Name,
            Price = request.Price,
            Description = request.Description,
            StockQuantity = request.StockQuantity,
            IsActive = request.IsActive,
            CreatedAt = existingProduct.CreatedAt // Preserve original creation date
        };

        return _database.UpdateProduct(id, updatedProduct);
    }

    public bool DeleteProduct(int id) => _database.DeleteProduct(id);

    public bool ProductExists(int id) => _database.ProductExists(id);
}
