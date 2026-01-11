namespace REslava.Result.Tests;

/// <summary>
/// Comprehensive tests for Result_TValue implicit conversion operators
/// </summary>
[TestClass]
public sealed class ResultConversionsTests
{
    #region Implicit Conversion - From TValue

    [TestMethod]
    public void ImplicitConversion_FromInt_CreatesSuccessResult()
    {
        // Act
        Result<int> result = 42;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.IsFailed);
        Assert.AreEqual(42, result.Value);
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void ImplicitConversion_FromString_CreatesSuccessResult()
    {
        // Act
        Result<string> result = "Hello World";

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Hello World", result.Value);
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void ImplicitConversion_FromNull_CreatesSuccessResultWithNull()
    {
        // Act
        Result<string> result = (string)null!;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.Value);
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void ImplicitConversion_FromComplexType_CreatesSuccessResult()
    {
        // Arrange
        var person = new Person { Name = "Alice", Age = 25 };

        // Act
        Result<Person> result = person;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Alice", result.Value!.Name);
        Assert.AreEqual(25, result.Value.Age);
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void ImplicitConversion_FromValueType_CreatesSuccessResult()
    {
        // Act
        Result<bool> result = true;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Value);
    }

    [TestMethod]
    public void ImplicitConversion_FromDouble_CreatesSuccessResult()
    {
        // Act
        Result<double> result = 3.14159;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(3.14159, result.Value);
    }

    #endregion

    #region Implicit Conversion - From Error

    [TestMethod]
    public void ImplicitConversion_FromError_CreatesFailedResult()
    {
        // Arrange
        var error = new Error("Something went wrong");

        // Act
        Result<int> result = error;

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual("Something went wrong", result.Errors[0].Message);
    }

    [TestMethod]
    public void ImplicitConversion_FromErrorWithMetadata_PreservesMetadata()
    {
        // Arrange
        var error = new Error("Validation failed")
            .WithTags("Field", "Email")
            .WithTags("Code", 400);

        // Act
        Result<string> result = error;

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual("Validation failed", result.Errors[0].Message);
        Assert.Contains("Field", result.Errors[0].Tags.Keys);
        Assert.Contains("Code", result.Errors[0].Tags.Keys);
        Assert.AreEqual("Email", result.Errors[0].Tags["Field"]);
        Assert.AreEqual(400, result.Errors[0].Tags["Code"]);
    }     

    #endregion

    #region Implicit Conversion - From Error Array

    [TestMethod]
    public void ImplicitConversion_FromErrorArray_CreatesFailedResultWithMultipleErrors()
    {
        // Arrange
        var errors = new[]
        {
            new Error("Error 1"),
            new Error("Error 2"),
            new Error("Error 3")
        };

        // Act
        Result<int> result = errors;

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(3, result.Errors);
        Assert.AreEqual("Error 1", result.Errors[0].Message);
        Assert.AreEqual("Error 2", result.Errors[1].Message);
        Assert.AreEqual("Error 3", result.Errors[2].Message);
    }

    [TestMethod]
    public void ImplicitConversion_FromEmptyErrorArray_CreatesFailedResult()
    {
        // Arrange
        var errors = Array.Empty<Error>();

        // Act & Assert
        // Note: This will throw because Fail(errors) validates non-empty
        Assert.Throws<ArgumentException>(() =>
        {
            Result<int> result = errors;
        });
    }

    [TestMethod]
    public void ImplicitConversion_FromSingleElementErrorArray_CreatesFailedResult()
    {
        // Arrange
        var errors = new[] { new Error("Single error") };

        // Act
        Result<string> result = errors;

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual("Single error", result.Errors[0].Message);
    }    

    #endregion

    #region Implicit Conversion - From Error List

    [TestMethod]
    public void ImplicitConversion_FromErrorList_CreatesFailedResultWithMultipleErrors()
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
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(2, result.Errors);
        Assert.AreEqual("List error 1", result.Errors[0].Message);
        Assert.AreEqual("List error 2", result.Errors[1].Message);
    }

    [TestMethod]
    public void ImplicitConversion_FromEmptyErrorList_ThrowsArgumentException()
    {
        // Arrange
        var errors = new List<Error>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            Result<int> result = errors;
        });
    }

    [TestMethod]
    public void ImplicitConversion_FromLargeErrorList_CreatesFailedResult()
    {
        // Arrange
        var errors = Enumerable.Range(1, 10)
            .Select(i => new Error($"Error {i}"))
            .ToList();

        // Act
        Result<string> result = errors;

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(10, result.Errors);
        Assert.AreEqual("Error 1", result.Errors[0].Message);
        Assert.AreEqual("Error 10", result.Errors[9].Message);
    }

    #endregion

    #region Method Return Type Conversions

    [TestMethod]
    public void MethodReturn_ImplicitConversionFromValue_Works()
    {
        // Act
        var result = GetSuccessfulResult();

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(100, result.Value);
    }

    [TestMethod]
    public void MethodReturn_ImplicitConversionFromError_Works()
    {
        // Act
        var result = GetFailedResult();

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual("Operation failed", result.Errors[0].Message);
    }

    [TestMethod]
    public void MethodReturn_ImplicitConversionFromErrorArray_Works()
    {
        // Act
        var result = GetMultipleErrorsResult();

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(2, result.Errors);
    }

    [TestMethod]
    public void MethodReturn_ConditionalConversion_Works()
    {
        // Act
        var successResult = ValidateAge(25);
        var failureResult = ValidateAge(15);

        // Assert
        Assert.IsTrue(successResult.IsSuccess);
        Assert.AreEqual(25, successResult.Value);

        Assert.IsTrue(failureResult.IsFailed);
        Assert.Contains("must be 18 or older", failureResult.Errors[0].Message);
    }

    #endregion

    #region Assignment and Variable Conversions

    [TestMethod]
    public void VariableAssignment_ImplicitConversionFromValue_Works()
    {
        // Arrange
        Result<int> result;

        // Act
        result = 42;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
    }

    [TestMethod]
    public void VariableAssignment_ImplicitConversionFromError_Works()
    {
        // Arrange
        Result<string> result;

        // Act
        result = new Error("Assignment error");

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Assignment error", result.Errors[0].Message);
    }

    [TestMethod]
    public void CollectionInitialization_ImplicitConversions_Work()
    {
        // Act
        var results = new List<Result<int>>
        {
            42,
            new Error("Error 1"),
            100,
            new Error("Error 2")
        };

        // Assert
        Assert.HasCount(4, results);
        Assert.IsTrue(results[0].IsSuccess);
        Assert.AreEqual(42, results[0].Value);
        Assert.IsTrue(results[1].IsFailed);
        Assert.IsTrue(results[2].IsSuccess);
        Assert.AreEqual(100, results[2].Value);
        Assert.IsTrue(results[3].IsFailed);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void ImplicitConversion_FromZero_CreatesSuccessResult()
    {
        // Act
        Result<int> result = 0;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Value);
    }

    [TestMethod]
    public void ImplicitConversion_FromEmptyString_CreatesSuccessResult()
    {
        // Act
        Result<string> result = "";

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("", result.Value);
    }

    [TestMethod]
    public void ImplicitConversion_FromDefaultStruct_CreatesSuccessResult()
    {
        // Act
        Result<DateTime> result = default(DateTime);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(default(DateTime), result.Value);
    }

    [TestMethod]
    public void ImplicitConversion_ChainedAssignment_Works()
    {
        // Act
        Result<int> result1, result2, result3;
        result1 = result2 = result3 = 99;

        // Assert
        Assert.IsTrue(result1.IsSuccess);
        Assert.IsTrue(result2.IsSuccess);
        Assert.IsTrue(result3.IsSuccess);
        Assert.AreEqual(99, result1.Value);
        Assert.AreEqual(99, result2.Value);
        Assert.AreEqual(99, result3.Value);
    }

    #endregion

    #region Integration with Other Features

    [TestMethod]
    public void ImplicitConversion_WithMap_Works()
    {
        // Arrange
        Result<int> result = 10;

        // Act
        var mapped = result.Map(x => x * 2);

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.AreEqual(20, mapped.Value);
    }

    [TestMethod]
    public void ImplicitConversion_WithBind_Works()
    {
        // Arrange
        Result<int> result = 5;

        // Act
        var bound = result.Bind(x => Result<string>.Ok(x.ToString()));

        // Assert
        Assert.IsTrue(bound.IsSuccess);
        Assert.AreEqual("5", bound.Value);
    }

    [TestMethod]
    public void ImplicitConversion_WithMatch_Works()
    {
        // Arrange
        Result<int> successResult = 42;
        Result<int> failureResult = new Error("Failed");

        // Act
        var successOutput = successResult.Match(
            onSuccess: v => $"Success: {v}",
            onFailure: e => "Failed"
        );

        var failureOutput = failureResult.Match(
            onSuccess: v => $"Success: {v}",
            onFailure: e => "Failed"
        );

        // Assert
        Assert.AreEqual("Success: 42", successOutput);
        Assert.AreEqual("Failed", failureOutput);
    }

    [TestMethod]
    public void ImplicitConversion_WithFluentMethods_Works()
    {
        // Arrange
        Result<int> result = 50;

        // Act
        var enhanced = result
            .WithSuccess("Created successfully")
            .WithSuccess("Validated");

        // Assert
        Assert.IsTrue(enhanced.IsSuccess);
        Assert.AreEqual(50, enhanced.Value);
        Assert.HasCount(2, enhanced.Successes);
    }

    #endregion

    #region Helper Methods

    private Result<int> GetSuccessfulResult()
    {
        return 100; // Implicit conversion from int
    }

    private Result<string> GetFailedResult()
    {
        return new Error("Operation failed"); // Implicit conversion from Error
    }

    private Result<int> GetMultipleErrorsResult()
    {
        return new[]
        {
            new Error("Error 1"),
            new Error("Error 2")
        }; // Implicit conversion from Error[]
    }

    private Result<int> ValidateAge(int age)
    {
        if (age < 18)
        {
            return new Error("Age must be 18 or older"); // Implicit conversion
        }

        return age; // Implicit conversion from int
    }

    #endregion

    #region Helper Classes

    private class Person
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    #endregion
}
