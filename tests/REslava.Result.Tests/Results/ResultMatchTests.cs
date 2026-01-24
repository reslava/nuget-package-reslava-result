using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.Reasons;
using System.Collections.Immutable;

namespace REslava.Result.Tests.Results;

[TestClass]
public sealed class ResultMatchTests
{
    #region Match Tests

    [TestMethod]
    public void Match_WithSuccessResult_ShouldExecuteOnSuccess()
    {
        // Arrange
        var result = new Result<int>(42, new Success("Success"));
        var successValue = string.Empty;
        var failureValue = string.Empty;
        
        // Act
        var output = result.Match(
            onSuccess: () => { successValue = "Success"; return successValue; },
            onFailure: errors => { failureValue = $"Failed: {errors.Count}"; return failureValue; }
        );
        
        // Assert
        Assert.AreEqual("Success", output);
        Assert.AreEqual("Success", successValue);
        Assert.AreEqual(string.Empty, failureValue);
    }

    [TestMethod]
    public void Match_WithFailedResult_ShouldExecuteOnFailure()
    {
        // Arrange
        var result = new Result<int>(default, ImmutableList.Create<IReason>(new Error("Test error")));
        var successValue = string.Empty;
        var failureValue = string.Empty;
        
        // Act
        var output = result.Match(
            onSuccess: () => { successValue = "Success"; return successValue; },
            onFailure: errors => { failureValue = $"Failed: {errors.Count}"; return failureValue; }
        );
        
        // Assert
        Assert.AreEqual("Failed: 1", output);
        Assert.AreEqual(string.Empty, successValue);
        Assert.AreEqual("Failed: 1", failureValue);
    }

    [TestMethod]
    public void Match_WithSuccessResultAndValue_ShouldExecuteOnSuccessWithValue()
    {
        // Arrange
        var result = new Result<int>(42, new Success("Success"));
        
        // Act
        var output = result.Match(
            onSuccess: value => $"Value: {value}",
            onFailure: errors => $"Error: {errors[0].Message}"
        );
        
        // Assert
        Assert.AreEqual("Value: 42", output);
    }

    [TestMethod]
    public void Match_WithFailedResultAndErrors_ShouldExecuteOnFailureWithErrors()
    {
        // Arrange
        var result = new Result<int>(default, ImmutableList.Create<IReason>(new Error("Test error")));
        
        // Act
        var output = result.Match(
            onSuccess: value => $"Value: {value}",
            onFailure: errors => $"Error: {errors[0].Message}"
        );
        
        // Assert
        Assert.AreEqual("Error: Test error", output);
    }

    [TestMethod]
    public void Match_WithNullOnSuccess_ShouldThrowArgumentNullException()
    {
        // Arrange
        var result = new Result<int>(42, new Success("Success"));
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result.Match(
            onSuccess: null!,
            onFailure: errors => "Error"
        ));
    }

    [TestMethod]
    public void Match_WithNullOnFailure_ShouldThrowArgumentNullException()
    {
        // Arrange
        var result = new Result<int>(default, ImmutableList.Create<IReason>(new Error("Test error")));
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result.Match(
            onSuccess: value => "Success",
            onFailure: null!
        ));
    }

    #endregion

    #region Complex Match Scenarios

    [TestMethod]
    public void Match_WithComplexSuccessLogic_ShouldWorkCorrectly()
    {
        // Arrange
        var result = new Result<string>("test", new Success("String created"));
        
        // Act
        var output = result.Match(
            onSuccess: str => str.ToUpper(),
            onFailure: errors => $"Error: {string.Join(", ", errors.Select(e => e.Message))}"
        );
        
        // Assert
        Assert.AreEqual("TEST", output);
    }

    [TestMethod]
    public void Match_WithComplexFailureLogic_ShouldWorkCorrectly()
    {
        // Arrange
        var errors = ImmutableList.Create<IReason>(new Error("Error 1"), new Error("Error 2"));
        var result = new Result<int>(errors);
        
        // Act
        var output = result.Match(
            onSuccess: value => $"Success: {value}",
            onFailure: errorList => $"Errors: {string.Join(", ", errorList.Select(e => e.Message))}"
        );
        
        // Assert
        Assert.AreEqual("Errors: Error 1, Error 2", output);
    }

    [TestMethod]
    public void Match_WithObjectResult_ShouldWorkCorrectly()
    {
        // Arrange
        var user = new User { Id = 1, Name = "John" };
        var result = new Result<User>(user, new Success("User found"));
        
        // Act
        var output = result.Match(
            onSuccess: u => $"User: {u.Name} (ID: {u.Id})",
            onFailure: errors => $"Error: {errors[0].Message}"
        );
        
        // Assert
        Assert.AreEqual("User: John (ID: 1)", output);
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
