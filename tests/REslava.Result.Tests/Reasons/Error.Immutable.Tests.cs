using System.Collections.Immutable;
using System.Reflection.Metadata;
using Newtonsoft.Json.Linq;

namespace REslava.Result.Reasons.Tests;

/// <summary>
/// Comprehensive tests for the Error class (immutable version)
/// </summary>
[TestClass]
public sealed class ErrorImmutableTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_Default_CreatesErrorWithTestMessage()
    {
        // Act
        var error = new Error("test");

        // Assert
        Assert.IsNotNull(error);
        Assert.AreEqual("test", error.Message);
        Assert.IsNotNull(error.Tags);
        Assert.IsTrue(error.Tags.IsEmpty);
    }

    [TestMethod]
    public void Constructor_WithMessage_CreatesErrorWithMessage()
    {
        // Act
        var error = new Error("Something went wrong");

        // Assert
        Assert.AreEqual("Something went wrong", error.Message);
        Assert.IsTrue(error.Tags.IsEmpty);
    }

    [TestMethod]
    public void Constructor_WithNullMessage_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Error(null!));
    }

    #endregion

    #region WithMessage Tests

    [TestMethod]
    public void WithMessage_CreatesNewInstanceWithUpdatedMessage()
    {
        // Arrange
        var original = new Error("Original message");

        // Act
        var updated = original.WithMessage("Updated message");

        // Assert
        Assert.AreNotSame(original, updated);
        Assert.AreEqual("Original message", original.Message);
        Assert.AreEqual("Updated message", updated.Message);
    }

    [TestMethod]
    public void WithMessage_ReturnsErrorType()
    {
        // Arrange
        var error = new Error("Test");

        // Act
        var result = error.WithMessage("New message");

        // Assert
        Assert.IsInstanceOfType<Error>(result);
    }

    [TestMethod]
    public void WithMessage_NullMessage_ThrowsArgumentException()
    {
        // Arrange
        var error = new Error("Test");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            error.WithMessage(null!));

        Assert.Contains(ValidationExtensions.DefaultNullOrWhitespaceMessage, exception.Message);
    }

    [TestMethod]
    public void WithMessage_EmptyString_ThrowsArgumentException()
    {
        // Arrange
        var error = new Error("Original");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => error.WithMessage(""));
    }

    [TestMethod]
    public void WithMessage_Whitespace_ThrowsArgumentException()
    {
        // Arrange
        var error = new Error("Original");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => error.WithMessage("   "));
    }

    [TestMethod]
    public void WithMessage_Chaining_CreatesIndependentInstances()
    {
        // Arrange
        var error = new Error("Original");

        // Act
        var e1 = error.WithMessage("First");
        var e2 = e1.WithMessage("Second");
        var e3 = e2.WithMessage("Third");

        // Assert
        Assert.AreEqual("Original", error.Message);
        Assert.AreEqual("First", e1.Message);
        Assert.AreEqual("Second", e2.Message);
        Assert.AreEqual("Third", e3.Message);
    }

    #endregion

    #region WithTags Tests - Single Tag

    [TestMethod]
    public void WithTags_SingleTag_CreatesNewInstanceWithTag()
    {
        // Arrange
        var original = new Error("Validation failed");

        // Act
        var updated = original.WithTag("Field", "Email");

        // Assert
        Assert.AreNotSame(original, updated);
        Assert.IsTrue(original.Tags.IsEmpty);
        Assert.HasCount(1, updated.Tags);
        Assert.IsTrue(updated.Tags.ContainsKey("Field"));
        Assert.AreEqual("Email", updated.Tags["Field"]);
    }

    [TestMethod]
    public void WithTags_SingleTag_ReturnsErrorType()
    {
        // Arrange
        var error = new Error("Test");

        // Act
        var result = error.WithTag("Key", "Value");

        // Assert
        Assert.IsInstanceOfType<Error>(result);
    }

    [TestMethod]
    public void WithTags_SingleTag_PreservesMessage()
    {
        // Arrange
        var error = new Error("Original message");

        // Act
        var updated = error.WithTag("Key", "Value");

        // Assert
        Assert.AreEqual("Original message", updated.Message);
    }

    [TestMethod]
    public void WithTags_MultipleIndividualTags_AddsAllTags()
    {
        // Arrange
        var error = new Error("Error");

        // Act
        var result = error
            .WithTag("Key1", "Value1")
            .WithTag("Key2", 42)
            .WithTag("Key3", true);

        // Assert
        Assert.HasCount(3, result.Tags);
        Assert.AreEqual("Value1", result.Tags["Key1"]);
        Assert.AreEqual(42, result.Tags["Key2"]);
        Assert.IsTrue((bool)result.Tags["Key3"]);
    }

    [TestMethod]
    public void WithTags_DifferentValueTypes_StoresCorrectly()
    {
        // Arrange
        var error = new Error("Test");
        var dateTime = DateTime.Now;

        // Act
        var result = error
            .WithTag("String", "text")
            .WithTag("Int", 123)
            .WithTag("Double", 3.14)
            .WithTag("Bool", false)
            .WithTag("DateTime", dateTime)
            .WithTag("Null", null!);

        // Assert
        Assert.HasCount(6, result.Tags);
        Assert.AreEqual("text", result.Tags["String"]);
        Assert.AreEqual(123, result.Tags["Int"]);
        Assert.AreEqual(3.14, result.Tags["Double"]);
        Assert.IsFalse((bool)result.Tags["Bool"]);
        Assert.AreEqual(dateTime, result.Tags["DateTime"]);
        Assert.IsNull(result.Tags["Null"]);
    }

    [TestMethod]
    public void WithTags_DuplicateKey_ThrowsArgumentException()
    {
        // Arrange
        var error = new Error("Test");
        var withTag = error.WithTag("Key", "Value1");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            withTag.WithTag("Key", "Value2"));
        
        Assert.Contains("Key", exception.Message);
        Assert.Contains(ValidationExtensions.DefaultKeyExistsMessage, exception.Message);
    }

    #endregion

    #region WithTags Tests - Multiple Tags

    [TestMethod]
    public void WithTags_MultipleTagsParams_CreatesNewInstanceWithAllTags()
    {
        // Arrange
        var error = new Error("Error with tags");

        // Act
        var updated = error.WithTags(
            ("Field", "Username"),
            ("Code", 400),
            ("Severity", "High")
        );

        // Assert
        Assert.AreNotSame(error, updated);
        Assert.IsTrue(error.Tags.IsEmpty);
        Assert.HasCount(3, updated.Tags);
        Assert.AreEqual("Username", updated.Tags["Field"]);
        Assert.AreEqual(400, updated.Tags["Code"]);
        Assert.AreEqual("High", updated.Tags["Severity"]);
    }

    [TestMethod]
    public void WithTags_EmptyArray_ReturnsSameInstance()
    {
        // Arrange
        var error = new Error("Test");

        // Act
        var result = error.WithTags(Array.Empty<(string, object)>());

        // Assert
        Assert.AreSame(error, result);
    }

    [TestMethod]
    public void WithTags_NullArray_ReturnsSameInstance()
    {
        // Arrange
        var error = new Error("Test");

        // Act
        var result = error.WithTags(null!);

        // Assert
        Assert.AreSame(error, result);
    }

    [TestMethod]
    public void WithTags_MultipleTagsAccumulate_CreatesCorrectInstance()
    {
        // Arrange
        var error = new Error("Test");

        // Act
        var result = error
            .WithTags(("A", 1), ("B", 2))
            .WithTags(("C", 3), ("D", 4));

        // Assert
        Assert.HasCount(4, result.Tags);
        Assert.AreEqual(1, result.Tags["A"]);
        Assert.AreEqual(2, result.Tags["B"]);
        Assert.AreEqual(3, result.Tags["C"]);
        Assert.AreEqual(4, result.Tags["D"]);
    }

    [TestMethod]
    public void WithTags_MultipleTagsParams_DuplicateKey_ThrowsArgumentException()
    {
        // Arrange
        var error = new Error("Test");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            error.WithTags(
                ("Key1", "Value1"),
                ("Key1", "Value2")  // Duplicate
            ));
        
        Assert.Contains("Key1", exception.Message);
        Assert.Contains(ValidationExtensions.DefaultKeyExistsMessage, exception.Message);
    }

    #endregion

    #region Fluent Interface Tests

    [TestMethod]
    public void FluentInterface_CombinedUsage_CreatesCorrectInstance()
    {
        // Act
        var error = new Error("Validation failed")
            .WithTag("Field", "Email")
            .WithTag("Code", 422)
            .WithMessage("Email validation failed")
            .WithTag("Regex", "^[a-z]+@[a-z]+\\.[a-z]+$");

        // Assert
        Assert.AreEqual("Email validation failed", error.Message);
        Assert.HasCount(3, error.Tags);
        Assert.AreEqual("Email", error.Tags["Field"]);
        Assert.AreEqual(422, error.Tags["Code"]);
    }

    [TestMethod]
    public void FluentInterface_ComplexChaining_MaintainsCorrectState()
    {
        // Act
        var error = new Error("Step 0")
            .WithMessage("Step 1")
            .WithTag("A", 1)
            .WithMessage("Step 2")
            .WithTag("B", 2)
            .WithTag("C", 3)
            .WithMessage("Final");

        // Assert
        Assert.AreEqual("Final", error.Message);
        Assert.HasCount(3, error.Tags);
        Assert.AreEqual(1, error.Tags["A"]);
        Assert.AreEqual(2, error.Tags["B"]);
        Assert.AreEqual(3, error.Tags["C"]);
    }

    #endregion

    #region Immutability Tests

    [TestMethod]
    public void Immutability_OriginalNeverModified()
    {
        // Arrange
        var original = new Error("Original");

        // Act
        var modified = original
            .WithMessage("Modified")
            .WithTag("Key", "Value");

        // Assert
        Assert.AreEqual("Original", original.Message);
        Assert.IsTrue(original.Tags.IsEmpty);
        
        Assert.AreEqual("Modified", modified.Message);
        Assert.HasCount(1, modified.Tags);
    }

    [TestMethod]
    public void Immutability_IntermediateInstancesIndependent()
    {
        // Arrange
        var e0 = new Error("E0");

        // Act
        var e1 = e0.WithTag("T1", 1);
        var e2 = e1.WithTag("T2", 2);
        var e1b = e1.WithTag("T1B", "1b");

        // Assert - e2 unaffected by e1b
        Assert.HasCount(1, e1.Tags);
        Assert.HasCount(2, e1b.Tags);
        Assert.HasCount(2, e2.Tags);
        Assert.IsFalse(e2.Tags.ContainsKey("T1B"));
    }

    #endregion

    #region Interface Implementation Tests

    [TestMethod]
    public void Error_ImplementsIError()
    {
        // Arrange
        var error = new Error("test");

        // Assert
        Assert.IsInstanceOfType<IError>(error);
    }

    [TestMethod]
    public void Error_ImplementsIReason()
    {
        // Arrange
        var error = new Error("test");

        // Assert
        Assert.IsInstanceOfType<IReason>(error);
    }

    [TestMethod]
    public void Error_InheritsFromReasonOfError()
    {
        // Arrange
        var error = new Error("test");

        // Assert
        Assert.IsInstanceOfType<Reason<Error>>(error);
    }

    #endregion

    #region Use in Result Tests

    [TestMethod]
    public void Error_UsedInResult_Works()
    {
        // Arrange
        var error = new Error("Database connection failed")
            .WithTag("Server", "localhost")
            .WithTag("Port", 5432);

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
            .WithTag("Field", "Email");

        // Act
        var result = Result<string>.Fail(error);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Validation failed", result.Errors[0].Message);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void EdgeCase_VeryLongMessage_HandlesCorrectly()
    {
        // Arrange
        var longMessage = new string('x', 10000);

        // Act
        var error = new Error(longMessage);

        // Assert
        Assert.AreEqual(10000, error.Message.Length);
    }

    [TestMethod]
    public void EdgeCase_SpecialCharacters_PreservesCorrectly()
    {
        // Arrange
        var specialMessage = "Error\n\r\t\"'\\<>&";

        // Act
        var error = new Error(specialMessage);

        // Assert
        Assert.AreEqual(specialMessage, error.Message);
    }

    [TestMethod]
    public void EdgeCase_ManyTagsChained_MaintainsState()
    {
        // Arrange
        var error = new Error("Test");

        // Act
        var result = error;
        for (int i = 0; i < 100; i++)
        {
            result = result.WithTag($"Key{i}", i);
        }

        // Assert
        Assert.HasCount(100, result.Tags);
        Assert.AreEqual(50, result.Tags["Key50"]);
        
        // Original unchanged
        Assert.IsTrue(error.Tags.IsEmpty);
    }

    #endregion

    #region CRTP Pattern Tests

    [TestMethod]
    public void CRTP_FluentMethods_ReturnErrorType()
    {
        // Arrange
        var error = new Error("Test");

        // Act
        Error e1 = error.WithMessage("M1");
        Error e2 = e1.WithTag("K", "V");
        Error e3 = e2.WithTags(("K2", "V2"));

        // Assert - All return Error, enabling fluent chaining
        Assert.IsInstanceOfType<Error>(e1);
        Assert.IsInstanceOfType<Error>(e2);
        Assert.IsInstanceOfType<Error>(e3);
    }

    #endregion
}
