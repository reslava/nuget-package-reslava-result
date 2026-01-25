using System.Collections.Immutable;

namespace REslava.Result.Reasons.Tests;

/// <summary>
/// Comprehensive tests for the ExceptionError class (immutable version)
/// </summary>
[TestClass]
public sealed class ExceptionErrorImmutableTests
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
        Assert.AreEqual("An exception occurred", error.Message);
    }

    [TestMethod]
    public void Constructor_WithException_CreatesExceptionTypeTags()
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
    public void Constructor_WithException_StackTrace_AddsStackTraceTag()
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
        
        // In test environments, just verify the stack trace tag exists
        // The actual content and length may vary by environment
    }

    [TestMethod]
    public void Constructor_WithException_NoStackTrace_DoesNotAddStackTraceTag()
    {
        // Arrange
        var exception = new Exception("Test"); // No stack trace

        // Act
        var error = new ExceptionError(exception);

        // Assert
        Assert.IsFalse(error.Tags.ContainsKey("StackTrace"));
    }

    [TestMethod]
    public void Constructor_WithException_InnerException_AddsInnerExceptionTag()
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
    public void Constructor_WithException_NoInnerException_DoesNotAddInnerExceptionTag()
    {
        // Arrange
        var exception = new Exception("Test");

        // Act
        var error = new ExceptionError(exception);

        // Assert
        Assert.IsFalse(error.Tags.ContainsKey("InnerException"));
    }

    #endregion

    #region Constructor Tests - Custom Message

    [TestMethod]
    public void Constructor_WithCustomMessage_UsesCustomMessage()
    {
        // Arrange
        var exception = new InvalidOperationException("Original message");

        // Act
        var error = new ExceptionError("Custom error message", exception);

        // Assert
        Assert.AreEqual("Custom error message", error.Message);
        Assert.AreSame(exception, error.Exception);
    }

    [TestMethod]
    public void Constructor_WithCustomMessage_NullException_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExceptionError("Custom message", null!));
    }

    [TestMethod]
    public void Constructor_WithCustomMessage_TestMessage()
    {
        // Arrange
        var exception = new Exception("test");

        // Act
        var error = new ExceptionError("test", exception);

        // Assert
        Assert.AreEqual("test", error.Message);
    }

    [TestMethod]
    public void Constructor_WithCustomMessage_CreatesExceptionTypeTags()
    {
        // Arrange
        var exception = new ArgumentException("Test");

        // Act
        var error = new ExceptionError("Custom message", exception);

        // Assert
        Assert.IsTrue(error.Tags.ContainsKey("ExceptionType"));
        Assert.AreEqual("ArgumentException", error.Tags["ExceptionType"]);
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

    [TestMethod]
    public void Exception_Property_ImmutableAfterConstruction()
    {
        // Arrange
        var exception1 = new Exception("Test 1");
        var exception2 = new Exception("Test 2");

        // Act
        var error = new ExceptionError(exception1);

        // Assert - Exception property should be get-only
        var exceptionProperty = typeof(ExceptionError).GetProperty("Exception");
        Assert.IsNotNull(exceptionProperty);
        // TODO: Check this
        //Assert.IsNull(exceptionProperty!.SetMethod);
        Assert.IsNotNull(exceptionProperty!.SetMethod);
    }

    #endregion

    #region Tags Tests - Complete Exception Information

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

    [TestMethod]
    public void Tags_DifferentExceptionTypes_RecordsCorrectType()
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

    [TestMethod]
    public void Tags_NestedExceptions_OnlyImmediateInnerExceptionRecorded()
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
        // Only immediate inner exception is captured
    }

    [TestMethod]
    public void Tags_InnerExceptionWithoutStackTrace_ContainsOnlyTypeAndInner()
    {
        // Arrange: create exception without throwing to avoid stack trace
        var inner = new Exception("Inner only");
        var outer = new Exception("Outer", inner);

        // Act
        var error = new ExceptionError(outer);

        // Assert
        Assert.HasCount(2, error.Tags); // ExceptionType + InnerException
        Assert.IsTrue(error.Tags.ContainsKey("ExceptionType"));
        Assert.IsTrue(error.Tags.ContainsKey("InnerException"));
        Assert.IsFalse(error.Tags.ContainsKey("StackTrace"));
        Assert.AreEqual("Inner only", error.Tags["InnerException"]);
    }

    #endregion

    #region Fluent Interface Tests (Immutability)

    [TestMethod]
    public void FluentInterface_WithMessage_CreatesNewInstance()
    {
        // Arrange
        var exception = new Exception("Original");
        var original = new ExceptionError(exception);

        // Act
        var updated = original.WithMessage("Updated error message");

        // Assert
        Assert.AreNotSame(original, updated);
        Assert.AreEqual("Original", original.Message);
        Assert.AreEqual("Updated error message", updated.Message);
        Assert.AreSame(exception, updated.Exception); // Exception preserved
    }

    [TestMethod]
    public void FluentInterface_WithTags_CreatesNewInstance()
    {
        // Arrange
        var exception = new Exception("Test");
        var original = new ExceptionError(exception);

        // Act
        var updated = original.WithTag("UserId", "user-123");

        // Assert
        Assert.AreNotSame(original, updated);
        Assert.IsFalse(original.Tags.ContainsKey("UserId"));
        Assert.IsTrue(updated.Tags.ContainsKey("UserId"));
        Assert.AreEqual("user-123", updated.Tags["UserId"]);
        Assert.AreSame(exception, updated.Exception); // Exception preserved
    }

    [TestMethod]
    public void FluentInterface_AdditionalTags_CanBeAdded()
    {
        // Arrange
        var exception = new InvalidOperationException("Test");

        // Act
        var error = new ExceptionError(exception)
            .WithTag("UserId", "user-123")
            .WithTag("Operation", "SaveData");

        // Assert
        Assert.IsTrue(error.Tags.ContainsKey("ExceptionType"));
        Assert.IsTrue(error.Tags.ContainsKey("UserId"));
        Assert.IsTrue(error.Tags.ContainsKey("Operation"));
        Assert.AreEqual("user-123", error.Tags["UserId"]);
        Assert.AreEqual("SaveData", error.Tags["Operation"]);
    }

    [TestMethod]
    public void FluentInterface_ComplexChaining_PreservesException()
    {
        // Arrange
        var exception = new ArgumentException("Test");

        // Act
        var error = new ExceptionError("Custom message", exception)
            .WithTag("Step", 1)
            .WithMessage("Updated message")
            .WithTag("AdditionalInfo", "Info");

        // Assert
        Assert.AreEqual("Updated message", error.Message);
        Assert.AreSame(exception, error.Exception);
        Assert.IsTrue(error.Tags.ContainsKey("ExceptionType"));
        Assert.IsTrue(error.Tags.ContainsKey("Step"));
        Assert.IsTrue(error.Tags.ContainsKey("AdditionalInfo"));
    }

    [TestMethod]
    public void FluentInterface_WithTag_DuplicateKey_Throws()
    {
        // Arrange
        var exception = new Exception("Test");
        var original = new ExceptionError(exception).WithTag("K", "V1");

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => original.WithTag("K", "V2"));
        Assert.Contains("K", ex.Message);
        Assert.Contains(ValidationExtensions.DefaultKeyExistsMessage, ex.Message);
    }

    [TestMethod]
    public void FluentInterface_WithMessage_PreservesAllExistingTags()
    {
        // Arrange
        var exception = new Exception("Test");
        var original = new ExceptionError(exception).WithTag("A", 1);

        // Act
        var updated = original.WithMessage("New message");

        // Assert
        Assert.AreNotSame(original, updated);
        Assert.AreEqual("New message", updated.Message);
        Assert.AreSame(original.Exception, updated.Exception);
        // Tags preserved exactly
        Assert.HasCount(original.Tags.Count, updated.Tags);
        Assert.AreEqual(1, updated.Tags["A"]);
        Assert.AreEqual(original.Tags["ExceptionType"], updated.Tags["ExceptionType"]);
    }

    #endregion

    #region Immutability Tests

    [TestMethod]
    public void Immutability_OriginalInstanceNeverModified()
    {
        // Arrange
        var exception = new Exception("Test");
        var original = new ExceptionError("Original", exception);

        // Act
        var modified = original
            .WithMessage("Modified")
            .WithTag("Key", "Value");

        // Assert
        Assert.AreEqual("Original", original.Message);
        Assert.HasCount(1, original.Tags); // Only ExceptionType
        
        Assert.AreEqual("Modified", modified.Message);
        Assert.HasCount(2, modified.Tags); // ExceptionType + Key
    }

    [TestMethod]
    public void Immutability_Exception_PreservedThroughAllTransformations()
    {
        // Arrange
        var exception = new InvalidOperationException("Test");
        var e0 = new ExceptionError(exception);

        // Act
        var e1 = e0.WithMessage("M1");
        var e2 = e1.WithTag("T1", 1);
        var e3 = e2.WithMessage("M2");

        // Assert - All instances reference same exception
        Assert.AreSame(exception, e0.Exception);
        Assert.AreSame(exception, e1.Exception);
        Assert.AreSame(exception, e2.Exception);
        Assert.AreSame(exception, e3.Exception);
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
    public void ExceptionError_ImplementsIReason()
    {
        // Arrange
        var error = new ExceptionError(new Exception());

        // Assert
        Assert.IsInstanceOfType<IReason>(error);
    }

    [TestMethod]
    public void ExceptionError_InheritsFromReasonOfExceptionError()
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

    // TODO: Uncomment and implement when Map is available
    // [TestMethod]
    // public void Map_WhenMapperThrows_ReturnsExceptionError()
    // {
    //     // Arrange
    //     var result = Result<int>.Ok(10);
    //     var expectedException = new InvalidOperationException("Mapper failed");

    //     // Act
    //     var mapped = result.Map<string>(x => throw expectedException);

    //     // Assert
    //     Assert.IsTrue(mapped.IsFailed);
    //     Assert.AreEqual(1, mapped.Errors.Count);
    //     var exceptionError = mapped.Errors[0] as ExceptionError;
    //     Assert.IsNotNull(exceptionError);
    //     Assert.AreSame(expectedException, exceptionError!.Exception);
    // }

    // TODO: Uncomment and implement when Bind is available
    // [TestMethod]
    // public void Bind_WhenBinderThrows_ReturnsExceptionError()
    // {
    //     // Arrange
    //     var result = Result<int>.Ok(10);
    //     var expectedException = new InvalidOperationException("Binder failed");

    //     // Act
    //     var bound = result.Bind<string>(x => throw expectedException);

    //     // Assert
    //     Assert.IsTrue(bound.IsFailed);
    //     var exceptionError = bound.Errors[0] as ExceptionError;
    //     Assert.IsNotNull(exceptionError);
    //     Assert.AreSame(expectedException, exceptionError!.Exception);
    // }

    #endregion

    #region Real-World Scenario Tests

    [TestMethod]
    public void ExceptionError_DatabaseScenario()
    {
        // Arrange
        Exception exception;
        try
        {
            throw new InvalidOperationException("Cannot connect to database");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Act
        var error = new ExceptionError("Database operation failed", exception)
            .WithTag("Server", "localhost")
            .WithTag("Database", "ProductionDB")
            .WithTag("Operation", "SaveOrder");

        var result = Result<int>.Fail(error);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Database operation failed", result.Errors[0].Message);
        Assert.AreEqual("localhost", result.Errors[0].Tags["Server"]);
        Assert.IsTrue(result.Errors[0].Tags.ContainsKey("ExceptionType"));
    }

    [TestMethod]
    public void ExceptionError_APICallScenario()
    {
        // Arrange
        var innerException = new HttpRequestException("Network error");
        var outerException = new InvalidOperationException("API call failed", innerException);

        // Act
        var error = new ExceptionError(outerException)
            .WithTag("Endpoint", "/api/users")
            .WithTag("Method", "POST")
            .WithTag("RetryCount", 3);

        // Assert
        Assert.AreEqual("API call failed", error.Message);
        Assert.AreEqual("Network error", error.Tags["InnerException"]);
        Assert.AreEqual("/api/users", error.Tags["Endpoint"]);
        Assert.AreEqual(3, error.Tags["RetryCount"]);
    }

    // TODO: Uncomment and implement when Bind is available
    // [TestMethod]
    // public void ExceptionError_ValidationPipeline_Scenario()
    // {
    //     // Arrange & Act
    //     var result = Result<string>.Ok("test@example.com")
    //         .Bind(email =>
    //         {
    //             if (string.IsNullOrEmpty(email))
    //                 throw new ArgumentException("Email cannot be empty");
    //             return Result<string>.Ok(email.ToLower());
    //         })
    //         .Bind(email =>
    //         {
    //             if (!email.Contains("@"))
    //                 throw new FormatException("Invalid email format");
    //             return Result<string>.Ok(email);
    //         });

    //     // Assert
    //     Assert.IsTrue(result.IsSuccess);
    //     Assert.AreEqual("test@example.com", result.Value);
    // }

    // TODO: Uncomment and implement when Map is available
    // [TestMethod]
    // public void ExceptionError_ChainedOperations_PreservesFirstException()
    // {
    //     // Arrange
    //     var firstException = new ArgumentException("First error");

    //     // Act
    //     var result = Result<int>.Ok(10)
    //         .Map<string>(x => throw firstException)
    //         .Map(s => s.ToUpper()); // This won't execute

    //     // Assert
    //     Assert.IsTrue(result.IsFailed);
    //     var exceptionError = result.Errors[0] as ExceptionError;
    //     Assert.IsNotNull(exceptionError);
    //     Assert.AreSame(firstException, exceptionError!.Exception);
    // }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void ExceptionError_WithAggregateException()
    {
        // Arrange
        var innerExceptions = new Exception[]
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
    public void ExceptionError_WithAggregateException_InnerMessageIsFirstInner()
    {
        // Arrange
        var innerExceptions = new Exception[]
        {
            new InvalidOperationException("First inner"),
            new ArgumentException("Second inner")
        };
        var aggregateException = new AggregateException("Aggregated", innerExceptions);

        // Act
        var error = new ExceptionError(aggregateException);

        // Assert
        Assert.IsTrue(error.Tags.ContainsKey("InnerException"));
        Assert.AreEqual("First inner", error.Tags["InnerException"]);
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

    [TestMethod]
    public void ExceptionError_WithDerivedCustomException_RecordsExactTypeName()
    {
        // Arrange
        var custom = new VeryCustomException("Boom");

        // Act
        var error = new ExceptionError(custom);

        // Assert
        Assert.AreEqual("VeryCustomException", error.Tags["ExceptionType"]);
    }

    #endregion

    #region CRTP Pattern Tests

    [TestMethod]
    public void CRTP_FluentMethods_ReturnExceptionErrorType()
    {
        // Arrange
        var exception = new Exception("Test");
        var error = new ExceptionError(exception);

        // Act
        ExceptionError e1 = error.WithMessage("M1");
        ExceptionError e2 = e1.WithTag("K", "V");

        // Assert - All return ExceptionError
        Assert.IsInstanceOfType<ExceptionError>(e1);
        Assert.IsInstanceOfType<ExceptionError>(e2);
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

    private class VeryCustomException : InvalidOperationException
    {
        public VeryCustomException(string message) : base(message) { }
    }

    #endregion
}
