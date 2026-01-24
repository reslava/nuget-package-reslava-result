using System.Collections.Immutable;

namespace REslava.Result.Reasons.Tests;

/// <summary>
/// Comprehensive tests for the Success class (immutable version)
/// </summary>
[TestClass]
public sealed class SuccessImmutableTests
{
    #region Constructor Tests
    
    [TestMethod]
    public void Constructor_WithMessage_CreatesSuccessWithMessage()
    {
        // Act
        var success = new Success("Operation completed successfully");

        // Assert
        Assert.AreEqual("Operation completed successfully", success.Message);
        Assert.IsTrue(success.Tags.IsEmpty);
    }

    [TestMethod]
    public void Constructor_WithNullMessage_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new Success(null!));
        Assert.Contains(ValidationExtensions.DefaultNullOrWhitespaceMessage, ex.Message);
    }

    [TestMethod]
    public void Constructor_WithEmptyMessage_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new Success(""));
        Assert.Contains(ValidationExtensions.DefaultNullOrWhitespaceMessage, ex.Message);
    }

    #endregion

    #region WithMessage Tests

    [TestMethod]
    public void WithMessage_CreatesNewInstanceWithUpdatedMessage()
    {
        // Arrange
        var original = new Success("Original message");

        // Act
        var updated = original.WithMessage("Updated message");

        // Assert
        Assert.AreNotSame(original, updated);
        Assert.AreEqual("Original message", original.Message);
        Assert.AreEqual("Updated message", updated.Message);
    }

    [TestMethod]
    public void WithMessage_ReturnsSuccessType()
    {
        // Arrange
        var success = new Success("Test");

        // Act
        var result = success.WithMessage("New message");

        // Assert
        Assert.IsInstanceOfType<Success>(result);
    }

    [TestMethod]
    public void WithMessage_NullMessage_ThrowsArgumentException()
    {
        // Arrange
        var success = new Success("Test");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            success.WithMessage(null!));
        
        Assert.Contains(ValidationExtensions.DefaultNullOrWhitespaceMessage, exception.Message);
    }

    [TestMethod]
    public void WithMessage_EmptyString_ThrowsArgumentException()
    {
        // Arrange
        var success = new Success("Original");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => success.WithMessage(""));
    }

    [TestMethod]
    public void WithMessage_Chaining_CreatesIndependentInstances()
    {
        // Arrange
        var success = new Success("Original");

        // Act
        var s1 = success.WithMessage("First");
        var s2 = s1.WithMessage("Second");
        var s3 = s2.WithMessage("Third");

        // Assert
        Assert.AreEqual("Original", success.Message);
        Assert.AreEqual("First", s1.Message);
        Assert.AreEqual("Second", s2.Message);
        Assert.AreEqual("Third", s3.Message);
    }

    #endregion

    #region WithTags Tests - Single Tag

    [TestMethod]
    public void WithTags_SingleTag_CreatesNewInstanceWithTag()
    {
        // Arrange
        var original = new Success("User created");

        // Act
        var updated = original.WithTag("UserId", "12345");

        // Assert
        Assert.AreNotSame(original, updated);
        Assert.IsTrue(original.Tags.IsEmpty);
        Assert.HasCount(1, updated.Tags);
        Assert.IsTrue(updated.Tags.ContainsKey("UserId"));
        Assert.AreEqual("12345", updated.Tags["UserId"]);
    }

    [TestMethod]
    public void WithTags_SingleTag_ReturnsSuccessType()
    {
        // Arrange
        var success = new Success("Test");

        // Act
        var result = success.WithTag("Key", "Value");

        // Assert
        Assert.IsInstanceOfType<Success>(result);
    }

    [TestMethod]
    public void WithTags_SingleTag_PreservesMessage()
    {
        // Arrange
        var success = new Success("Original message");

        // Act
        var updated = success.WithTag("Key", "Value");

        // Assert
        Assert.AreEqual("Original message", updated.Message);
    }

    [TestMethod]
    public void WithTags_MultipleIndividualTags_AddsAllTags()
    {
        // Arrange
        var success = new Success("Record saved");

        // Act
        var result = success
            .WithTag("Timestamp", DateTime.Now)
            .WithTag("RecordCount", 42)
            .WithTag("IsPartialSave", false);

        // Assert
        Assert.HasCount(3, result.Tags);
    }

    [TestMethod]
    public void WithTags_DifferentValueTypes_StoresCorrectly()
    {
        // Arrange
        var success = new Success("Operation completed");
        var dateTime = DateTime.Now;
        var timeSpan = TimeSpan.FromMinutes(5);

        // Act
        var result = success
            .WithTag("String", "text")
            .WithTag("Int", 123)
            .WithTag("Double", 3.14)
            .WithTag("Bool", true)
            .WithTag("DateTime", dateTime)
            .WithTag("TimeSpan", timeSpan)
            .WithTag("Null", null!);

        // Assert
        Assert.HasCount(7, result.Tags);
        Assert.AreEqual("text", result.Tags["String"]);
        Assert.AreEqual(123, result.Tags["Int"]);
        Assert.AreEqual(3.14, result.Tags["Double"]);
        Assert.IsTrue((bool)result.Tags["Bool"]);
        Assert.AreEqual(dateTime, result.Tags["DateTime"]);
        Assert.AreEqual(timeSpan, result.Tags["TimeSpan"]);
        Assert.IsNull(result.Tags["Null"]);
    }

    [TestMethod]
    public void WithTags_DuplicateKey_ThrowsArgumentException()
    {
        // Arrange
        var success = new Success("Test");
        var withTag = success.WithTag("Key", "Value1");

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
        var success = new Success("Data processed");

        // Act
        var updated = success.WithTags(
            ("ProcessedRecords", 100),
            ("Duration", TimeSpan.FromSeconds(5)),
            ("BatchId", "BATCH-001")
        );

        // Assert
        Assert.AreNotSame(success, updated);
        Assert.IsTrue(success.Tags.IsEmpty);
        Assert.HasCount(3, updated.Tags);
        Assert.AreEqual(100, updated.Tags["ProcessedRecords"]);
        Assert.AreEqual(TimeSpan.FromSeconds(5), updated.Tags["Duration"]);
        Assert.AreEqual("BATCH-001", updated.Tags["BatchId"]);
    }

    [TestMethod]
    public void WithTags_EmptyArray_ReturnsSameInstance()
    {
        // Arrange
        var success = new Success("Test");

        // Act
        var result = success.WithTags(Array.Empty<(string, object)>());

        // Assert
        Assert.AreSame(success, result);
    }

    [TestMethod]
    public void WithTags_NullArray_ReturnsSameInstance()
    {
        // Arrange
        var success = new Success("Test");

        // Act
        var result = success.WithTags(null!);

        // Assert
        Assert.AreSame(success, result);
    }

    [TestMethod]
    public void WithTags_MultipleTagsAccumulate_CreatesCorrectInstance()
    {
        // Arrange
        var success = new Success("Multi-step operation");

        // Act
        var result = success
            .WithTags(("Step1", "Completed"), ("Step2", "Completed"))
            .WithTags(("Step3", "Completed"), ("Step4", "Completed"));

        // Assert
        Assert.HasCount(4, result.Tags);
        Assert.AreEqual("Completed", result.Tags["Step1"]);
        Assert.AreEqual("Completed", result.Tags["Step4"]);
    }

    [TestMethod]
    public void WithTags_MultipleTagsParams_DuplicateKey_ThrowsArgumentException()
    {
        // Arrange
        var success = new Success("Test");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            success.WithTags(
                ("Key1", "Value1"),
                ("Key1", "Value2")
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
        var success = new Success("Initial message")
            .WithTag("UserId", "user-123")
            .WithTag("Action", "Create")
            .WithMessage("User created successfully")
            .WithTag("CreatedAt", DateTime.Now);

        // Assert
        Assert.AreEqual("User created successfully", success.Message);
        Assert.HasCount(3, success.Tags);
        Assert.AreEqual("user-123", success.Tags["UserId"]);
        Assert.AreEqual("Create", success.Tags["Action"]);
    }

    [TestMethod]
    public void FluentInterface_ComplexChaining_MaintainsCorrectState()
    {
        // Act
        var success = new Success("Step 0")
            .WithMessage("Step 1")
            .WithTag("A", 1)
            .WithMessage("Step 2")
            .WithTag("B", 2)
            .WithTag("C", 3)
            .WithMessage("Final step");

        // Assert
        Assert.AreEqual("Final step", success.Message);
        Assert.HasCount(3, success.Tags);
        Assert.AreEqual(1, success.Tags["A"]);
        Assert.AreEqual(2, success.Tags["B"]);
        Assert.AreEqual(3, success.Tags["C"]);
    }

    #endregion

    #region Immutability Tests

    [TestMethod]
    public void Immutability_OriginalNeverModified()
    {
        // Arrange
        var original = new Success("Original");

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
        var s0 = new Success("S0");

        // Act
        var s1 = s0.WithTag("T1", 1);
        var s2 = s1.WithTag("T2", 2);
        var s1b = s1.WithTag("T1B", "1b");

        // Assert - s2 unaffected by s1b
        Assert.HasCount(1, s1.Tags);
        Assert.HasCount(2, s1b.Tags);
        Assert.HasCount(2, s2.Tags);
        Assert.IsFalse(s2.Tags.ContainsKey("T1B"));
    }

    #endregion

    #region Interface Implementation Tests

    [TestMethod]
    public void Success_ImplementsISuccess()
    {
        // Arrange
        // Act & Assert
        Assert.IsInstanceOfType<ISuccess>(new Success("X"));
    }

    [TestMethod]
    public void Success_ImplementsIReason()
    {
        // Arrange
        // Act & Assert
        Assert.IsInstanceOfType<IReason>(new Success("X"));
    }

    [TestMethod]
    public void Success_InheritsFromReasonOfSuccess()
    {
        // Arrange
        // Act & Assert
        Assert.IsInstanceOfType<Reason<Success>>(new Success("X"));
    }

    #endregion

    #region Use in Result Tests

    [TestMethod]
    public void Success_UsedInResult_Works()
    {
        // Arrange
        var success = new Success("Email sent successfully")
            .WithTag("RecipientCount", 5)
            .WithTag("MessageId", "MSG-12345");

        // Act
        var result = Result.Ok()
            .WithSuccess(success);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(1, result.Successes);
        Assert.AreEqual("Email sent successfully", result.Successes[0].Message);
        Assert.AreEqual(5, result.Successes[0].Tags["RecipientCount"]);
        Assert.AreEqual("MSG-12345", result.Successes[0].Tags["MessageId"]);
    }

    [TestMethod]
    public void Success_UsedInResultOfT_Works()
    {
        // Arrange
        var success = new Success("Record created")
            .WithTag("RecordId", "REC-001");

        // Act
        var result = Result<int>.Ok(42)
            .WithSuccess(success);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
        Assert.HasCount(1, result.Successes);
        Assert.AreEqual("Record created", result.Successes[0].Message);
    }

    [TestMethod]
    public void Success_MultipleSuccessReasons_Works()
    {
        // Arrange
        var success1 = new Success("Step 1 completed")
            .WithTag("Step", 1);
        var success2 = new Success("Step 2 completed")
            .WithTag("Step", 2);

        // Act
        var result = Result<string>.Ok("Final result")
            .WithSuccess(success1)
            .WithSuccess(success2);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(2, result.Successes);
        Assert.AreEqual("Step 1 completed", result.Successes[0].Message);
        Assert.AreEqual("Step 2 completed", result.Successes[1].Message);
    }

    #endregion

    #region Comparison with Error Tests

    [TestMethod]
    public void Success_DifferentFromError_InType()
    {
        // Arrange
        var success = new Success("Success message");
        var error = new Error("Error message");

        // Assert
        Assert.IsInstanceOfType<ISuccess>(success);
        Assert.IsInstanceOfType<IError>(error);
        Assert.IsNotInstanceOfType<IError>(success);
        Assert.IsNotInstanceOfType<ISuccess>(error);
    }

    [TestMethod]
    public void Success_AndError_CanCoexistInResult()
    {
        // Act
        var result = Result.Ok()
            .WithSuccess("Partial success")
            .WithError("Minor issue");

        // Assert
        Assert.IsTrue(result.IsFailed); // Has error, so it's failed
        Assert.HasCount(1, result.Successes);
        Assert.HasCount(1, result.Errors);
    }

    #endregion

    #region Real-World Scenario Tests

    [TestMethod]
    public void Success_BatchProcessing_Scenario()
    {
        // Arrange & Act
        var success = new Success("Batch processing completed")
            .WithTag("TotalRecords", 1000)
            .WithTag("ProcessedRecords", 1000)
            .WithTag("FailedRecords", 0)
            .WithTag("Duration", TimeSpan.FromMinutes(5))
            .WithTag("BatchId", "BATCH-20250111-001");

        var result = Result<int>.Ok(1000)
            .WithSuccess(success);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1000, result.Value);
        Assert.AreEqual(0, result.Successes[0].Tags["FailedRecords"]);
        Assert.AreEqual("BATCH-20250111-001", result.Successes[0].Tags["BatchId"]);
    }

    [TestMethod]
    public void Success_AuditTrail_Scenario()
    {
        // Arrange
        var userId = "user-123";
        var timestamp = DateTime.UtcNow;

        // Act
        var success = new Success("User login successful")
            .WithTag("UserId", userId)
            .WithTag("Timestamp", timestamp)
            .WithTag("IPAddress", "192.168.1.1")
            .WithTag("UserAgent", "Mozilla/5.0");

        // Assert
        Assert.AreEqual(userId, success.Tags["UserId"]);
        Assert.AreEqual(timestamp, success.Tags["Timestamp"]);
        Assert.HasCount(4, success.Tags);
    }

    [TestMethod]
    public void Success_WorkflowTracking_Scenario()
    {
        // Arrange & Act
        var result = Result<string>.Ok("Order-12345")
            .WithSuccess(new Success("Order validated").WithTag("Step", "Validation"))
            .WithSuccess(new Success("Payment processed").WithTag("Step", "Payment"))
            .WithSuccess(new Success("Inventory reserved").WithTag("Step", "Inventory"))
            .WithSuccess(new Success("Order confirmed").WithTag("Step", "Confirmation"));

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(4, result.Successes);
        Assert.IsTrue(result.Successes.All(s => s.Tags.ContainsKey("Step")));
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void EdgeCase_ManyTagsChained_MaintainsState()
    {
        // Arrange
        var success = new Success("Test");

        // Act
        var result = success;
        for (int i = 0; i < 100; i++)
        {
            result = result.WithTag($"Key{i}", i);
        }

        // Assert
        Assert.HasCount(100, result.Tags);
        Assert.AreEqual(50, result.Tags["Key50"]);
        
        // Original unchanged
        Assert.IsTrue(success.Tags.IsEmpty);
    }

    #endregion

    #region CRTP Pattern Tests

    [TestMethod]
    public void CRTP_FluentMethods_ReturnSuccessType()
    {
        // Arrange
        var success = new Success("Test");

        // Act
        Success s1 = success.WithMessage("M1");
        Success s2 = s1.WithTag("K", "V");
        Success s3 = s2.WithTags(("K2", "V2"));

        // Assert - All return Success, enabling fluent chaining
        Assert.IsInstanceOfType<Success>(s1);
        Assert.IsInstanceOfType<Success>(s2);
        Assert.IsInstanceOfType<Success>(s3);
    }

    #endregion
}
