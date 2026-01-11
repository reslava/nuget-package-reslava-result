namespace REslava.Result.Reasons.Tests;

/// <summary>
/// Comprehensive tests for the Success class
/// </summary>
[TestClass]
public sealed class SuccessTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_Default_CreatesSuccessWithEmptyMessage()
    {
        // Act
        var success = new Success();

        // Assert
        Assert.IsNotNull(success);
        Assert.AreEqual(string.Empty, success.Message);
        Assert.IsNotNull(success.Tags);
        Assert.IsEmpty(success.Tags);
    }

    [TestMethod]
    public void Constructor_WithMessage_CreatesSuccessWithMessage()
    {
        // Act
        var success = new Success("Operation completed successfully");

        // Assert
        Assert.AreEqual("Operation completed successfully", success.Message);
        Assert.IsEmpty(success.Tags);
    }

    [TestMethod]
    public void Constructor_WithEmptyMessage_CreatesSuccessWithEmptyMessage()
    {
        // Act
        var success = new Success("");

        // Assert
        Assert.AreEqual("", success.Message);
    }

    [TestMethod]
    public void Constructor_WithNullMessage_CreatesSuccessWithNullMessage()
    {
        // Act
        var success = new Success(null!);

        // Assert
        Assert.IsNull(success.Message);
    }

    #endregion

    #region WithMessage Tests

    [TestMethod]
    public void WithMessage_UpdatesMessage()
    {
        // Arrange
        var success = new Success("Original message");

        // Act
        var result = success.WithMessage("Updated message");

        // Assert
        Assert.AreEqual("Updated message", success.Message);
        Assert.AreSame(success, result); // Fluent interface returns same instance
    }

    [TestMethod]
    public void WithMessage_ReturnsSameInstance()
    {
        // Arrange
        var success = new Success();

        // Act
        var result = success.WithMessage("New message");

        // Assert
        Assert.AreSame(success, result);
    }

    [TestMethod]
    public void WithMessage_EmptyString_UpdatesToEmpty()
    {
        // Arrange
        var success = new Success("Original");

        // Act
        success.WithMessage("");

        // Assert
        Assert.AreEqual("", success.Message);
    }

    [TestMethod]
    public void WithMessage_Chaining_Works()
    {
        // Arrange
        var success = new Success();

        // Act
        success.WithMessage("First")
               .WithMessage("Second")
               .WithMessage("Third");

        // Assert
        Assert.AreEqual("Third", success.Message);
    }

    #endregion

    #region WithTags Tests - Single Tag

    [TestMethod]
    public void WithTags_SingleTag_AddsTagCorrectly()
    {
        // Arrange
        var success = new Success("User created");

        // Act
        var result = success.WithTags("UserId", "12345");

        // Assert
        Assert.HasCount(1, success.Tags);
        Assert.IsTrue(success.Tags.ContainsKey("UserId"));
        Assert.AreEqual("12345", success.Tags["UserId"]);
        Assert.AreSame(success, result);
    }

    [TestMethod]
    public void WithTags_MultipleIndividualTags_AddsAllTags()
    {
        // Arrange
        var success = new Success("Record saved");

        // Act
        success.WithTags("Timestamp", DateTime.Now)
               .WithTags("RecordCount", 42)
               .WithTags("IsPartialSave", false);

        // Assert
        Assert.HasCount(3, success.Tags);
    }

    [TestMethod]
    public void WithTags_DifferentValueTypes_StoresCorrectly()
    {
        // Arrange
        var success = new Success("Operation completed");
        var dateTime = DateTime.Now;

        // Act
        success.WithTags("String", "text")
               .WithTags("Int", 123)
               .WithTags("Double", 3.14)
               .WithTags("Bool", true)
               .WithTags("DateTime", dateTime)
               .WithTags("Null", null!);

        // Assert
        Assert.HasCount(6, success.Tags);
        Assert.AreEqual("text", success.Tags["String"]);
        Assert.AreEqual(123, success.Tags["Int"]);
        Assert.AreEqual(3.14, success.Tags["Double"]);
        Assert.IsTrue((bool)success.Tags["Bool"]);
        Assert.AreEqual(dateTime, success.Tags["DateTime"]);
        Assert.IsNull(success.Tags["Null"]);
    }

    #endregion

    #region WithTags Tests - Dictionary

    [TestMethod]
    public void WithTags_Dictionary_AddsAllTagsCorrectly()
    {
        // Arrange
        var success = new Success("Data processed");
        var tags = new Dictionary<string, object>
        {
            { "ProcessedRecords", 100 },
            { "Duration", TimeSpan.FromSeconds(5) },
            { "BatchId", "BATCH-001" }
        };

        // Act
        var result = success.WithTags(tags);

        // Assert
        Assert.HasCount(3, success.Tags);
        Assert.AreEqual(100, success.Tags["ProcessedRecords"]);
        Assert.AreEqual(TimeSpan.FromSeconds(5), success.Tags["Duration"]);
        Assert.AreEqual("BATCH-001", success.Tags["BatchId"]);
        Assert.AreSame(success, result);
    }

    [TestMethod]
    public void WithTags_EmptyDictionary_AddsNoTags()
    {
        // Arrange
        var success = new Success();
        var emptyTags = new Dictionary<string, object>();

        // Act
        success.WithTags(emptyTags);

        // Assert
        Assert.IsEmpty(success.Tags);
    }

    [TestMethod]
    public void WithTags_MultipleTagsDictionary_AccumulatesTags()
    {
        // Arrange
        var success = new Success("Multi-step operation");
        var tags1 = new Dictionary<string, object> { { "Step1", "Completed" }, { "Step2", "Completed" } };
        var tags2 = new Dictionary<string, object> { { "Step3", "Completed" }, { "Step4", "Completed" } };

        // Act
        success.WithTags(tags1)
               .WithTags(tags2);

        // Assert
        Assert.HasCount(4, success.Tags);
        Assert.AreEqual("Completed", success.Tags["Step1"]);
        Assert.AreEqual("Completed", success.Tags["Step4"]);
    }

    #endregion

    #region Fluent Interface Tests

    [TestMethod]
    public void FluentInterface_CombinedUsage_Works()
    {
        // Act
        var success = new Success("Initial message")
            .WithTags("UserId", "user-123")
            .WithTags("Action", "Create")
            .WithMessage("User created successfully")
            .WithTags("CreatedAt", DateTime.Now);

        // Assert
        Assert.AreEqual("User created successfully", success.Message);
        Assert.HasCount(3, success.Tags);
        Assert.AreEqual("user-123", success.Tags["UserId"]);
        Assert.AreEqual("Create", success.Tags["Action"]);
    }

    [TestMethod]
    public void FluentInterface_ComplexChaining_MaintainsState()
    {
        // Act
        var success = new Success()
            .WithMessage("Step 1")
            .WithTags("A", 1)
            .WithMessage("Step 2")
            .WithTags("B", 2)
            .WithTags("C", 3)
            .WithMessage("Final step");

        // Assert
        Assert.AreEqual("Final step", success.Message);
        Assert.HasCount(3, success.Tags);
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void ToString_WithoutTags_ShowsMessageOnly()
    {
        // Arrange
        var success = new Success("Simple success");

        // Act
        var result = success.ToString();

        // Assert
        Assert.Contains("Success:", result);
        Assert.Contains("Simple success", result);
        Assert.DoesNotContain("Tags:", result);
    }

    [TestMethod]
    public void ToString_WithTags_ShowsMessageAndTags()
    {
        // Arrange
        var success = new Success("Success with tags")
            .WithTags("RecordId", "REC-001")
            .WithTags("Status", "Active");

        // Act
        var result = success.ToString();

        // Assert
        Assert.Contains("Success:", result);
        Assert.Contains("Success with tags", result);
        Assert.Contains("Tags:", result);
    }

    [TestMethod]
    public void ToString_EmptySuccess_ShowsSuccessType()
    {
        // Arrange
        var success = new Success();

        // Act
        var result = success.ToString();

        // Assert
        Assert.Contains("Success:", result);
    }

    #endregion

    #region Interface Implementation Tests

    [TestMethod]
    public void Success_ImplementsISuccess()
    {
        // Arrange
        var success = new Success();

        // Assert
        Assert.IsInstanceOfType<ISuccess>(success);
    }

    [TestMethod]
    public void Success_ImplementsIReason()
    {
        // Arrange
        var success = new Success();

        // Assert
        Assert.IsInstanceOfType<IReason>(success);
    }

    [TestMethod]
    public void Success_InheritsFromReasonOfSuccess()
    {
        // Arrange
        var success = new Success();

        // Assert
        Assert.IsInstanceOfType<Reason<Success>>(success);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void WithTags_DuplicateKey_ThrowsArgumentException()
    {
        // Arrange
        var success = new Success();
        success.WithTags("Key", "Value1");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => success.WithTags("Key", "Value2"));
    }

    [TestMethod]
    public void WithTags_Dictionary_DuplicateKeyInDictionary_ThrowsArgumentException()
    {
        // Arrange
        var success = new Success();
        success.WithTags("Existing", "Value");
        var tags = new Dictionary<string, object> { { "Existing", "NewValue" } };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => success.WithTags(tags));
    }

    [TestMethod]
    public void Tags_Property_IsNotNull()
    {
        // Arrange
        var success = new Success();

        // Assert
        Assert.IsNotNull(success.Tags);
    }

    #endregion

    #region Use in Result Tests

    [TestMethod]
    public void Success_UsedInResult_Works()
    {
        // Arrange
        var success = new Success("Email sent successfully")
            .WithTags("RecipientCount", 5)
            .WithTags("MessageId", "MSG-12345");

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
            .WithTags("RecordId", "REC-001");

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
            .WithTags("Step", 1);
        var success2 = new Success("Step 2 completed")
            .WithTags("Step", 2);

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
            .WithTags("TotalRecords", 1000)
            .WithTags("ProcessedRecords", 1000)
            .WithTags("FailedRecords", 0)
            .WithTags("Duration", TimeSpan.FromMinutes(5))
            .WithTags("BatchId", "BATCH-20250111-001");

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
            .WithTags("UserId", userId)
            .WithTags("Timestamp", timestamp)
            .WithTags("IPAddress", "192.168.1.1")
            .WithTags("UserAgent", "Mozilla/5.0");

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
            .WithSuccess(new Success("Order validated").WithTags("Step", "Validation"))
            .WithSuccess(new Success("Payment processed").WithTags("Step", "Payment"))
            .WithSuccess(new Success("Inventory reserved").WithTags("Step", "Inventory"))
            .WithSuccess(new Success("Order confirmed").WithTags("Step", "Confirmation"));

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(4, result.Successes);
        Assert.IsTrue(result.Successes.All(s => s.Tags.ContainsKey("Step")));
    }

    #endregion
}
