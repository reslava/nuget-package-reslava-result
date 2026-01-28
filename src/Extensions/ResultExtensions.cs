using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using REslava.Result;
using System.Collections.Generic;
using System.Linq;

namespace REslava.Result.Extensions
{
    /// <summary>
    /// Extension methods for converting Result&lt;T&gt; to IResult for Minimal API endpoints.
    /// Manual implementation for immediate functionality.
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Converts a Result&lt;T&gt; to Microsoft.AspNetCore.Http.IResult for use in Minimal API endpoints.
        /// Success returns 200 OK with the value, failure returns appropriate error response.
        /// </summary>
        public static IResult ToIResult<T>(this Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Results.Ok(result.Value);
            }

            var statusCode = DetermineStatusCode(result.Errors);
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Message));
            
            return statusCode switch
            {
                400 => Results.BadRequest(new { error = errorMessage }),
                404 => Results.NotFound(new { error = errorMessage }),
                500 => Results.Problem(detail: errorMessage, statusCode: 500),
                _ => Results.Problem(detail: errorMessage, statusCode: statusCode)
            };
        }

        /// <summary>
        /// Converts a non-generic Result to Microsoft.AspNetCore.Http.IResult.
        /// </summary>
        public static IResult ToIResult(this Result result)
        {
            if (result.IsSuccess)
            {
                return Results.Ok();
            }

            var statusCode = DetermineStatusCode(result.Errors);
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Message));
            
            return statusCode switch
            {
                400 => Results.BadRequest(new { error = errorMessage }),
                404 => Results.NotFound(new { error = errorMessage }),
                500 => Results.Problem(detail: errorMessage, statusCode: 500),
                _ => Results.Problem(detail: errorMessage, statusCode: statusCode)
            };
        }

        private static int DetermineStatusCode(IEnumerable<Error> errors)
        {
            if (errors.Any(e => e.Type == ErrorType.Validation))
                return 400;
            
            if (errors.Any(e => e.Type == ErrorType.NotFound))
                return 404;
            
            if (errors.Any(e => e.Type == ErrorType.Unauthorized))
                return 401;
            
            if (errors.Any(e => e.Type == ErrorType.Forbidden))
                return 403;
            
            if (errors.Any(e => e.Type == ErrorType.Conflict))
                return 409;
            
            return 500; // Default to server error
        }
    }
}
