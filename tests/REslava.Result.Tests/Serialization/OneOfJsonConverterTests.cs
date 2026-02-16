using System.Text.Json;
using REslava.Result.AdvancedPatterns;
using REslava.Result.Serialization;

namespace REslava.Result.Tests.Serialization;

[TestClass]
public class OneOfJsonConverterTests
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions().AddREslavaResultConverters();

    // =========================================================================
    // OneOf<T1, T2>
    // =========================================================================

    [TestMethod]
    public void OneOf2_T1_RoundTrip()
    {
        OneOf<string, int> original = OneOf<string, int>.FromT1("hello");
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<OneOf<string, int>>(json, _options);

        Assert.IsTrue(deserialized.IsT1);
        Assert.AreEqual("hello", deserialized.AsT1);
    }

    [TestMethod]
    public void OneOf2_T2_RoundTrip()
    {
        OneOf<string, int> original = OneOf<string, int>.FromT2(42);
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<OneOf<string, int>>(json, _options);

        Assert.IsTrue(deserialized.IsT2);
        Assert.AreEqual(42, deserialized.AsT2);
    }

    [TestMethod]
    public void OneOf2_JsonFormat_HasIndexAndValue()
    {
        OneOf<string, int> original = OneOf<string, int>.FromT1("test");
        var json = JsonSerializer.Serialize(original, _options);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.AreEqual(0, root.GetProperty("index").GetInt32());
        Assert.AreEqual("test", root.GetProperty("value").GetString());
    }

    [TestMethod]
    public void OneOf2_ComplexType_RoundTrip()
    {
        var user = new TestUser("Alice", 30);
        OneOf<Error, TestUser> original = OneOf<Error, TestUser>.FromT2(user);
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<OneOf<Error, TestUser>>(json, _options);

        Assert.IsTrue(deserialized.IsT2);
        Assert.AreEqual("Alice", deserialized.AsT2.Name);
        Assert.AreEqual(30, deserialized.AsT2.Age);
    }

    // =========================================================================
    // OneOf<T1, T2, T3>
    // =========================================================================

    [TestMethod]
    public void OneOf3_T1_RoundTrip()
    {
        OneOf<string, int, bool> original = OneOf<string, int, bool>.FromT1("hello");
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<OneOf<string, int, bool>>(json, _options);

        Assert.IsTrue(deserialized.IsT1);
        Assert.AreEqual("hello", deserialized.AsT1);
    }

    [TestMethod]
    public void OneOf3_T2_RoundTrip()
    {
        OneOf<string, int, bool> original = OneOf<string, int, bool>.FromT2(99);
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<OneOf<string, int, bool>>(json, _options);

        Assert.IsTrue(deserialized.IsT2);
        Assert.AreEqual(99, deserialized.AsT2);
    }

    [TestMethod]
    public void OneOf3_T3_RoundTrip()
    {
        OneOf<string, int, bool> original = OneOf<string, int, bool>.FromT3(true);
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<OneOf<string, int, bool>>(json, _options);

        Assert.IsTrue(deserialized.IsT3);
        Assert.IsTrue(deserialized.AsT3);
    }

    // =========================================================================
    // OneOf<T1, T2, T3, T4>
    // =========================================================================

    [TestMethod]
    public void OneOf4_T1_RoundTrip()
    {
        OneOf<string, int, bool, double> original = OneOf<string, int, bool, double>.FromT1("test");
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<OneOf<string, int, bool, double>>(json, _options);

        Assert.IsTrue(deserialized.IsT1);
        Assert.AreEqual("test", deserialized.AsT1);
    }

    [TestMethod]
    public void OneOf4_T4_RoundTrip()
    {
        OneOf<string, int, bool, double> original = OneOf<string, int, bool, double>.FromT4(3.14);
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<OneOf<string, int, bool, double>>(json, _options);

        Assert.IsTrue(deserialized.IsT4);
        Assert.AreEqual(3.14, deserialized.AsT4, 0.001);
    }

    // =========================================================================
    // Error cases
    // =========================================================================

    [TestMethod]
    public void OneOf2_InvalidIndex_ThrowsJsonException()
    {
        var json = """{"index": 5, "value": "oops"}""";
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<OneOf<string, int>>(json, _options));
    }

    [TestMethod]
    public void OneOf2_MissingIndex_ThrowsJsonException()
    {
        var json = """{"value": "oops"}""";
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<OneOf<string, int>>(json, _options));
    }

    [TestMethod]
    public void OneOf2_MissingValue_ThrowsJsonException()
    {
        var json = """{"index": 0}""";
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<OneOf<string, int>>(json, _options));
    }

    // =========================================================================
    // Test helpers
    // =========================================================================

    public record TestUser(string Name, int Age);
}
