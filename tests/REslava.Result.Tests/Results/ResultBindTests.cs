using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.Reasons;
using System.Collections.Immutable;

namespace REslava.Result.Tests.Results;

[TestClass]
public sealed class ResultBindTests
{
    #region Bind Tests

    [TestMethod]
    public void Bind_WithSuccessResult_ShouldApplyBinder()
    {
        // Arrange
        var result = new Result<int>(42, new Success("Initial"));
        
        // Act
        var boundResult = result.Bind(x => new Result<string>(x.ToString(), new Success("Bound")));
        
        // Assert
        Assert.IsTrue(boundResult.IsSuccess);
        Assert.AreEqual("42", boundResult.Value);
        Assert.HasCount(2, boundResult.Successes);
    }

    [TestMethod]
    public void Bind_WithFailedResult_ShouldPropagateFailure()
    {
        // Arrange
        var result = new Result<int>(ImmutableList.Create<IReason>(new Error("Original error")));
        
        // Act
        var boundResult = result.Bind(x => new Result<string>("never called", new Success("Never")));
        
        // Assert
        Assert.IsTrue(boundResult.IsFailed);
        Assert.HasCount(1, boundResult.Errors);
        Assert.AreEqual("Original error", boundResult.Errors[0].Message);
    }

    [TestMethod]
    public void Bind_WithBinderReturningFailure_ShouldPropagateFailure()
    {
        // Arrange
        var result = new Result<int>(42, new Success("Initial"));
        var binderError = new Error("Binder error");
        
        // Act
        var boundResult = result.Bind(x => new Result<string>("value", ImmutableList.Create<IReason>(binderError)));
        
        // Assert
        Assert.IsTrue(boundResult.IsFailed);
        Assert.HasCount(1, boundResult.Errors);
        Assert.AreEqual("Binder error", boundResult.Errors[0].Message);
    }

    [TestMethod]
    public void Bind_WithBinderThrowing_ShouldReturnExceptionError()
    {
        // Arrange
        var result = new Result<int>(42, new Success("Initial"));
        
        // Act
        var boundResult = result.Bind(x => throw new InvalidOperationException("Binder error"));
        
        // Assert
        Assert.IsTrue(boundResult.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(boundResult.Errors[0]);
        var exceptionError = (ExceptionError)boundResult.Errors[0];
        Assert.AreEqual("Binder error", exceptionError.Exception.Message);
    }

    [TestMethod]
    public void Bind_WithNullBinder_ShouldThrowArgumentNullException()
    {
        // Arrange
        var result = new Result<int>(42, new Success("Initial"));
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result.Bind<int, string>(null!));
    }

    #endregion

    #region Complex Bind Scenarios

    [TestMethod]
    public void Bind_WithChainedBinds_ShouldMaintainSuccessChain()
    {
        // Arrange
        var initial = new Result<int>(42, new Success("Initial"));
        
        // Act
        var result = initial
            .Bind(x => new Result<string>(x.ToString(), new Success("ToString")))
            .Bind(s => new Result<int>(int.Parse(s), new Success("Parse")));
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
        Assert.HasCount(3, result.Successes);
    }

    [TestMethod]
    public void Bind_WithFailureInMiddle_ShouldStopAtFirstFailure()
    {
        // Arrange
        var initial = new Result<int>(42, new Success("Initial"));
        var middleError = new Error("Middle failure");
        
        // Act
        var result = initial
            .Bind(x => new Result<string>(x.ToString(), new Success("ToString")))
            .Bind(s => new Result<int>(ImmutableList.Create<IReason>(middleError)))
            .Bind(i => new Result<double>(i * 2.0, new Success("Never")));
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual("Middle failure", result.Errors[0].Message);
    }

    [TestMethod]
    public void Bind_WithComplexObjectMapping_ShouldWorkCorrectly()
    {
        // Arrange
        var user = new User { Id = 1, Name = "John" };
        var result = new Result<User>(user, new Success("User found"));
        
        // Act
        var boundResult = result.Bind(u => new Result<UserDto>(new UserDto { Id = u.Id, Name = u.Name }, new Success("Mapped")));
        
        // Assert
        Assert.IsTrue(boundResult.IsSuccess);
        Assert.AreEqual(1, boundResult.Value.Id);
        Assert.AreEqual("John", boundResult.Value.Name);
        Assert.HasCount(2, boundResult.Successes);
    }

    #endregion

    #region Helper Classes

    private class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}
