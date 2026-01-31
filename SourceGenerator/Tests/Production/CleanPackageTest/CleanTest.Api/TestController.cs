using Microsoft.AspNetCore.Mvc;
using REslava.Result;
using Generated.ResultExtensions;

namespace CleanTest.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("success")]
        public IResult GetSuccess()
        {
            var result = Result<string>.Ok("Test successful!");
            return result.ToIResult();
        }

        [HttpGet("error")]
        public IResult GetError()
        {
            var result = Result<string>.Fail("Test error message");
            return result.ToIResult();
        }

        [HttpPost("create")]
        public IResult PostCreate([FromBody] CreateRequest request)
        {
            if (string.IsNullOrEmpty(request.Name))
                return Result<object>.Fail("Name is required").ToIResult();

            var result = Result<CreateResponse>.Ok(new CreateResponse 
            { 
                Id = 1, 
                Name = request.Name, 
                CreatedAt = DateTime.UtcNow 
            });
            
            return result.ToPostResult();
        }

        [HttpPut("update/{id}")]
        public IResult PutUpdate(int id, [FromBody] UpdateRequest request)
        {
            if (id <= 0)
                return Result<object>.Fail("Invalid ID").ToIResult();

            var result = Result<UpdateResponse>.Ok(new UpdateResponse 
            { 
                Id = id, 
                Name = request.Name, 
                UpdatedAt = DateTime.UtcNow 
            });
            
            return result.ToPutResult();
        }

        [HttpDelete("delete/{id}")]
        public IResult Delete(int id)
        {
            if (id <= 0)
                return Result<object>.Fail("Invalid ID").ToIResult();

            // Simulate successful deletion
            return Result<object>.Ok(new { Deleted = true, Id = id }).ToDeleteResult();
        }
    }

    public class CreateRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class CreateResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }
}
