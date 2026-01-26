using System;
using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.AdvancedPatterns;

namespace REslava.Result.Tests.AdvancedPatterns;

[TestClass]
public class OneOfResultIntegrationExtensionsTests
{
    #region SelectToResult Tests - Primary Overload

    [TestMethod]
    public void SelectToResult_Primary_WithSuccess_ShouldCreateResultWithMappedValue()
    {
        // Arrange
        var user = new User("Alice");
        OneOf<string, User> oneOf = OneOf<string, User>.FromT2(user);

        // Act
        Result<UserDto> result = oneOf.SelectToResult(
            user => new UserDto(user.Name),
            error => new Error($"User error: {error}")
        );

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Alice", result.Value.Name);
    }

    [TestMethod]
    public void SelectToResult_Primary_WithError_ShouldCreateResultWithMappedError()
    {
        // Arrange
        OneOf<string, User> oneOf = OneOf<string, User>.FromT1("User not found");

        // Act
        Result<UserDto> result = oneOf.SelectToResult(
            user => new UserDto(user.Name),
            error => new Error($"User error: {error}")
        );

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("User error: User not found", result.Errors.First().Message);
    }

    [TestMethod]
    public void SelectToResult_Primary_WithNullErrorMapper_ShouldUseStringConversion()
    {
        // Arrange
        OneOf<string, User> oneOf = OneOf<string, User>.FromT1("Database error");

        // Act
        Result<UserDto> result = oneOf.SelectToResult(user => new UserDto(user.Name));

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Database error", result.Errors.First().Message);
    }

    [TestMethod]
    public void SelectToResult_Primary_WithNullSelector_ShouldThrowException()
    {
        // Arrange
        var user = new User("Alice");
        OneOf<string, User> oneOf = OneOf<string, User>.FromT2(user);

        // Act & Assert
        try
        {
            oneOf.SelectToResult<string, User, UserDto>(null!);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    #endregion

    #region SelectToResult Tests - IError Overload

    [TestMethod]
    public void SelectToResult_IError_WithSuccess_ShouldCreateResultWithMappedValue()
    {
        // Arrange
        var user = new User("Alice");
        OneOf<ApiError, User> oneOf = OneOf<ApiError, User>.FromT2(user);

        // Act
        Result<UserDto> result = oneOf.SelectToResult(user => new UserDto(user.Name));

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Alice", result.Value.Name);
    }

    [TestMethod]
    public void SelectToResult_IError_WithError_ShouldCreateResultWithDirectError()
    {
        // Arrange
        var apiError = new ApiError("User not found", 404);
        OneOf<ApiError, User> oneOf = OneOf<ApiError, User>.FromT1(apiError);

        // Act
        Result<UserDto> result = oneOf.SelectToResult(user => new UserDto(user.Name));

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("User not found", result.Errors.First().Message);
    }

    [TestMethod]
    public void SelectToResult_IError_WithNullSelector_ShouldThrowException()
    {
        // Arrange
        var user = new User("Alice");
        OneOf<ApiError, User> oneOf = OneOf<ApiError, User>.FromT2(user);

        // Act & Assert
        try
        {
            oneOf.SelectToResult<ApiError, User, UserDto>(null!);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    #endregion

    #region BindToResult Tests - Primary Overload

    [TestMethod]
    public void BindToResult_Primary_WithSuccess_ShouldBindToResult()
    {
        // Arrange
        var user = new User("Alice");
        OneOf<string, User> oneOf = OneOf<string, User>.FromT2(user);

        // Act
        Result<User> result = oneOf.BindToResult(
            user => ValidateUser(user),
            error => new Error($"Bind error: {error}")
        );

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(user, result.Value);
    }

    [TestMethod]
    public void BindToResult_Primary_WithError_ShouldCreateResultWithMappedError()
    {
        // Arrange
        OneOf<string, User> oneOf = OneOf<string, User>.FromT1("User not found");

        // Act
        Result<User> result = oneOf.BindToResult(
            user => ValidateUser(user),
            error => new Error($"Bind error: {error}")
        );

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Bind error: User not found", result.Errors.First().Message);
    }

    [TestMethod]
    public void BindToResult_Primary_WithNullBinder_ShouldThrowException()
    {
        // Arrange
        var user = new User("Alice");
        OneOf<string, User> oneOf = OneOf<string, User>.FromT2(user);

        // Act & Assert
        try
        {
            oneOf.BindToResult<string, User, User>(null!);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    #endregion

    #region BindToResult Tests - IError Overload

    [TestMethod]
    public void BindToResult_IError_WithSuccess_ShouldBindToResult()
    {
        // Arrange
        var user = new User("Alice");
        OneOf<ApiError, User> oneOf = OneOf<ApiError, User>.FromT2(user);

        // Act
        Result<User> result = oneOf.BindToResult(user => ValidateUser(user));

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(user, result.Value);
    }

    [TestMethod]
    public void BindToResult_IError_WithError_ShouldCreateResultWithDirectError()
    {
        // Arrange
        var apiError = new ApiError("User not found", 404);
        OneOf<ApiError, User> oneOf = OneOf<ApiError, User>.FromT1(apiError);

        // Act
        Result<User> result = oneOf.BindToResult(user => ValidateUser(user));

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("User not found", result.Errors.First().Message);
    }

    #endregion

    #region Filter Tests

    [TestMethod]
    public void Filter_WithPassingPredicate_ShouldReturnOriginal()
    {
        // Arrange
        var user = new User("Alice", true);
        OneOf<Error, User> oneOf = OneOf<Error, User>.FromT2(user);

        // Act
        OneOf<Error, User> result = oneOf.Filter(u => u.IsActive);

        // Assert
        Assert.IsTrue(result.IsT2);
        Assert.AreEqual(user, result.AsT2);
    }

    [TestMethod]
    public void Filter_WithFailingPredicate_ShouldThrowException()
    {
        // Arrange
        var user = new User("Alice", false);
        OneOf<Error, User> oneOf = OneOf<Error, User>.FromT2(user);

        // Act & Assert
        try
        {
            oneOf.Filter(u => u.IsActive);
            Assert.Fail("Expected InvalidOperationException was not thrown");
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual("Predicate not satisfied", ex.Message);
        }
    }

    [TestMethod]
    public void Filter_WithError_ShouldReturnOriginal()
    {
        // Arrange
        var error = new Error("Database error");
        OneOf<Error, User> oneOf = OneOf<Error, User>.FromT1(error);

        // Act
        OneOf<Error, User> result = oneOf.Filter(u => u.IsActive);

        // Assert
        Assert.IsTrue(result.IsT1);
        Assert.AreEqual(error, result.AsT1);
    }

    [TestMethod]
    public void Filter_WithNullPredicate_ShouldThrowException()
    {
        // Arrange
        var user = new User("Alice");
        OneOf<Error, User> oneOf = OneOf<Error, User>.FromT2(user);

        // Act & Assert
        try
        {
            oneOf.Filter(null!);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    #endregion

    #region ToOneOfCustom Tests

    [TestMethod]
    public void ToOneOfCustom_WithSuccess_ShouldCreateOneOfWithT2()
    {
        // Arrange
        var user = new User("Alice");
        Result<User> result = Result<User>.Ok(user);

        // Act
        OneOf<ValidationError, User> oneOf = result.ToOneOfCustom(reason => new ValidationError(reason.Message));

        // Assert
        Assert.IsTrue(oneOf.IsT2);
        Assert.AreEqual(user, oneOf.AsT2);
    }

    [TestMethod]
    public void ToOneOfCustom_WithError_ShouldCreateOneOfWithT1()
    {
        // Arrange
        Result<User> result = Result<User>.Fail("User not found");

        // Act
        OneOf<ValidationError, User> oneOf = result.ToOneOfCustom(reason => new ValidationError(reason.Message));

        // Assert
        Assert.IsTrue(oneOf.IsT1);
        Assert.AreEqual("User not found", oneOf.AsT1.Message);
    }

    [TestMethod]
    public void ToOneOfCustom_WithNullErrorMapper_ShouldThrowException()
    {
        // Arrange
        var user = new User("Alice");
        Result<User> result = Result<User>.Ok(user);

        // Act & Assert
        try
        {
            result.ToOneOfCustom<ValidationError, User>(null!);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void Integration_MixedPipeline_ShouldWorkCorrectly()
    {
        // Arrange
        OneOf<ApiError, User> apiResult = OneOf<ApiError, User>.FromT2(new User("Alice"));

        // Act - Mixed pipeline: OneOf → Result → OneOf
        Result<UserDto> resultStep = apiResult.SelectToResult(user => new UserDto(user.Name));
        OneOf<ValidationError, UserDto> finalResult = resultStep.ToOneOfCustom(reason => new ValidationError(reason.Message));

        // Assert
        Assert.IsTrue(finalResult.IsT2);
        Assert.AreEqual("Alice", finalResult.AsT2.Name);
    }

    [TestMethod]
    public void Integration_ErrorFlow_ShouldPropagateCorrectly()
    {
        // Arrange
        OneOf<ApiError, User> apiResult = OneOf<ApiError, User>.FromT1(new ApiError("Service unavailable", 503));

        // Act - Error flow through mixed pipeline
        Result<UserDto> resultStep = apiResult.SelectToResult(user => new UserDto(user.Name));
        OneOf<ValidationError, UserDto> finalResult = resultStep.ToOneOfCustom(reason => new ValidationError(reason.Message));

        // Assert
        Assert.IsTrue(finalResult.IsT1);
        Assert.AreEqual("Service unavailable", finalResult.AsT1.Message);
    }

    #endregion

    #region Helper Classes

    public class User
    {
        public string Name { get; }
        public bool IsActive { get; }

        public User(string name, bool isActive = true)
        {
            Name = name;
            IsActive = isActive;
        }

        public override bool Equals(object? obj)
        {
            return obj is User other && Name == other.Name && IsActive == other.IsActive;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, IsActive);
        }
    }

    public class UserDto
    {
        public string Name { get; }

        public UserDto(string name)
        {
            Name = name;
        }

        public override bool Equals(object? obj)
        {
            return obj is UserDto other && Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    public class ApiError : IError
    {
        public string Message { get; init; }
        public System.Collections.Immutable.ImmutableDictionary<string, object> Tags { get; init; }
        public int Code { get; }

        public ApiError(string message, int code = 500)
        {
            Message = message;
            Code = code;
            Tags = System.Collections.Immutable.ImmutableDictionary<string, object>.Empty;
        }
    }

    public class ValidationError
    {
        public string Message { get; }

        public ValidationError(string message)
        {
            Message = message;
        }
    }

    #endregion

    #region Helper Methods

    private static Result<User> ValidateUser(User user)
    {
        return string.IsNullOrEmpty(user.Name) 
            ? Result<User>.Fail("User name is required")
            : Result<User>.Ok(user);
    }

    #endregion
}
