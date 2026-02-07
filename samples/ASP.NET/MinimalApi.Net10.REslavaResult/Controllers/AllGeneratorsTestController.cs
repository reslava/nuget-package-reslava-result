using Microsoft.AspNetCore.Mvc;
using REslava.Result;
using REslava.Result.AdvancedPatterns;
using MinimalApi.Net10.REslavaResult.Models;

namespace MinimalApi.Net10.REslavaResult.Controllers
{
    [ApiController]
    [Route("api/test-all")]
    public class AllGeneratorsTestController : ControllerBase
    {
        /// <summary>
        /// Simple test endpoint
        /// </summary>
        [HttpGet("simple")]
        public IActionResult TestSimple()
        {
            return Ok("Simple test works!");
        }

        /// <summary>
        /// Test Result&lt;T&gt; to IResult conversion
        /// </summary>
        [HttpGet("result")]
        public Result<string> TestResult()
        {
            return Result<string>.Ok("Result conversion works!");
        }

        /// <summary>
        /// Test OneOf2 to IResult conversion
        /// </summary>
        [HttpGet("oneof2")]
        public OneOf<string, string> TestOneOf2()
        {
            return OneOf<string, string>.FromT1("OneOf2 conversion works!");
        }

        /// <summary>
        /// Test OneOf3 to IResult conversion
        /// </summary>
        [HttpGet("oneof3")]
        public OneOf<string, string, string> TestOneOf3()
        {
            return OneOf<string, string, string>.FromT2("OneOf3 conversion works!");
        }

        /// <summary>
        /// Test OneOf4 to IResult conversion
        /// </summary>
        [HttpGet("oneof4")]
        public OneOf<string, string, string, string> TestOneOf4()
        {
            return OneOf<string, string, string, string>.FromT3("OneOf4 conversion works!");
        }

        /// <summary>
        /// Test error scenarios
        /// </summary>
        [HttpGet("error")]
        public Result<string> TestError()
        {
            return Result<string>.Fail("Test error scenario");
        }
    }
}
