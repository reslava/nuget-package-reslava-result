namespace REslava.Result.SourceGenerators.Core.OneOf.Models;

/// <summary>
/// Represents the HTTP mapping result for a OneOf type argument.
/// </summary>
public class OneOfMappingResult
{
    /// <summary>
    /// Gets the type symbol being mapped.
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the clean type name for code generation (without namespaces).
    /// </summary>
    public string CleanTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the HTTP status code to return.
    /// </summary>
    public int StatusCode { get; set; } = 400;

    /// <summary>
    /// Gets the HTTP method response type (Ok, Created, NotFound, etc.).
    /// </summary>
    public string ResponseType { get; set; } = "Problem";

    /// <summary>
    /// Gets the location template for Created responses (e.g., "/api/users/{Id}").
    /// </summary>
    public string? LocationTemplate { get; set; }

    /// <summary>
    /// Gets whether to include ProblemDetails extensions.
    /// </summary>
    public bool IncludeProblemDetails { get; set; } = true;

    /// <summary>
    /// Gets whether this is an error type.
    /// </summary>
    public bool IsErrorType { get; set; } = true;

    /// <summary>
    /// Gets whether this is a success type.
    /// </summary>
    public bool IsSuccessType => !IsErrorType;

    /// <summary>
    /// Gets the variable name to use in generated code.
    /// </summary>
    public string VariableName => GenerateVariableName(CleanTypeName);

    /// <summary>
    /// Gets a string representation for debugging.
    /// </summary>
    public override string ToString()
    {
        return $"{TypeName} → {StatusCode} ({ResponseType})";
    }

    private static string GenerateVariableName(string typeName)
    {
        // Convert PascalCase to camelCase
        if (string.IsNullOrEmpty(typeName))
            return "item";

        var firstChar = char.ToLower(typeName[0]);
        if (typeName.Length == 1)
            return firstChar.ToString();

        return firstChar + typeName.Substring(1);
    }
}
