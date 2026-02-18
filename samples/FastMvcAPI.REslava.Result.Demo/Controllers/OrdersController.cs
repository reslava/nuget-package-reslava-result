using FastMvcAPI.REslava.Result.Demo.Models;
using FastMvcAPI.REslava.Result.Demo.Services;
using Generated.ActionResultExtensions;
using Generated.OneOfActionResultExtensions;
using Microsoft.AspNetCore.Mvc;
using REslava.Result;

namespace FastMvcAPI.REslava.Result.Demo.Controllers;

/// <summary>
/// MVC Controller for Orders — showcases OneOf&lt;&gt;.ToActionResult() for auto-mapped error handling
/// and Result&lt;T&gt;.ToActionResult() for simple cases.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _service;

    public OrdersController(OrderService service) => _service = service;

    /// <summary>
    /// Get all orders — Result&lt;T&gt;.ToActionResult()
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => (await _service.GetAllOrdersAsync()).ToActionResult();

    /// <summary>
    /// Get order by ID — OneOf&lt;NotFoundError, OrderResponse&gt;.ToActionResult() one-liner
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => (await _service.GetOrderByIdAsync(id)).ToActionResult();

    /// <summary>
    /// Get orders by user — OneOf&lt;NotFoundError, List&lt;OrderResponse&gt;&gt;.ToActionResult()
    /// </summary>
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUser(int userId)
        => (await _service.GetOrdersByUserIdAsync(userId)).ToActionResult();

    /// <summary>
    /// Create order — OneOf4&lt;NotFoundError, ConflictError, ValidationError, OrderResponse&gt;.ToActionResult()
    /// Showcases the most complex error handling pattern — all auto-mapped:
    ///   NotFoundError (404) | ConflictError (409) | ValidationError (422) | OrderResponse (200)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        => (await _service.CreateOrderAsync(request)).ToActionResult();

    /// <summary>
    /// Update order status — OneOf3&lt;NotFoundError, ValidationError, OrderResponse&gt;.ToActionResult()
    /// </summary>
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        => (await _service.UpdateOrderStatusAsync(id, request.Status.ToString())).ToActionResult();

    /// <summary>
    /// Cancel order — OneOf3&lt;NotFoundError, ValidationError, OrderResponse&gt;.ToActionResult()
    /// </summary>
    [HttpDelete("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
        => (await _service.CancelOrderAsync(id)).ToActionResult();

    /// <summary>
    /// Get user order statistics — Result&lt;T&gt;.ToActionResult()
    /// </summary>
    [HttpGet("user/{userId:int}/statistics")]
    public async Task<IActionResult> GetStatistics(int userId)
        => (await _service.GetUserOrderStatisticsAsync(userId)).ToActionResult();
}

public record UpdateOrderStatusRequest(OrderStatus Status);
