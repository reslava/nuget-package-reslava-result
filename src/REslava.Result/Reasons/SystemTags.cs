namespace REslava.Result;

/// <summary>
/// Predefined <see cref="TagKey{T}"/> constants for integration and infrastructure context.
/// Used by <c>REslava.Result.Http</c> and <c>REslava.Result.AspNetCore</c> packages.
/// </summary>
public static class SystemTags
{
    /// <summary>The HTTP status code associated with this error.</summary>
    public static readonly TagKey<int> HttpStatus = new("HttpStatusCode");

    /// <summary>An application-specific error code (e.g., "ERR_001").</summary>
    public static readonly TagKey<string> ErrorCode = new("ErrorCode");

    /// <summary>The retry delay in seconds (for rate-limiting errors).</summary>
    public static readonly TagKey<int> RetryAfter = new("RetryAfter");

    /// <summary>The name of the service that produced this error.</summary>
    public static readonly TagKey<string> Service = new("Service");
}
