using System.Collections.Immutable;

namespace REslava.Result.Reasons.Tests;

/// <summary>
/// Comprehensive tests for the generic Reason<TReason> class (CRTP pattern)
/// </summary>
[TestClass]
public sealed class ReasonGenericTests
{
    #region Test Helper Class

    // Concrete implementation for testing abstract Reason<T> class
    private class TestReason : Reason<TestReason>
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

    #region WithMessage Tests

    [TestMethod]
    public void WithMessage_CreatesNewInstance()
    {
        // Arrange
        var original = new TestReason("Original");

        // Act
        var updated = original.WithMessage("Updated");

        // Assert
        Assert.AreNotSame(original, updated);
        Assert.AreEqual("Original", original.Message);
        Assert.AreEqual("Updated", updated.Message);
    }

    [TestMethod]
    public void WithMessage_ReturnsCorrectType()
    {
        // Arrange
        var reason = new TestReason("Test");

        // Act
        var result = reason.WithMessage("New");

        // Assert
        Assert.IsInstanceOfType<TestReason>(result);
    }

    [TestMethod]
    public void WithMessage_PreservesTags()
    {
        // Arrange
        var tags = ImmutableDictionary<string, object>.Empty
            .Add("Key1", "Value1")
            .Add("Key2", 42);
        var reason = new TestReason("Original", tags);

        // Act
        var updated = reason.WithMessage("Updated");

        // Assert
        Assert.AreEqual("Updated", updated.Message);
        Assert.AreEqual(2, updated.Tags.Count);
        Assert.AreEqual("Value1", updated.Tags["Key1"]);
        Assert.AreEqual(42, updated.Tags["Key2"]);
    }

    [TestMethod]
    public void WithMessage_NullMessage_ThrowsArgumentException()
    {
        // Arrange
        var reason = new TestReason("Test");

        // Act & Assert        
        var exception = Assert.Throws<ArgumentException>(() => 
            reason.WithMessage(null!));
            
        
        Assert.AreEqual("Value cannot be null. (Parameter 'message')", exception.Message);
    }

    [TestMethod]
    public void WithMessage_EmptyMessage_ThrowsArgumentException()
    {
        // Arrange
        var reason = new TestReason("Test");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            reason.WithMessage(""));
        
        Assert.AreEqual("The value cannot be an empty string or composed entirely of whitespace. (Parameter 'message')", exception.Message);
    }

    [TestMethod]
    public void WithMessage_WhitespaceMessage_ThrowsArgumentException()
    {
        // Arrange
        var reason = new TestReason("Test");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            reason.WithMessage("   "));
        
        Assert.AreEqual("The value cannot be an empty string or composed entirely of whitespace. (Parameter 'message')", exception.Message);
    }

    [TestMethod]
    public void WithMessage_Chaining_CreatesMultipleNewInstances()
    {
        // Arrange
        var original = new TestReason("Original");

        // Act
        var step1 = original.WithMessage("Step 1");
        var step2 = step1.WithMessage("Step 2");
        var step3 = step2.WithMessage("Step 3");

        // Assert
        Assert.AreEqual("Original", original.Message);
        Assert.AreEqual("Step 1", step1.Message);
        Assert.AreEqual("Step 2", step2.Message);
        Assert.AreEqual("Step 3", step3.Message);
        
        Assert.AreNotSame(original, step1);
        Assert.AreNotSame(step1, step2);
        Assert.AreNotSame(step2, step3);
    }

    #endregion

    #region WithTags Tests - Single Tag

    [TestMethod]
    public void WithTags_SingleTag_CreatesNewInstanceWithTag()
    {
        // Arrange
        var original = new TestReason("Test");

        // Act
        var updated = original.WithTags("Key", "Value");

        // Assert
        Assert.AreNotSame(original, updated);
        Assert.IsTrue(original.Tags.IsEmpty);
        Assert.AreEqual(1, updated.Tags.Count);
        Assert.AreEqual("Value", updated.Tags["Key"]);
    }

    [TestMethod]
    public void WithTags_SingleTag_ReturnsCorrectType()
    {
        // Arrange
        var reason = new TestReason("Test");

        // Act
        var result = reason.WithTags("Key", "Value");

        // Assert
        Assert.IsInstanceOfType<TestReason>(result);
    }

    [TestMethod]
    public void WithTags_SingleTag_PreservesMessage()
    {
        // Arrange
        var reason = new TestReason("Original message");

        // Act
        var updated = reason.WithTags("Key", "Value");

        // Assert
        Assert.AreEqual("Original message", updated.Message);
    }

    [TestMethod]
    public void WithTags_SingleTag_PreservesExistingTags()
    {
        // Arrange
        var tags = ImmutableDictionary<string, object>.Empty
            .Add("Existing", "ExistingValue");
        var reason = new TestReason("Test", tags);

        // Act
        var updated = reason.WithTags("New", "NewValue");

        // Assert
        Assert.AreEqual(2, updated.Tags.Count);
        Assert.AreEqual("ExistingValue", updated.Tags["Existing"]);
        Assert.AreEqual("NewValue", updated.Tags["New"]);
    }

    [TestMethod]
    public void WithTags_SingleTag_NullKey_ThrowsArgumentException()
    {
        // Arrange
        var reason = new TestReason("Test");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            reason.WithTags(null!, "Value"));
        
        Assert.AreEqual("Tag key cannot be empty (Parameter 'key')", exception.Message);
    }

    [TestMethod]
    public void WithTags_SingleTag_EmptyKey_ThrowsArgumentException()
    {
        // Arrange
        var reason = new TestReason("Test");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            reason.WithTags("", "Value"));
        
        Assert.AreEqual("Tag key cannot be empty (Parameter 'key')", exception.Message);
    }

    [TestMethod]
    public void WithTags_SingleTag_WhitespaceKey_ThrowsArgumentException()
    {
        // Arrange
        var reason = new TestReason("Test");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            reason.WithTags("   ", "Value"));
        
        Assert.AreEqual("Tag key cannot be empty (Parameter 'key')", exception.Message);
    }

    [TestMethod]
    public void WithTags_SingleTag_DuplicateKey_ThrowsArgumentException()
    {
        // Arrange
        var tags = ImmutableDictionary<string, object>.Empty
            .Add("Existing", "Value1");
        var reason = new TestReason("Test", tags);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            reason.WithTags("Existing", "Value2"));
        
        Assert.AreEqual("Tag with key 'Existing' already exists (Parameter 'key')", exception.Message);
    }

    [TestMethod]
    public void WithTags_SingleTag_NullValue_Allowed()
    {
        // Arrange
        var reason = new TestReason("Test");

        // Act
        var updated = reason.WithTags("Key", null!);

        // Assert
        Assert.AreEqual(1, updated.Tags.Count);
        Assert.IsTrue(updated.Tags.ContainsKey("Key"));
        Assert.IsNull(updated.Tags["Key"]);
    }

    [TestMethod]
    public void WithTags_SingleTag_VariousValueTypes_Supported()
    {
        // Arrange
        var reason = new TestReason("Test");
        var now = DateTime.UtcNow;

        // Act
        var r1 = reason.WithTags("String", "text");
        var r2 = r1.WithTags("Int", 42);
        var r3 = r2.WithTags("Bool", true);
        var r4 = r3.WithTags("DateTime", now);

        // Assert
        Assert.AreEqual("text", r4.Tags["String"]);
        Assert.AreEqual(42, r4.Tags["Int"]);
        Assert.IsTrue((bool)r4.Tags["Bool"]);
        Assert.AreEqual(now, r4.Tags["DateTime"]);
    }

    #endregion

    #region WithTags Tests - Multiple Tags (params)

    [TestMethod]
    public void WithTags_MultipleTags_CreatesNewInstanceWithAllTags()
    {
        // Arrange
        var original = new TestReason("Test");

        // Act
        var updated = original.WithTags(
            ("Key1", "Value1"),
            ("Key2", 42),
            ("Key3", true)
        );

        // Assert
        Assert.AreNotSame(original, updated);
        Assert.IsTrue(original.Tags.IsEmpty);
        Assert.AreEqual(3, updated.Tags.Count);
        Assert.AreEqual("Value1", updated.Tags["Key1"]);
        Assert.AreEqual(42, updated.Tags["Key2"]);
        Assert.IsTrue((bool)updated.Tags["Key3"]);
    }

    [TestMethod]
    public void WithTags_MultipleTags_EmptyArray_ReturnsSameInstance()
    {
        // Arrange
        var original = new TestReason("Test");

        // Act
        var result = original.WithTags(Array.Empty<(string, object)>());

        // Assert
        Assert.AreSame(original, result);
    }

    [TestMethod]
    public void WithTags_MultipleTags_NullArray_ReturnsSameInstance()
    {
        // Arrange
        var original = new TestReason("Test");

        // Act
        var result = original.WithTags(null!);

        // Assert
        Assert.AreSame(original, result);
    }

    [TestMethod]
    public void WithTags_MultipleTags_PreservesExistingTags()
    {
        // Arrange
        var tags = ImmutableDictionary<string, object>.Empty
            .Add("Existing", "ExistingValue");
        var reason = new TestReason("Test", tags);

        // Act
        var updated = reason.WithTags(
            ("New1", "Value1"),
            ("New2", "Value2")
        );

        // Assert
        Assert.AreEqual(3, updated.Tags.Count);
        Assert.AreEqual("ExistingValue", updated.Tags["Existing"]);
        Assert.AreEqual("Value1", updated.Tags["New1"]);
        Assert.AreEqual("Value2", updated.Tags["New2"]);
    }

    [TestMethod]
    public void WithTags_MultipleTags_NullKeyInArray_ThrowsArgumentException()
    {
        // Arrange
        var reason = new TestReason("Test");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            reason.WithTags(
                ("Valid", "Value"),
                (null!, "Value2")
            ));
        
        Assert.AreEqual("Tag key cannot be empty", exception.Message);
    }

    [TestMethod]
    public void WithTags_MultipleTags_EmptyKeyInArray_ThrowsArgumentException()
    {
        // Arrange
        var reason = new TestReason("Test");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            reason.WithTags(
                ("Valid", "Value"),
                ("", "Value2")
            ));
        
        Assert.AreEqual("Tag key cannot be empty", exception.Message);
    }

    [TestMethod]
    public void WithTags_MultipleTags_DuplicateKeyInArray_ThrowsArgumentException()
    {
        // Arrange
        var reason = new TestReason("Test");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            reason.WithTags(
                ("Key1", "Value1"),
                ("Key1", "Value2")  // Duplicate
            ));
        
        Assert.AreEqual("Tag with key 'Key1' already exists (Parameter 'key')", exception.Message);
    }

    [TestMethod]
    public void WithTags_MultipleTags_DuplicateWithExisting_ThrowsArgumentException()
    {
        // Arrange
        var tags = ImmutableDictionary<string, object>.Empty
            .Add("Existing", "Value");
        var reason = new TestReason("Test", tags);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            reason.WithTags(
                ("New", "NewValue"),
                ("Existing", "DuplicateValue")
            ));
        
        Assert.AreEqual("Tag with key 'Existing' already exists (Parameter 'key')", exception.Message);
    }

    #endregion

    #region Fluent Chaining Tests

    [TestMethod]
    public void FluentChaining_WithMessageAndTags_CreatesCorrectInstance()
    {
        // Arrange
        var original = new TestReason("Original");

        // Act
        var result = original
            .WithMessage("Updated")
            .WithTags("Key1", "Value1")
            .WithTags("Key2", 42)
            .WithMessage("Final");

        // Assert
        Assert.AreEqual("Final", result.Message);
        Assert.AreEqual(2, result.Tags.Count);
        Assert.AreEqual("Value1", result.Tags["Key1"]);
        Assert.AreEqual(42, result.Tags["Key2"]);
        
        // Original unchanged
        Assert.AreEqual("Original", original.Message);
        Assert.IsTrue(original.Tags.IsEmpty);
    }

    [TestMethod]
    public void FluentChaining_ComplexChain_EachStepCreatesNewInstance()
    {
        // Arrange
        var step0 = new TestReason("Step 0");

        // Act
        var step1 = step0.WithTags("S1", 1);
        var step2 = step1.WithMessage("Step 2");
        var step3 = step2.WithTags("S3", 3);
        var step4 = step3.WithTags(
            ("S4A", "4a"),
            ("S4B", "4b")
        );

        // Assert - Each step is independent
        Assert.AreEqual("Step 0", step0.Message);
        Assert.IsTrue(step0.Tags.IsEmpty);

        Assert.AreEqual("Step 0", step1.Message);
        Assert.AreEqual(1, step1.Tags.Count);

        Assert.AreEqual("Step 2", step2.Message);
        Assert.AreEqual(1, step2.Tags.Count);

        Assert.AreEqual("Step 2", step3.Message);
        Assert.AreEqual(2, step3.Tags.Count);

        Assert.AreEqual("Step 2", step4.Message);
        Assert.AreEqual(4, step4.Tags.Count);
    }

    [TestMethod]
    public void FluentChaining_ReturnsCorrectTypeAtEachStep()
    {
        // Arrange
        var reason = new TestReason("Test");

        // Act & Assert - Each method returns TestReason
        TestReason r1 = reason.WithMessage("M1");
        TestReason r2 = r1.WithTags("K1", "V1");
        TestReason r3 = r2.WithTags(("K2", "V2"), ("K3", "V3"));

        Assert.IsInstanceOfType<TestReason>(r1);
        Assert.IsInstanceOfType<TestReason>(r2);
        Assert.IsInstanceOfType<TestReason>(r3);
    }

    #endregion

    #region Immutability Tests

    [TestMethod]
    public void Immutability_OriginalInstanceNeverModified()
    {
        // Arrange
        var tags = ImmutableDictionary<string, object>.Empty
            .Add("Original", "OriginalValue");
        var original = new TestReason("Original Message", tags);

        // Act
        var modified = original
            .WithMessage("New Message")
            .WithTags("New", "NewValue");

        // Assert - Original completely unchanged
        Assert.AreEqual("Original Message", original.Message);
        Assert.AreEqual(1, original.Tags.Count);
        Assert.AreEqual("OriginalValue", original.Tags["Original"]);
        Assert.IsFalse(original.Tags.ContainsKey("New"));

        // Modified is different
        Assert.AreEqual("New Message", modified.Message);
        Assert.AreEqual(2, modified.Tags.Count);
        Assert.AreEqual("OriginalValue", modified.Tags["Original"]);
        Assert.AreEqual("NewValue", modified.Tags["New"]);
    }

    [TestMethod]
    public void Immutability_IntermediateInstancesIndependent()
    {
        // Arrange
        var r0 = new TestReason("R0");

        // Act
        var r1 = r0.WithTags("T1", 1);
        var r2 = r1.WithTags("T2", 2);
        var r3 = r2.WithTags("T3", 3);

        // Now modify r1 again
        var r1b = r1.WithTags("T1B", "1b");

        // Assert - r2 and r3 should be unaffected by r1b
        Assert.AreEqual(1, r1.Tags.Count);
        Assert.AreEqual(2, r1b.Tags.Count);
        Assert.AreEqual(2, r2.Tags.Count);
        Assert.AreEqual(3, r3.Tags.Count);

        Assert.IsFalse(r2.Tags.ContainsKey("T1B"));
        Assert.IsFalse(r3.Tags.ContainsKey("T1B"));
        Assert.IsTrue(r1b.Tags.ContainsKey("T1B"));
    }

    #endregion

    #region CRTP Pattern Tests

    [TestMethod]
    public void CRTP_WithMessage_ReturnsCorrectDerivedType()
    {
        // Arrange
        var reason = new TestReason("Test");

        // Act
        var result = reason.WithMessage("Updated");

        // Assert - Returns TestReason, not Reason<TestReason>
        Assert.IsInstanceOfType<TestReason>(result);
        
        // Can call TestReason methods directly
        var result2 = result.WithTags("Key", "Value");
        Assert.IsInstanceOfType<TestReason>(result2);
    }

    [TestMethod]
    public void CRTP_WithTags_ReturnsCorrectDerivedType()
    {
        // Arrange
        var reason = new TestReason("Test");

        // Act
        var result = reason.WithTags("Key", "Value");

        // Assert
        Assert.IsInstanceOfType<TestReason>(result);
    }

    [TestMethod]
    public void CRTP_CreateNew_CalledCorrectly()
    {
        // Arrange
        var reason = new TestReason("Original");

        // Act - CreateNew should be called internally
        var result = reason.WithMessage("Updated");

        // Assert - Verify it's a new instance with correct values
        Assert.AreNotSame(reason, result);
        Assert.AreEqual("Updated", result.Message);
        Assert.IsInstanceOfType<TestReason>(result);
    }

    #endregion

    #region Inheritance Tests

    [TestMethod]
    public void Inheritance_InheritsFromReason()
    {
        // Arrange
        var reason = new TestReason("Test");

        // Assert
        Assert.IsInstanceOfType<Reason>(reason);
    }

    [TestMethod]
    public void Inheritance_ImplementsIReason()
    {
        // Arrange
        var reason = new TestReason("Test");

        // Assert
        Assert.IsInstanceOfType<IReason>(reason);
    }

    [TestMethod]
    public void Inheritance_GenericParameter_MatchesDerivedType()
    {
        // Arrange
        var reason = new TestReason("Test");

        // Assert - TestReason : Reason<TestReason>
        Assert.IsInstanceOfType<Reason<TestReason>>(reason);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void EdgeCase_VeryLongChain_MaintainsCorrectState()
    {
        // Arrange
        var reason = new TestReason("Start");

        // Act - Build long chain
        var result = reason;
        for (int i = 0; i < 100; i++)
        {
            result = result.WithTags($"Key{i}", i);
        }
        result = result.WithMessage("End");

        // Assert
        Assert.AreEqual("End", result.Message);
        Assert.AreEqual(100, result.Tags.Count);
        Assert.AreEqual(0, result.Tags["Key0"]);
        Assert.AreEqual(99, result.Tags["Key99"]);
        
        // Original unchanged
        Assert.AreEqual("Start", reason.Message);
        Assert.IsTrue(reason.Tags.IsEmpty);
    }

    [TestMethod]
    public void EdgeCase_LargeNumberOfTags_SingleCall()
    {
        // Arrange
        var reason = new TestReason("Test");
        var tagArray = Enumerable.Range(0, 1000)
            .Select(i => ($"Key{i}", (object)i))
            .ToArray();

        // Act
        var result = reason.WithTags(tagArray);

        // Assert
        Assert.AreEqual(1000, result.Tags.Count);
        Assert.AreEqual(500, result.Tags["Key500"]);
    }

    #endregion
}
