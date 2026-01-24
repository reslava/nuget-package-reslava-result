using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.Reasons;

namespace REslava.Result.Tests.Reasons;

[TestClass]
public sealed class SuccessTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_WithMessage_ShouldCreateSuccess()
    {
        // Arrange
        var message = "Test success";
        
        // Act
        var success = new Success(message);
        
        // Assert
        Assert.AreEqual(message, success.Message);
        Assert.IsTrue(success.Tags.IsEmpty);
    }

    [TestMethod]
    public void Constructor_WithNullMessage_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Success(null!));
    }

    [TestMethod]
    public void Constructor_WithEmptyMessage_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Success(string.Empty));
    }

    [TestMethod]
    public void Constructor_WithWhitespaceMessage_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Success("   "));
    }

    #endregion

    #region WithTag Tests

    [TestMethod]
    public void WithTag_WithValidParameters_ShouldAddTag()
    {
        // Arrange
        var success = new Success("Test success");
        
        // Act
        var taggedSuccess = success.WithTag("Key", "Value");
        
        // Assert
        Assert.AreEqual("Test success", taggedSuccess.Message);
        Assert.AreEqual("Value", taggedSuccess.Tags["Key"]);
    }

    [TestMethod]
    public void WithTag_WithNullKey_ShouldThrowArgumentNullException()
    {
        // Arrange
        var success = new Success("Test success");
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => success.WithTag(null!, "Value"));
    }

    [TestMethod]
    public void WithTag_WithEmptyKey_ShouldThrowArgumentException()
    {
        // Arrange
        var success = new Success("Test success");
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => success.WithTag("", "Value"));
    }

    #endregion

    #region Immutability Tests

    [TestMethod]
    public void WithTag_ShouldReturnNewInstance_OriginalUnchanged()
    {
        // Arrange
        var original = new Success("Test success");
        
        // Act
        var modified = original.WithTag("Key", "Value");
        
        // Assert
        Assert.AreNotSame(original, modified);
        Assert.IsTrue(original.Tags.IsEmpty);
        Assert.AreEqual("Value", modified.Tags["Key"]);
    }

    #endregion
}
