using REslava.Result;
using System.Collections.Generic;
using System.Linq;

namespace REslava.Result
{
    /// <summary>
    /// Extension methods for HTTP response handling with Result&lt;T&gt;.
    /// Manual implementation for immediate functionality in v1.7.3.
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Converts a Result&lt;T&gt; to an HTTP response tuple for use in Minimal API endpoints.
        /// Returns (statusCode, value) for success, (statusCode, error) for failure.
        /// </summary>
        public static (int statusCode, object? value) ToHttpResponse<T>(this Result<T> result)
        {
            if (result.IsSuccess)
            {
                return (200, result.Value);
            }

            var statusCode = DetermineStatusCode(result.Errors);
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Message));
            
            return (statusCode, new { error = errorMessage });
        }

        /// <summary>
        /// Converts a non-generic Result to an HTTP response tuple.
        /// </summary>
        public static (int statusCode, object? value) ToHttpResponse(this Result result)
        {
            if (result.IsSuccess)
            {
                return (200, null);
            }

            var statusCode = DetermineStatusCode(result.Errors);
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Message));
            
            return (statusCode, new { error = errorMessage });
        }

        /// <summary>
        /// Gets the appropriate HTTP status code for a Result's errors.
        /// </summary>
        public static int GetHttpStatusCode<T>(this Result<T> result)
        {
            return result.IsSuccess ? 200 : DetermineStatusCode(result.Errors);
        }

        /// <summary>
        /// Gets the appropriate HTTP status code for a non-generic Result's errors.
        /// </summary>
        public static int GetHttpStatusCode(this Result result)
        {
            return result.IsSuccess ? 200 : DetermineStatusCode(result.Errors);
        }

        /// <summary>
        /// Gets the error message from a Result, or null if successful.
        /// </summary>
        public static string? GetErrorMessage<T>(this Result<T> result)
        {
            return result.IsSuccess ? null : string.Join(", ", result.Errors.Select(e => e.Message));
        }

        /// <summary>
        /// Gets the error message from a non-generic Result, or null if successful.
        /// </summary>
        public static string? GetErrorMessage(this Result result)
        {
            return result.IsSuccess ? null : string.Join(", ", result.Errors.Select(e => e.Message));
        }

        private static int DetermineStatusCode(IEnumerable<IError> errors)
        {
            // Simple error classification based on message content
            var errorMessages = errors.Select(e => e.Message).ToList();
            
            if (errorMessages.Any(m => m.Contains("not found", StringComparison.OrdinalIgnoreCase)))
                return 404;
            
            if (errorMessages.Any(m => m.Contains("unauthorized", StringComparison.OrdinalIgnoreCase)))
                return 401;
            
            if (errorMessages.Any(m => m.Contains("forbidden", StringComparison.OrdinalIgnoreCase)))
                return 403;
            
            if (errorMessages.Any(m => m.Contains("validation", StringComparison.OrdinalIgnoreCase)))
                return 400;
            
            if (errorMessages.Any(m => m.Contains("conflict", StringComparison.OrdinalIgnoreCase)))
                return 409;
            
            return 500; // Default to server error
        }
    }
}
