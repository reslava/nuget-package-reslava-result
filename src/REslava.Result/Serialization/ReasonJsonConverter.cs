using System.Collections.Immutable;
using System.Text.Json;

namespace REslava.Result.Serialization;

/// <summary>
/// Helper methods for serializing and deserializing IError and ISuccess instances.
/// Not registered as a converter directly — used internally by ResultJsonConverter.
/// </summary>
internal static class ReasonJsonConverter
{
    /// <summary>
    /// Writes an IReason (IError or ISuccess) to JSON.
    /// Format: { "type": "Error", "message": "...", "tags": { ... } }
    /// </summary>
    internal static void WriteReason(Utf8JsonWriter writer, IReason reason, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("type", reason.GetType().Name);
        writer.WriteString("message", reason.Message);

        writer.WritePropertyName("tags");
        WriteTags(writer, reason.Tags, options);

        writer.WriteEndObject();
    }

    /// <summary>
    /// Reads an IError from JSON.
    /// All error types deserialize as Error with message + tags.
    /// </summary>
    internal static IError ReadError(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        var (message, tags) = ReadReasonProperties(ref reader);
        var error = new Error(message);
        return tags.Count > 0 ? error.WithTagsFrom(tags) : error;
    }

    /// <summary>
    /// Reads an ISuccess from JSON.
    /// </summary>
    internal static ISuccess ReadSuccess(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        var (message, tags) = ReadReasonProperties(ref reader);
        var success = new Success(message);
        return tags.Count > 0 ? success.WithTagsFrom(tags) : success;
    }

    private static (string message, ImmutableDictionary<string, object> tags) ReadReasonProperties(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject for reason.");

        string? message = null;
        var tags = ImmutableDictionary<string, object>.Empty;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected PropertyName in reason object.");

            var propertyName = reader.GetString()!;
            reader.Read();

            switch (propertyName)
            {
                case "type":
                    // Read but don't use for deserialization — all errors become Error
                    reader.GetString();
                    break;
                case "message":
                    message = reader.GetString();
                    break;
                case "tags":
                    tags = ReadTags(ref reader);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return (message ?? "Unknown error", tags);
    }

    private static void WriteTags(Utf8JsonWriter writer, ImmutableDictionary<string, object> tags, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var kvp in tags)
        {
            writer.WritePropertyName(kvp.Key);
            JsonSerializer.Serialize(writer, kvp.Value, kvp.Value?.GetType() ?? typeof(object), options);
        }

        writer.WriteEndObject();
    }

    private static ImmutableDictionary<string, object> ReadTags(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject for tags.");

        var builder = ImmutableDictionary.CreateBuilder<string, object>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected PropertyName in tags object.");

            var key = reader.GetString()!;
            reader.Read();

            // Store tag values as JsonElement — consumer can extract typed values
            var element = JsonElement.ParseValue(ref reader);
            builder.Add(key, element);
        }

        return builder.ToImmutable();
    }
}
