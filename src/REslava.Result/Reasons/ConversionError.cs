using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace REslava.Result;

/// <summary>
/// Represents an error that occurred during implicit type conversion.
/// This error is used when implicit operators receive invalid input (null, empty collections, etc.)
/// instead of throwing exceptions, keeping the API consistent with the Result pattern philosophy.
/// </summary>
/// <remarks>
/// ConversionError is automatically created in these scenarios:
/// <list type="bullet">
///   <item>Null error provided to implicit conversion</item>
///   <item>Empty error array provided to implicit conversion</item>
///   <item>Empty error list provided to implicit conversion</item>
/// </list>
/// </remarks>
public class ConversionError : Reason<ConversionError>, IError
{
    // ========================================================================
    // Public Constructor - Main entry point
    // ========================================================================
    /// <summary>
    /// Creates a new ConversionError with the specified reason.
    /// Automatically tags the error as a conversion error with Warning severity.
    /// </summary>
    public ConversionError(
        string reason,
        [CallerMemberName] string? callerMember = null,
        [CallerFilePath]   string? callerFile   = null,
        [CallerLineNumber] int     callerLine   = 0)
        : base(
            $"Conversion failed: {reason}",
            CreateDefaultTags(),
            ReasonMetadata.FromCaller(callerMember, callerFile, callerLine))
    {
    }

    // ========================================================================
    // Private Constructor - For immutable copies (CRTP pattern)
    // ========================================================================
    private ConversionError(
        string message,
        ImmutableDictionary<string, object> tags)
        : base(message, tags)
    {
    }

    // ========================================================================
    // CRTP Factory Method
    // ========================================================================
    /// <summary>
    /// Creates a new ConversionError with updated message and tags (CRTP pattern).
    /// </summary>
    protected override ConversionError CreateNew(
        string message,
        ImmutableDictionary<string, object> tags)
    {
        return new ConversionError(message, tags);
    }

    // ========================================================================
    // Helper - Create default tags (static, called once in constructor)
    // ========================================================================
    private static ImmutableDictionary<string, object> CreateDefaultTags()
    {
        return ImmutableDictionary<string, object>.Empty
            .Add("ErrorType", "Conversion")
            .Add("Severity", "Warning")
            .Add("Timestamp", DateTime.UtcNow);
    }

    // ========================================================================
    // Fluent API Extensions (ConversionError-specific)
    // ========================================================================
    /// <summary>
    /// Adds conversion-specific context to the error.
    /// Returns ConversionError for fluent chaining.
    /// </summary>
    public ConversionError WithConversionType(string conversionType)
    {
        return WithTag("ConversionType", conversionType);
    }

    /// <summary>
    /// Adds the provided value to error context.
    /// Returns ConversionError for fluent chaining.
    /// </summary>
    public ConversionError WithProvidedValue(object? value)
    {
        return WithTag("ProvidedValue", value?.ToString() ?? "null");
    }
}
