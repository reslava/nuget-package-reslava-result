using System.Collections.Generic;
using System.Linq;

namespace REslava.Result.SourceGenerators.Core.OneOf.Utilities;

/// <summary>
/// Maps OneOf type names to HTTP status codes using convention-based rules.
/// Reuses the same logic as HttpStatusCodeMapper but specialized for OneOf patterns.
/// </summary>
public class OneOfHttpStatusCodeMapper
{
    private readonly Dictionary<string, int> _customMappings;
    private readonly int _defaultSuccessStatus;
    private readonly int _defaultErrorStatus;

    public OneOfHttpStatusCodeMapper(
        int defaultSuccessStatus = 200,
        int defaultErrorStatus = 400)
    {
        _customMappings = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        _defaultSuccessStatus = defaultSuccessStatus;
        _defaultErrorStatus = defaultErrorStatus;
    }

    /// <summary>
    /// Adds a custom mapping from type name to HTTP status code.
    /// </summary>
    public void AddMapping(string typeName, int statusCode)
    {
        _customMappings[typeName] = statusCode;
    }

    /// <summary>
    /// Adds multiple custom mappings from a string array.
    /// Format: "TypeName:StatusCode"
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
    /// Determines the HTTP status code for a given type name.
    /// Priority: Custom mappings > Convention-based > Default
    /// </summary>
    public int DetermineStatusCode(string typeName)
    {
        if (string.IsNullOrEmpty(typeName))
            return _defaultErrorStatus;

        // Check custom mappings first
        if (_customMappings.TryGetValue(typeName, out var statusCode))
            return statusCode;

        // Apply convention-based mapping
        return MapByConvention(typeName);
    }

    /// <summary>
    /// Determines the response type (Ok, Created, NotFound, etc.) for a given type name.
    /// </summary>
    public string DetermineResponseType(string typeName, int statusCode)
    {
        if (string.IsNullOrEmpty(typeName))
            return "Problem";

        var name = typeName.ToLowerInvariant();

        // Success response types
        if (statusCode >= 200 && statusCode < 300)
        {
            if (name.Contains("created") || statusCode == 201)
                return "Created";
            if (name.Contains("accepted") || statusCode == 202)
                return "Accepted";
            if (name.Contains("nocontent") || statusCode == 204)
                return "NoContent";
            return "Ok";
        }

        // Error response types
        return statusCode switch
        {
            400 => "BadRequest",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "NotFound",
            409 => "Conflict",
            422 => "UnprocessableEntity",
            429 => "TooManyRequests",
            500 => "InternalServerError",
            503 => "ServiceUnavailable",
            _ => "Problem"
        };
    }

    /// <summary>
    /// Determines if a type is an error type based on naming conventions.
    /// </summary>
    public bool IsErrorType(string typeName)
    {
        if (string.IsNullOrEmpty(typeName))
            return true;

        var name = typeName.ToLowerInvariant();
        
        // Check for error patterns
        return ContainsAny(name, "error", "exception", "invalid", "failed", "denied", "forbidden", "unauthorized");
    }

    private int MapByConvention(string typeName)
    {
        if (string.IsNullOrEmpty(typeName))
            return _defaultErrorStatus;

        var name = typeName.ToLowerInvariant();

        // Success patterns
        if (ContainsAny(name, "created", "accepted"))
            return 201;
        if (ContainsAny(name, "nocontent"))
            return 204;
        if (ContainsAny(name, "success", "ok", "result"))
            return 200;

        // Error patterns
        if (ContainsAny(name, "notfound", "doesnotexist", "missing"))
            return 404;
        if (ContainsAny(name, "unauthorized", "unauthenticated"))
            return 401;
        if (ContainsAny(name, "forbidden", "accessdenied", "denied"))
            return 403;
        if (ContainsAny(name, "conflict", "duplicate", "alreadyexists"))
            return 409;
        if (ContainsAny(name, "validation", "invalid"))
            return 422;
        if (ContainsAny(name, "ratelimit", "throttle", "toomanyrequests"))
            return 429;
        if (ContainsAny(name, "timeout", "timedout", "expired"))
            return 408;
        if (ContainsAny(name, "unavailable", "serviceunavailable", "maintenance"))
            return 503;
        if (ContainsAny(name, "servererror", "internalerror", "systemerror", "critical"))
            return 500;

        // Default based on type detection
        return IsErrorType(typeName) ? _defaultErrorStatus : _defaultSuccessStatus;
    }

    private static bool ContainsAny(string text, params string[] keywords)
    {
        return keywords.Any(keyword => text.Contains(keyword));
    }
}
