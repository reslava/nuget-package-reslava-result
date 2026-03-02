namespace REslava.Result.SourceGenerators.Attributes;

/// <summary>
/// Attribute to explicitly map error types to HTTP status codes and ProblemDetails.
/// This provides fine-grained control over HTTP responses for specific error types.
/// </summary>
/// <example>
/// <code>
/// [MapToProblemDetails(
///     StatusCode = 404,
///     Type = "https://api.example.com/errors/user-not-found",
///     Title = "User Not Found")]
/// public class UserNotFoundError : Error
/// {
///     public UserNotFoundError(int userId) 
///         : base($"User {userId} not found")
///     {
///         this.WithTag("UserId", userId);
///     }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class MapToProblemDetailsAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the HTTP status code (e.g., 404, 422, 500).
    /// Default is 400 (Bad Request).
    /// </summary>
    public int StatusCode { get; set; } = 400;

    /// <summary>
    /// Gets or sets the RFC 7807 type URI (e.g., "https://api.example.com/errors/not-found").
    /// If not specified, defaults to "https://httpstatuses.io/{StatusCode}".
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the human-readable title for this error type.
    /// If not specified, uses HTTP status text (e.g., "Not Found" for 404).
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets whether to include error tags in ProblemDetails.Extensions.
    /// Default is true.
    /// </summary>
    public bool IncludeTags { get; set; } = true;
}
