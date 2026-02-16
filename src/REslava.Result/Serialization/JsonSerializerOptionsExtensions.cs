using System.Text.Json;

namespace REslava.Result.Serialization;

/// <summary>
/// Extension methods for registering REslava.Result JSON converters.
/// </summary>
public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// Registers JSON converters for Result&lt;T&gt;, OneOf, and Maybe&lt;T&gt;.
    /// </summary>
    /// <param name="options">The JsonSerializerOptions to configure.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <example>
    /// <code>
    /// var options = new JsonSerializerOptions();
    /// options.AddREslavaResultConverters();
    ///
    /// var json = JsonSerializer.Serialize(Result&lt;int&gt;.Ok(42), options);
    /// </code>
    /// </example>
    public static JsonSerializerOptions AddREslavaResultConverters(this JsonSerializerOptions options)
    {
        options.Converters.Add(new ResultJsonConverterFactory());
        options.Converters.Add(new OneOfJsonConverterFactory());
        options.Converters.Add(new MaybeJsonConverterFactory());
        return options;
    }
}
