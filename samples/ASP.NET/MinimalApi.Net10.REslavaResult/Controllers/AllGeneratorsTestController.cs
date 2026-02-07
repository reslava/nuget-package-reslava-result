using Microsoft.AspNetCore.Mvc;
using REslava.Result;
using REslava.Result.AdvancedPatterns;
using MinimalApi.Net10.REslavaResult.Models;
using Generated.ResultExtensions;
using Generated.OneOfExtensions;

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
        public IActionResult TestResult()
        {
            var result = Result<string>.Ok("Result conversion works!");
            return (IActionResult)result.ToIResult();
        }

        /// <summary>
        /// Test OneOf2 to IResult conversion
        /// </summary>
        [HttpGet("oneof2")]
        public IActionResult TestOneOf2()
        {
            var oneOfResult = OneOf<string, string>.FromT1("OneOf2 conversion works!");
            return (IActionResult)oneOfResult.ToIResult();
        }

        /// <summary>
        /// Test OneOf3 to IResult conversion
        /// </summary>
        [HttpGet("oneof3")]
        public IActionResult TestOneOf3()
        {
            var oneOfResult = OneOf<string, string, string>.FromT1("OneOf3 conversion works!");
            return (IActionResult)oneOfResult.ToIResult();
        }

        /// <summary>
        /// Test OneOf4 to IResult conversion
        /// </summary>
        [HttpGet("oneof4")]
        public IActionResult TestOneOf4()
        {
            var oneOfResult = OneOf<string, string, string, string>.FromT1("OneOf4 conversion works!");
            return (IActionResult)oneOfResult.ToIResult();
        }

        /// <summary>
        /// Test error scenarios
        /// </summary>
        [HttpGet("error")]
        public IActionResult TestError()
        {
            var result = Result<string>.Fail("Test error scenario");
            return (IActionResult)result.ToIResult();
        }
    }
}
