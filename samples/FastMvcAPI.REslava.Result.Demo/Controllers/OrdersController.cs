using FastMvcAPI.REslava.Result.Demo.Models;
using FastMvcAPI.REslava.Result.Demo.Services;
using Generated.ActionResultExtensions;
using Microsoft.AspNetCore.Mvc;
using REslava.Result;

namespace FastMvcAPI.REslava.Result.Demo.Controllers;

/// <summary>
/// MVC Controller for Orders — showcases OneOf4.Match() for complex error handling
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
    /// Get order by ID — OneOf.Match()
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetOrderByIdAsync(id);
        return result.Match(
            notFound => new NotFoundObjectResult(new { error = notFound.Message }) as IActionResult,
            order => new OkObjectResult(order));
    }

    /// <summary>
    /// Get orders by user — OneOf.Match()
    /// </summary>
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var result = await _service.GetOrdersByUserIdAsync(userId);
        return result.Match(
            notFound => new NotFoundObjectResult(new { error = notFound.Message }) as IActionResult,
            orders => new OkObjectResult(orders));
    }

    /// <summary>
    /// Create order — OneOf4&lt;NotFoundError, ConflictError, ValidationError, OrderResponse&gt;.Match()
    /// Showcases the most complex error handling pattern:
    ///   NotFoundError (404) | ConflictError (409) | ValidationError (422) | OrderResponse (201)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        var result = await _service.CreateOrderAsync(request);
        return result.Match(
            notFound => new NotFoundObjectResult(new { error = notFound.Message }) as IActionResult,
            conflict => new ConflictObjectResult(new { error = conflict.Message }),
            validation => new ObjectResult(new { error = validation.Message, field = validation.FieldName }) { StatusCode = 422 },
            order => new CreatedAtActionResult(nameof(GetById), "Orders", new { id = order.Id }, order));
    }

    /// <summary>
    /// Update order status — OneOf3.Match()
    /// </summary>
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        var result = await _service.UpdateOrderStatusAsync(id, request.Status.ToString());
        return result.Match(
            notFound => new NotFoundObjectResult(new { error = notFound.Message }) as IActionResult,
            validation => new ObjectResult(new { error = validation.Message, field = validation.FieldName }) { StatusCode = 422 },
            order => new OkObjectResult(order));
    }

    /// <summary>
    /// Cancel order — OneOf3.Match()
    /// </summary>
    [HttpDelete("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _service.CancelOrderAsync(id);
        return result.Match(
            notFound => new NotFoundObjectResult(new { error = notFound.Message }) as IActionResult,
            validation => new ObjectResult(new { error = validation.Message, field = validation.FieldName }) { StatusCode = 422 },
            order => new OkObjectResult(order));
    }

    /// <summary>
    /// Get user order statistics — Result&lt;T&gt;.ToActionResult()
    /// </summary>
    [HttpGet("user/{userId:int}/statistics")]
    public async Task<IActionResult> GetStatistics(int userId)
        => (await _service.GetUserOrderStatisticsAsync(userId)).ToActionResult();
}

public record UpdateOrderStatusRequest(OrderStatus Status);
