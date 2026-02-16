using System.Text.Json;
using REslava.Result.Serialization;

namespace REslava.Result.Tests.Serialization;

[TestClass]
public class ResultJsonConverterTests
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions().AddREslavaResultConverters();

    // =========================================================================
    // Success round-trips
    // =========================================================================

    [TestMethod]
    public void Ok_String_RoundTrip()
    {
        var original = Result<string>.Ok("Hello");
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Result<string>>(json, _options)!;

        Assert.IsTrue(deserialized.IsSuccess);
        Assert.AreEqual("Hello", deserialized.Value);
        Assert.AreEqual(0, deserialized.Errors.Count);
    }

    [TestMethod]
    public void Ok_Int_RoundTrip()
    {
        var original = Result<int>.Ok(42);
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Result<int>>(json, _options)!;

        Assert.IsTrue(deserialized.IsSuccess);
        Assert.AreEqual(42, deserialized.Value);
    }

    [TestMethod]
    public void Ok_Int_Zero_RoundTrip()
    {
        var original = Result<int>.Ok(0);
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Result<int>>(json, _options)!;

        Assert.IsTrue(deserialized.IsSuccess);
        Assert.AreEqual(0, deserialized.Value);
    }

    [TestMethod]
    public void Ok_ComplexObject_RoundTrip()
    {
        var original = Result<TestUser>.Ok(new TestUser("Alice", "alice@test.com", 30));
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Result<TestUser>>(json, _options)!;

        Assert.IsTrue(deserialized.IsSuccess);
        Assert.AreEqual("Alice", deserialized.Value!.Name);
        Assert.AreEqual("alice@test.com", deserialized.Value.Email);
        Assert.AreEqual(30, deserialized.Value.Age);
    }

    [TestMethod]
    public void Ok_NullValue_RoundTrip()
    {
        var original = Result<string>.Ok(null!);
        var json = JsonSerializer.Serialize(original, _options);

        Assert.IsTrue(json.Contains("\"value\":null"));

        var deserialized = JsonSerializer.Deserialize<Result<string>>(json, _options)!;

        Assert.IsTrue(deserialized.IsSuccess);
        Assert.IsNull(deserialized.Value);
    }

    [TestMethod]
    public void Ok_WithSuccessMessage_RoundTrip()
    {
        var original = Result<int>.Ok(1, "Created successfully");
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Result<int>>(json, _options)!;

        Assert.IsTrue(deserialized.IsSuccess);
        Assert.AreEqual(1, deserialized.Value);
        Assert.AreEqual(1, deserialized.Successes.Count);
        Assert.AreEqual("Created successfully", deserialized.Successes[0].Message);
    }

    // =========================================================================
    // Failure round-trips
    // =========================================================================

    [TestMethod]
    public void Fail_SingleError_RoundTrip()
    {
        var original = Result<string>.Fail("Not found");
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Result<string>>(json, _options)!;

        Assert.IsTrue(deserialized.IsFailed);
        Assert.AreEqual(1, deserialized.Errors.Count);
        Assert.AreEqual("Not found", deserialized.Errors[0].Message);
    }

    [TestMethod]
    public void Fail_MultipleErrors_RoundTrip()
    {
        var errors = new IError[]
        {
            new Error("Name is required"),
            new Error("Email is invalid")
        };
        var original = Result<string>.Fail(errors);
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Result<string>>(json, _options)!;

        Assert.IsTrue(deserialized.IsFailed);
        Assert.AreEqual(2, deserialized.Errors.Count);
        Assert.AreEqual("Name is required", deserialized.Errors[0].Message);
        Assert.AreEqual("Email is invalid", deserialized.Errors[1].Message);
    }

    [TestMethod]
    public void Fail_Value_IsNull_InJson()
    {
        var original = Result<int>.Fail("Error");
        var json = JsonSerializer.Serialize(original, _options);

        Assert.IsTrue(json.Contains("\"value\":null"));
        Assert.IsTrue(json.Contains("\"isSuccess\":false"));
    }

    // =========================================================================
    // JSON format verification
    // =========================================================================

    [TestMethod]
    public void Ok_JsonFormat_HasExpectedStructure()
    {
        var original = Result<int>.Ok(42);
        var json = JsonSerializer.Serialize(original, _options);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.IsTrue(root.GetProperty("isSuccess").GetBoolean());
        Assert.AreEqual(42, root.GetProperty("value").GetInt32());
        Assert.AreEqual(0, root.GetProperty("errors").GetArrayLength());
        Assert.AreEqual(0, root.GetProperty("successes").GetArrayLength());
    }

    [TestMethod]
    public void Fail_JsonFormat_HasExpectedStructure()
    {
        var original = Result<int>.Fail(new Error("Bad").WithTag("Code", 400));
        var json = JsonSerializer.Serialize(original, _options);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.IsFalse(root.GetProperty("isSuccess").GetBoolean());
        Assert.AreEqual(JsonValueKind.Null, root.GetProperty("value").ValueKind);
        Assert.AreEqual(1, root.GetProperty("errors").GetArrayLength());

        var errorEl = root.GetProperty("errors")[0];
        Assert.AreEqual("Error", errorEl.GetProperty("type").GetString());
        Assert.AreEqual("Bad", errorEl.GetProperty("message").GetString());
        Assert.AreEqual(400, errorEl.GetProperty("tags").GetProperty("Code").GetInt32());
    }

    // =========================================================================
    // Edge cases
    // =========================================================================

    [TestMethod]
    public void Deserialize_MissingIsSuccess_ThrowsJsonException()
    {
        var json = """{"value": 42, "errors": [], "successes": []}""";
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Result<int>>(json, _options));
    }

    [TestMethod]
    public void Deserialize_FailedWithNoErrors_ThrowsJsonException()
    {
        var json = """{"isSuccess": false, "value": null, "errors": [], "successes": []}""";
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Result<int>>(json, _options));
    }

    [TestMethod]
    public void Deserialize_UnknownProperties_AreIgnored()
    {
        var json = """{"isSuccess": true, "value": 42, "errors": [], "successes": [], "extra": "ignored"}""";
        var deserialized = JsonSerializer.Deserialize<Result<int>>(json, _options)!;

        Assert.IsTrue(deserialized.IsSuccess);
        Assert.AreEqual(42, deserialized.Value);
    }

    // =========================================================================
    // Test helpers
    // =========================================================================

    public record TestUser(string Name, string Email, int Age);
}
