using System.Text.Json;
using REslava.Result.AdvancedPatterns;
using REslava.Result.Serialization;

namespace REslava.Result.Tests.Serialization;

[TestClass]
public class MaybeJsonConverterTests
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions().AddREslavaResultConverters();

    // =========================================================================
    // Some round-trips
    // =========================================================================

    [TestMethod]
    public void Some_String_RoundTrip()
    {
        var original = Maybe<string>.Some("hello");
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Maybe<string>>(json, _options);

        Assert.IsTrue(deserialized.HasValue);
        Assert.AreEqual("hello", deserialized.Value);
    }

    [TestMethod]
    public void Some_Int_RoundTrip()
    {
        var original = Maybe<int>.Some(42);
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Maybe<int>>(json, _options);

        Assert.IsTrue(deserialized.HasValue);
        Assert.AreEqual(42, deserialized.Value);
    }

    [TestMethod]
    public void Some_ComplexObject_RoundTrip()
    {
        var original = Maybe<TestUser>.Some(new TestUser("Alice", "alice@test.com"));
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Maybe<TestUser>>(json, _options);

        Assert.IsTrue(deserialized.HasValue);
        Assert.AreEqual("Alice", deserialized.Value!.Name);
        Assert.AreEqual("alice@test.com", deserialized.Value.Email);
    }

    // =========================================================================
    // None round-trips
    // =========================================================================

    [TestMethod]
    public void None_String_RoundTrip()
    {
        var original = Maybe<string>.None;
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Maybe<string>>(json, _options);

        Assert.IsFalse(deserialized.HasValue);
    }

    [TestMethod]
    public void None_Int_RoundTrip()
    {
        var original = Maybe<int>.None;
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Maybe<int>>(json, _options);

        Assert.IsFalse(deserialized.HasValue);
    }

    // =========================================================================
    // JSON format verification
    // =========================================================================

    [TestMethod]
    public void Some_JsonFormat_HasValueProperty()
    {
        var original = Maybe<int>.Some(42);
        var json = JsonSerializer.Serialize(original, _options);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.IsTrue(root.GetProperty("hasValue").GetBoolean());
        Assert.AreEqual(42, root.GetProperty("value").GetInt32());
    }

    [TestMethod]
    public void None_JsonFormat_OmitsValue()
    {
        var original = Maybe<int>.None;
        var json = JsonSerializer.Serialize(original, _options);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.IsFalse(root.GetProperty("hasValue").GetBoolean());
        Assert.IsFalse(root.TryGetProperty("value", out _));
    }

    // =========================================================================
    // Error cases
    // =========================================================================

    [TestMethod]
    public void Deserialize_MissingHasValue_ThrowsJsonException()
    {
        var json = """{"value": 42}""";
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Maybe<int>>(json, _options));
    }

    [TestMethod]
    public void Deserialize_UnknownProperties_AreIgnored()
    {
        var json = """{"hasValue": true, "value": "test", "extra": true}""";
        var deserialized = JsonSerializer.Deserialize<Maybe<string>>(json, _options);

        Assert.IsTrue(deserialized.HasValue);
        Assert.AreEqual("test", deserialized.Value);
    }

    // =========================================================================
    // Test helpers
    // =========================================================================

    public record TestUser(string Name, string Email);
}
