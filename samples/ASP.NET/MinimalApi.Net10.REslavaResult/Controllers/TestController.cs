using Microsoft.AspNetCore.Mvc;
 
namespace MinimalApi.Net10.REslavaResult.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetTest()
        {
            return Ok("Simple test works - controllers are registered!");
        }
    }
}