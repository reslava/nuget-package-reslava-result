using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.Reasons;
using System.Collections.Immutable;

namespace REslava.Result.Tests.Results;

[TestClass]
public sealed class ResultTapTests
{
    #region Tap Tests

    [TestMethod]
    public void Tap_WithSuccessResult_ShouldExecuteAction()
    {
        // Arrange
        var result = new Result<int>(42, new Success("Initial"));
        var tappedValue = 0;
        
        // Act
        var tappedResult = result.Tap(x => tappedValue = x);
        
        // Assert
        Assert.IsTrue(tappedResult.IsSuccess);
        Assert.AreEqual(42, tappedResult.Value);
        Assert.AreEqual(42, tappedValue);
        Assert.HasCount(1, tappedResult.Successes);
    }

    [TestMethod]
    public void Tap_WithFailedResult_ShouldNotExecuteAction()
    {
        // Arrange
        var result = new Result<int>(ImmutableList.Create<IReason>(new Error("Test error")));
        var tappedValue = 0;
        
        // Act
        var tappedResult = result.Tap(x => tappedValue = x);
        
        // Assert
        Assert.IsTrue(tappedResult.IsFailed);
        Assert.AreEqual(0, tappedValue);
        Assert.HasCount(1, tappedResult.Errors);
    }

    [TestMethod]
    public void Tap_WithActionThrowing_ShouldReturnExceptionError()
    {
        // Arrange
        var result = new Result<int>(42, new Success("Initial"));
        
        // Act
        var tappedResult = result.Tap(x => throw new InvalidOperationException("Tap error"));
        
        // Assert
        Assert.IsTrue(tappedResult.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(tappedResult.Errors[0]);
        var exceptionError = (ExceptionError)tappedResult.Errors[0];
        Assert.AreEqual("Tap error", exceptionError.Exception.Message);
    }

    [TestMethod]
    public void Tap_WithNullAction_ShouldThrowArgumentNullException()
    {
        // Arrange
        var result = new Result<int>(42, new Success("Initial"));
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result.Tap<int>(null!));
    }

    #endregion

    #region Complex Tap Scenarios

    [TestMethod]
    public void Tap_WithChainedTaps_ShouldExecuteAllActions()
    {
        // Arrange
        var result = new Result<int>(42, new Success("Initial"));
        var tappedValue1 = 0;
        var tappedValue2 = 0;
        var tappedValue3 = 0;
        
        // Act
        var tappedResult = result
            .Tap(x => tappedValue1 = x)
            .Tap(x => tappedValue2 = x * 2)
            .Tap(x => tappedValue3 = x * 3);
        
        // Assert
        Assert.IsTrue(tappedResult.IsSuccess);
        Assert.AreEqual(42, tappedResult.Value);
        Assert.AreEqual(42, tappedValue1);
        Assert.AreEqual(84, tappedValue2);
        Assert.AreEqual(126, tappedValue3);
    }

    [TestMethod]
    public void Tap_WithFailureInMiddle_ShouldStopAtFirstFailure()
    {
        // Arrange
        var result = new Result<int>(42, new Success("Initial"));
        var tappedValue = 0;
        
        // Act
        var tappedResult = result
            .Tap(x => tappedValue = x)
            .Tap(x => throw new InvalidOperationException("Tap error"))
            .Tap(x => tappedValue = x * 2);
        
        // Assert
        Assert.IsTrue(tappedResult.IsFailed);
        Assert.AreEqual(42, tappedValue);
        Assert.IsInstanceOfType<ExceptionError>(tappedResult.Errors[0]);
    }

    [TestMethod]
    public void Tap_WithComplexObject_ShouldWorkCorrectly()
    {
        // Arrange
        var user = new User { Id = 1, Name = "John" };
        var result = new Result<User>(user, new Success("User found"));
        var loggedName = string.Empty;
        
        // Act
        var tappedResult = result.Tap(u => loggedName = u.Name);
        
        // Assert
        Assert.IsTrue(tappedResult.IsSuccess);
        Assert.AreEqual("John", tappedResult.Value.Name);
        Assert.AreEqual("John", loggedName);
        Assert.HasCount(1, tappedResult.Successes);
    }

    [TestMethod]
    public void Tap_WithSideEffect_ShouldMaintainImmutability()
    {
        // Arrange
        var result = new Result<int>(42, new Success("Initial"));
        
        // Act
        var tappedResult = result.Tap(x => { /* side effect */ });
        
        // Assert
        Assert.IsTrue(tappedResult.IsSuccess);
        Assert.AreEqual(42, tappedResult.Value);
        Assert.AreSame(result, tappedResult);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void Tap_WithValueType_ShouldWorkCorrectly()
    {
        // Arrange
        var result = new Result<DateTime>(new DateTime(2023, 1, 1), new Success("Date created"));
        var loggedYear = 0;
        
        // Act
        var tappedResult = result.Tap(d => loggedYear = d.Year);
        
        // Assert
        Assert.IsTrue(tappedResult.IsSuccess);
        Assert.AreEqual(2023, loggedYear);
    }

    [TestMethod]
    public void Tap_WithNullableType_ShouldWorkCorrectly()
    {
        // Arrange
        string? value = "test";
        var result = new Result<string>(value, new Success("String created"));
        var loggedLength = 0;
        
        // Act
        var tappedResult = result.Tap(s => loggedLength = s?.Length ?? 0);
        
        // Assert
        Assert.IsTrue(tappedResult.IsSuccess);
        Assert.AreEqual(4, loggedLength);
    }

    [TestMethod]
    public void Tap_WithNullValue_ShouldWorkCorrectly()
    {
        // Arrange
        string? value = null;
        var result = new Result<string>(value, new Success("Null string"));
        var loggedLength = 0;
        
        // Act
        var tappedResult = result.Tap(s => loggedLength = s?.Length ?? 0);
        
        // Assert
        Assert.IsTrue(tappedResult.IsSuccess);
        Assert.AreEqual(0, loggedLength);
    }

    [TestMethod]
    public void Tap_WithLargeNumberOfOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var result = new Result<int>(1, new Success("Start"));
        var sum = 0;
        
        // Act
        var tappedResult = result
            .Tap(x => sum += x)
            .Tap(x => sum += x)
            .Tap(x => sum += x)
            .Tap(x => sum += x)
            .Tap(x => sum += x);
        
        // Assert
        Assert.IsTrue(tappedResult.IsSuccess);
        Assert.AreEqual(6, sum);
    }

    #endregion

    #region Helper Classes

    private class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}
