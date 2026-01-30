using System;
using System.Collections.Generic;
using System.Linq;

namespace REslava.Result.SourceGenerators.Core.Utilities
{
    /// <summary>
    /// Maps error types and messages to appropriate HTTP status codes.
    /// Provides convention-based and configurable mapping strategies.
    /// </summary>
    public class HttpStatusCodeMapper
    {
        private readonly Dictionary<string, int> _customMappings;
        private readonly int _defaultStatusCode;

        public HttpStatusCodeMapper(int defaultStatusCode = 400)
        {
            _customMappings = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            _defaultStatusCode = defaultStatusCode;
        }

        /// <summary>
        /// Adds a custom mapping from error type name to HTTP status code.
        /// </summary>
        public void AddMapping(string errorTypeName, int statusCode)
        {
            _customMappings[errorTypeName] = statusCode;
        }

        /// <summary>
        /// Adds multiple custom mappings from a string array.
        /// Format: "ErrorTypeName:StatusCode"
        /// </summary>
        public void AddMappings(string[] mappings)
        {
            foreach (var mapping in mappings)
            {
                var parts = mapping.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[1], out var statusCode))
                {
                    AddMapping(parts[0], statusCode);
                }
            }
        }

        /// <summary>
        /// Determines the HTTP status code for a given error type name.
        /// Priority: Custom mappings > Convention-based > Default
        /// </summary>
        public int DetermineStatusCode(string errorTypeName)
        {
            // Handle null or empty error type name
            if (string.IsNullOrEmpty(errorTypeName))
                return _defaultStatusCode;

            // Check custom mappings first
            if (_customMappings.TryGetValue(errorTypeName, out var statusCode))
                return statusCode;

            // Apply convention-based mapping
            return MapByConvention(errorTypeName);
        }

        /// <summary>
        /// Determines the HTTP status code from an error message.
        /// Uses pattern matching on common error message phrases.
        /// </summary>
        public int DetermineStatusCodeFromMessage(string errorMessage)
        {
            var message = errorMessage.ToLowerInvariant();

            // Not Found patterns
            if (ContainsAny(message, "not found", "does not exist"))
                return 404;

            // Conflict patterns
            if (ContainsAny(message, "already exists", "duplicate"))
                return 409;

            // Validation patterns
            if (ContainsAny(message, "validation", "invalid"))
                return 422;

            // Unauthorized patterns
            if (ContainsAny(message, "unauthorized", "not authorized"))
                return 401;

            // Forbidden patterns
            if (ContainsAny(message, "forbidden", "access denied"))
                return 403;

            return _defaultStatusCode;
        }

        /// <summary>
        /// Gets the HTTP status text for a given status code.
        /// </summary>
        public static string GetStatusText(int statusCode)
        {
            return statusCode switch
            {
                400 => "Bad Request",
                401 => "Unauthorized",
                402 => "Payment Required",
                403 => "Forbidden",
                404 => "Not Found",
                405 => "Method Not Allowed",
                406 => "Not Acceptable",
                407 => "Proxy Authentication Required",
                408 => "Request Timeout",
                409 => "Conflict",
                410 => "Gone",
                411 => "Length Required",
                412 => "Precondition Failed",
                413 => "Payload Too Large",
                414 => "URI Too Long",
                415 => "Unsupported Media Type",
                416 => "Range Not Satisfiable",
                417 => "Expectation Failed",
                418 => "I'm a teapot",
                421 => "Misdirected Request",
                422 => "Unprocessable Entity",
                423 => "Locked",
                424 => "Failed Dependency",
                425 => "Too Early",
                426 => "Upgrade Required",
                428 => "Precondition Required",
                429 => "Too Many Requests",
                431 => "Request Header Fields Too Large",
                451 => "Unavailable For Legal Reasons",
                500 => "Internal Server Error",
                501 => "Not Implemented",
                502 => "Bad Gateway",
                503 => "Service Unavailable",
                504 => "Gateway Timeout",
                505 => "HTTP Version Not Supported",
                506 => "Variant Also Negotiates",
                507 => "Insufficient Storage",
                508 => "Loop Detected",
                510 => "Not Extended",
                511 => "Network Authentication Required",
                _ => "Error"
            };
        }

        /// <summary>
        /// Gets all configured custom mappings.
        /// </summary>
        public IReadOnlyDictionary<string, int> GetCustomMappings()
        {
            return _customMappings;
        }

        private int MapByConvention(string errorTypeName)
        {
            if (string.IsNullOrEmpty(errorTypeName))
                return _defaultStatusCode;

            var name = errorTypeName.ToLowerInvariant();

            // Not Found (404)
            if (ContainsAny(name, "notfound", "doesnotexist", "missing", "nosuch"))
                return 404;

            // Conflict (409)
            if (ContainsAny(name, "conflict", "duplicate", "alreadyexists", "exists"))
                return 409;

            // Validation (422)
            if (ContainsAny(name, "validation", "invalid", "malformed", "badformat"))
                return 422;

            // Unauthorized (401)
            if (ContainsAny(name, "unauthorized", "unauthenticated", "notauthenticated"))
                return 401;

            // Forbidden (403)
            if (ContainsAny(name, "forbidden", "accessdenied", "notauthorized", "denied"))
                return 403;

            // Rate Limit (429)
            if (ContainsAny(name, "ratelimit", "throttle", "toomanyrequests"))
                return 429;

            // Timeout (408)
            if (ContainsAny(name, "timeout", "timedout", "expired"))
                return 408;

            // Server Error (500)
            if (ContainsAny(name, "servererror", "internalerror", "systemerror", "critical"))
                return 500;

            // Service Unavailable (503)
            if (ContainsAny(name, "unavailable", "serviceunavailable", "maintenance"))
                return 503;

            return _defaultStatusCode;
        }

        private static bool ContainsAny(string text, params string[] keywords)
        {
            return keywords.Any(keyword => text.Contains(keyword));
        }
    }
}
