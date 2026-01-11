namespace REslava.Result.Results.Tests;

/// <summary>
/// Comprehensive tests for the Result class (non-generic)
/// </summary>
[TestClass]
public sealed class ResultTests
{
    #region Factory Methods - Ok

    [TestMethod]
    public void Ok_CreatesSuccessResult()
    {
        // Act
        var result = Result.Ok();

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.IsFailed);
        Assert.IsEmpty(result.Reasons);
        Assert.IsEmpty(result.Errors);
        Assert.IsEmpty(result.Successes);
    }

    #endregion

    #region Factory Methods - Fail

    [TestMethod]
    public void Fail_WithString_CreatesFailedResult()
    {
        // Act
        var result = Result.Fail("Error message");

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual("Error message", result.Errors[0].Message);
    }

    [TestMethod]
    public void Fail_WithError_CreatesFailedResult()
    {
        // Arrange
        var error = new Error("Custom error");

        // Act
        var result = Result.Fail(error);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual("Custom error", result.Errors[0].Message);
    }

    [TestMethod]
    public void Fail_WithMessages_CreatesFailedResultWithMultipleErrors()
    {
        // Arrange
        var messages = new[] { "Error 1", "Error 2", "Error 3" };

        // Act
        var result = Result.Fail(messages);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(3, result.Errors);
        Assert.AreEqual("Error 1", result.Errors[0].Message);
        Assert.AreEqual("Error 2", result.Errors[1].Message);
        Assert.AreEqual("Error 3", result.Errors[2].Message);
    }

    [TestMethod]
    public void Fail_WithErrors_CreatesFailedResultWithMultipleErrors()
    {
        // Arrange
        var errors = new[] { new Error("E1"), new Error("E2") };

        // Act
        var result = Result.Fail(errors);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(2, result.Errors);
    }

    [TestMethod]
    public void Fail_WithNullMessages_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Result.Fail((IEnumerable<string>)null!));
    }

    [TestMethod]
    public void Fail_WithEmptyMessages_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Result.Fail(Array.Empty<string>()));
    }

    [TestMethod]
    public void Fail_WithNullErrors_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Result.Fail((IEnumerable<Error>)null!));
    }

    [TestMethod]
    public void Fail_WithEmptyErrors_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Result.Fail(Array.Empty<Error>()));
    }

    #endregion

    #region Fluent Methods - With

    [TestMethod]
    public void WithSuccess_String_AddsSuccessReason()
    {
        // Arrange
        var result = Result.Ok();

        // Act
        result.WithSuccess("Operation completed");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(1, result.Successes);
        Assert.AreEqual("Operation completed", result.Successes[0].Message);
    }

    [TestMethod]
    public void WithSuccess_Object_AddsSuccessReason()
    {
        // Arrange
        var result = Result.Ok();
        var success = new Success("Custom success");

        // Act
        result.WithSuccess(success);

        // Assert
        Assert.HasCount(1, result.Successes);
        Assert.AreEqual("Custom success", result.Successes[0].Message);
    }

    [TestMethod]
    public void WithError_String_AddsErrorReason()
    {
        // Arrange
        var result = Result.Ok();

        // Act
        result.WithError("Validation failed");

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual("Validation failed", result.Errors[0].Message);
    }

    [TestMethod]
    public void WithError_Object_AddsErrorReason()
    {
        // Arrange
        var result = Result.Ok();
        var error = new Error("Custom error");

        // Act
        result.WithError(error);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
    }

    [TestMethod]
    public void WithSuccesses_AddsMultipleSuccessReasons()
    {
        // Arrange
        var result = Result.Ok();
        var successes = new[] { new Success("S1"), new Success("S2") };

        // Act
        result.WithSuccesses(successes);

        // Assert
        Assert.HasCount(2, result.Successes);
    }

    [TestMethod]
    public void WithErrors_AddsMultipleErrorReasons()
    {
        // Arrange
        var result = Result.Ok();
        var errors = new[] { new Error("E1"), new Error("E2") };

        // Act
        result.WithErrors(errors);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(2, result.Errors);
    }

    [TestMethod]
    public void FluentChaining_Works()
    {
        // Act
        var result = Result.Ok()
            .WithSuccess("Step 1 done")
            .WithSuccess("Step 2 done")
            .WithError("Warning: Something noticed");

        // Assert
        Assert.IsTrue(result.IsFailed); // Has error
        Assert.HasCount(2, result.Successes);
        Assert.HasCount(1, result.Errors);
    }

    #endregion

    #region Match Methods

    [TestMethod]
    public void Match_OnSuccess_ExecutesOnSuccessFunction()
    {
        // Arrange
        var result = Result.Ok();
        var executed = false;

        // Act
        var output = result.Match(
            onSuccess: () => { executed = true; return "success"; },
            onFailure: errors => "failure"
        );

        // Assert
        Assert.IsTrue(executed);
        Assert.AreEqual("success", output);
    }

    [TestMethod]
    public void Match_OnFailure_ExecutesOnFailureFunction()
    {
        // Arrange
        var result = Result.Fail("Error");
        var executed = false;

        // Act
        var output = result.Match(
            onSuccess: () => "success",
            onFailure: errors => { executed = true; return $"failure: {errors.Count}"; }
        );

        // Assert
        Assert.IsTrue(executed);
        Assert.AreEqual("failure: 1", output);
    }

    [TestMethod]
    public void Match_Action_OnSuccess_ExecutesOnSuccessAction()
    {
        // Arrange
        var result = Result.Ok();
        var executed = false;

        // Act
        result.Match(
            onSuccess: () => executed = true,
            onFailure: errors => { }
        );

        // Assert
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public void Match_Action_OnFailure_ExecutesOnFailureAction()
    {
        // Arrange
        var result = Result.Fail("Error");
        var errorCount = 0;

        // Act
        result.Match(
            onSuccess: () => { },
            onFailure: errors => errorCount = errors.Count
        );

        // Assert
        Assert.AreEqual(1, errorCount);
    }

    [TestMethod]
    public void Match_WithNullOnSuccess_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result.Ok();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.Match(null!, errors => "failure"));
    }

    [TestMethod]
    public void Match_WithNullOnFailure_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result.Ok();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.Match(() => "success", null!));
    }

    #endregion

    #region ToString

    [TestMethod]
    public void ToString_OnSuccess_ShowsIsSuccess()
    {
        // Arrange
        var result = Result.Ok();

        // Act
        var str = result.ToString();

        // Assert
        Assert.Contains("IsSuccess='True'", str);
    }

    [TestMethod]
    public void ToString_OnFailure_ShowsReason()
    {
        // Arrange
        var result = Result.Fail("Test error");

        // Act
        var str = result.ToString();

        // Assert
        Assert.Contains("IsSuccess='False'", str);
        Assert.Contains("Test error", str);
    }

    #endregion
}
