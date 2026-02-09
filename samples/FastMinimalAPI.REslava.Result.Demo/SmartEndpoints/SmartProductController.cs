using FastMinimalAPI.REslava.Result.Demo.Errors;
using FastMinimalAPI.REslava.Result.Demo.Models;
using FastMinimalAPI.REslava.Result.Demo.Services;
using REslava.Result;
using REslava.Result.AdvancedPatterns;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace FastMinimalAPI.REslava.Result.Demo.SmartEndpoints;

/// <summary>
/// SmartEndpoint controller for Products — auto-generates Minimal API endpoints
/// via source generator. Compare with Endpoints/ProductEndpoints.cs (209 lines)
/// to see ~85% boilerplate reduction.
///
/// The [AutoGenerateEndpoints] attribute + naming conventions auto-generate:
///   GET    /api/smart/products        → GetProducts()
///   GET    /api/smart/products/{id}   → GetProduct(int id)
///   POST   /api/smart/products        → CreateProduct(request)
///   PUT    /api/smart/products/{id}   → UpdateProduct(int id, request)
///   DELETE /api/smart/products/{id}   → DeleteProduct(int id)
/// </summary>
[AutoGenerateEndpoints(RoutePrefix = "/api/smart/products")]
public class SmartProductController
{
    private readonly ProductService _service;

    public SmartProductController(ProductService service) => _service = service;

    public async Task<Result<List<ProductResponse>>> GetProducts()
        => await _service.GetAllProductsAsync();

    public async Task<OneOf<ProductNotFoundError, ProductResponse>> GetProduct(int id)
        => await _service.GetProductByIdAsync(id);

    public async Task<OneOf<ValidationError, InvalidPriceError, ProductResponse>> CreateProduct(CreateProductRequest request)
        => await _service.CreateProductAsync(request);

    public async Task<OneOf<ValidationError, ProductNotFoundError, InvalidPriceError, ProductResponse>> UpdateProduct(int id, UpdateProductRequest request)
        => await _service.UpdateProductAsync(id, request);

    public async Task<Result<bool>> DeleteProduct(int id)
        => await _service.DeleteProductAsync(id);
}
