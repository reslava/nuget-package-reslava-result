using FastMinimalAPI.REslava.Result.Demo.Models;
using FastMinimalAPI.REslava.Result.Demo.Services;
using REslava.Result;
using REslava.Result.AdvancedPatterns;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace FastMinimalAPI.REslava.Result.Demo.SmartEndpoints;

/// <summary>
/// SmartEndpoint controller for Orders — auto-generates Minimal API endpoints
/// via source generator. Compare with Endpoints/OrderEndpoints.cs to see
/// ~84% boilerplate reduction.
///
/// The [AutoGenerateEndpoints] attribute + naming conventions auto-generate:
///   GET    /api/smart/orders        → GetOrders()
///   GET    /api/smart/orders/{id}   → GetOrder(int id)
///   POST   /api/smart/orders        → CreateOrder(request)
///   DELETE /api/smart/orders/{id}   → DeleteOrder(int id)
///
/// Showcases OneOf4 pattern: CreateOrder can return 4 different types
/// (NotFoundError | ConflictError | ValidationError | OrderResponse)
/// </summary>
[AutoGenerateEndpoints(RoutePrefix = "/api/smart/orders", RequiresAuth = true)]
public class SmartOrderController
{
    private readonly OrderService _service;

    public SmartOrderController(OrderService service) => _service = service;

    /// <summary>
    /// Public read access — no authentication required
    /// </summary>
    [SmartAllowAnonymous]
    public async Task<Result<List<OrderResponse>>> GetOrders()
        => await _service.GetAllOrdersAsync();

    public async Task<OneOf<NotFoundError, OrderResponse>> GetOrder(int id)
        => await _service.GetOrderByIdAsync(id);

    /// <summary>
    /// Create order — OneOf4 showcasing the most complex error handling pattern:
    /// NotFoundError (404) | ConflictError (409) | ValidationError (422) | OrderResponse (200)
    /// </summary>
    public async Task<OneOf<NotFoundError, ConflictError, ValidationError, OrderResponse>> CreateOrder(CreateOrderRequest request)
        => await _service.CreateOrderAsync(request);

    public async Task<OneOf<NotFoundError, ValidationError, OrderResponse>> DeleteOrder(int id)
        => await _service.CancelOrderAsync(id);
}
