using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using REslava.Result;
using System.Collections.Generic;
using System.Linq;

namespace Generated.ResultExtensions
{
    /// <summary>
    /// Extension methods for converting Result&lt;T&gt; to IResult for Minimal API endpoints.
    /// This is a temporary manual implementation to demonstrate the concept.
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
                404 => Results.NotFound(errorMessage),
                400 => Results.BadRequest(errorMessage),
                _ => Results.Problem(errorMessage)
            };
        }

        /// <summary>
        /// Converts a non-generic Result to Microsoft.AspNetCore.Http.IResult for operations without return values.
        /// Success returns 204 No Content, failure returns appropriate error response.
        /// </summary>
        public static IResult ToIResult(this Result result)
        {
            if (result.IsSuccess)
            {
                return Results.NoContent();
            }

            var statusCode = DetermineStatusCode(result.Errors);
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Message));
            
            return statusCode switch
            {
                404 => Results.NotFound(errorMessage),
                400 => Results.BadRequest(errorMessage),
                _ => Results.Problem(errorMessage)
            };
        }

        private static int DetermineStatusCode(IReadOnlyList<IReason> errors)
        {
            if (errors.Count == 0) return 500;
            
            var errorMessage = errors.First().Message.ToLowerInvariant();
            
            if (errorMessage.Contains("not found")) return 404;
            if (errorMessage.Contains("validation") || errorMessage.Contains("invalid")) return 400;
            if (errorMessage.Contains("unauthorized")) return 401;
            if (errorMessage.Contains("forbidden")) return 403;
            
            return 400; // Default to Bad Request
        }
    }
}
