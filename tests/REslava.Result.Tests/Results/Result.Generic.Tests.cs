namespace REslava.Result.Tests;

/// <summary>
/// Comprehensive tests for the Result_TValue class (generic)
/// </summary>
[TestClass]
public sealed class ResultGenericTests
{
    #region Factory Methods - Ok

    [TestMethod]
    public void Ok_WithValue_CreatesSuccessResult()
    {
        // Act
        var result = Result<int>.Ok(42);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.IsFailed);
        Assert.AreEqual(42, result.Value);
        Assert.AreEqual(42, result.ValueOrDefault);
    }

    [TestMethod]
    public void Ok_WithNullValue_CreatesSuccessResult()
    {
        // Act
        var result = Result<string>.Ok(null!);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.Value);
        Assert.IsNull(result.ValueOrDefault);
    }

    [TestMethod]
    public void Ok_WithComplexType_CreatesSuccessResult()
    {
        // Arrange
        var person = new Person { Name = "John", Age = 30 };

        // Act
        var result = Result<Person>.Ok(person);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("John", result.Value!.Name);
        Assert.AreEqual(30, result.Value.Age);
    }

    #endregion

    #region Factory Methods - Fail

    [TestMethod]
    public void Fail_WithString_CreatesFailedResult()
    {
        // Act
        var result = Result<int>.Fail("Error occurred");

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual("Error occurred", result.Errors[0].Message);
    }

    [TestMethod]
    public void Fail_WithError_CreatesFailedResult()
    {
        // Arrange
        var error = new Error("Custom error");

        // Act
        var result = Result<int>.Fail(error);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
    }

    [TestMethod]
    public void Fail_WithMessages_CreatesFailedResultWithMultipleErrors()
    {
        // Arrange
        var messages = new[] { "Error 1", "Error 2" };

        // Act
        var result = Result<int>.Fail(messages);

        // Assert
        Assert.HasCount(2, result.Errors);
    }

    [TestMethod]
    public void Fail_WithErrors_CreatesFailedResultWithMultipleErrors()
    {
        // Arrange
        var errors = new[] { new Error("E1"), new Error("E2") };

        // Act
        var result = Result<int>.Fail(errors);

        // Assert
        Assert.HasCount(2, result.Errors);
    }

    #endregion

    #region Factory Methods - From

    [TestMethod]
    public void From_WithFailedResult_ConvertsToTypedResult()
    {
        // Arrange
        var baseResult = Result.Fail("Validation error");

        // Act
        var typedResult = Result<int>.FromResult(baseResult);

        // Assert
        Assert.IsTrue(typedResult.IsFailed);
        Assert.HasCount(1, typedResult.Errors);
        Assert.AreEqual("Validation error", typedResult.Errors[0].Message);
    }

    [TestMethod]
    public void From_WithSuccessResult_ThrowsInvalidOperationException()
    {
        // Arrange
        var baseResult = Result.Ok();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => Result<int>.FromResult(baseResult));
    }

    [TestMethod]
    public void From_WithNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Result<int>.FromResult(null!));
    }

    #endregion

    #region Value Property

    [TestMethod]
    public void Value_OnSuccess_ReturnsValue()
    {
        // Arrange
        var result = Result<int>.Ok(100);

        // Act
        var value = result.Value;

        // Assert
        Assert.AreEqual(100, value);
    }

    [TestMethod]
    public void Value_OnFailure_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result<int>.Fail("Error");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [TestMethod]
    public void ValueOrDefault_OnSuccess_ReturnsValue()
    {
        // Arrange
        var result = Result<int>.Ok(50);

        // Act
        var value = result.ValueOrDefault;

        // Assert
        Assert.AreEqual(50, value);
    }

    [TestMethod]
    public void ValueOrDefault_OnFailure_ReturnsDefault()
    {
        // Arrange
        var result = Result<int>.Fail("Error");

        // Act
        var value = result.ValueOrDefault;

        // Assert
        Assert.AreEqual(default(int), value);
    }

    #endregion

    #region Fluent Methods - With

    [TestMethod]
    public void WithSuccess_String_AddsSuccessAndReturnsTypedResult()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act
        var newResult = result.WithSuccess("Operation completed");

        // Assert
        Assert.IsInstanceOfType<Result<int>>(newResult);
        Assert.HasCount(1, newResult.Successes);
        Assert.AreEqual(10, newResult.Value);
    }

    [TestMethod]
    public void WithError_String_AddsErrorAndReturnsTypedResult()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act
        var newResult = result.WithError("Validation warning");

        // Assert
        Assert.IsInstanceOfType<Result<int>>(newResult);
        Assert.IsTrue(newResult.IsFailed);
        Assert.HasCount(1, newResult.Errors);
    }

    [TestMethod]
    public void FluentChaining_MaintainsType()
    {
        // Act
        var result = Result<string>.Ok("test")
            .WithSuccess("Step 1")
            .WithSuccess("Step 2")
            .WithError("Warning");

        // Assert
        Assert.IsInstanceOfType<Result<string>>(result);
        Assert.AreEqual("test", result.ValueOrDefault);
        Assert.HasCount(2, result.Successes);
        Assert.HasCount(1, result.Errors);
    }

    #endregion
             
    #region ToString

    [TestMethod]
    public void ToString_OnSuccess_ShowsValueAndStatus()
    {
        // Arrange
        var result = Result<int>.Ok(42);

        // Act
        var str = result.ToString();

        // Assert
        Assert.Contains("IsSuccess='True'", str);
        Assert.Contains("Value = 42", str);
    }

    [TestMethod]
    public void ToString_OnFailure_ShowsErrorAndStatus()
    {
        // Arrange
        var result = Result<int>.Fail("Test error");

        // Act
        var str = result.ToString();

        // Assert
        Assert.Contains("IsSuccess='False'", str);
        Assert.Contains("Test error", str);
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void Integration_CompleteWorkflow_Success()
    {
        // Arrange & Act
        // Note: Map creates a new Result without preserving success reasons from the original
        // Only Bind preserves original success reasons when the new result is successful
        var result = Result<int>.Ok(10)
            .WithSuccess("Created")
            .Bind(x => Result<int>.Ok(x * 2).WithSuccess("Doubled"))
            .Bind(x => Result<string>.Ok($"Value: {x}").WithSuccess("Formatted"));

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Value: 20", result.Value);
        // Bind preserves successes: "Created" from original + "Doubled" from first Bind + "Formatted" from second Bind
        Assert.HasCount(3, result.Successes);
        Assert.IsTrue(result.Successes.Any(s => s.Message == "Created"));
        Assert.IsTrue(result.Successes.Any(s => s.Message == "Doubled"));
        Assert.IsTrue(result.Successes.Any(s => s.Message == "Formatted"));
    }

    [TestMethod]
    public void Integration_CompleteWorkflow_Failure()
    {
        // Arrange & Act
        var result = Result<int>.Ok(10)
            .WithSuccess("Created")
            .Bind(x => Result<int>.Fail("Processing failed"))
            .Map(x => x * 2);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual("Processing failed", result.Errors[0].Message);
    }

    #endregion

    #region Helper Classes

    private class Person
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    private class DueDateError : Error
    {
        public DueDateError(DateTime dueDate)
        {
            base.Message = ($"The due date {dueDate} must be on current date {DateTime.Today.ToShortDateString()} or later.");
            WithTags("ErrorCode", "101");
        }
    }
    #endregion
}
