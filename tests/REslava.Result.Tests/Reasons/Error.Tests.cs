namespace REslava.Result.Reasons.Tests;

/// <summary>
/// Comprehensive tests for the Error class
/// </summary>
[TestClass]
public sealed class ErrorTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_Default_CreatesErrorWithEmptyMessage()
    {
        // Act
        var error = new Error();

        // Assert
        Assert.IsNotNull(error);
        Assert.AreEqual(string.Empty, error.Message);
        Assert.IsNotNull(error.Tags);
        Assert.IsEmpty(error.Tags);
    }

    [TestMethod]
    public void Constructor_WithMessage_CreatesErrorWithMessage()
    {
        // Act
        var error = new Error("Something went wrong");

        // Assert
        Assert.AreEqual("Something went wrong", error.Message);
        Assert.IsEmpty(error.Tags);
    }

    [TestMethod]
    public void Constructor_WithEmptyMessage_CreatesErrorWithEmptyMessage()
    {
        // Act
        var error = new Error("");

        // Assert
        Assert.AreEqual("", error.Message);
    }

    [TestMethod]
    public void Constructor_WithNullMessage_CreatesErrorWithNullMessage()
    {
        // Act
        var error = new Error(null!);

        // Assert
        Assert.IsNull(error.Message);
    }

    #endregion

    #region WithMessage Tests

    [TestMethod]
    public void WithMessage_UpdatesMessage()
    {
        // Arrange
        var error = new Error("Original message");

        // Act
        var result = error.WithMessage("Updated message");

        // Assert
        Assert.AreEqual("Updated message", error.Message);
        Assert.AreSame(error, result); // Fluent interface returns same instance
    }

    [TestMethod]
    public void WithMessage_ReturnsSameInstance()
    {
        // Arrange
        var error = new Error();

        // Act
        var result = error.WithMessage("New message");

        // Assert
        Assert.AreSame(error, result);
    }

    [TestMethod]
    public void WithMessage_EmptyString_UpdatesToEmpty()
    {
        // Arrange
        var error = new Error("Original");

        // Act
        error.WithMessage("");

        // Assert
        Assert.AreEqual("", error.Message);
    }

    [TestMethod]
    public void WithMessage_Chaining_Works()
    {
        // Arrange
        var error = new Error().WithMessage("First")
             .WithMessage("Second")
             .WithMessage("Third");
        

        // Act
        //error.WithMessage("First")
        //     .WithMessage("Second")
        //     .WithMessage("Third");

        // Assert
        Assert.AreEqual("Third", error.Message);
    }

    #endregion

    #region WithTags Tests - Single Tag

    [TestMethod]
    public void WithTags_SingleTag_AddsTagCorrectly()
    {
        // Arrange
        var error = new Error("Validation failed");

        // Act
        var result = error.WithTags("Field", "Email");

        // Assert
        Assert.HasCount(1, error.Tags);
        Assert.IsTrue(error.Tags.ContainsKey("Field"));
        Assert.AreEqual("Email", error.Tags["Field"]);
        Assert.AreSame(error, result);
    }

    [TestMethod]
    public void WithTags_MultipleIndividualTags_AddsAllTags()
    {
        // Arrange
        var error = new Error("Error");

        // Act
        error.WithTags("Key1", "Value1")
             .WithTags("Key2", 42)
             .WithTags("Key3", true);

        // Assert
        Assert.HasCount(3, error.Tags);
        Assert.AreEqual("Value1", error.Tags["Key1"]);
        Assert.AreEqual(42, error.Tags["Key2"]);
        Assert.IsTrue((bool)error.Tags["Key3"]);
    }

    [TestMethod]
    public void WithTags_DifferentValueTypes_StoresCorrectly()
    {
        // Arrange
        var error = new Error();
        var dateTime = DateTime.Now;

        // Act
        error.WithTags("String", "text")
             .WithTags("Int", 123)
             .WithTags("Double", 3.14)
             .WithTags("Bool", false)
             .WithTags("DateTime", dateTime)
             .WithTags("Null", null!);

        // Assert
        Assert.HasCount(6, error.Tags);
        Assert.AreEqual("text", error.Tags["String"]);
        Assert.AreEqual(123, error.Tags["Int"]);
        Assert.AreEqual(3.14, error.Tags["Double"]);
        Assert.IsFalse((bool)error.Tags["Bool"]);
        Assert.AreEqual(dateTime, error.Tags["DateTime"]);
        Assert.IsNull(error.Tags["Null"]);
    }

    #endregion

    #region WithTags Tests - Dictionary

    [TestMethod]
    public void WithTags_Dictionary_AddsAllTagsCorrectly()
    {
        // Arrange
        var error = new Error("Error");
        var tags = new Dictionary<string, object>
        {
            { "Field", "Username" },
            { "Code", 400 },
            { "Severity", "High" }
        };

        var tagsArr = tags.Select(kvp => (kvp.Key, kvp.Value)).ToArray();

        // Act
        var result = error.WithTags(tagsArr);

        // Assert
        Assert.HasCount(3, error.Tags);
        Assert.AreEqual("Username", error.Tags["Field"]);
        Assert.AreEqual(400, error.Tags["Code"]);
        Assert.AreEqual("High", error.Tags["Severity"]);
        Assert.AreSame(error, result);
    }

    [TestMethod]
    public void WithTags_EmptyDictionary_AddsNoTags()
    {
        // Arrange
        var error = new Error();
        var emptyTags = new Dictionary<string, object>();

        var tagsArr = emptyTags.Select(kvp => (kvp.Key, kvp.Value)).ToArray();

        // Act
        error.WithTags(tagsArr);

        // Assert
        Assert.IsEmpty(error.Tags);
    }

    [TestMethod]
    public void WithTags_MultipleTagsDictionary_AccumulatesTags()
    {
        // Arrange
        var error = new Error();
        var tags1 = new Dictionary<string, object> { { "A", 1 }, { "B", 2 } };
        var tags2 = new Dictionary<string, object> { { "C", 3 }, { "D", 4 } };

        var tags1Arr = tags1.Select(kvp => (kvp.Key, kvp.Value)).ToArray();
        var tags2Arr = tags2.Select(kvp => (kvp.Key, kvp.Value)).ToArray();

        // Act
        error.WithTags(tags1Arr)
             .WithTags(tags2Arr);

        // Assert
        Assert.HasCount(4, error.Tags);
        Assert.AreEqual(1, error.Tags["A"]);
        Assert.AreEqual(2, error.Tags["B"]);
        Assert.AreEqual(3, error.Tags["C"]);
        Assert.AreEqual(4, error.Tags["D"]);
    }

    #endregion

    #region Fluent Interface Tests

    [TestMethod]
    public void FluentInterface_CombinedUsage_Works()
    {
        // Act
        var error = new Error("Validation failed")
            .WithTags("Field", "Email")
            .WithTags("Code", 422)
            .WithMessage("Email validation failed")
            .WithTags("Regex", "^[a-z]+@[a-z]+\\.[a-z]+$");

        // Assert
        Assert.AreEqual("Email validation failed", error.Message);
        Assert.HasCount(3, error.Tags);
        Assert.AreEqual("Email", error.Tags["Field"]);
        Assert.AreEqual(422, error.Tags["Code"]);
    }

    [TestMethod]
    public void FluentInterface_ComplexChaining_MaintainsState()
    {
        // Act
        var error = new Error()
            .WithMessage("Step 1")
            .WithTags("A", 1)
            .WithMessage("Step 2")
            .WithTags("B", 2)
            .WithTags("C", 3)
            .WithMessage("Final");

        // Assert
        Assert.AreEqual("Final", error.Message);
        Assert.HasCount(3, error.Tags);
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void ToString_WithoutTags_ShowsMessageOnly()
    {
        // Arrange
        var error = new Error("Simple error");

        // Act
        var result = error.ToString();

        // Assert
        Assert.Contains("Error:", result);
        Assert.Contains("Simple error", result);
        Assert.DoesNotContain("Tags:", result);
    }

    [TestMethod]
    public void ToString_WithTags_ShowsMessageAndTags()
    {
        // Arrange
        var error = new Error("Error with tags")
            .WithTags("Field", "Username")
            .WithTags("Code", 400);

        // Act
        var result = error.ToString();

        // Assert
        Assert.Contains("Error:", result);
        Assert.Contains("Error with tags", result);
        Assert.Contains("Tags:", result);
    }

    [TestMethod]
    public void ToString_EmptyError_ShowsErrorType()
    {
        // Arrange
        var error = new Error();

        // Act
        var result = error.ToString();

        // Assert
        Assert.Contains("Error:", result);
    }

    #endregion

    #region Interface Implementation Tests

    [TestMethod]
    public void Error_ImplementsIError()
    {
        // Arrange
        var error = new Error();

        // Assert
        Assert.IsInstanceOfType<IError>(error);
    }

    [TestMethod]
    public void Error_ImplementsIReason()
    {
        // Arrange
        var error = new Error();

        // Assert
        Assert.IsInstanceOfType<IReason>(error);
    }

    [TestMethod]
    public void Error_InheritsFromReasonOfError()
    {
        // Arrange
        var error = new Error();

        // Assert
        Assert.IsInstanceOfType<Reason<Error>>(error);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void WithTags_DuplicateKey_ThrowsArgumentException()
    {
        // Arrange
        var error = new Error();
        error.WithTags("Key", "Value1");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => error.WithTags("Key", "Value2"));
    }

    
    [TestMethod]
    public void WithTags_Dictionary_DuplicateKeyInDictionary_ThrowsArgumentException()
    {
        // Arrange
        var error = new Error();
        error.WithTags("Existing", "Value");
        //var tags = new Dictionary<string, object> { { "Existing", "NewValue" } };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => error.WithTags("Existing", "NewValue"));
    }

    [TestMethod]
    public void Tags_Property_IsNotNull()
    {
        // Arrange
        var error = new Error();

        // Assert
        Assert.IsNotNull(error.Tags);
    }

    #endregion

    #region Use in Result Tests

    [TestMethod]
    public void Error_UsedInResult_Works()
    {
        // Arrange
        var error = new Error("Database connection failed")
            .WithTags("Server", "localhost")
            .WithTags("Port", 5432);

        // Act
        var result = Result.Fail(error);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual("Database connection failed", result.Errors[0].Message);
        Assert.AreEqual("localhost", result.Errors[0].Tags["Server"]);
        Assert.AreEqual(5432, result.Errors[0].Tags["Port"]);
    }

    [TestMethod]
    public void Error_UsedInResultOfT_Works()
    {
        // Arrange
        var error = new Error("Validation failed")
            .WithTags("Field", "Email");

        // Act
        var result = Result<string>.Fail(error);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Validation failed", result.Errors[0].Message);
    }

    #endregion
}
