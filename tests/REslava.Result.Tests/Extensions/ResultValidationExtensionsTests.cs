using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.Reasons;
using System.Collections.Immutable;

namespace REslava.Result.Tests.Extensions;

[TestClass]
public sealed class ResultValidationExtensionsTests
{
    #region Ensure Tests

    [TestMethod]
    public void Ensure_WithValidCondition_ShouldReturnSuccess()
    {
        // Arrange
        var result = Result<int>.Ok(42);
        
        // Act
        var validatedResult = result.Ensure(x => x > 0, new Error("Value must be positive"));
        
        // Assert
        Assert.IsTrue(validatedResult.IsSuccess);
        Assert.AreEqual(42, validatedResult.Value);
    }

    [TestMethod]
    public void Ensure_WithInvalidCondition_ShouldReturnFailure()
    {
        // Arrange
        var result = Result<int>.Ok(-5);
        
        // Act
        var validatedResult = result.Ensure(x => x > 0, new Error("Value must be positive"));
        
        // Assert
        Assert.IsTrue(validatedResult.IsFailed);
        Assert.HasCount(1, validatedResult.Errors);
        Assert.AreEqual("Value must be positive", validatedResult.Errors[0].Message);
    }

    [TestMethod]
    public void Ensure_WithFailedResult_ShouldPropagateFailure()
    {
        // Arrange
        var result = Result<int>.Fail(new Error("Original error"));
        
        // Act
        var validatedResult = result.Ensure(x => x > 0, new Error("Value must be positive"));
        
        // Assert
        Assert.IsTrue(validatedResult.IsFailed);
        Assert.HasCount(1, validatedResult.Errors);
        Assert.AreEqual("Original error", validatedResult.Errors[0].Message);
    }

    [TestMethod]
    public void Ensure_WithNullCondition_ShouldThrowArgumentNullException()
    {
        // Arrange
        var result = Result<int>.Ok(42);
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result.Ensure<int>(null!, new Error("Message")));
    }

    [TestMethod]
    public void Ensure_WithNullMessage_ShouldThrowArgumentNullException()
    {
        // Arrange
        var result = Result<int>.Ok(42);
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result.Ensure(x => x > 0, (Error)null!));
    }

    #endregion

    #region EnsureAsync Tests

    [TestMethod]
    public async Task EnsureAsync_WithValidCondition_ShouldReturnSuccess()
    {
        // Arrange
        var result = Result<int>.Ok(42);
        
        // Act
        var validatedResult = await result.EnsureAsync(async x =>
        {
            await Task.Delay(10);
            return x > 0;
        }, new Error("Value must be positive"));
        
        // Assert
        Assert.IsTrue(validatedResult.IsSuccess);
        Assert.AreEqual(42, validatedResult.Value);
    }

    [TestMethod]
    public async Task EnsureAsync_WithInvalidCondition_ShouldReturnFailure()
    {
        // Arrange
        var result = Result<int>.Ok(-5);
        
        // Act
        var validatedResult = await result.EnsureAsync(async x =>
        {
            await Task.Delay(10);
            return x > 0;
        }, new Error("Value must be positive"));
        
        // Assert
        Assert.IsTrue(validatedResult.IsFailed);
        Assert.HasCount(1, validatedResult.Errors);
        Assert.AreEqual("Value must be positive", validatedResult.Errors[0].Message);
    }

    [TestMethod]
    public async Task EnsureAsync_WithFailedResult_ShouldPropagateFailure()
    {
        // Arrange
        var result = Result<int>.Fail(new Error("Original error"));
        
        // Act
        var validatedResult = await result.EnsureAsync(async x =>
        {
            await Task.Delay(10);
            return x > 0;
        }, new Error("Value must be positive"));
        
        // Assert
        Assert.IsTrue(validatedResult.IsFailed);
        Assert.HasCount(1, validatedResult.Errors);
        Assert.AreEqual("Original error", validatedResult.Errors[0].Message);
    }

    #endregion

    #region Complex Validation Scenarios

    [TestMethod]
    public void Ensure_WithChainedValidations_ShouldPassAllValidations()
    {
        // Arrange
        var result = Result<int>.Ok(42);
        
        // Act
        var validatedResult = result
            .Ensure(x => x > 0, new Error("Value must be positive"))
            .Ensure(x => x < 100, new Error("Value must be less than 100"))
            .Ensure(x => x % 2 == 0, new Error("Value must be even"));
        
        // Assert
        Assert.IsTrue(validatedResult.IsSuccess);
        Assert.AreEqual(42, validatedResult.Value);
    }

    [TestMethod]
    public void Ensure_WithChainedValidations_ShouldStopAtFirstFailure()
    {
        // Arrange
        var result = Result<int>.Ok(-5);
        
        // Act
        var validatedResult = result
            .Ensure(x => x > 0, new Error("Value must be positive"))
            .Ensure(x => x < 100, new Error("Value must be less than 100"))
            .Ensure(x => x % 2 == 0, new Error("Value must be even"));
        
        // Assert
        Assert.IsTrue(validatedResult.IsFailed);
        Assert.HasCount(1, validatedResult.Errors);
        Assert.AreEqual("Value must be positive", validatedResult.Errors[0].Message);
    }

    [TestMethod]
    public void Ensure_WithComplexObjectValidation_ShouldWorkCorrectly()
    {
        // Arrange
        var user = new User { Id = 1, Name = "John" };
        var result = Result<User>.Ok(user);
        
        // Act
        var validatedResult = result
            .Ensure(u => u.Id > 0, "User ID must be positive")
            .Ensure(u => !string.IsNullOrEmpty(u.Name), "User name is required");
        
        // Assert
        Assert.IsTrue(validatedResult.IsSuccess);
        Assert.AreEqual(1, validatedResult.Value.Id);
        Assert.AreEqual("John", validatedResult.Value.Name);
    }

    [TestMethod]
    public void Ensure_WithComplexObjectValidationFailure_ShouldReturnFailure()
    {
        // Arrange
        var user = new User { Id = -1, Name = "" };
        var result = Result<User>.Ok(user);
        
        // Act
        var validatedResult = result
            .Ensure(u => u.Id > 0, "User ID must be positive")
            .Ensure(u => !string.IsNullOrEmpty(u.Name), "User name is required");
        
        // Assert
        Assert.IsTrue(validatedResult.IsFailed);
        Assert.HasCount(1, validatedResult.Errors);
        Assert.AreEqual("User ID must be positive", validatedResult.Errors[0].Message);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void Ensure_WithValueType_ShouldWorkCorrectly()
    {
        // Arrange
        var result = Result<DateTime>.Ok(new DateTime(2023, 1, 1));
        
        // Act
        var validatedResult = result.Ensure(d => d.Year > 2000, "Date must be after 2000");
        
        // Assert
        Assert.IsTrue(validatedResult.IsSuccess);
        Assert.AreEqual(2023, validatedResult.Value.Year);
    }

    [TestMethod]
    public void Ensure_WithReferenceType_ShouldWorkCorrectly()
    {
        // Arrange
        var result = Result<string>.Ok("test");
        
        // Act
        var validatedResult = result.Ensure(s => s.Length > 0, "String must not be empty");
        
        // Assert
        Assert.IsTrue(validatedResult.IsSuccess);
        Assert.AreEqual("test", validatedResult.Value);
    }

    [TestMethod]
    public void Ensure_WithNullValue_ShouldWorkCorrectly()
    {
        // Arrange
        string? value = null;
        var result = Result<string>.Ok(value);
        
        // Act
        var validatedResult = result.Ensure(s => s != null, "String must not be null");
        
        // Assert
        Assert.IsTrue(validatedResult.IsFailed);
        Assert.HasCount(1, validatedResult.Errors);
        Assert.AreEqual("String must not be null", validatedResult.Errors[0].Message);
    }

    [TestMethod]
    public void Ensure_WithExceptionInCondition_ShouldReturnExceptionError()
    {
        // Arrange
        var result = Result<string>.Ok("test");
        
        // Act
        var validatedResult = result.Ensure(s => throw new InvalidOperationException("Validation error"), "Message");
        
        // Assert
        Assert.IsTrue(validatedResult.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(validatedResult.Errors[0]);
        var exceptionError = (ExceptionError)validatedResult.Errors[0];
        Assert.AreEqual("Validation error", exceptionError.Exception.Message);
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
