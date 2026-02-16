using System.Text.Json;
using REslava.Result.Serialization;

namespace REslava.Result.Tests.Serialization;

[TestClass]
public class ReasonJsonConverterTests
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions().AddREslavaResultConverters();

    [TestMethod]
    public void Error_RoundTrip_PreservesMessage()
    {
        var original = Result<string>.Fail("Something went wrong");
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Result<string>>(json, _options)!;

        Assert.IsTrue(deserialized.IsFailed);
        Assert.AreEqual("Something went wrong", deserialized.Errors[0].Message);
    }

    [TestMethod]
    public void Error_WithTags_RoundTrip_PreservesTagsAsJsonElement()
    {
        var error = new Error("Validation failed")
            .WithTag("Field", "Email")
            .WithTag("Code", "INVALID_FORMAT");
        var original = Result<string>.Fail(error);

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Result<string>>(json, _options)!;

        Assert.IsTrue(deserialized.IsFailed);
        Assert.AreEqual("Validation failed", deserialized.Errors[0].Message);
        Assert.AreEqual(2, deserialized.Errors[0].Tags.Count);

        // Tags deserialize as JsonElement
        var fieldTag = (JsonElement)deserialized.Errors[0].Tags["Field"];
        Assert.AreEqual("Email", fieldTag.GetString());

        var codeTag = (JsonElement)deserialized.Errors[0].Tags["Code"];
        Assert.AreEqual("INVALID_FORMAT", codeTag.GetString());
    }

    [TestMethod]
    public void Error_WithNumericTag_PreservesValue()
    {
        var error = new Error("Rate limited").WithTag("RetryAfter", 60);
        var original = Result<string>.Fail(error);

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Result<string>>(json, _options)!;

        var retryTag = (JsonElement)deserialized.Errors[0].Tags["RetryAfter"];
        Assert.AreEqual(60, retryTag.GetInt32());
    }

    [TestMethod]
    public void ExceptionError_SerializesTypeField_DeserializesAsError()
    {
        var ex = new InvalidOperationException("Test exception");
        var exError = new ExceptionError(ex);
        var original = Result<string>.Fail(exError);

        var json = JsonSerializer.Serialize(original, _options);

        // Verify the JSON contains the type field
        Assert.IsTrue(json.Contains("\"type\":\"ExceptionError\""));

        var deserialized = JsonSerializer.Deserialize<Result<string>>(json, _options)!;

        Assert.IsTrue(deserialized.IsFailed);
        Assert.AreEqual("Test exception", deserialized.Errors[0].Message);
        // Deserializes as Error, not ExceptionError
        Assert.IsInstanceOfType<Error>(deserialized.Errors[0]);
    }

    [TestMethod]
    public void Success_RoundTrip_PreservesMessage()
    {
        var original = Result<int>.Ok(42, "Operation completed");

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Result<int>>(json, _options)!;

        Assert.IsTrue(deserialized.IsSuccess);
        Assert.AreEqual(42, deserialized.Value);
        Assert.AreEqual(1, deserialized.Successes.Count);
        Assert.AreEqual("Operation completed", deserialized.Successes[0].Message);
    }

    [TestMethod]
    public void Success_WithTags_RoundTrip_PreservesTagsAsJsonElement()
    {
        var success = new Success("Created").WithTag("UserId", 123);
        var original = Result<int>.Ok(1, success);

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Result<int>>(json, _options)!;

        Assert.AreEqual(1, deserialized.Successes.Count);
        Assert.AreEqual("Created", deserialized.Successes[0].Message);

        var userIdTag = (JsonElement)deserialized.Successes[0].Tags["UserId"];
        Assert.AreEqual(123, userIdTag.GetInt32());
    }

    [TestMethod]
    public void MultipleErrors_RoundTrip_PreservesAll()
    {
        var errors = new IError[]
        {
            new Error("Error 1"),
            new Error("Error 2").WithTag("Severity", "High"),
            new Error("Error 3")
        };
        var original = Result<string>.Fail(errors);

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Result<string>>(json, _options)!;

        Assert.AreEqual(3, deserialized.Errors.Count);
        Assert.AreEqual("Error 1", deserialized.Errors[0].Message);
        Assert.AreEqual("Error 2", deserialized.Errors[1].Message);
        Assert.AreEqual("Error 3", deserialized.Errors[2].Message);

        var severityTag = (JsonElement)deserialized.Errors[1].Tags["Severity"];
        Assert.AreEqual("High", severityTag.GetString());
    }

    [TestMethod]
    public void Error_EmptyTags_RoundTrip_Works()
    {
        var original = Result<string>.Fail("No tags");

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Result<string>>(json, _options)!;

        Assert.IsTrue(deserialized.IsFailed);
        Assert.AreEqual(0, deserialized.Errors[0].Tags.Count);
    }
}
