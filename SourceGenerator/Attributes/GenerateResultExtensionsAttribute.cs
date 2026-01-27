using System;

namespace REslava.Result.SourceGenerators;

/// <summary>
/// Enables automatic generation of extension methods for converting Result&lt;T&gt; to Microsoft.AspNetCore.Http.IResult.
/// Apply this assembly-level attribute to projects that want to use the source generator.
/// </summary>
/// <example>
/// <code>
/// [assembly: GenerateResultExtensions]
/// [assembly: GenerateResultExtensions(Namespace = "MyApp.Generated")]
/// [assembly: GenerateResultExtensions(IncludeErrorTags = false)]
/// [assembly: GenerateResultExtensions(CustomErrorMappings = new[] { "ValidationError:422", "NotFoundError:404" })]
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class GenerateResultExtensionsAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the namespace for the generated extension methods.
    /// Default is "Generated.ResultExtensions".
    /// </summary>
    public string Namespace { get; set; } = "Generated.ResultExtensions";

    /// <summary>
    /// Gets or sets a value indicating whether to include error tags in ProblemDetails.
    /// Default is true.
    /// </summary>
    public bool IncludeErrorTags { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to log errors during conversion.
    /// Default is false.
    /// </summary>
    public bool LogErrors { get; set; } = false;

    /// <summary>
    /// Gets or sets custom error type to HTTP status code mappings.
    /// Format: "ErrorType:StatusCode,AnotherErrorType:AnotherStatusCode".
    /// </summary>
    public string[] CustomErrorMappings { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets a value indicating whether to generate HTTP method-specific extension methods (e.g., ToPostResult).
    /// Default is true.
    /// </summary>
    public bool GenerateHttpMethodExtensions { get; set; } = true;

    /// <summary>
    /// Gets or sets the default HTTP status code for errors that don't have a specific mapping.
    /// Default is 400 (Bad Request).
    /// </summary>
    public int DefaultErrorStatusCode { get; set; } = 400;
}
