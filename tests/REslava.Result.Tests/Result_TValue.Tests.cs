namespace REslava.Result.Tests;

/// <summary>
/// Comprehensive tests for the Result<TValue> class (generic)
/// </summary>
[TestClass]
public sealed class ResultTValueTests
{
    #region Factory Methods - Ok

    [TestMethod]
    public void Ok_WithValue_CreatesSuccessResult()
    {
        // Act
        var result = Result<int>.Ok(42);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.IsFailed);
        Assert.AreEqual(42, result.Value);
        Assert.AreEqual(42, result.ValueOrDefault);
    }

    [TestMethod]
    public void Ok_WithNullValue_CreatesSuccessResult()
    {
        // Act
        var result = Result<string>.Ok(null!);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.Value);
        Assert.IsNull(result.ValueOrDefault);
    }

    [TestMethod]
    public void Ok_WithComplexType_CreatesSuccessResult()
    {
        // Arrange
        var person = new Person { Name = "John", Age = 30 };

        // Act
        var result = Result<Person>.Ok(person);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("John", result.Value.Name);
        Assert.AreEqual(30, result.Value.Age);
    }

    #endregion

    #region Factory Methods - Fail

    [TestMethod]
    public void Fail_WithString_CreatesFailedResult()
    {
        // Act
        var result = Result<int>.Fail("Error occurred");

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.AreEqual("Error occurred", result.Errors[0].Message);
    }

    [TestMethod]
    public void Fail_WithError_CreatesFailedResult()
    {
        // Arrange
        var error = new Error("Custom error");

        // Act
        var result = Result<int>.Fail(error);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual(1, result.Errors.Count);
    }

    [TestMethod]
    public void Fail_WithMessages_CreatesFailedResultWithMultipleErrors()
    {
        // Arrange
        var messages = new[] { "Error 1", "Error 2" };

        // Act
        var result = Result<int>.Fail(messages);

        // Assert
        Assert.AreEqual(2, result.Errors.Count);
    }

    [TestMethod]
    public void Fail_WithErrors_CreatesFailedResultWithMultipleErrors()
    {
        // Arrange
        var errors = new[] { new Error("E1"), new Error("E2") };

        // Act
        var result = Result<int>.Fail(errors);

        // Assert
        Assert.AreEqual(2, result.Errors.Count);
    }

    #endregion

    #region Factory Methods - From

    [TestMethod]
    public void From_WithFailedResult_ConvertsToTypedResult()
    {
        // Arrange
        var baseResult = Result.Fail("Validation error");

        // Act
        var typedResult = Result<int>.From(baseResult);

        // Assert
        Assert.IsTrue(typedResult.IsFailed);
        Assert.AreEqual(1, typedResult.Errors.Count);
        Assert.AreEqual("Validation error", typedResult.Errors[0].Message);
    }

    [TestMethod]
    public void From_WithSuccessResult_ThrowsInvalidOperationException()
    {
        // Arrange
        var baseResult = Result.Ok();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => Result<int>.From(baseResult));
    }

    [TestMethod]
    public void From_WithNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Result<int>.From(null!));
    }

    #endregion

    #region Value Property

    [TestMethod]
    public void Value_OnSuccess_ReturnsValue()
    {
        // Arrange
        var result = Result<int>.Ok(100);

        // Act
        var value = result.Value;

        // Assert
        Assert.AreEqual(100, value);
    }

    [TestMethod]
    public void Value_OnFailure_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result<int>.Fail("Error");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [TestMethod]
    public void ValueOrDefault_OnSuccess_ReturnsValue()
    {
        // Arrange
        var result = Result<int>.Ok(50);

        // Act
        var value = result.ValueOrDefault;

        // Assert
        Assert.AreEqual(50, value);
    }

    [TestMethod]
    public void ValueOrDefault_OnFailure_ReturnsDefault()
    {
        // Arrange
        var result = Result<int>.Fail("Error");

        // Act
        var value = result.ValueOrDefault;

        // Assert
        Assert.AreEqual(default(int), value);
    }

    #endregion

    #region Fluent Methods - With

    [TestMethod]
    public void WithSuccess_String_AddsSuccessAndReturnsTypedResult()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act
        var newResult = result.WithSuccess("Operation completed");

        // Assert
        Assert.IsInstanceOfType<Result<int>>(newResult);
        Assert.AreEqual(1, newResult.Successes.Count);
        Assert.AreEqual(10, newResult.Value);
    }

    [TestMethod]
    public void WithError_String_AddsErrorAndReturnsTypedResult()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act
        var newResult = result.WithError("Validation warning");

        // Assert
        Assert.IsInstanceOfType<Result<int>>(newResult);
        Assert.IsTrue(newResult.IsFailed);
        Assert.AreEqual(1, newResult.Errors.Count);
    }

    [TestMethod]
    public void FluentChaining_MaintainsType()
    {
        // Act
        var result = Result<string>.Ok("test")
            .WithSuccess("Step 1")
            .WithSuccess("Step 2")
            .WithError("Warning");

        // Assert
        Assert.IsInstanceOfType<Result<string>>(result);
        Assert.AreEqual("test", result.ValueOrDefault);
        Assert.AreEqual(2, result.Successes.Count);
        Assert.AreEqual(1, result.Errors.Count);
    }

    #endregion

    #region Match Methods

    [TestMethod]
    public void Match_OnSuccess_ExecutesOnSuccessWithValue()
    {
        // Arrange
        var result = Result<int>.Ok(25);
        var capturedValue = 0;

        // Act
        var output = result.Match(
            onSuccess: value => { capturedValue = value; return value * 2; },
            onFailure: errors => 0
        );

        // Assert
        Assert.AreEqual(25, capturedValue);
        Assert.AreEqual(50, output);
    }

    [TestMethod]
    public void Match_OnFailure_ExecutesOnFailureWithErrors()
    {
        // Arrange
        var result = Result<int>.Fail("Error");
        var errorCount = 0;

        // Act
        var output = result.Match(
            onSuccess: value => value,
            onFailure: errors => { errorCount = errors.Count; return -1; }
        );

        // Assert
        Assert.AreEqual(1, errorCount);
        Assert.AreEqual(-1, output);
    }

    [TestMethod]
    public void Match_Action_OnSuccess_ExecutesWithValue()
    {
        // Arrange
        var result = Result<string>.Ok("Hello");
        var capturedValue = "";

        // Act
        result.Match(
            onSuccess: value => capturedValue = value,
            onFailure: errors => { }
        );

        // Assert
        Assert.AreEqual("Hello", capturedValue);
    }

    [TestMethod]
    public void Match_Action_OnFailure_ExecutesWithErrors()
    {
        // Arrange
        var result = Result<int>.Fail("Error");
        var errorMessages = new List<string>();

        // Act
        result.Match(
            onSuccess: value => { },
            onFailure: errors => errorMessages.AddRange(errors.Select(e => e.Message))
        );

        // Assert
        Assert.AreEqual(1, errorMessages.Count);
        Assert.AreEqual("Error", errorMessages[0]);
    }

    [TestMethod]
    public void Match_WithNullOnSuccess_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.Match<int>(null!, errors => 0));
    }

    [TestMethod]
    public void Match_WithNullOnFailure_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.Match(value => value, null!));
    }

    #endregion

    #region Map Methods

    [TestMethod]
    public void Map_OnSuccess_TransformsValue()
    {
        // Arrange
        var result = Result<int>.Ok(7);

        // Act
        var mapped = result.Map(x => x * 2);

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.AreEqual(14, mapped.Value);
    }

    [TestMethod]
    public void Map_OnSuccess_ChangesType()
    {
        // Arrange
        var result = Result<int>.Ok(42);

        // Act
        var mapped = result.Map(x => x.ToString());

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.AreEqual("42", mapped.Value);
        Assert.IsInstanceOfType<Result<string>>(mapped);
    }

    [TestMethod]
    public void Map_OnFailure_ReturnsSameErrors()
    {
        // Arrange
        var result = Result<int>.Fail("Original error");

        // Act
        var mapped = result.Map(x => x.ToString());

        // Assert
        Assert.IsTrue(mapped.IsFailed);
        Assert.AreEqual(1, mapped.Errors.Count);
        Assert.AreEqual("Original error", mapped.Errors[0].Message);
    }

    [TestMethod]
    public void Map_WithNullMapper_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result.Map<string>(null!));
    }

    [TestMethod]
    public void Map_WhenMapperThrows_ReturnsFailedResult()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act
        var mapped = result.Map<string>(x => throw new InvalidOperationException("Mapper failed"));

        // Assert
        Assert.IsTrue(mapped.IsFailed);
        Assert.IsTrue(mapped.Errors.Any(e => e.Message.Contains("Exception")));
        Assert.IsTrue(mapped.Errors.Any(e => e.Message.Contains("Mapper failed")));
    }

    [TestMethod]
    public void Map_ComplexTransformation_Works()
    {
        // Arrange
        var result = Result<Person>.Ok(new Person { Name = "John", Age = 30 });

        // Act
        var mapped = result.Map(p => $"{p.Name} ({p.Age})");

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.AreEqual("John (30)", mapped.Value);
    }

    [TestMethod]
    public void Map_Chaining_Works()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var mapped = result
            .Map(x => x * 2)
            .Map(x => x + 10)
            .Map(x => x.ToString());

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.AreEqual("20", mapped.Value);
    }

    #endregion

    #region Bind Methods

    [TestMethod]
    public void Bind_OnSuccess_ExecutesBinderFunction()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act
        var bound = result.Bind(x => Result<string>.Ok((x * 2).ToString()));

        // Assert
        Assert.IsTrue(bound.IsSuccess);
        Assert.AreEqual("20", bound.Value);
    }

    [TestMethod]
    public void Bind_OnSuccess_ChangesType()
    {
        // Arrange
        var result = Result<string>.Ok("5");

        // Act
        var bound = result.Bind(s => Result<int>.Ok(int.Parse(s)));

        // Assert
        Assert.IsTrue(bound.IsSuccess);
        Assert.AreEqual(5, bound.Value);
        Assert.IsInstanceOfType<Result<int>>(bound);
    }

    [TestMethod]
    public void Bind_OnFailure_ReturnsOriginalErrors()
    {
        // Arrange
        var result = Result<int>.Fail("Original error");

        // Act
        var bound = result.Bind(x => Result<string>.Ok(x.ToString()));

        // Assert
        Assert.IsTrue(bound.IsFailed);        
        Assert.HasCount(1, bound.Errors);
        Assert.AreEqual("Original error", bound.Errors[0].Message);
    }

    [TestMethod]
    public void Bind_BinderReturnsFailed_ReturnsNewErrors()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act
        var bound = result.Bind(x => Result<string>.Fail("Binder failed"));

        // Assert
        Assert.IsTrue(bound.IsFailed);
        Assert.HasCount(1, bound.Errors);
        Assert.AreEqual("Binder failed", bound.Errors[0].Message);
    }

    [TestMethod]
    public void Bind_WithSuccessReasons_PreservesOriginalSuccesses()
    {
        // Arrange
        var result = Result<int>.Ok(10).WithSuccess("Step 1 done");

        // Act
        var bound = result.Bind(x => Result<string>.Ok((x * 2).ToString()).WithSuccess("Step 2 done"));

        // Assert
        Assert.IsTrue(bound.IsSuccess);
        Assert.AreEqual(2, bound.Successes.Count);
        Assert.IsTrue(bound.Successes.Any(s => s.Message == "Step 1 done"));
        Assert.IsTrue(bound.Successes.Any(s => s.Message == "Step 2 done"));
    }

    [TestMethod]
    public void Bind_WithNullBinder_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result.Bind<string>(null!));
    }

    [TestMethod]
    public void Bind_WhenBinderThrows_ReturnsFailedResult()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act
        var bound = result.Bind<string>(x => throw new InvalidOperationException("Binder failed"));

        // Assert
        Assert.IsTrue(bound.IsFailed);
        Assert.IsTrue(bound.Errors.Any(e => e.Message.Contains("Exception")));
    }

    [TestMethod]
    public void Bind_Chaining_Works()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var bound = result
            .Bind(x => Result<int>.Ok(x * 2))
            .Bind(x => Result<int>.Ok(x + 10))
            .Bind(x => Result<string>.Ok(x.ToString()));

        // Assert
        Assert.IsTrue(bound.IsSuccess);
        Assert.AreEqual("20", bound.Value);
    }

    [TestMethod]
    public void Bind_ComplexWorkflow_Works()
    {
        // Arrange
        var result = Result<string>.Ok("42");

        // Act
        var bound = result
            .Bind(s => int.TryParse(s, out var num)
                ? Result<int>.Ok(num)
                : Result<int>.Fail("Invalid number"))
            .Bind(n => n > 0
                ? Result<int>.Ok(n * 2)
                : Result<int>.Fail("Number must be positive"))
            .Map(n => $"Result: {n}");

        // Assert
        Assert.IsTrue(bound.IsSuccess);
        Assert.AreEqual("Result: 84", bound.Value);
    }

    #endregion

    #region ToString

    [TestMethod]
    public void ToString_OnSuccess_ShowsValueAndStatus()
    {
        // Arrange
        var result = Result<int>.Ok(42);

        // Act
        var str = result.ToString();

        // Assert        
        Assert.Contains("IsSuccess='True'", str);
        Assert.Contains("Value = 42", str);
    }

    [TestMethod]
    public void ToString_OnFailure_ShowsErrorAndStatus()
    {
        // Arrange
        var result = Result<int>.Fail("Test error");

        // Act
        var str = result.ToString();

        // Assert
        Assert.IsTrue(str.Contains("IsSuccess='False'"));
        Assert.IsTrue(str.Contains("Test error"));
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void Integration_CompleteWorkflow_Success()
    {
        // Arrange & Act
        // Note: Map creates a new Result without preserving success reasons from the original
        // Only Bind preserves original success reasons when the new result is successful
        var result = Result<int>.Ok(10)
            .WithSuccess("Created")
            .Bind(x => Result<int>.Ok(x * 2).WithSuccess("Doubled"))
            .Bind(x => Result<string>.Ok($"Value: {x}").WithSuccess("Formatted"));

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Value: 20", result.Value);
        // Bind preserves successes: "Created" from original + "Doubled" from first Bind + "Formatted" from second Bind
        Assert.AreEqual(3, result.Successes.Count);
        Assert.IsTrue(result.Successes.Any(s => s.Message == "Created"));
        Assert.IsTrue(result.Successes.Any(s => s.Message == "Doubled"));
        Assert.IsTrue(result.Successes.Any(s => s.Message == "Formatted"));
    }

    [TestMethod]
    public void Integration_CompleteWorkflow_Failure()
    {
        // Arrange & Act
        var result = Result<int>.Ok(10)
            .WithSuccess("Created")
            .Bind(x => Result<int>.Fail("Processing failed"))
            .Map(x => x * 2);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.AreEqual("Processing failed", result.Errors[0].Message);
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
