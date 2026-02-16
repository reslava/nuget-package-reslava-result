using System.Text.Json;
using System.Text.Json.Serialization;
using REslava.Result.AdvancedPatterns;
using REslava.Result.Serialization;

namespace REslava.Result.Tests.Serialization;

[TestClass]
public class JsonSerializerOptionsExtensionsTests
{
    [TestMethod]
    public void AddREslavaResultConverters_RegistersThreeFactories()
    {
        var options = new JsonSerializerOptions();
        var returned = options.AddREslavaResultConverters();

        Assert.AreSame(options, returned);
        Assert.AreEqual(3, options.Converters.Count);
        Assert.IsInstanceOfType<ResultJsonConverterFactory>(options.Converters[0]);
        Assert.IsInstanceOfType<OneOfJsonConverterFactory>(options.Converters[1]);
        Assert.IsInstanceOfType<MaybeJsonConverterFactory>(options.Converters[2]);
    }

    [TestMethod]
    public void ConvertersWork_WithDefaultOptions()
    {
        var options = new JsonSerializerOptions().AddREslavaResultConverters();

        // Result
        var resultJson = JsonSerializer.Serialize(Result<int>.Ok(1), options);
        var result = JsonSerializer.Deserialize<Result<int>>(resultJson, options)!;
        Assert.AreEqual(1, result.Value);

        // OneOf
        var oneOfJson = JsonSerializer.Serialize(OneOf<string, int>.FromT2(42), options);
        var oneOf = JsonSerializer.Deserialize<OneOf<string, int>>(oneOfJson, options);
        Assert.AreEqual(42, oneOf.AsT2);

        // Maybe
        var maybeJson = JsonSerializer.Serialize(Maybe<string>.Some("hi"), options);
        var maybe = JsonSerializer.Deserialize<Maybe<string>>(maybeJson, options);
        Assert.AreEqual("hi", maybe.Value);
    }

    [TestMethod]
    public void ConvertersWork_WithCamelCasePolicy()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }.AddREslavaResultConverters();

        // Our converters hardcode property names, so camelCase policy should not affect them
        var json = JsonSerializer.Serialize(Result<int>.Ok(42), options);
        Assert.IsTrue(json.Contains("\"isSuccess\""));
        Assert.IsTrue(json.Contains("\"value\""));
    }

    [TestMethod]
    public void Nested_ResultOfMaybe_RoundTrip()
    {
        var options = new JsonSerializerOptions().AddREslavaResultConverters();
        var original = Result<Maybe<string>>.Ok(Maybe<string>.Some("nested"));

        var json = JsonSerializer.Serialize(original, options);
        var deserialized = JsonSerializer.Deserialize<Result<Maybe<string>>>(json, options)!;

        Assert.IsTrue(deserialized.IsSuccess);
        Assert.IsTrue(deserialized.Value.HasValue);
        Assert.AreEqual("nested", deserialized.Value.Value);
    }

    [TestMethod]
    public void Nested_ResultOfOneOf_RoundTrip()
    {
        var options = new JsonSerializerOptions().AddREslavaResultConverters();
        var original = Result<OneOf<string, int>>.Ok(OneOf<string, int>.FromT2(99));

        var json = JsonSerializer.Serialize(original, options);
        var deserialized = JsonSerializer.Deserialize<Result<OneOf<string, int>>>(json, options)!;

        Assert.IsTrue(deserialized.IsSuccess);
        Assert.IsTrue(deserialized.Value.IsT2);
        Assert.AreEqual(99, deserialized.Value.AsT2);
    }
}
