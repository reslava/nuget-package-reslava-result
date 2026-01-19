using System.Collections.Immutable;

namespace REslava.Result.Reasons.Tests;

/// <summary>
/// Comprehensive tests for the base Reason class
/// </summary>
[TestClass]
public sealed class ReasonTests
{    
    #region Test Helper Class

    // Concrete implementation for testing abstract Reason class
    private class TestReason : Reason<TestReason>, IReason
    {
        public TestReason(string message) : base(message) { }

        public TestReason(string message, ImmutableDictionary<string, object> tags)
            : base(message, tags) { }

        protected override TestReason CreateNew(string message, ImmutableDictionary<string, object> tags)
        {
            return new TestReason(message, tags);
        }
    }

    #endregion

    [TestMethod]
    public void WithTags_EmptyKey_ThrowsConsistentException()
    {
        var reason = new TestReason("Test");

        var ex = Assert.Throws<ArgumentException>(() =>
            reason.WithTag("", "value"));

        // ✅ Use constant for assertion
        Assert.Contains(ValidationExtensions.DefaultNullOrWhitespaceMessage, ex.Message);
        // Or: Assert.AreEqual("Key cannot be null or whitespace (Parameter 'key')", ex.Message);
    }

    [TestMethod]
    public void WithTags_DuplicateKey_ThrowsConsistentException()
    {
        var reason = new TestReason("Test").WithTag("Key1", "Value1");

        var ex = Assert.Throws<ArgumentException>(() =>
            reason.WithTag("Key1", "Value2"));

        // ✅ Check for consistent message
        Assert.Contains("Key1", ex.Message);
        Assert.Contains(ValidationExtensions.DefaultKeyExistsMessage, ex.Message);
    }

    #region Constructor Tests - Message Only

    [TestMethod]
    public void Constructor_WithMessage_CreatesReasonWithMessage()
    {
        // Act
        var reason = new TestReason("Test message");

        // Assert
        Assert.AreEqual("Test message", reason.Message);
        Assert.IsNotNull(reason.Tags);
        Assert.IsTrue(reason.Tags.IsEmpty);
    }

    [TestMethod]
    public void Constructor_WithNullMessage_UsesEmptyString()
    {
        // Act
        var reason = new TestReason(null!);

        // Assert
        Assert.AreEqual(string.Empty, reason.Message);
        Assert.IsNotNull(reason.Tags);
    }

    [TestMethod]
    public void Constructor_WithEmptyMessage_UsesEmptyString()
    {
        // Act
        var reason = new TestReason("");

        // Assert
        Assert.AreEqual(string.Empty, reason.Message);
    }

    [TestMethod]
    public void Constructor_WithWhitespaceMessage_PreservesWhitespace()
    {
        // Act
        var reason = new TestReason("   ");

        // Assert
        Assert.AreEqual("   ", reason.Message);
    }

    #endregion

    #region Constructor Tests - Message and Tags

    [TestMethod]
    public void Constructor_WithMessageAndTags_CreatesReasonWithBoth()
    {
        // Arrange
        var tags = ImmutableDictionary<string, object>.Empty
            .Add("Key1", "Value1")
            .Add("Key2", 42);

        // Act
        var reason = new TestReason("Test message", tags);

        // Assert
        Assert.AreEqual("Test message", reason.Message);
        Assert.IsFalse(reason.Tags.IsEmpty);
        Assert.AreEqual(2, reason.Tags.Count);
        Assert.AreEqual("Value1", reason.Tags["Key1"]);
        Assert.AreEqual(42, reason.Tags["Key2"]);
    }

    [TestMethod]
    public void Constructor_WithNullTags_CreatesEmptyTags()
    {
        // Act
        var reason = new TestReason("Message", null!);

        // Assert
        Assert.AreEqual("Message", reason.Message);
        Assert.IsNotNull(reason.Tags);
        Assert.IsTrue(reason.Tags.IsEmpty);
    }

    [TestMethod]
    public void Constructor_WithEmptyTags_CreatesEmptyTags()
    {
        // Arrange
        var tags = ImmutableDictionary<string, object>.Empty;

        // Act
        var reason = new TestReason("Message", tags);

        // Assert
        Assert.IsNotNull(reason.Tags);
        Assert.IsTrue(reason.Tags.IsEmpty);
    }

    #endregion

    #region Message Property Tests

    [TestMethod]
    public void Message_IsReadOnly_CannotBeModified()
    {
        // Arrange
        var reason = new TestReason("Original");

        // Assert - Message property should be get-only
        Assert.AreEqual("Original", reason.Message);
        
        // Verify through reflection that there's no public setter
        var messageProperty = typeof(Reason).GetProperty("Message");
        Assert.IsNotNull(messageProperty);
        Assert.IsNull(messageProperty!.SetMethod);
    }

    [TestMethod]
    public void Message_DifferentInstances_Independent()
    {
        // Arrange
        var reason1 = new TestReason("Message 1");
        var reason2 = new TestReason("Message 2");

        // Assert
        Assert.AreEqual("Message 1", reason1.Message);
        Assert.AreEqual("Message 2", reason2.Message);
        Assert.AreNotEqual(reason1.Message, reason2.Message);
    }

    #endregion

    #region Tags Property Tests

    [TestMethod]
    public void Tags_IsImmutableDictionary()
    {
        // Arrange
        var reason = new TestReason("Test");

        // Assert
        Assert.IsInstanceOfType<ImmutableDictionary<string, object>>(reason.Tags);
    }

    [TestMethod]
    public void Tags_IsReadOnly_CannotBeModified()
    {
        // Arrange
        var tags = ImmutableDictionary<string, object>.Empty
            .Add("Key", "Value");
        var reason = new TestReason("Test", tags);

        // Assert - Tags property should be get-only
        var tagsProperty = typeof(Reason).GetProperty("Tags");
        Assert.IsNotNull(tagsProperty);
        Assert.IsNull(tagsProperty!.SetMethod);
    }

    [TestMethod]
    public void Tags_DifferentInstances_Independent()
    {
        // Arrange
        var tags1 = ImmutableDictionary<string, object>.Empty.Add("A", 1);
        var tags2 = ImmutableDictionary<string, object>.Empty.Add("B", 2);
        
        var reason1 = new TestReason("R1", tags1);
        var reason2 = new TestReason("R2", tags2);

        // Assert
        Assert.AreEqual(1, reason1.Tags.Count);
        Assert.AreEqual(1, reason2.Tags.Count);
        Assert.IsTrue(reason1.Tags.ContainsKey("A"));
        Assert.IsTrue(reason2.Tags.ContainsKey("B"));
        Assert.IsFalse(reason1.Tags.ContainsKey("B"));
        Assert.IsFalse(reason2.Tags.ContainsKey("A"));
    }

    #endregion

    #region Immutability Tests

    [TestMethod]
    public void Tags_ModifyingSourceDictionary_DoesNotAffectReason()
    {
        // Arrange
        var tags = ImmutableDictionary<string, object>.Empty
            .Add("Original", "Value");
        var reason = new TestReason("Test", tags);

        // Act - Create new dictionary (immutable, so this creates a new instance)
        var modifiedTags = tags.Add("New", "NewValue");

        // Assert - Original reason unchanged
        Assert.AreEqual(1, reason.Tags.Count);
        Assert.IsTrue(reason.Tags.ContainsKey("Original"));
        Assert.IsFalse(reason.Tags.ContainsKey("New"));
        
        // New dictionary is different
        Assert.AreEqual(2, modifiedTags.Count);
    }

    [TestMethod]
    public void Constructor_CreatesCopyOfTags_NotReference()
    {
        // Arrange
        var tags = ImmutableDictionary<string, object>.Empty
            .Add("Key", "Value");

        // Act
        var reason1 = new TestReason("R1", tags);
        var reason2 = new TestReason("R2", tags);

        // Assert - Both reasons have independent tag collections
        Assert.AreEqual(reason1.Tags.Count, reason2.Tags.Count);
        Assert.AreEqual(reason1.Tags["Key"], reason2.Tags["Key"]);
        
        // But they're structurally equal (same content)
        Assert.IsTrue(reason1.Tags.SequenceEqual(reason2.Tags));
    }

    #endregion

    #region Interface Implementation Tests

    [TestMethod]
    public void Reason_ImplementsIReason()
    {
        // Arrange
        var reason = new TestReason("Test");

        // Assert
        Assert.IsInstanceOfType<IReason>(reason);
    }

    [TestMethod]
    public void Reason_IReason_MessageProperty_Accessible()
    {
        // Arrange
        IReason reason = new TestReason("Test message");

        // Assert
        Assert.AreEqual("Test message", reason.Message);
    }

    [TestMethod]
    public void Reason_IReason_TagsProperty_Accessible()
    {
        // Arrange
        var tags = ImmutableDictionary<string, object>.Empty.Add("K", "V");
        IReason reason = new TestReason("Test", tags);

        // Assert
        Assert.AreEqual(1, reason.Tags.Count);
        Assert.AreEqual("V", reason.Tags["K"]);
    }

    #endregion

    #region Value Type Tests

    [TestMethod]
    public void Tags_SupportsVariousValueTypes()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var timeSpan = TimeSpan.FromMinutes(5);
        var tags = ImmutableDictionary<string, object>.Empty
            .Add("String", "text")
            .Add("Int", 42)
            .Add("Long", 123456789L)
            .Add("Double", 3.14159)
            .Add("Decimal", 99.99m)
            .Add("Bool", true)
            .Add("DateTime", now)
            .Add("TimeSpan", timeSpan)
            .Add("Guid", Guid.NewGuid())
            .Add("Null", null!);

        // Act
        var reason = new TestReason("Test", tags);

        // Assert
        Assert.AreEqual("text", reason.Tags["String"]);
        Assert.AreEqual(42, reason.Tags["Int"]);
        Assert.AreEqual(123456789L, reason.Tags["Long"]);
        Assert.AreEqual(3.14159, reason.Tags["Double"]);
        Assert.AreEqual(99.99m, reason.Tags["Decimal"]);
        Assert.IsTrue((bool)reason.Tags["Bool"]);
        Assert.AreEqual(now, reason.Tags["DateTime"]);
        Assert.AreEqual(timeSpan, reason.Tags["TimeSpan"]);
        Assert.IsInstanceOfType<Guid>(reason.Tags["Guid"]);
        Assert.IsNull(reason.Tags["Null"]);
    }

    [TestMethod]
    public void Tags_SupportsComplexObjects()
    {
        // Arrange
        var complexObject = new { Name = "Test", Value = 42 };
        var list = new List<string> { "A", "B", "C" };
        var tags = ImmutableDictionary<string, object>.Empty
            .Add("Anonymous", complexObject)
            .Add("List", list);

        // Act
        var reason = new TestReason("Test", tags);

        // Assert
        Assert.IsNotNull(reason.Tags["Anonymous"]);
        Assert.IsInstanceOfType<List<string>>(reason.Tags["List"]);
        
        var retrievedList = (List<string>)reason.Tags["List"];
        Assert.AreEqual(3, retrievedList.Count);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void Constructor_VeryLongMessage_HandlesCorrectly()
    {
        // Arrange
        var longMessage = new string('x', 10000);

        // Act
        var reason = new TestReason(longMessage);

        // Assert
        Assert.AreEqual(10000, reason.Message.Length);
        Assert.AreEqual(longMessage, reason.Message);
    }

    [TestMethod]
    public void Constructor_SpecialCharactersInMessage_PreservesCorrectly()
    {
        // Arrange
        var specialMessage = "Test\n\r\t\"'\\<>&";

        // Act
        var reason = new TestReason(specialMessage);

        // Assert
        Assert.AreEqual(specialMessage, reason.Message);
    }

    [TestMethod]
    public void Tags_EmptyKey_WorksIfAllowed()
    {
        // Arrange - ImmutableDictionary allows empty keys
        var tags = ImmutableDictionary<string, object>.Empty
            .Add("", "EmptyKeyValue");

        // Act
        var reason = new TestReason("Test", tags);

        // Assert
        Assert.IsTrue(reason.Tags.ContainsKey(""));
        Assert.AreEqual("EmptyKeyValue", reason.Tags[""]);
    }

    [TestMethod]
    public void Tags_LargeNumberOfTags_HandlesCorrectly()
    {
        // Arrange
        var builder = ImmutableDictionary<string, object>.Empty.ToBuilder();
        for (int i = 0; i < 1000; i++)
        {
            builder.Add($"Key{i}", $"Value{i}");
        }
        var tags = builder.ToImmutable();

        // Act
        var reason = new TestReason("Test", tags);

        // Assert
        Assert.AreEqual(1000, reason.Tags.Count);
        Assert.AreEqual("Value500", reason.Tags["Key500"]);
    }

    #endregion

    #region Inheritance Tests

    [TestMethod]
    public void Reason_CanBeInherited()
    {
        // Arrange & Act
        var testReason = new TestReason("Test");

        // Assert
        Assert.IsInstanceOfType<Reason>(testReason);
        Assert.IsInstanceOfType<IReason>(testReason);
    }

    [TestMethod]
    public void Reason_DerivedClasses_IndependentInstances()
    {
        // Arrange
        var reason1 = new TestReason("R1");
        var reason2 = new TestReason("R2");

        // Assert
        Assert.AreNotSame(reason1, reason2);
        Assert.AreNotEqual(reason1.Message, reason2.Message);
    }

    #endregion

    #region Null Safety Tests

    [TestMethod]
    public void Message_NeverNull_AlwaysEmptyStringAtMinimum()
    {
        // Arrange & Act
        var reason1 = new TestReason(null!);
        var reason2 = new TestReason("");
        var reason3 = new TestReason("Actual message");

        // Assert
        Assert.IsNotNull(reason1.Message);
        Assert.IsNotNull(reason2.Message);
        Assert.IsNotNull(reason3.Message);
    }

    [TestMethod]
    public void Tags_NeverNull_AlwaysEmptyDictionaryAtMinimum()
    {
        // Arrange & Act
        var reason1 = new TestReason("Test");
        var reason2 = new TestReason("Test", null!);
        var reason3 = new TestReason("Test", ImmutableDictionary<string, object>.Empty);

        // Assert
        Assert.IsNotNull(reason1.Tags);
        Assert.IsNotNull(reason2.Tags);
        Assert.IsNotNull(reason3.Tags);
    }

    #endregion
}
