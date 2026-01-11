namespace REslava.Result.Tests;

/// <summary>
/// Comprehensive tests for Map
/// </summary>
[TestClass]
public sealed class ResultMatchTests
{
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

    #region Match Methods Generic

    [TestMethod]
    public void Match_OnSuccess_ExecutesOnSuccessWithValue()
    {
        // Arrange
        var result = Result<int>.Ok(25);
        var capturedValue = 0;

        // Act
        var output = result.Match(
            onSuccess: value => { capturedValue = value; return value * 2; },
            onFailure: errors => 0
        );

        // Assert
        Assert.AreEqual(25, capturedValue);
        Assert.AreEqual(50, output);
    }

    [TestMethod]
    public void Match_OnFailure_ExecutesOnFailureWithErrors()
    {
        // Arrange
        var result = Result<int>.Fail("Error");
        var errorCount = 0;

        // Act
        var output = result.Match(
            onSuccess: value => value,
            onFailure: errors => { errorCount = errors.Count; return -1; }
        );

        // Assert
        Assert.AreEqual(1, errorCount);
        Assert.AreEqual(-1, output);
    }

    [TestMethod]
    public void Match_Action_OnSuccess_ExecutesWithValue()
    {
        // Arrange
        var result = Result<string>.Ok("Hello");
        var capturedValue = "";

        // Act
        result.Match(
            onSuccess: value => capturedValue = value,
            onFailure: errors => { }
        );

        // Assert
        Assert.AreEqual("Hello", capturedValue);
    }

    [TestMethod]
    public void Match_Action_OnFailure_ExecutesWithErrors()
    {
        // Arrange
        var result = Result<int>.Fail("Error");
        var errorMessages = new List<string>();

        // Act
        result.Match(
            onSuccess: value => { },
            onFailure: errors => errorMessages.AddRange(errors.Select(e => e.Message))
        );

        // Assert
        Assert.HasCount(1, errorMessages);
        Assert.AreEqual("Error", errorMessages[0]);
    }

    [TestMethod]
    public void Match_WithNullOnSuccess_ThrowsArgumentNullException_Generic()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.Match<int>(null!, errors => 0));
    }

    [TestMethod]
    public void Match_WithNullOnFailure_ThrowsArgumentNullException_Generic()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.Match(value => value, null!));
    }

    #endregion
}
