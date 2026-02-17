using FastMvcAPI.REslava.Result.Demo.Models;
using FastMvcAPI.REslava.Result.Demo.Services;
using Generated.ActionResultExtensions;
using Microsoft.AspNetCore.Mvc;
using REslava.Result;

namespace FastMvcAPI.REslava.Result.Demo.Controllers;

/// <summary>
/// MVC Controller for Users — showcases Result&lt;T&gt;.ToActionResult() and OneOf.Match().
/// Compare with FastMinimalAPI's Endpoints/UserEndpoints.cs for side-by-side comparison.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly UserService _service;

    public UsersController(UserService service) => _service = service;

    /// <summary>
    /// Get all users — Result&lt;T&gt;.ToActionResult() one-liner
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => (await _service.GetAllUsersAsync()).ToActionResult();

    /// <summary>
    /// Get user by ID — OneOf&lt;NotFoundError, UserResponse&gt;.Match()
    /// NotFoundError (404) auto-mapped via domain error HttpStatusCode tag
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetUserByIdAsync(id);
        return result.Match(
            notFound => new NotFoundObjectResult(new { error = notFound.Message, userId = id }) as IActionResult,
            user => new OkObjectResult(user));
    }

    /// <summary>
    /// Create user — OneOf&lt;ValidationError, ConflictError, UserResponse&gt;.Match()
    /// ValidationError → 422, ConflictError → 409
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var result = await _service.CreateUserAsync(request);
        return result.Match(
            validation => new ObjectResult(new { error = validation.Message, field = validation.FieldName }) { StatusCode = 422 } as IActionResult,
            conflict => new ConflictObjectResult(new { error = conflict.Message }),
            user => new CreatedAtActionResult(nameof(GetById), "Users", new { id = user.Id }, user));
    }

    /// <summary>
    /// Update user — OneOf4&lt;ValidationError, NotFoundError, ConflictError, UserResponse&gt;.Match()
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        var result = await _service.UpdateUserAsync(id, request);
        return result.Match(
            validation => new ObjectResult(new { error = validation.Message, field = validation.FieldName }) { StatusCode = 422 } as IActionResult,
            notFound => new NotFoundObjectResult(new { error = notFound.Message }),
            conflict => new ConflictObjectResult(new { error = conflict.Message }),
            user => new OkObjectResult(user));
    }

    /// <summary>
    /// Delete user — Result&lt;bool&gt;.ToDeleteActionResult() one-liner
    /// Domain errors auto-mapped: NotFoundError → 404, ConflictError → 409
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => (await _service.DeleteUserAsync(id)).ToDeleteActionResult();
}
