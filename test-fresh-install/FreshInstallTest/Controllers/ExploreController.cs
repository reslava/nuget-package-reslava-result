using Microsoft.AspNetCore.Mvc;

// Test to explore available types
namespace FreshInstallTest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExploreController : ControllerBase
{
    [HttpGet("test")]
    public IActionResult Test()
    {
        // Try to create a simple OneOf to see what's available
        // We'll start with basic Result<T> first
        
        var result = REslava.Result.Result<string>.Ok("Hello World");
        return Ok($"Result<T> works: {result.IsSuccess}");
    }
}
