using System.Text.Json;
using System.Text.Json.Serialization;
using REslava.Result.AdvancedPatterns;

namespace REslava.Result.Serialization;

/// <summary>
/// JsonConverterFactory for Maybe&lt;T&gt;.
/// Some: { "hasValue": true, "value": T }
/// None: { "hasValue": false }
/// </summary>
public class MaybeJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsGenericType
        && typeToConvert.GetGenericTypeDefinition() == typeof(Maybe<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(MaybeJsonConverter<>).MakeGenericType(valueType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

internal sealed class MaybeJsonConverter<T> : JsonConverter<Maybe<T>>
{
    public override Maybe<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject for Maybe<T>.");

        bool? hasValue = null;
        T? value = default;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected PropertyName in Maybe object.");

            var propertyName = reader.GetString()!;
            reader.Read();

            switch (propertyName)
            {
                case "hasValue":
                    hasValue = reader.GetBoolean();
                    break;
                case "value":
                    value = JsonSerializer.Deserialize<T>(ref reader, options);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        if (hasValue is null)
            throw new JsonException("Missing 'hasValue' property in Maybe JSON.");

        return hasValue.Value ? Maybe<T>.Some(value!) : Maybe<T>.None;
    }

    public override void Write(Utf8JsonWriter writer, Maybe<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteBoolean("hasValue", value.HasValue);

        if (value.HasValue)
        {
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, value.Value, options);
        }

        writer.WriteEndObject();
    }
}
