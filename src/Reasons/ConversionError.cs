using System.Collections.Immutable;
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
/// 
/// This allows developers to detect and handle conversion issues without exceptions:
/// <code>
/// Error[] errors = null;
/// Result&lt;User&gt; result = errors; // No exception, returns ConversionError
/// 
/// if (result.Errors[0] is ConversionError conversionError)
/// {
///     _logger.LogWarning($"Conversion issue: {conversionError.Message}");
///     // Check tags for details
///     var providedValue = conversionError.Tags["ProvidedValue"];
/// }
/// </code>
/// </remarks>
//public class ConversionError : Reason<ConversionError>, IError
public class ConversionError : Error
{
    /// <summary>
    /// Creates a new ConversionError with the specified reason.
    /// Automatically tags the error as a conversion error with Warning severity.
    /// </summary>
    /// <param name="reason">The reason for the conversion failure.</param>
    /// <example>
    /// <code>
    /// var error = new ConversionError("Empty error array provided")
    ///     .WithTags("ArrayLength", 0);
    /// </code>
    /// </example>
    public ConversionError(string reason) 
        : base($"Conversion failed: {reason}")
    {
        WithTags("ErrorType", "Conversion")
            .WithTags("Severity", "Warning")
            .WithTags("Timestamp", DateTime.UtcNow);
    }

    /// <summary>
    /// Private constructor for CRTP pattern (used by WithMessage/WithTags).
    /// </summary>
    ///

    private ConversionError(string message, ImmutableDictionary<string, object> tags)
        : base(message)
    {

        //// Copy tags from base
        base.Tags = tags.ToImmutableDictionary();
        //foreach (var tag in tags)
        //{
        //    base.WithTags(tag.Key, tag.Value);
        //}
    }

    /// <summary>
    /// Creates a new ConversionError with updated message and tags (CRTP pattern).
    /// </summary>
    protected override ConversionError CreateNew(string message, ImmutableDictionary<string, object> tags)
    {
        return new ConversionError(message, tags);
    }

    /// <summary>
    /// Adds conversion-specific context to the error.
    /// </summary>
    /// <param name="conversionType">The type being converted (e.g., "Error[]", "List&lt;Error&gt;").</param>
    /// <returns>This ConversionError for fluent chaining.</returns>
    public ConversionError WithConversionType(string conversionType)
    {
        return (ConversionError)WithTags("ConversionType", conversionType);
    }

    /// <summary>
    /// Adds the provided value to error context.
    /// </summary>
    /// <param name="value">The value that was provided (e.g., "null", "0 items").</param>
    /// <returns>This ConversionError for fluent chaining.</returns>
    public ConversionError WithProvidedValue(object value)
    {
        return (ConversionError)WithTags("ProvidedValue", value?.ToString() ?? "null");
    }
}
