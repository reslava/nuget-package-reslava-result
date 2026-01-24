using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.Reasons;
using System.Collections.Immutable;

namespace REslava.Result.Tests.Results;

[TestClass]
public sealed class ResultMapTests
{
    #region Map Tests

    [TestMethod]
    public void Map_WithSuccessResult_ShouldApplyMapper()
    {
        // Arrange
        var result = new Result<int>(42, new Success("Initial"));
        
        // Act
        var mappedResult = result.Map(x => x.ToString());
        
        // Assert
        Assert.IsTrue(mappedResult.IsSuccess);
        Assert.AreEqual("42", mappedResult.Value);
        Assert.HasCount(1, mappedResult.Successes);
    }

    [TestMethod]
    public void Map_WithFailedResult_ShouldPropagateFailure()
    {
        // Arrange
        var result = new Result<int>(default, ImmutableList.Create<IReason>(new Error("Test error")));
        
        // Act
        var mappedResult = result.Map(x => x.ToString());
        
        // Assert
        Assert.IsTrue(mappedResult.IsFailed);
        Assert.HasCount(1, mappedResult.Errors);
        Assert.AreEqual("Original error", mappedResult.Errors[0].Message);
    }

    [TestMethod]
    public void Map_WithMapperThrowing_ShouldReturnExceptionError()
    {
        // Arrange
        var result = new Result<int>(42, new Success("Initial"));
        
        // Act
        var mappedResult = result.Map<int>(x => throw new InvalidOperationException("Mapper error"));
        
        // Assert
        Assert.IsTrue(mappedResult.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(mappedResult.Errors[0]);
        var exceptionError = (ExceptionError)mappedResult.Errors[0];
        Assert.AreEqual("Mapper error", exceptionError.Exception.Message);
    }

    [TestMethod]
    public void Map_WithNullMapper_ShouldThrowArgumentNullException()
    {
        // Arrange
        var result = new Result<int>(42, new Success("Initial"));
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result.Map<int>(null!));
    }

    #endregion

    #region Complex Map Scenarios

    [TestMethod]
    public void Map_WithChainedMaps_ShouldMaintainSuccessChain()
    {
        // Arrange
        var initial = new Result<int>(42, new Success("Initial"));
        
        // Act
        var result = initial
            .Map(x => x * 2)
            .Map(x => x.ToString())
            .Map(s => s.Length);
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, result.Value);
        Assert.HasCount(1, result.Successes);
    }

    [TestMethod]
    public void Map_WithFailureInMiddle_ShouldStopAtFirstFailure()
    {
        // Arrange
        var initial = new Result<int>(42, new Success("Initial"));
        var middleError = new Error("Middle failure");
        
        // Act
        var result = initial
            .Map(x => x * 2)
            .Map<int>(x => throw new InvalidOperationException("Map error"))
            .Map(x => x.ToString());
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
    }

    [TestMethod]
    public void Map_WithComplexObjectMapping_ShouldWorkCorrectly()
    {
        // Arrange
        var user = new User { Id = 1, Name = "John" };
        var result = new Result<User>(user, new Success("User found"));
        
        // Act
        var mappedResult = result.Map(u => new UserDto { Id = u.Id, Name = u.Name });
        
        // Assert
        Assert.IsTrue(mappedResult.IsSuccess);
        Assert.AreEqual(1, mappedResult.Value?.Id);
        Assert.AreEqual("John", mappedResult.Value?.Name);
        Assert.HasCount(1, mappedResult.Successes);
    }

    [TestMethod]
    public void Map_WithValueTypeMapping_ShouldWorkCorrectly()
    {
        // Arrange
        var result = new Result<DateTime>(new DateTime(2023, 1, 1), new Success("Date created"));
        
        // Act
        var mappedResult = result.Map(d => d.Year);
        
        // Assert
        Assert.IsTrue(mappedResult.IsSuccess);
        Assert.AreEqual(2023, mappedResult.Value);
    }

    [TestMethod]
    public void Map_WithNullableMapping_ShouldWorkCorrectly()
    {
        // Arrange
        var result = new Result<string>("test", new Success("String created"));
        
        // Act
        var mappedResult = result.Map(s => s?.Length ?? 0);
        
        // Assert
        Assert.IsTrue(mappedResult.IsSuccess);
        Assert.AreEqual(4, mappedResult.Value);
    }

    [TestMethod]
    public void Map_WithNullValue_ShouldWorkCorrectly()
    {
        // Arrange
        string? value = null;
        var result = new Result<string>(value, new Success("Null string"));
        
        // Act
        var mappedResult = result.Map(s => s?.Length ?? 0);
        
        // Assert
        Assert.IsTrue(mappedResult.IsSuccess);
        Assert.AreEqual(0, mappedResult.Value);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void Map_WithReferenceType_ShouldMaintainReference()
    {
        // Arrange
        var list = new List<string> { "item1", "item2" };
        var result = new Result<List<string>>(list, new Success("List created"));
        
        // Act
        var mappedResult = result.Map(l => l.Count);
        
        // Assert
        Assert.IsTrue(mappedResult.IsSuccess);
        Assert.AreEqual(2, mappedResult.Value);
        Assert.AreSame(list, result.Value);
    }

    [TestMethod]
    public void Map_WithLargeNumberOfOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var result = new Result<int>(1, new Success("Start"));
        
        // Act
        var mappedResult = result
            .Map(x => x * 2)
            .Map(x => x * 2)
            .Map(x => x * 2)
            .Map(x => x * 2)
            .Map(x => x * 2);
        
        // Assert
        Assert.IsTrue(mappedResult.IsSuccess);
        Assert.AreEqual(32, mappedResult.Value);
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
