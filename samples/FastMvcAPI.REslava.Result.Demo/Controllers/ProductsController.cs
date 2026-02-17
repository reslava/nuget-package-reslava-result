using FastMvcAPI.REslava.Result.Demo.Models;
using FastMvcAPI.REslava.Result.Demo.Services;
using Generated.ActionResultExtensions;
using Microsoft.AspNetCore.Mvc;
using REslava.Result;

namespace FastMvcAPI.REslava.Result.Demo.Controllers;

/// <summary>
/// MVC Controller for Products — showcases ToActionResult() family and explicit overload escape hatch.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _service;

    public ProductsController(ProductService service) => _service = service;

    /// <summary>
    /// Get all products — Result&lt;T&gt;.ToActionResult()
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => (await _service.GetAllProductsAsync()).ToActionResult();

    /// <summary>
    /// Get product by ID — OneOf.Match()
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetProductByIdAsync(id);
        return result.Match(
            notFound => new NotFoundObjectResult(new { error = notFound.Message }) as IActionResult,
            product => new OkObjectResult(product));
    }

    /// <summary>
    /// Get products by category — Result&lt;T&gt;.ToActionResult()
    /// </summary>
    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetByCategory(string category)
        => (await _service.GetProductsByCategoryAsync(category)).ToActionResult();

    /// <summary>
    /// Create product — OneOf.Match() with ValidationError → 422
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var result = await _service.CreateProductAsync(request);
        return result.Match(
            validation => new ObjectResult(new { error = validation.Message, field = validation.FieldName }) { StatusCode = 422 } as IActionResult,
            product => new CreatedAtActionResult(nameof(GetById), "Products", new { id = product.Id }, product));
    }

    /// <summary>
    /// Update product — OneOf3.Match()
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
    {
        var result = await _service.UpdateProductAsync(id, request);
        return result.Match(
            validation => new ObjectResult(new { error = validation.Message, field = validation.FieldName }) { StatusCode = 422 } as IActionResult,
            notFound => new NotFoundObjectResult(new { error = notFound.Message }),
            product => new OkObjectResult(product));
    }

    /// <summary>
    /// Delete product — Result&lt;bool&gt;.ToDeleteActionResult()
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => (await _service.DeleteProductAsync(id)).ToDeleteActionResult();

    /// <summary>
    /// Update stock — Showcases the explicit ToActionResult(onSuccess, onFailure) overload.
    /// This is the "escape hatch" when you need full control over the response.
    /// </summary>
    [HttpPatch("{id:int}/stock")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockRequest request)
    {
        var result = await _service.UpdateProductAsync(id, new UpdateProductRequest(StockQuantity: request.Stock));
        return result.Match(
            validation => new ObjectResult(new { error = validation.Message, field = validation.FieldName }) { StatusCode = 422 } as IActionResult,
            notFound => new NotFoundObjectResult(new { error = notFound.Message }),
            product => new OkObjectResult(product));
    }
}

public record UpdateStockRequest(int Stock);
