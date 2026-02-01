namespace REslava.Result.SourceGenerators.Generators.OneOfToIResult;

/// <summary>
/// Configuration for OneOf to IResult generation.
/// Following the same pattern as ResultToIResultConfig.
/// </summary>
public class OneOfToIResultConfig
{
    /// <summary>
    /// Gets or sets the default namespace for generated extensions.
    /// Default is "Generated.OneOfExtensions".
    /// </summary>
    public string DefaultNamespace { get; set; } = "Generated.OneOfExtensions";

    /// <summary>
    /// Gets or sets the default HTTP status code for success types.
    /// Default is 200 (OK).
    /// </summary>
    public int DefaultSuccessStatus { get; set; } = 200;

    /// <summary>
    /// Gets or sets the default HTTP status code for error types.
    /// Default is 400 (Bad Request).
    /// </summary>
    public int DefaultErrorStatus { get; set; } = 400;

    /// <summary>
    /// Gets or sets custom type-to-status-code mappings.
    /// Format: "TypeName:StatusCode".
    /// </summary>
    public string[] CustomMappings { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets whether to enable ProblemDetails generation.
    /// Default is true.
    /// </summary>
    public bool EnableProblemDetails { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable convention-based mapping.
    /// Default is true.
    /// </summary>
    public bool EnableConventionMapping { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to generate all HTTP method extensions.
    /// When false, only generates ToIResult().
    /// Default is true.
    /// </summary>
    public bool GenerateAllHttpMethods { get; set; } = true;
}
