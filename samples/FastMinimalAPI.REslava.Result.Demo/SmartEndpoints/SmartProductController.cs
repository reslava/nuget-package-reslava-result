using FastMinimalAPI.REslava.Result.Demo.Models;
using FastMinimalAPI.REslava.Result.Demo.Services;
using REslava.Result;
using REslava.Result.AdvancedPatterns;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace FastMinimalAPI.REslava.Result.Demo.SmartEndpoints;

/// <summary>
/// SmartEndpoint controller for Products — auto-generates Minimal API endpoints
/// via source generator. Compare with Endpoints/ProductEndpoints.cs to see
/// ~85% boilerplate reduction.
///
/// The [AutoGenerateEndpoints] attribute + naming conventions auto-generate:
///   GET    /api/smart/products        → GetProducts()
///   GET    /api/smart/products/{id}   → GetProduct(int id)
///   POST   /api/smart/products        → CreateProduct(request)
///   PUT    /api/smart/products/{id}   → UpdateProduct(int id, request)
///   DELETE /api/smart/products/{id}   → DeleteProduct(int id)
///
/// [Validate] on CreateProductRequest: since v1.24.0, the generator detects the
/// [Validate] attribute on the POST body parameter and auto-emits the guard block:
///   var validation = request.Validate();
///   if (!validation.IsSuccess) return validation.ToIResult();
/// — before the service call, with no code change needed here.
///
/// CancellationToken: service methods accept a CancellationToken (v1.27.0).
/// The generator injects it as a lambda parameter automatically.
/// </summary>
[AutoGenerateEndpoints(RoutePrefix = "/api/smart/products")]
public class SmartProductController
{
    private readonly ProductService _service;

    public SmartProductController(ProductService service) => _service = service;

    public async Task<Result<List<ProductResponse>>> GetProducts(CancellationToken cancellationToken = default)
        => await _service.GetAllProductsAsync();

    public async Task<OneOf<NotFoundError, ProductResponse>> GetProduct(int id, CancellationToken cancellationToken = default)
        => await _service.GetProductByIdAsync(id);

    // CreateProductRequest is decorated with [Validate] — the generator emits the
    // validation guard automatically in the POST lambda (no extra code needed here).
    public async Task<OneOf<ValidationError, ProductResponse>> CreateProduct(CreateProductRequest request, CancellationToken cancellationToken = default)
        => await _service.CreateProductAsync(request, cancellationToken);

    public async Task<OneOf<ValidationError, NotFoundError, ProductResponse>> UpdateProduct(int id, UpdateProductRequest request, CancellationToken cancellationToken = default)
        => await _service.UpdateProductAsync(id, request);

    public async Task<Result<bool>> DeleteProduct(int id, CancellationToken cancellationToken = default)
        => await _service.DeleteProductAsync(id);
}
