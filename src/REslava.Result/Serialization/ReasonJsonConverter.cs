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
    /// Format: { "type": "Error", "message": "...", "tags": { ... }, "metadata": { ... } }
    /// The "metadata" key is omitted when Metadata is empty.
    /// </summary>
    internal static void WriteReason(Utf8JsonWriter writer, IReason reason, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("type", reason.GetType().Name);
        writer.WriteString("message", reason.Message);

        writer.WritePropertyName("tags");
        WriteTags(writer, reason.Tags, options);

        if (reason is IReasonMetadata m && m.Metadata != ReasonMetadata.Empty)
        {
            writer.WritePropertyName("metadata");
            WriteMetadata(writer, m.Metadata);
        }

        writer.WriteEndObject();
    }

    /// <summary>
    /// Reads an IError from JSON.
    /// All error types deserialize as Error with message + tags.
    /// </summary>
    internal static IError ReadError(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        var (message, tags, metadata) = ReadReasonProperties(ref reader);
        Error error = new Error(message);
        if (tags.Count > 0)
            error = error.WithTagsFrom(tags);
        // WithMetadata overrides the [CallerMemberName] auto-captured during new Error(message)
        return error.WithMetadata(metadata);
    }

    /// <summary>
    /// Reads an ISuccess from JSON.
    /// </summary>
    internal static ISuccess ReadSuccess(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        var (message, tags, metadata) = ReadReasonProperties(ref reader);
        Success success = new Success(message);
        if (tags.Count > 0)
            success = success.WithTagsFrom(tags);
        return success.WithMetadata(metadata);
    }

    private static (string message, ImmutableDictionary<string, object> tags, ReasonMetadata metadata) ReadReasonProperties(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject for reason.");

        string? message = null;
        var tags = ImmutableDictionary<string, object>.Empty;
        var metadata = ReasonMetadata.Empty;

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
                case "metadata":
                    metadata = ReadMetadata(ref reader);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return (message ?? "Unknown error", tags, metadata);
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

    private static void WriteMetadata(Utf8JsonWriter writer, ReasonMetadata metadata)
    {
        writer.WriteStartObject();

        if (metadata.CallerMember is not null)
            writer.WriteString("CallerMember", metadata.CallerMember);
        if (metadata.CallerFile is not null)
            writer.WriteString("CallerFile", metadata.CallerFile);
        if (metadata.CallerLine is not null)
            writer.WriteNumber("CallerLine", metadata.CallerLine.Value);

        writer.WriteEndObject();
    }

    private static ReasonMetadata ReadMetadata(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject for metadata.");

        string? callerMember = null;
        string? callerFile = null;
        int callerLine = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected PropertyName in metadata object.");

            var propertyName = reader.GetString()!;
            reader.Read();

            switch (propertyName)
            {
                case "CallerMember": callerMember = reader.GetString(); break;
                case "CallerFile":   callerFile   = reader.GetString(); break;
                case "CallerLine":   callerLine   = reader.GetInt32();  break;
                default:             reader.Skip();                     break;
            }
        }

        return ReasonMetadata.FromCaller(callerMember, callerFile, callerLine);
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
