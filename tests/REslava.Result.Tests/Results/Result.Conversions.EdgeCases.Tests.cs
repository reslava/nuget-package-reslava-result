namespace REslava.Result.Tests;

[TestClass]
public sealed class ResultConversionsEdgeCasesTests
{
    #region Implicit Conversion - TValue Tests

    [TestMethod]
    public void ImplicitConversion_FromValue_CreatesSuccessResult()
    {
        // Act
        Result<int> result = 42;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void ImplicitConversion_FromNullValue_CreatesSuccessResult()
    {
        // Act
        Result<string> result = (string)null!;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.Value);
        Assert.IsEmpty(result.Errors);
    }

    #endregion

    #region Implicit Conversion - Single Error (Null Safety)

    [TestMethod]
    public void ImplicitConversion_FromError_CreatesFailedResult()
    {
        // Arrange
        var error = new Error("Something went wrong");

        // Act
        Result<int> result = error;

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual("Something went wrong", result.Errors[0].Message);
    }

    [TestMethod]
    public void ImplicitConversion_FromNullError_ThrowsArgumentNullException()
    {
        // Arrange
        Error error = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            Result<int> result = error;
        });
    }

    [TestMethod]
    public void ImplicitConversion_FromNullExceptionError_ThrowsArgumentNullException()
    {
        // Arrange
        ExceptionError error = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            Result<string> result = error;
        });
    }

    #endregion

    #region Implicit Conversion - Error Array (Null/Empty Safety)

    [TestMethod]
    public void ImplicitConversion_FromValidErrorArray_CreatesFailedResult()
    {
        // Arrange
        var errors = new[]
        {
            new Error("Error 1"),
            new Error("Error 2")
        };

        // Act
        Result<int> result = errors;

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(2, result.Errors);
        Assert.AreEqual("Error 1", result.Errors[0].Message);
        Assert.AreEqual("Error 2", result.Errors[1].Message);
    }

    [TestMethod]
    public void ImplicitConversion_FromNullErrorArray_ThrowsArgumentNullException()
    {
        // Arrange
        Error[] errors = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            Result<int> result = errors;
        });
    }

    [TestMethod]
    public void ImplicitConversion_FromEmptyErrorArray_CreatesConversionError()
    {
        // Arrange
        Error[] errors = Array.Empty<Error>();

        // Act
        Result<int> result = errors;

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        
        var conversionError = result.Errors[0] as ConversionError;
        Assert.IsNotNull(conversionError);
        Assert.Contains("Empty error array provided", conversionError.Message);
        Assert.AreEqual("Error[]", conversionError.Tags["ConversionType"]);
        Assert.AreEqual(0, conversionError.Tags["ArrayLength"]);
    }

    #endregion

    #region Implicit Conversion - Error List (Null/Empty Safety)

    [TestMethod]
    public void ImplicitConversion_FromValidErrorList_CreatesFailedResult()
    {
        // Arrange
        var errors = new List<Error>
        {
            new Error("List error 1"),
            new Error("List error 2")
        };

        // Act
        Result<int> result = errors;

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(2, result.Errors);
        Assert.AreEqual("List error 1", result.Errors[0].Message);
    }

    [TestMethod]
    public void ImplicitConversion_FromNullErrorList_ThrowsArgumentNullException()
    {
        // Arrange
        List<Error> errors = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            Result<string> result = errors;
        });
    }

    [TestMethod]
    public void ImplicitConversion_FromEmptyErrorList_CreatesConversionError()
    {
        // Arrange
        var errors = new List<Error>();

        // Act
        Result<int> result = errors;

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        
        var conversionError = result.Errors[0] as ConversionError;
        Assert.IsNotNull(conversionError);
        Assert.Contains("Conversion failed: Empty error list provided", conversionError.Message);
        Assert.AreEqual(0, conversionError.Tags["ListCount"]);
        Assert.AreEqual("List<Error>", conversionError.Tags["ConversionType"]);        
    }

    #endregion

    #region Implicit Conversion - ExceptionError Collections

    [TestMethod]
    public void ImplicitConversion_FromValidExceptionErrorArray_CreatesFailedResult()
    {
        // Arrange
        var errors = new[]
        {
            new ExceptionError(new InvalidOperationException("Ex 1")),
            new ExceptionError(new ArgumentException("Ex 2"))
        };

        // Act
        Result<int> result = errors;

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(2, result.Errors);
    }

    [TestMethod]
    public void ImplicitConversion_FromNullExceptionErrorArray_ThrowsArgumentNullException()
    {
        // Arrange
        ExceptionError[] errors = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            Result<int> result = errors;
        });
    }

    [TestMethod]
    public void ImplicitConversion_FromEmptyExceptionErrorArray_CreatesConversionError()
    {
        // Arrange
        var errors = Array.Empty<ExceptionError>();

        // Act
        Result<string> result = errors;

        // Assert
        Assert.IsTrue(result.IsFailed);
        var conversionError = result.Errors[0] as ConversionError;
        Assert.IsNotNull(conversionError);
        Assert.Contains("Empty exception error array provided", conversionError.Message);
    }

    [TestMethod]
    public void ImplicitConversion_FromNullExceptionErrorList_ThrowsArgumentNullException()
    {
        // Arrange
        List<ExceptionError> errors = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            Result<int> result = errors;
        });
    }

    [TestMethod]
    public void ImplicitConversion_FromEmptyExceptionErrorList_CreatesConversionError()
    {
        // Arrange
        var errors = new List<ExceptionError>();

        // Act
        Result<int> result = errors;

        // Assert
        Assert.IsTrue(result.IsFailed);
        var conversionError = result.Errors[0] as ConversionError;
        Assert.IsNotNull(conversionError);
        Assert.Contains("Empty exception error list provided", conversionError.Message);
    }

    #endregion

    #region ConversionError Specific Tests

    [TestMethod]
    public void ConversionError_HasCorrectDefaultTags()
    {
        // Act
        var error = new ConversionError("Test reason");

        // Assert
        Assert.Contains("Test reason", error.Message);
        Assert.AreEqual("Conversion", error.Tags["ErrorType"]);
        Assert.AreEqual("Warning", error.Tags["Severity"]);
        Assert.IsTrue(error.Tags.ContainsKey("Timestamp"));
    }

    [TestMethod]
    public void ConversionError_FluentAPI_WithConversionType_Works()
    {
        // Act
        var error = new ConversionError("Empty array")
            .WithConversionType("Error[]")
            .WithProvidedValue("0 items");

        // Assert
        Assert.AreEqual("Error[]", error.Tags["ConversionType"]);
        Assert.AreEqual("0 items", error.Tags["ProvidedValue"]);
    }

    [TestMethod]
    public void ConversionError_CanBeDetectedByType()
    {
        // Arrange: Use empty array to create ConversionError (not null)
        var errors = Array.Empty<Error>();
        Result<int> result = errors;

        // Act
        var isConversionError = result.Errors[0] is ConversionError;
        var conversionError = result.Errors[0] as ConversionError;

        // Assert
        Assert.IsTrue(isConversionError);
        Assert.IsNotNull(conversionError);
    }

    [TestMethod]
    public void ConversionError_InheritsFromIError()
    {
        // Act
        var error = new ConversionError("Test");

        // Assert
        //Assert.IsInstanceOfType<Error>(error);
        Assert.IsInstanceOfType<IError>(error);
        Assert.IsInstanceOfType<IReason>(error);
    }

    #endregion

    #region ToResult Conversions

    [TestMethod]
    public void ToResult_FromTypedToNonGeneric_DiscardsValue()
    {
        // Arrange
        var typedResult = Result<int>.Ok(42)
            .WithSuccess("Created");

        // Act
        Result baseResult = typedResult.ToResult();

        // Assert
        Assert.IsTrue(baseResult.IsSuccess);
        Assert.HasCount(1, baseResult.Successes);
        Assert.AreEqual("Created", baseResult.Successes[0].Message);
    }

    [TestMethod]
    public void ToResult_FromTypedToNonGeneric_PreservesErrors()
    {
        // Arrange
        var typedResult = Result<string>.Fail("Error occurred");

        // Act
        Result baseResult = typedResult.ToResult();

        // Assert
        Assert.IsTrue(baseResult.IsFailed);
        Assert.HasCount(1, baseResult.Errors);
        Assert.AreEqual("Error occurred", baseResult.Errors[0].Message);
    }

    [TestMethod]
    public void ToResult_FromNonGenericToTyped_WithValue_PreservesSuccesses()
    {
        // Arrange
        var baseResult = Result.Ok()
            .WithSuccess("Step 1")
            .WithSuccess("Step 2");

        // Act
        var typedResult = baseResult.ToResult(100);

        // Assert
        Assert.IsTrue(typedResult.IsSuccess);
        Assert.AreEqual(100, typedResult.Value);
        Assert.HasCount(2, typedResult.Successes);
    }

    [TestMethod]
    public void ToResult_FromNonGenericToTyped_WhenFailed_PreservesErrors()
    {
        // Arrange
        var baseResult = Result.Fail("Base error");

        // Act
        var typedResult = baseResult.ToResult(42);

        // Assert
        Assert.IsTrue(typedResult.IsFailed);
        Assert.HasCount(1, typedResult.Errors);
        Assert.AreEqual("Base error", typedResult.Errors[0].Message);
    }

    #endregion

    #region Real-World Scenarios

    [TestMethod]
    public void RealWorld_DynamicErrorCollection_HandlesEmptyGracefully()
    {
        // Arrange: Simulating dynamic error collection that might be empty
        var validationErrors = new List<Error>();
        
        // Simulate conditional validation
        // No validation errors in this test case

        // Act: Implicit conversion with empty list
        Result<User> result = validationErrors;

        // Assert: Should get ConversionError, not crash
        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ConversionError>(result.Errors[0]);
        Assert.Contains("Empty error list", result.Errors[0].Message);
    }

    [TestMethod]
    public void RealWorld_NullCheckPattern_ThrowsForNull()
    {
        // Arrange: Simulating method that might return null error
        Error error = GetErrorOrNull(shouldReturnNull: true);

        // Act & Assert: Should throw for null input (fail-fast)
        Assert.Throws<ArgumentNullException>(() =>
        {
            Result<string> result = error;
        });
    }

    [TestMethod]
    public void RealWorld_FactoryMethod_StillValidatesAndThrows()
    {
        // Arrange
        var emptyList = new List<IError>();

        // Act & Assert: Factory methods SHOULD throw for programmer errors
        Assert.Throws<ArgumentException>(() => 
            Result<int>.Fail(emptyList));
    }

    [TestMethod]
    public void RealWorld_ImplicitVsExplicit_Comparison()
    {
        // Scenario 1: Implicit conversion (now throws for null)
        Error[] nullArray = null!;
        Assert.Throws<ArgumentNullException>(() => 
        {
            Result<int> implicitResult = nullArray; // ✅ Now throws - fail fast
        });

        // Scenario 2: Explicit factory call (user sees it)
        List<IError> emptyList = new();
        Assert.Throws<ArgumentException>(() => 
            Result<int>.Fail(emptyList)); // ✅ Throws - programmer error
    }

    [TestMethod]
    public void RealWorld_ConversionErrorDetection_ForMonitoring()
    {
        // Arrange: Simulate a batch operation that collects errors
        var errors = CollectErrorsFromBatchOperation(shouldFail: false);
        
        // Act
        Result<int> result = errors;

        // Assert: Can detect conversion issues for monitoring
        if (result.IsFailed && result.Errors.Any(e => e is ConversionError))
        {
            var conversionError = result.Errors.OfType<ConversionError>().First();
            
            // In production: Send to monitoring/alerting
            Assert.Contains("Empty error array", conversionError.Message);
            
            // Can get diagnostic information
            var severity = conversionError.Tags["Severity"];
            Assert.AreEqual("Warning", severity);
        }
    }

    #endregion

    #region Helper Methods

    private Error GetErrorOrNull(bool shouldReturnNull)
    {
        return shouldReturnNull ? null! : new Error("Actual error");
    }

    private Error[] CollectErrorsFromBatchOperation(bool shouldFail)
    {
        if (shouldFail)
        {
            return new[] { new Error("Batch failed") };
        }
        return Array.Empty<Error>(); // Empty when no errors
    }

    private class User
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    #endregion
}
