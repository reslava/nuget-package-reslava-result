using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.Reasons;

namespace REslava.Result.Tests.Reasons;

[TestClass]
public sealed class ErrorTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_WithMessage_ShouldCreateError()
    {
        // Arrange
        var message = "Test error";
        
        // Act
        var error = new Error(message);
        
        // Assert
        Assert.AreEqual(message, error.Message);
        Assert.IsTrue(error.Tags.IsEmpty);
    }

    [TestMethod]
    public void Constructor_WithNullMessage_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Error(null!));
    }

    [TestMethod]
    public void Constructor_WithEmptyMessage_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Error(string.Empty));
    }

    [TestMethod]
    public void Constructor_WithWhitespaceMessage_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Error("   "));
    }

    #endregion

    #region WithTag Tests

    [TestMethod]
    public void WithTag_WithValidParameters_ShouldAddTag()
    {
        // Arrange
        var error = new Error("Test error");
        
        // Act
        var taggedError = error.WithTag("Key", "Value");
        
        // Assert
        Assert.AreEqual("Test error", taggedError.Message);
        Assert.AreEqual("Value", taggedError.Tags["Key"]);
    }

    [TestMethod]
    public void WithTag_WithNullKey_ShouldThrowArgumentNullException()
    {
        // Arrange
        var error = new Error("Test error");
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => error.WithTag(null!, "Value"));
    }

    [TestMethod]
    public void WithTag_WithEmptyKey_ShouldThrowArgumentException()
    {
        // Arrange
        var error = new Error("Test error");
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => error.WithTag("", "Value"));
    }

    [TestMethod]
    public void WithTag_WithWhitespaceKey_ShouldThrowArgumentException()
    {
        // Arrange
        var error = new Error("Test error");
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => error.WithTag("   ", "Value"));
    }

    [TestMethod]
    public void WithTag_WithExistingKey_ShouldThrowArgumentException()
    {
        // Arrange
        var error = new Error("Test error").WithTag("Key", "Value");
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => error.WithTag("Key", "NewValue"));
    }

    #endregion

    #region WithTags Tests

    [TestMethod]
    public void WithTags_WithValidParameters_ShouldAddMultipleTags()
    {
        // Arrange
        var error = new Error("Test error");
        var tags = new[] { ("Key1", "Value1"), ("Key2", "Value2") };
        
        // Act
        var taggedError = error.WithTags(tags);
        
        // Assert
        Assert.AreEqual("Test error", taggedError.Message);
        Assert.AreEqual("Value1", taggedError.Tags["Key1"]);
        Assert.AreEqual("Value2", taggedError.Tags["Key2"]);
    }

    [TestMethod]
    public void WithTags_WithNullTags_ShouldThrowArgumentNullException()
    {
        // Arrange
        var error = new Error("Test error");
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => error.WithTags(null!));
    }

    #endregion

    #region Immutability Tests

    [TestMethod]
    public void WithTag_ShouldReturnNewInstance_OriginalUnchanged()
    {
        // Arrange
        var original = new Error("Test error");
        
        // Act
        var modified = original.WithTag("Key", "Value");
        
        // Assert
        Assert.AreNotSame(original, modified);
        Assert.IsTrue(original.Tags.IsEmpty);
        Assert.AreEqual("Value", modified.Tags["Key"]);
    }

    [TestMethod]
    public void WithTags_ShouldReturnNewInstance_OriginalUnchanged()
    {
        // Arrange
        var original = new Error("Test error");
        var tags = new[] { ("Key", "Value") };
        
        // Act
        var modified = original.WithTags(tags);
        
        // Assert
        Assert.AreNotSame(original, modified);
        Assert.IsTrue(original.Tags.IsEmpty);
        Assert.AreEqual("Value", modified.Tags["Key"]);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void Error_WithComplexTags_ShouldWorkCorrectly()
    {
        // Arrange
        var error = new Error("Complex error");
        
        // Act
        var taggedError = error
            .WithTag("String", "value")
            .WithTag("Number", 42)
            .WithTag("Boolean", true)
            .WithTag("Null", (object?)null);
        
        // Assert
        Assert.AreEqual("value", taggedError.Tags["String"]);
        Assert.AreEqual(42, taggedError.Tags["Number"]);
        Assert.AreEqual(true, taggedError.Tags["Boolean"]);
        Assert.IsNull(taggedError.Tags["Null"]);
    }

    [TestMethod]
    public void Error_WithChainedWithTags_ShouldAccumulateTags()
    {
        // Arrange
        var error = new Error("Test error");
        
        // Act
        var taggedError = error
            .WithTag("Key1", "Value1")
            .WithTag("Key2", "Value2")
            .WithTag("Key3", "Value3");
        
        // Assert
        Assert.HasCount(3, taggedError.Tags);
        Assert.AreEqual("Value1", taggedError.Tags["Key1"]);
        Assert.AreEqual("Value2", taggedError.Tags["Key2"]);
        Assert.AreEqual("Value3", taggedError.Tags["Key3"]);
    }

    #endregion
}
