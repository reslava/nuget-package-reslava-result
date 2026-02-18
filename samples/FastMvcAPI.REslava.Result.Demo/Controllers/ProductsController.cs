using FastMvcAPI.REslava.Result.Demo.Models;
using FastMvcAPI.REslava.Result.Demo.Services;
using Generated.ActionResultExtensions;
using Generated.OneOfActionResultExtensions;
using Microsoft.AspNetCore.Mvc;
using REslava.Result;

namespace FastMvcAPI.REslava.Result.Demo.Controllers;

/// <summary>
/// MVC Controller for Products — showcases ToActionResult() one-liners and explicit Match() escape hatch.
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
    /// Get product by ID — OneOf&lt;NotFoundError, ProductResponse&gt;.ToActionResult() one-liner
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => (await _service.GetProductByIdAsync(id)).ToActionResult();

    /// <summary>
    /// Get products by category — Result&lt;T&gt;.ToActionResult()
    /// </summary>
    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetByCategory(string category)
        => (await _service.GetProductsByCategoryAsync(category)).ToActionResult();

    /// <summary>
    /// Create product — OneOf&lt;ValidationError, ProductResponse&gt;.ToActionResult()
    /// ValidationError → 422, success → 200
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        => (await _service.CreateProductAsync(request)).ToActionResult();

    /// <summary>
    /// Update product — OneOf3&lt;ValidationError, NotFoundError, ProductResponse&gt;.ToActionResult()
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
        => (await _service.UpdateProductAsync(id, request)).ToActionResult();

    /// <summary>
    /// Delete product — Result&lt;bool&gt;.ToDeleteActionResult()
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => (await _service.DeleteProductAsync(id)).ToDeleteActionResult();

    /// <summary>
    /// Update stock — Showcases .Match() as an escape hatch for full control over responses.
    /// Use .Match() when you need custom response bodies or non-standard status codes.
    /// </summary>
    [HttpPatch("{id:int}/stock")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockRequest request)
    {
        var result = await _service.UpdateProductAsync(id, new UpdateProductRequest(StockQuantity: request.Stock));
        return result.Match(
            validation => new ObjectResult(new { error = validation.Message, field = validation.FieldName }) { StatusCode = 422 } as IActionResult,
            notFound => new NotFoundObjectResult(new { error = notFound.Message }),
            product => new OkObjectResult(new { product, message = "Stock updated successfully" }));
    }
}

public record UpdateStockRequest(int Stock);
