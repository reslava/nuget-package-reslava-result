using System.Text.Json;
using System.Text.Json.Serialization;
using REslava.Result.AdvancedPatterns;

namespace REslava.Result.Serialization;

/// <summary>
/// JsonConverterFactory for OneOf&lt;T1,T2&gt;, OneOf&lt;T1,T2,T3&gt;, and OneOf&lt;T1,T2,T3,T4&gt;.
/// Format: { "index": 0, "value": ... }
/// </summary>
public class OneOfJsonConverterFactory : JsonConverterFactory
{
    private static readonly HashSet<Type> SupportedDefinitions =
    [
        typeof(OneOf<,>),
        typeof(OneOf<,,>),
        typeof(OneOf<,,,>)
    ];

    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsGenericType
        && SupportedDefinitions.Contains(typeToConvert.GetGenericTypeDefinition());

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var args = typeToConvert.GetGenericArguments();
        var converterType = args.Length switch
        {
            2 => typeof(OneOf2JsonConverter<,>).MakeGenericType(args),
            3 => typeof(OneOf3JsonConverter<,,>).MakeGenericType(args),
            4 => typeof(OneOf4JsonConverter<,,,>).MakeGenericType(args),
            _ => throw new JsonException($"Unsupported OneOf arity: {args.Length}")
        };
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

internal sealed class OneOf2JsonConverter<T1, T2> : JsonConverter<OneOf<T1, T2>>
{
    public override OneOf<T1, T2> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var (index, valueElement) = OneOfJsonHelper.ReadIndexAndValue(ref reader);

        return index switch
        {
            0 => OneOf<T1, T2>.FromT1(valueElement.Deserialize<T1>(options)!),
            1 => OneOf<T1, T2>.FromT2(valueElement.Deserialize<T2>(options)!),
            _ => throw new JsonException($"Invalid index {index} for OneOf<T1,T2>. Expected 0 or 1.")
        };
    }

    public override void Write(Utf8JsonWriter writer, OneOf<T1, T2> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value.IsT1)
        {
            writer.WriteNumber("index", 0);
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, value.AsT1, options);
        }
        else
        {
            writer.WriteNumber("index", 1);
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, value.AsT2, options);
        }

        writer.WriteEndObject();
    }
}

internal sealed class OneOf3JsonConverter<T1, T2, T3> : JsonConverter<OneOf<T1, T2, T3>>
{
    public override OneOf<T1, T2, T3> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var (index, valueElement) = OneOfJsonHelper.ReadIndexAndValue(ref reader);

        return index switch
        {
            0 => OneOf<T1, T2, T3>.FromT1(valueElement.Deserialize<T1>(options)!),
            1 => OneOf<T1, T2, T3>.FromT2(valueElement.Deserialize<T2>(options)!),
            2 => OneOf<T1, T2, T3>.FromT3(valueElement.Deserialize<T3>(options)!),
            _ => throw new JsonException($"Invalid index {index} for OneOf<T1,T2,T3>. Expected 0-2.")
        };
    }

    public override void Write(Utf8JsonWriter writer, OneOf<T1, T2, T3> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value.IsT1)
        {
            writer.WriteNumber("index", 0);
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, value.AsT1, options);
        }
        else if (value.IsT2)
        {
            writer.WriteNumber("index", 1);
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, value.AsT2, options);
        }
        else
        {
            writer.WriteNumber("index", 2);
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, value.AsT3, options);
        }

        writer.WriteEndObject();
    }
}

internal sealed class OneOf4JsonConverter<T1, T2, T3, T4> : JsonConverter<OneOf<T1, T2, T3, T4>>
{
    public override OneOf<T1, T2, T3, T4> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var (index, valueElement) = OneOfJsonHelper.ReadIndexAndValue(ref reader);

        return index switch
        {
            0 => OneOf<T1, T2, T3, T4>.FromT1(valueElement.Deserialize<T1>(options)!),
            1 => OneOf<T1, T2, T3, T4>.FromT2(valueElement.Deserialize<T2>(options)!),
            2 => OneOf<T1, T2, T3, T4>.FromT3(valueElement.Deserialize<T3>(options)!),
            3 => OneOf<T1, T2, T3, T4>.FromT4(valueElement.Deserialize<T4>(options)!),
            _ => throw new JsonException($"Invalid index {index} for OneOf<T1,T2,T3,T4>. Expected 0-3.")
        };
    }

    public override void Write(Utf8JsonWriter writer, OneOf<T1, T2, T3, T4> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value.IsT1)
        {
            writer.WriteNumber("index", 0);
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, value.AsT1, options);
        }
        else if (value.IsT2)
        {
            writer.WriteNumber("index", 1);
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, value.AsT2, options);
        }
        else if (value.IsT3)
        {
            writer.WriteNumber("index", 2);
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, value.AsT3, options);
        }
        else
        {
            writer.WriteNumber("index", 3);
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, value.AsT4, options);
        }

        writer.WriteEndObject();
    }
}

/// <summary>
/// Shared helper for reading OneOf JSON.
/// </summary>
internal static class OneOfJsonHelper
{
    internal static (int index, JsonElement valueElement) ReadIndexAndValue(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject for OneOf.");

        int? index = null;
        JsonElement? valueElement = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected PropertyName in OneOf object.");

            var propertyName = reader.GetString()!;
            reader.Read();

            switch (propertyName)
            {
                case "index":
                    index = reader.GetInt32();
                    break;
                case "value":
                    valueElement = JsonElement.ParseValue(ref reader);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        if (index is null)
            throw new JsonException("Missing 'index' property in OneOf JSON.");
        if (valueElement is null)
            throw new JsonException("Missing 'value' property in OneOf JSON.");

        return (index.Value, valueElement.Value);
    }
}
