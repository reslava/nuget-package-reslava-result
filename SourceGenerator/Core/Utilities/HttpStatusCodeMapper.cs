using System;
using System.Collections.Generic;
using System.Linq;

namespace REslava.Result.SourceGenerators.Core.Utilities
{
    /// <summary>
    /// Intelligent HTTP status mapper for error types.
    /// Maps common error patterns to appropriate HTTP status codes.
    /// This provides the mapping logic that will be used in generated code.
    /// </summary>
    public static class HttpStatusCodeMapper
    {
        private static readonly Dictionary<string, string> ErrorMappings = new()
        {
            // Validation Errors → 400 Bad Request
            { "ValidationError", "BadRequest" },
            
            // Not Found Errors → 404 Not Found  
            { "UserNotFoundError", "NotFound" },
            { "OrderNotFoundError", "NotFound" },
            { "ProductNotFoundError", "NotFound" },
            
            // Conflict Errors → 409 Conflict
            { "ConflictError", "Conflict" },
            { "DuplicateError", "Conflict" },
            
            // Authorization Errors → 401 Unauthorized
            { "UnauthorizedError", "Unauthorized" },
            { "AuthenticationError", "Unauthorized" },
            
            // Forbidden Errors → 403 Forbidden
            { "ForbiddenError", "Forbid" },
            { "PermissionError", "Forbid" },
            
            // Server Errors → 500 Internal Server Error
            { "DatabaseError", "Problem" },
            { "SystemError", "Problem" },
            { "InfrastructureError", "Problem" }
        };

        /// <summary>
        /// Gets the HTTP result method name for an error type.
        /// </summary>
        public static string GetHttpResultMethod(Type errorType)
        {
            if (errorType == null)
                return "Problem";

            // Check for exact type match
            var errorTypeName = errorType.Name;
            if (ErrorMappings.TryGetValue(errorTypeName, out var method))
            {
                return method;
            }

            // Check for base type matches (inheritance)
            var baseMapping = ErrorMappings.FirstOrDefault(kvp => errorType.Name.Contains(kvp.Key));
            if (!string.IsNullOrEmpty(baseMapping.Value))
            {
                return baseMapping.Value;
            }

            // Check for naming patterns
            var typeName = errorType.Name;
            
            if (typeName.Contains("NotFound") || typeName.Contains("Missing"))
                return "NotFound";
                
            if (typeName.Contains("Conflict") || typeName.Contains("Duplicate"))
                return "Conflict";
                
            if (typeName.Contains("Unauthorized") || typeName.Contains("Authentication"))
                return "Unauthorized";
                
            if (typeName.Contains("Forbidden") || typeName.Contains("Permission"))
                return "Forbid";
                
            if (typeName.Contains("Validation") || typeName.Contains("Invalid"))
                return "BadRequest";
                
            if (typeName.Contains("Database") || typeName.Contains("System") || typeName.Contains("Infrastructure"))
                return "Problem";

            // Default to 400 for unknown error types
            return "BadRequest";
        }

        /// <summary>
        /// Checks if a type is likely an error type for intelligent HTTP mapping.
        /// </summary>
        public static bool IsErrorType(Type type)
        {
            if (type == null) return false;
            
            // Check if it inherits from Error base class
            try 
            {
                if (type.BaseType != null && type.BaseType.Name == "Error")
                    return true;
            }
            catch
            {
                // Ignore reflection errors in source generator context
            }
                
            // Check naming patterns
            var typeName = type.Name.ToLowerInvariant();
            return typeName.Contains("error") || 
                   typeName.Contains("exception") || 
                   typeName.Contains("fault") ||
                   typeName.Contains("failure");
        }
    }
}
