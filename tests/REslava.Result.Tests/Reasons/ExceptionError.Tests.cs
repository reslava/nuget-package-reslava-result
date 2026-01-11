namespace REslava.Result.Reasons.Tests;

/// <summary>
/// Comprehensive tests for the ExceptionError class
/// </summary>
[TestClass]
public sealed class ExceptionErrorTests
{
    #region Constructor Tests - Default Message

    [TestMethod]
    public void Constructor_WithException_UsesExceptionMessage()
    {
        // Arrange
        var exception = new InvalidOperationException("Something went wrong");

        // Act
        var error = new ExceptionError(exception);

        // Assert
        Assert.AreEqual("Something went wrong", error.Message);
        Assert.AreSame(exception, error.Exception);
    }

    [TestMethod]
    public void Constructor_WithNullException_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ExceptionError(null!));
    }

    [TestMethod]
    public void Constructor_WithExceptionWithNullMessage_UsesFallbackMessage()
    {
        // Arrange
        var exception = new CustomExceptionWithNullMessage();

        // Act
        var error = new ExceptionError(exception);

        // Assert
        Assert.AreEqual("An exception ocurred", error.Message);
    }

    #endregion

    #region Constructor Tests - Custom Message

    [TestMethod]
    public void Constructor_WithCustomMessage_UsesCustomMessage()
    {
        // Arrange
        var exception = new InvalidOperationException("Original message");

        // Act
        var error = new ExceptionError(exception, "Custom error message");

        // Assert
        Assert.AreEqual("Custom error message", error.Message);
        Assert.AreSame(exception, error.Exception);
    }

    [TestMethod]
    public void Constructor_WithCustomMessage_NullException_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExceptionError(null!, "Custom message"));
    }

    [TestMethod]
    public void Constructor_WithCustomMessage_EmptyMessage_UsesEmptyMessage()
    {
        // Arrange
        var exception = new Exception("Original");

        // Act
        var error = new ExceptionError(exception, "");

        // Assert
        Assert.AreEqual("", error.Message);
    }

    #endregion

    #region Exception Property Tests

    [TestMethod]
    public void Exception_Property_ReturnsOriginalException()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");

        // Act
        var error = new ExceptionError(exception);

        // Assert
        Assert.AreSame(exception, error.Exception);
    }

    [TestMethod]
    public void Exception_Property_PreservesExceptionType()
    {
        // Arrange
        var exception = new ArgumentNullException("paramName");

        // Act
        var error = new ExceptionError(exception);

        // Assert
        Assert.IsInstanceOfType<ArgumentNullException>(error.Exception);
    }

    #endregion

    #region Tags Tests - ExceptionType

    [TestMethod]
    public void Tags_ContainsExceptionType()
    {
        // Arrange
        var exception = new InvalidOperationException("Test");

        // Act
        var error = new ExceptionError(exception);

        // Assert
        Assert.IsTrue(error.Tags.ContainsKey("ExceptionType"));
        Assert.AreEqual("InvalidOperationException", error.Tags["ExceptionType"]);
    }

    [TestMethod]
    public void Tags_ExceptionType_DifferentExceptionTypes()
    {
        // Arrange & Act
        var error1 = new ExceptionError(new ArgumentException("Test"));
        var error2 = new ExceptionError(new NullReferenceException("Test"));
        var error3 = new ExceptionError(new DivideByZeroException("Test"));

        // Assert
        Assert.AreEqual("ArgumentException", error1.Tags["ExceptionType"]);
        Assert.AreEqual("NullReferenceException", error2.Tags["ExceptionType"]);
        Assert.AreEqual("DivideByZeroException", error3.Tags["ExceptionType"]);
    }

    #endregion

    #region Tags Tests - StackTrace

    [TestMethod]
    public void Tags_ContainsStackTrace_WhenAvailable()
    {
        // Arrange
        Exception exception;
        try
        {
            throw new InvalidOperationException("Test exception");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Act
        var error = new ExceptionError(exception);

        // Assert
        Assert.IsTrue(error.Tags.ContainsKey("StackTrace"));
        Assert.IsNotNull(error.Tags["StackTrace"]);
        var stackTrace = error.Tags["StackTrace"] as string;
        Assert.IsNotNull(stackTrace);
        Assert.IsGreaterThan(0, stackTrace!.Length);
    }

    [TestMethod]
    public void Tags_DoesNotContainStackTrace_WhenNull()
    {
        // Arrange
        var exception = new Exception("Test"); // No stack trace

        // Act
        var error = new ExceptionError(exception);

        // Assert
        Assert.IsFalse(error.Tags.ContainsKey("StackTrace"));
    }

    #endregion

    #region Tags Tests - InnerException

    [TestMethod]
    public void Tags_ContainsInnerException_WhenAvailable()
    {
        // Arrange
        var innerException = new ArgumentException("Inner error");
        var outerException = new InvalidOperationException("Outer error", innerException);

        // Act
        var error = new ExceptionError(outerException);

        // Assert
        Assert.IsTrue(error.Tags.ContainsKey("InnerException"));
        Assert.AreEqual("Inner error", error.Tags["InnerException"]);
    }

    [TestMethod]
    public void Tags_DoesNotContainInnerException_WhenNull()
    {
        // Arrange
        var exception = new Exception("Test");

        // Act
        var error = new ExceptionError(exception);

        // Assert
        Assert.IsFalse(error.Tags.ContainsKey("InnerException"));
    }

    [TestMethod]
    public void Tags_InnerException_NestedExceptions()
    {
        // Arrange
        var level3 = new ArgumentException("Level 3");
        var level2 = new InvalidOperationException("Level 2", level3);
        var level1 = new Exception("Level 1", level2);

        // Act
        var error = new ExceptionError(level1);

        // Assert
        Assert.IsTrue(error.Tags.ContainsKey("InnerException"));
        Assert.AreEqual("Level 2", error.Tags["InnerException"]);
        // Note: Only immediate inner exception is captured
    }

    #endregion

    #region Complete Tags Tests

    [TestMethod]
    public void Tags_CompleteException_ContainsAllMetadata()
    {
        // Arrange
        Exception exception;
        try
        {
            try
            {
                throw new ArgumentException("Inner issue");
            }
            catch (Exception inner)
            {
                throw new InvalidOperationException("Outer issue", inner);
            }
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Act
        var error = new ExceptionError(exception);

        // Assert
        Assert.HasCount(3, error.Tags); // ExceptionType, StackTrace, InnerException
        Assert.IsTrue(error.Tags.ContainsKey("ExceptionType"));
        Assert.IsTrue(error.Tags.ContainsKey("StackTrace"));
        Assert.IsTrue(error.Tags.ContainsKey("InnerException"));
    }

    [TestMethod]
    public void Tags_MinimalException_ContainsOnlyExceptionType()
    {
        // Arrange
        var exception = new Exception("Test");

        // Act
        var error = new ExceptionError(exception);

        // Assert
        Assert.HasCount(1, error.Tags); // Only ExceptionType
        Assert.IsTrue(error.Tags.ContainsKey("ExceptionType"));
        Assert.IsFalse(error.Tags.ContainsKey("StackTrace"));
        Assert.IsFalse(error.Tags.ContainsKey("InnerException"));
    }

    #endregion

    #region Fluent Interface Tests

    [TestMethod]
    public void FluentInterface_AdditionalTags_CanBeAdded()
    {
        // Arrange
        var exception = new InvalidOperationException("Test");

        // Act
        var error = new ExceptionError(exception)
            .WithTags("UserId", "user-123")
            .WithTags("Operation", "SaveData");

        // Assert
        Assert.IsTrue(error.Tags.ContainsKey("ExceptionType"));
        Assert.IsTrue(error.Tags.ContainsKey("UserId"));
        Assert.IsTrue(error.Tags.ContainsKey("Operation"));
        Assert.AreEqual("user-123", error.Tags["UserId"]);
        Assert.AreEqual("SaveData", error.Tags["Operation"]);
    }

    [TestMethod]
    public void FluentInterface_MessageUpdate_Works()
    {
        // Arrange
        var exception = new Exception("Original");

        // Act
        var error = new ExceptionError(exception)
            .WithMessage("Updated error message");

        // Assert
        Assert.AreEqual("Updated error message", error.Message);
        Assert.AreSame(exception, error.Exception);
    }

    #endregion

    #region Interface Implementation Tests

    [TestMethod]
    public void ExceptionError_ImplementsIError()
    {
        // Arrange
        var error = new ExceptionError(new Exception());

        // Assert
        Assert.IsInstanceOfType<IError>(error);
    }

    [TestMethod]
    public void ExceptionError_CRTP()
    {
        // Arrange
        var error = new ExceptionError(new Exception());

        // Assert
        Assert.IsInstanceOfType<Reason<ExceptionError>>(error);
    }

    #endregion

    #region Use in Result Tests

    [TestMethod]
    public void ExceptionError_UsedInResult_Works()
    {
        // Arrange
        var exception = new InvalidOperationException("Database error");
        var error = new ExceptionError(exception);

        // Act
        var result = Result.Fail(error);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual("Database error", result.Errors[0].Message);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
    }

    [TestMethod]
    public void ExceptionError_UsedInResultOfT_Works()
    {
        // Arrange
        var exception = new ArgumentException("Invalid input");
        var error = new ExceptionError(exception);

        // Act
        var result = Result<int>.Fail(error);

        // Assert
        Assert.IsTrue(result.IsFailed);
        var exceptionError = result.Errors[0] as ExceptionError;
        Assert.IsNotNull(exceptionError);
        Assert.AreSame(exception, exceptionError!.Exception);
    }

    #endregion

    #region Map/Bind Exception Handling Tests

    [TestMethod]
    public void Map_WhenMapperThrows_ReturnsExceptionError()
    {
        // Arrange
        var result = Result<int>.Ok(10);
        var expectedException = new InvalidOperationException("Mapper failed");

        // Act
        var mapped = result.Map<string>(x => throw expectedException);

        // Assert
        Assert.IsTrue(mapped.IsFailed);
        Assert.HasCount(1, mapped.Errors);
        var exceptionError = mapped.Errors[0] as ExceptionError;
        Assert.IsNotNull(exceptionError);
        Assert.AreSame(expectedException, exceptionError!.Exception);
    }

    [TestMethod]
    public void Bind_WhenBinderThrows_ReturnsExceptionError()
    {
        // Arrange
        var result = Result<int>.Ok(10);
        var expectedException = new InvalidOperationException("Binder failed");

        // Act
        var bound = result.Bind<string>(x => throw expectedException);

        // Assert
        Assert.IsTrue(bound.IsFailed);
        var exceptionError = bound.Errors[0] as ExceptionError;
        Assert.IsNotNull(exceptionError);
        Assert.AreSame(expectedException, exceptionError!.Exception);
    }

    #endregion

    #region Real-World Scenario Tests

    [TestMethod]
    public void ExceptionError_DatabaseScenario()
    {
        // Arrange
        Exception exception;
        try
        {
            // Simulate database operation
            throw new InvalidOperationException("Cannot connect to database");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Act
        var error = new ExceptionError(exception, "Database operation failed")
            .WithTags("Server", "localhost")
            .WithTags("Database", "ProductionDB")
            .WithTags("Operation", "SaveOrder");

        var result = Result<int>.Fail(error);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Database operation failed", result.Errors[0].Message);
        Assert.AreEqual("localhost", result.Errors[0].Tags["Server"]);
        Assert.Contains("InvalidOperationException", (string)result.Errors[0].Tags["ExceptionType"]);
    }

    [TestMethod]
    public void ExceptionError_APICallScenario()
    {
        // Arrange
        var innerException = new HttpRequestException("Network error");
        var outerException = new InvalidOperationException("API call failed", innerException);

        // Act
        var error = new ExceptionError(outerException)
            .WithTags("Endpoint", "/api/users")
            .WithTags("Method", "POST")
            .WithTags("RetryCount", 3);

        // Assert
        Assert.AreEqual("API call failed", error.Message);
        Assert.AreEqual("Network error", error.Tags["InnerException"]);
        Assert.AreEqual("/api/users", error.Tags["Endpoint"]);
        Assert.AreEqual(3, error.Tags["RetryCount"]);
    }

    [TestMethod]
    public void ExceptionError_ValidationPipeline_Scenario()
    {
        // Arrange & Act
        var result = Result<string>.Ok("test@example.com")
            .Bind(email =>
            {
                if (string.IsNullOrEmpty(email))
                    throw new ArgumentException("Email cannot be empty");
                return Result<string>.Ok(email.ToLower());
            })
            .Bind(email =>
            {
                if (!email.Contains("@"))
                    throw new FormatException("Invalid email format");
                return Result<string>.Ok(email);
            });

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("test@example.com", result.Value);
    }

    [TestMethod]
    public void ExceptionError_ChainedOperations_PreservesFirstException()
    {
        // Arrange
        var firstException = new ArgumentException("First error");

        // Act
        var result = Result<int>.Ok(10)
            .Map<string>(x => throw firstException)
            .Map(s => s.ToUpper()); // This won't execute

        // Assert
        Assert.IsTrue(result.IsFailed);
        var exceptionError = result.Errors[0] as ExceptionError;
        Assert.IsNotNull(exceptionError);
        Assert.AreSame(firstException, exceptionError!.Exception);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void ExceptionError_WithAggregateException()
    {
        // Arrange
        SystemException[] innerExceptions = new SystemException[]
        {
            new InvalidOperationException("Error 1"),
            new ArgumentException("Error 2")
        };
        var aggregateException = new AggregateException("Multiple errors", innerExceptions);

        // Act
        var error = new ExceptionError(aggregateException);

        // Assert
        Assert.StartsWith("Multiple errors", error.Message);
        Assert.AreEqual("AggregateException", error.Tags["ExceptionType"]);
        Assert.IsInstanceOfType<AggregateException>(error.Exception);
    }

    [TestMethod]
    public void ExceptionError_WithCustomException()
    {
        // Arrange
        var customException = new CustomBusinessException("Business rule violation");

        // Act
        var error = new ExceptionError(customException);

        // Assert
        Assert.AreEqual("Business rule violation", error.Message);
        Assert.AreEqual("CustomBusinessException", error.Tags["ExceptionType"]);
    }

    #endregion

    #region Helper Classes

    private class CustomExceptionWithNullMessage : Exception
    {
        public override string? Message => null;
    }

    private class CustomBusinessException : Exception
    {
        public CustomBusinessException(string message) : base(message) { }
    }

    #endregion
}
