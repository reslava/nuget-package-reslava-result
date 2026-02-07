using Generated.SmartEndpoints;
using REslava.Result.AdvancedPatterns;
using REslava.Result;
using MinimalApi.Net10.REslavaResult.Models;
using Microsoft.AspNetCore.Mvc;

namespace MinimalApi.Net10.REslavaResult.Controllers;

[AutoGenerateEndpoints(RoutePrefix = "/api/simple")]
public class SimpleTestController
{
    [HttpGet("test")]
    public Result<string> GetTest()
    {
        return Result<string>.Ok("Simple test works");
    }
}
