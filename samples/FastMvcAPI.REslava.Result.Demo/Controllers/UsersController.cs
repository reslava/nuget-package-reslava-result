using FastMvcAPI.REslava.Result.Demo.Models;
using FastMvcAPI.REslava.Result.Demo.Services;
using Generated.ActionResultExtensions;
using Generated.OneOfActionResultExtensions;
using Microsoft.AspNetCore.Mvc;
using REslava.Result;

namespace FastMvcAPI.REslava.Result.Demo.Controllers;

/// <summary>
/// MVC Controller for Users — showcases Result&lt;T&gt;.ToActionResult() and OneOf&lt;&gt;.ToActionResult().
/// Domain errors auto-map: NotFoundError → 404, ValidationError → 422, ConflictError → 409.
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
    /// Get user by ID — OneOf&lt;NotFoundError, UserResponse&gt;.ToActionResult() one-liner
    /// NotFoundError (404) auto-mapped via domain error HttpStatusCode tag
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => (await _service.GetUserByIdAsync(id)).ToActionResult();

    /// <summary>
    /// Create user — OneOf&lt;ValidationError, ConflictError, UserResponse&gt;.ToActionResult()
    /// ValidationError → 422, ConflictError → 409, success → 200
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        => (await _service.CreateUserAsync(request)).ToActionResult();

    /// <summary>
    /// Update user — OneOf4&lt;ValidationError, NotFoundError, ConflictError, UserResponse&gt;.ToActionResult()
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
        => (await _service.UpdateUserAsync(id, request)).ToActionResult();

    /// <summary>
    /// Delete user — Result&lt;bool&gt;.ToDeleteActionResult() one-liner
    /// Domain errors auto-mapped: NotFoundError → 404, ConflictError → 409
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => (await _service.DeleteUserAsync(id)).ToDeleteActionResult();
}
