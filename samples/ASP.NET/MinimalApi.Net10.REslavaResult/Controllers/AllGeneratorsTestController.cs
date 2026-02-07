using Microsoft.AspNetCore.Mvc;
using REslava.Result;
using REslava.Result.AdvancedPatterns;
using MinimalApi.Net10.REslavaResult.Models;
using Generated.OneOfExtensions;
using Generated.ResultExtensions;

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
        /// Test Result to IResult conversion
        /// </summary>
        [HttpGet("result")]
        public IResult TestResult()
        {
            var result = Result<string>.Ok("Result conversion works!");
            return result.ToIResult();
        }

        /// <summary>
        /// Test OneOf2 to IResult conversion
        /// FIX: Use FromT2 (last position = success)
        /// </summary>
        [HttpGet("oneof2")]
        public IResult TestOneOf2()
        {
            // T2 is the success position
            var oneOf = OneOf<string, string>.FromT2("OneOf2 conversion works!");
            return oneOf.ToIResult();
        }

        /// <summary>
        /// Test OneOf3 to IResult conversion
        /// FIX: Use FromT3 (last position = success)
        /// </summary>
        [HttpGet("oneof3")]
        public IResult TestOneOf3()
        {
            // T3 is the success position
            var oneOf = OneOf<string, string, string>.FromT3("OneOf3 conversion works!");
            return oneOf.ToIResult();
        }

        /// <summary>
        /// Test OneOf4 to IResult conversion
        /// FIX: Use FromT4 (last position = success)
        /// </summary>
        [HttpGet("oneof4")]
        public IResult TestOneOf4()
        {
            // T4 is the success position
            var oneOf = OneOf<string, string, string, string>.FromT4("OneOf4 conversion works!");
            return oneOf.ToIResult();
        }

        /// <summary>
        /// Test error scenarios
        /// </summary>
        [HttpGet("error")]
        public IResult TestError()
        {
            var result = Result<string>.Fail("Test error scenario");
            return result.ToIResult();
        }
    }
}
