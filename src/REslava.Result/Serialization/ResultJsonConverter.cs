using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace REslava.Result.Serialization;

/// <summary>
/// JsonConverterFactory for Result&lt;T&gt;.
/// Produces JSON: { "isSuccess": bool, "value": T?, "errors": [...], "successes": [...] }
/// </summary>
public class ResultJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsGenericType
        && typeToConvert.GetGenericTypeDefinition() == typeof(Result<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(ResultJsonConverter<>).MakeGenericType(valueType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

internal sealed class ResultJsonConverter<TValue> : JsonConverter<Result<TValue>>
{
    public override Result<TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject for Result<T>.");

        bool? isSuccess = null;
        TValue? value = default;
        var errors = ImmutableList<IError>.Empty;
        var successes = ImmutableList<ISuccess>.Empty;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected PropertyName in Result object.");

            var propertyName = reader.GetString()!;
            reader.Read();

            switch (propertyName)
            {
                case "isSuccess":
                    isSuccess = reader.GetBoolean();
                    break;
                case "value":
                    if (reader.TokenType != JsonTokenType.Null)
                        value = JsonSerializer.Deserialize<TValue>(ref reader, options);
                    else
                        value = default;
                    break;
                case "errors":
                    errors = ReadReasonArray<IError>(ref reader, options, ReasonJsonConverter.ReadError);
                    break;
                case "successes":
                    successes = ReadReasonArray<ISuccess>(ref reader, options, ReasonJsonConverter.ReadSuccess);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        if (isSuccess is null)
            throw new JsonException("Missing 'isSuccess' property in Result JSON.");

        if (isSuccess.Value)
        {
            var result = Result<TValue>.Ok(value!);
            if (successes.Count > 0)
                result = result.WithSuccesses(successes);
            return result;
        }
        else
        {
            if (errors.Count == 0)
                throw new JsonException("Failed Result must have at least one error.");

            var result = Result<TValue>.Fail(errors);
            if (successes.Count > 0)
                result = result.WithSuccesses(successes);
            return result;
        }
    }

    public override void Write(Utf8JsonWriter writer, Result<TValue> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteBoolean("isSuccess", value.IsSuccess);

        writer.WritePropertyName("value");
        if (value.IsSuccess)
            JsonSerializer.Serialize(writer, value.Value, options);
        else
            writer.WriteNullValue();

        writer.WritePropertyName("errors");
        WriteReasonArray(writer, value.Errors, options);

        writer.WritePropertyName("successes");
        WriteReasonArray(writer, value.Successes, options);

        writer.WriteEndObject();
    }

    private static ImmutableList<T> ReadReasonArray<T>(
        ref Utf8JsonReader reader,
        JsonSerializerOptions options,
        ReadReasonDelegate<T> readFunc) where T : IReason
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException($"Expected StartArray for {typeof(T).Name} list.");

        var builder = ImmutableList.CreateBuilder<T>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            builder.Add(readFunc(ref reader, options));
        }

        return builder.ToImmutable();
    }

    private static void WriteReasonArray<T>(Utf8JsonWriter writer, ImmutableList<T> reasons, JsonSerializerOptions options) where T : IReason
    {
        writer.WriteStartArray();
        foreach (var reason in reasons)
        {
            ReasonJsonConverter.WriteReason(writer, reason, options);
        }
        writer.WriteEndArray();
    }

    internal delegate T ReadReasonDelegate<T>(ref Utf8JsonReader reader, JsonSerializerOptions options);
}
