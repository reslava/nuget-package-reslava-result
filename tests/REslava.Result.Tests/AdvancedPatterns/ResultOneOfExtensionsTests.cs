using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.AdvancedPatterns;

namespace REslava.Result.Tests.AdvancedPatterns;

[TestClass]
public class ResultOneOfExtensionsTests
{
    #region Result → OneOf Tests

    [TestMethod]
    public void Result_ToOneOf_WithSuccess_ShouldCreateOneOfWithT2()
    {
        // Arrange
        var user = new User("Alice");
        Result<User> result = Result<User>.Ok(user);

        // Act
        OneOf<ApiError, User> oneOf = result.ToOneOf(reason => new ApiError(reason.Message));

        // Assert
        Assert.IsTrue(oneOf.IsT2);
        Assert.AreEqual(user, oneOf.AsT2);
    }

    [TestMethod]
    public void Result_ToOneOf_WithFailure_ShouldCreateOneOfWithT1()
    {
        // Arrange
        Result<User> result = Result<User>.Fail("User not found");

        // Act
        OneOf<ApiError, User> oneOf = result.ToOneOf(reason => new ApiError(reason.Message));

        // Assert
        Assert.IsTrue(oneOf.IsT1);
        Assert.AreEqual("User not found", oneOf.AsT1.Message);
    }

    [TestMethod]
    public void Result_ToOneOf_WithNullErrorMapper_ShouldThrowException()
    {
        // Arrange
        Result<User> result = Result<User>.Ok(new User("Alice"));

        // Act & Assert
        try
        {
            result.ToOneOf<ApiError, User>(null!);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    #endregion

    #region OneOf → Result Tests

    [TestMethod]
    public void OneOf_ToResult_WithT2_ShouldCreateSuccessResult()
    {
        // Arrange
        var user = new User("Alice");
        OneOf<ApiError, User> oneOf = OneOf<ApiError, User>.FromT2(user);

        // Act
        Result<User> result = oneOf.ToResult(error => new Error(error.Message));

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(user, result.Value);
    }

    [TestMethod]
    public void OneOf_ToResult_WithT1_ShouldCreateFailureResult()
    {
        // Arrange
        var apiError = new ApiError("User not found", 404);
        OneOf<ApiError, User> oneOf = OneOf<ApiError, User>.FromT1(apiError);

        // Act
        Result<User> result = oneOf.ToResult(error => new Error(error.Message));

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("User not found", result.Errors.First().Message);
    }

    [TestMethod]
    public void OneOf_ToResult_WithNullErrorMapper_ShouldThrowException()
    {
        // Arrange
        var user = new User("Alice");
        OneOf<ApiError, User> oneOf = OneOf<ApiError, User>.FromT2(user);

        // Act & Assert
        try
        {
            oneOf.ToResult<ApiError, User>(null!);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    #endregion

    #region Helper Classes

    public class User
    {
        public string Name { get; }

        public User(string name)
        {
            Name = name;
        }

        public override bool Equals(object? obj)
        {
            return obj is User other && Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    public class ApiError
    {
        public string Message { get; set; }
        public int Code { get; set; }

        public ApiError(string message, int code = 500)
        {
            Message = message;
            Code = code;
        }
    }

    #endregion
}
