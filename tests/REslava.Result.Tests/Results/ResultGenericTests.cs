using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.Reasons;
using System.Collections.Immutable;

namespace REslava.Result.Tests.Results;

[TestClass]
public sealed class ResultGenericTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_WithValueAndReason_ShouldCreateSuccessResult()
    {
        // Arrange
        var value = 42;
        var reason = new Success("Operation completed");
        
        // Act
        var result = new Result<int>(value, reason);
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.IsFailed);
        Assert.AreEqual(value, result.Value);
        Assert.IsEmpty(result.Errors);
        Assert.HasCount(1, result.Successes);
    }

    [TestMethod]
    public void Constructor_WithValueAndReasons_ShouldCreateSuccessResultWithSuccesses()
    {
        // Arrange
        var value = 42;
        var reasons = ImmutableList.Create<IReason>(new Success("Operation completed"));
        
        // Act
        var result = new Result<int>(value, reasons);
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.IsFailed);
        Assert.AreEqual(value, result.Value);
        Assert.IsEmpty(result.Errors);
        Assert.HasCount(1, result.Successes);
    }

    [TestMethod]
    public void Constructor_WithReasons_ShouldCreateFailedResult()
    {
        // Arrange
        var errors = ImmutableList.Create<IReason>(new Error("Something went wrong"));
        
        // Act
        var result = new Result<int>(errors);
        
        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual("Something went wrong", result.Errors[0].Message);
        Assert.IsEmpty(result.Successes);
    }

    #endregion

    #region Value Property Tests

    [TestMethod]
    public void Value_WithSuccessResult_ShouldReturnValue()
    {
        // Arrange
        var value = "test";
        var result = new Result<string>(value, new Success("Success"));
        
        // Act & Assert
        Assert.AreEqual(value, result.Value);
    }

    [TestMethod]
    public void Value_WithFailedResult_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var result = new Result<int>(default, ImmutableList.Create<IReason>(new Error("Test error")));
        
        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.IsTrue(ex.Message.Contains("Cannot access Value on a failed Result"));
        Assert.IsTrue(ex.Message.Contains("Test error"));
    }

    [TestMethod]
    public void Value_WithFailedResultAndMultipleErrors_ShouldIncludeAllErrorsInException()
    {
        // Arrange
        var errors = ImmutableList.Create<IReason>(new Error("Error 1"), new Error("Error 2"));
        var result = new Result<int>(errors);
        
        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.IsTrue(ex.Message.Contains("Error 1"));
        Assert.IsTrue(ex.Message.Contains("Error 2"));
    }

    #endregion

    #region GetValueOr Tests

    [TestMethod]
    public void GetValueOr_WithSuccessResult_ShouldReturnValue()
    {
        // Arrange
        var value = 42;
        var result = new Result<int>(value, new Success("Success"));
        var defaultValue = 99;
        
        // Act
        var actualValue = result.GetValueOr(defaultValue);
        
        // Assert
        Assert.AreEqual(value, actualValue);
    }

    [TestMethod]
    public void GetValueOr_WithFailedResult_ShouldReturnDefaultValue()
    {
        // Arrange
        var result = new Result<int>(default, ImmutableList.Create<IReason>(new Error("Test error")));
        var defaultValue = 99;
        
        // Act
        var actualValue = result.GetValueOr(defaultValue);
        
        // Assert
        Assert.AreEqual(defaultValue, actualValue);
    }

    [TestMethod]
    public void GetValueOr_WithFailedResultAndNullDefaultValue_ShouldReturnNull()
    {
        // Arrange
        var result = new Result<string>(ImmutableList.Create<IReason>(new Error("Test error")));
        string? defaultValue = null;
        
        // Act
        var actualValue = result.GetValueOr(defaultValue!);
        
        // Assert
        Assert.IsNull(actualValue);
    }

    [TestMethod]
    public void GetValueOr_WithFactory_ShouldWorkCorrectly()
    {
        // Arrange
        var result = new Result<int>(default, ImmutableList.Create<IReason>(new Error("Test error")));
        
        // Act
        var actualValue = result.GetValueOr(() => 42);
        
        // Assert
        Assert.AreEqual(42, actualValue);
    }

    [TestMethod]
    public void GetValueOr_WithErrorHandler_ShouldWorkCorrectly()
    {
        // Arrange
        var errors = ImmutableList.Create<IReason>(new Error("Test error"));
        var result = new Result<int>(errors);
        
        // Act
        var actualValue = result.GetValueOr(errorList => errorList.Count);
        
        // Assert
        Assert.AreEqual(1, actualValue);
    }

    #endregion

    #region TryGetValue Tests

    [TestMethod]
    public void TryGetValue_WithSuccessResult_ShouldReturnTrue()
    {
        // Arrange
        var value = 42;
        var result = new Result<int>(value, new Success("Success"));
        
        // Act
        var success = result.TryGetValue(out var actualValue);
        
        // Assert
        Assert.IsTrue(success);
        Assert.AreEqual(value, actualValue);
    }

    [TestMethod]
    public void TryGetValue_WithFailedResult_ShouldReturnFalse()
    {
        // Arrange
        var result = new Result<int>(default, ImmutableList.Create<IReason>(new Error("Test error")));
        
        // Act
        var success = result.TryGetValue(out var actualValue);
        
        // Assert
        Assert.IsFalse(success);
        Assert.AreEqual(0, actualValue); // Default value
    }

    #endregion

    #region Implicit Conversion Tests

    [TestMethod]
    public void ImplicitConversion_FromValue_ShouldCreateSuccessResult()
    {
        // Arrange
        int value = 42;
        
        // Act
        Result<int> result = value;
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(value, result.Value);
    }

    [TestMethod]
    public void ImplicitConversion_FromError_ShouldCreateFailedResult()
    {
        // Arrange
        var error = new Error("Test error");
        
        // Act
        Result<int> result = error;
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreSame(error, result.Errors[0]);
    }

    #endregion

    #region WithReason Tests

    [TestMethod]
    public void WithReason_WithSuccessResult_ShouldAddReason()
    {
        // Arrange
        var result = new Result<int>(42, new Success("Initial"));
        
        // Act
        var newResult = result.WithReason(new Success("Added"));
        
        // Assert
        Assert.IsTrue(newResult.IsSuccess);
        Assert.AreEqual(42, newResult.Value);
        Assert.HasCount(2, newResult.Successes);
    }

    [TestMethod]
    public void WithReason_WithFailedResult_ShouldAddReason()
    {
        // Arrange
        var result = new Result<int>(ImmutableList.Create<IReason>(new Error("Initial")));
        
        // Act
        var newResult = result.WithReason(new Error("Added"));
        
        // Assert
        Assert.IsTrue(newResult.IsFailed);
        Assert.HasCount(2, newResult.Errors);
    }

    #endregion

    #region WithSuccess Tests

    [TestMethod]
    public void WithSuccess_WithSuccessResult_ShouldAddSuccess()
    {
        // Arrange
        var result = new Result<int>(42, new Success("Initial"));
        
        // Act
        var newResult = result.WithSuccess("Added success");
        
        // Assert
        Assert.IsTrue(newResult.IsSuccess);
        Assert.AreEqual(42, newResult.Value);
        Assert.HasCount(2, newResult.Successes);
    }

    [TestMethod]
    public void WithSuccess_WithFailedResult_ShouldMakeSuccess()
    {
        // Arrange
        var result = new Result<int>(ImmutableList.Create<IReason>(new Error("Error")));
        
        // Act
        var newResult = result.WithSuccess("Success");
        
        // Assert
        Assert.IsTrue(newResult.IsSuccess);
        Assert.AreEqual(42, newResult.Value);
        Assert.HasCount(2, newResult.Reasons);
    }

    #endregion

    #region WithError Tests

    [TestMethod]
    public void WithError_WithSuccessResult_ShouldMakeFailed()
    {
        // Arrange
        var result = new Result<int>(42, new Success("Success"));
        
        // Act
        var newResult = result.WithError("New error");
        
        // Assert
        Assert.IsTrue(newResult.IsFailed);
        Assert.HasCount(2, newResult.Reasons);
    }

    [TestMethod]
    public void WithError_WithFailedResult_ShouldAddError()
    {
        // Arrange
        var result = new Result<int>(ImmutableList.Create<IReason>(new Error("Initial")));
        
        // Act
        var newResult = result.WithError("Added error");
        
        // Assert
        Assert.IsTrue(newResult.IsFailed);
        Assert.HasCount(2, newResult.Errors);
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void ToString_WithSuccessResult_ShouldReturnSuccessString()
    {
        // Arrange
        var result = new Result<int>(42, new Success("Success"));
        
        // Act
        var resultString = result.ToString();
        
        // Assert
        Assert.IsTrue(resultString.Contains("Success"));
        Assert.IsTrue(resultString.Contains("42"));
    }

    [TestMethod]
    public void ToString_WithFailedResult_ShouldReturnFailureString()
    {
        // Arrange
        var result = new Result<int>(default, ImmutableList.Create<IReason>(new Error("Test error")));
        
        // Act
        var resultString = result.ToString();
        
        // Assert
        Assert.IsTrue(resultString.Contains("Failed"));
        Assert.IsTrue(resultString.Contains("Test error"));
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void Result_WithReferenceType_ShouldWorkCorrectly()
    {
        // Arrange
        var value = new List<string> { "item1", "item2" };
        
        // Act
        var result = new Result<List<string>>(value, new Success("Success"));
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreSame(value, result.Value);
        Assert.HasCount(2, result.Value);
    }

    [TestMethod]
    public void Result_WithValueType_ShouldWorkCorrectly()
    {
        // Arrange
        var value = new DateTime(2023, 1, 1);
        
        // Act
        var result = new Result<DateTime>(value, new Success("Success"));
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(value, result.Value);
    }

    [TestMethod]
    public void Result_WithNullValue_ShouldWorkCorrectly()
    {
        // Arrange
        string? value = null;
        
        // Act
        var result = new Result<string>(value, new Success("Success"));
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.Value);
    }

    #endregion

    #region Complex Scenarios

    [TestMethod]
    public void Result_WithChainedOperations_ShouldMaintainImmutability()
    {
        // Arrange
        var original = new Result<int>(42, new Success("Initial"));
        
        // Act
        var modified = original.WithSuccess("Modified");
        
        // Assert
        Assert.AreEqual(42, original.Value);
        Assert.AreEqual(42, modified.Value);
        Assert.HasCount(1, original.Successes);
        Assert.HasCount(2, modified.Successes);
    }

    [TestMethod]
    public void Result_WithComplexObject_ShouldWorkCorrectly()
    {
        // Arrange
        var user = new User { Id = 1, Name = "John" };
        
        // Act
        var result = new Result<User>(user, new Success("User created"));
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Value.Id);
        Assert.AreEqual("John", result.Value.Name);
        Assert.HasCount(1, result.Successes);
        Assert.AreEqual("User created", result.Successes[0].Message);
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
