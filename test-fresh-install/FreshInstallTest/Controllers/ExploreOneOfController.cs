using Microsoft.AspNetCore.Mvc;
using REslava.Result;

namespace FreshInstallTest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExploreOneOfController : ControllerBase
{
    [HttpGet("test")]
    public IActionResult Test()
    {
        // Let's explore what OneOf types are available
        try
        {
            // Try to create different OneOf types to see what works
            var result = "Testing OneOf availability";
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Ok($"Error: {ex.Message}");
        }
    }
}
