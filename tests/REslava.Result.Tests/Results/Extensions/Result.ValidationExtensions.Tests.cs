namespace REslava.Result.Tests;

/// <summary>
/// Comprehensive tests for Result validation extension methods (Ensure, EnsureNotNull)
/// </summary>
[TestClass]
public sealed class ResultValidationExtensionsTests
{
    #region Ensure Tests - Single Predicate with Error Object

    [TestMethod]
    public void Ensure_WithError_PredicateTrue_ReturnsOriginalResult()
    {
        // Arrange
        var result = Result<int>.Ok(10);
        var error = new Error("Value must be positive");

        // Act
        var validated = result.Ensure(v => v > 0, error);

        // Assert
        Assert.IsTrue(validated.IsSuccess);
        Assert.AreEqual(10, validated.Value);
        Assert.IsEmpty(validated.Errors);
    }

    [TestMethod]
    public void Ensure_WithError_PredicateFalse_ReturnsFailedResult()
    {
        // Arrange
        var result = Result<int>.Ok(-5);
        var error = new Error("Value must be positive");

        // Act
        var validated = result.Ensure(v => v > 0, error);

        // Assert
        Assert.IsTrue(validated.IsFailed);
        Assert.HasCount(1, validated.Errors);
        Assert.AreEqual("Value must be positive", validated.Errors[0].Message);
    }

    [TestMethod]
    public void Ensure_WithError_OnFailedResult_ReturnsOriginalErrors()
    {
        // Arrange
        var result = Result<int>.Fail("Original error");
        var error = new Error("Additional validation");

        // Act
        var validated = result.Ensure(v => v > 0, error);

        // Assert
        Assert.IsTrue(validated.IsFailed);
        Assert.HasCount(1, validated.Errors);
        Assert.AreEqual("Original error", validated.Errors[0].Message);
    }

    [TestMethod]
    public void Ensure_WithError_NullPredicate_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int>.Ok(10);
        var error = new Error("Test");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.Ensure(null!, error)
        );
    }

    [TestMethod]
    public void Ensure_WithError_NullError_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.Ensure(v => v > 0, (Error)null!)
        );
    }

    #endregion

    #region Ensure Tests - Single Predicate with String Message

    [TestMethod]
    public void Ensure_WithString_PredicateTrue_ReturnsOriginalResult()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act
        var validated = result.Ensure(v => v > 0, "Value must be positive");

        // Assert
        Assert.IsTrue(validated.IsSuccess);
        Assert.AreEqual(10, validated.Value);
    }

    [TestMethod]
    public void Ensure_WithString_PredicateFalse_ReturnsFailedResult()
    {
        // Arrange
        var result = Result<int>.Ok(-5);

        // Act
        var validated = result.Ensure(v => v > 0, "Value must be positive");

        // Assert
        Assert.IsTrue(validated.IsFailed);
        Assert.HasCount(1, validated.Errors);
        Assert.AreEqual("Value must be positive", validated.Errors[0].Message);
    }

    [TestMethod]
    public void Ensure_WithString_OnFailedResult_ReturnsOriginalErrors()
    {
        // Arrange
        var result = Result<int>.Fail("Original error");

        // Act
        var validated = result.Ensure(v => v > 0, "Additional validation");

        // Assert
        Assert.IsTrue(validated.IsFailed);
        Assert.HasCount(1, validated.Errors);
        Assert.AreEqual("Original error", validated.Errors[0].Message);
    }

    [TestMethod]
    public void Ensure_WithString_NullPredicate_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.Ensure(null!, "Test message")
        );
    }

    #endregion

    #region Ensure Tests - Multiple Validations

    [TestMethod]
    public void Ensure_MultipleValidations_AllPass_ReturnsOriginalResult()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act
        var validated = result.Ensure(
            (v => v > 0, new Error("Must be positive")),
            (v => v < 100, new Error("Must be less than 100")),
            (v => v % 2 == 0, new Error("Must be even"))
        );

        // Assert
        Assert.IsTrue(validated.IsSuccess);
        Assert.AreEqual(10, validated.Value);
        Assert.IsEmpty(validated.Errors);
    }

    [TestMethod]
    public void Ensure_MultipleValidations_OneFails_ReturnsFailedResult()
    {
        // Arrange
        var result = Result<int>.Ok(150);

        // Act
        var validated = result.Ensure(
            (v => v > 0, new Error("Must be positive")),
            (v => v < 100, new Error("Must be less than 100")),
            (v => v % 2 == 0, new Error("Must be even"))
        );

        // Assert
        Assert.IsTrue(validated.IsFailed);
        Assert.HasCount(1, validated.Errors);
        Assert.AreEqual("Must be less than 100", validated.Errors[0].Message);
    }

    [TestMethod]
    public void Ensure_MultipleValidations_MultipleFail_ReturnsAllErrors()
    {
        // Arrange
        var result = Result<int>.Ok(-5);

        // Act
        var validated = result.Ensure(
            (v => v > 0, new Error("Must be positive")),
            (v => v < 100, new Error("Must be less than 100")),
            (v => v % 2 == 0, new Error("Must be even"))
        );

        // Assert
        Assert.IsTrue(validated.IsFailed);
        Assert.HasCount(2, validated.Errors);
        Assert.IsTrue(validated.Errors.Any(e => e.Message == "Must be positive"));
        Assert.IsTrue(validated.Errors.Any(e => e.Message == "Must be even"));
    }

    [TestMethod]
    public void Ensure_MultipleValidations_OnFailedResult_ReturnsOriginalErrors()
    {
        // Arrange
        var result = Result<int>.Fail("Original error");

        // Act
        var validated = result.Ensure(
            (v => v > 0, new Error("Must be positive")),
            (v => v < 100, new Error("Must be less than 100"))
        );

        // Assert
        Assert.IsTrue(validated.IsFailed);
        Assert.HasCount(1, validated.Errors);
        Assert.AreEqual("Original error", validated.Errors[0].Message);
    }

    [TestMethod]
    public void Ensure_MultipleValidations_NullValidations_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.Ensure(null!)
        );
    }

    [TestMethod]
    public void Ensure_MultipleValidations_EmptyArray_ThrowsArgumentException()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            result.Ensure(Array.Empty<(Func<int, bool>, Error)>())
        );
    }

    #endregion

    #region EnsureNotNull Tests

    [TestMethod]
    public void EnsureNotNull_WithNonNullValue_ReturnsOriginalResult()
    {
        // Arrange
        var result = Result<string>.Ok("test");

        // Act
        var validated = result.EnsureNotNull("Value cannot be null");

        // Assert
        Assert.IsTrue(validated.IsSuccess);
        Assert.AreEqual("test", validated.Value);
    }

    [TestMethod]
    public void EnsureNotNull_WithNullValue_ReturnsFailedResult()
    {
        // Arrange
        var result = Result<string>.Ok(null!);

        // Act
        var validated = result.EnsureNotNull("Value cannot be null");

        // Assert
        Assert.IsTrue(validated.IsFailed);
        Assert.HasCount(1, validated.Errors);
        Assert.AreEqual("Value cannot be null", validated.Errors[0].Message);
    }

    [TestMethod]
    public void EnsureNotNull_WithNullErrorMessage_UsesDefaultMessage()
    {
        // Arrange
        var result = Result<string>.Ok(null!);

        // Act
        var validated = result.EnsureNotNull(null!);

        // Assert
        Assert.IsTrue(validated.IsFailed);
        Assert.AreEqual("Value can not be null", validated.Errors[0].Message);
    }

    [TestMethod]
    public void EnsureNotNull_OnFailedResult_ReturnsOriginalErrors()
    {
        // Arrange
        var result = Result<string>.Fail("Original error");

        // Act
        var validated = result.EnsureNotNull("Value cannot be null");

        // Assert
        Assert.IsTrue(validated.IsFailed);
        Assert.HasCount(1, validated.Errors);
        Assert.AreEqual("Original error", validated.Errors[0].Message);
    }

    [TestMethod]
    public void EnsureNotNull_WithComplexType_Works()
    {
        // Arrange
        var person = new Person { Name = "John", Age = 30 };
        var result = Result<Person>.Ok(person);

        // Act
        var validated = result.EnsureNotNull("Person cannot be null");

        // Assert
        Assert.IsTrue(validated.IsSuccess);
        Assert.AreEqual("John", validated.Value!.Name);
    }

    #endregion

    #region Chaining Tests

    [TestMethod]
    public void Ensure_ChainedWithMap_Works()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act
        var final = result
            .Ensure(v => v > 0, "Must be positive")
            .Map(v => v * 2)
            .Ensure(v => v < 100, "Result must be less than 100");

        // Assert
        Assert.IsTrue(final.IsSuccess);
        Assert.AreEqual(20, final.Value);
    }

    [TestMethod]
    public void Ensure_ChainedWithBind_Works()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act
        var final = result
            .Ensure(v => v > 0, "Must be positive")
            .Bind(v => Result<string>.Ok(v.ToString()))
            .Ensure(v => v.Length > 0, "String cannot be empty");

        // Assert
        Assert.IsTrue(final.IsSuccess);
        Assert.AreEqual("10", final.Value);
    }

    [TestMethod]
    public void Ensure_MultipleChained_StopsAtFirstFailure()
    {
        // Arrange
        var result = Result<int>.Ok(150);

        // Act
        var final = result
            .Ensure(v => v > 0, "Must be positive")          // Passes
            .Ensure(v => v < 100, "Must be less than 100")   // Fails here
            .Ensure(v => v % 2 == 0, "Must be even");        // Not evaluated

        // Assert
        Assert.IsTrue(final.IsFailed);
        Assert.HasCount(1, final.Errors);
        Assert.AreEqual("Must be less than 100", final.Errors[0].Message);
    }

    [TestMethod]
    public void EnsureNotNull_ChainedWithEnsure_Works()
    {
        // Arrange
        var result = Result<string>.Ok("test");

        // Act
        var final = result
            .EnsureNotNull("Cannot be null")
            .Ensure(v => v.Length > 0, "Cannot be empty")
            .Ensure(v => v.Length < 100, "Too long");

        // Assert
        Assert.IsTrue(final.IsSuccess);
        Assert.AreEqual("test", final.Value);
    }

    #endregion

    #region Real-World Scenarios

    [TestMethod]
    public void Ensure_EmailValidation_Scenario()
    {
        // Arrange
        var result = Result<string>.Ok("test@example.com");

        // Act
        var validated = result.Ensure(
            (v => !string.IsNullOrWhiteSpace(v), new Error("Email cannot be empty")),
            (v => v.Contains("@"), new Error("Email must contain @")),
            (v => v.Length <= 100, new Error("Email too long")),
            (v => !v.StartsWith("@"), new Error("Email cannot start with @"))
        );

        // Assert
        Assert.IsTrue(validated.IsSuccess);
        Assert.AreEqual("test@example.com", validated.Value);
    }

    [TestMethod]
    public void Ensure_AgeValidation_Scenario()
    {
        // Arrange
        var result = Result<int>.Ok(25);

        // Act
        var validated = result
            .Ensure(v => v >= 18, "Must be 18 or older")
            .Ensure(v => v <= 120, "Age seems unrealistic");

        // Assert
        Assert.IsTrue(validated.IsSuccess);
        Assert.AreEqual(25, validated.Value);
    }

    [TestMethod]
    public void Ensure_PasswordValidation_MultipleRules_Scenario()
    {
        // Arrange
        var result = Result<string>.Ok("Pass123!");

        // Act
        var validated = result.Ensure(
            (v => v.Length >= 8, new Error("Password must be at least 8 characters")
                .WithTags("Field", "Password")
                .WithTags("Rule", "MinLength")),
            (v => v.Any(char.IsDigit), new Error("Password must contain at least one digit")
                .WithTags("Field", "Password")
                .WithTags("Rule", "RequireDigit")),
            (v => v.Any(char.IsUpper), new Error("Password must contain at least one uppercase letter")
                .WithTags("Field", "Password")
                .WithTags("Rule", "RequireUppercase"))
        );

        // Assert
        Assert.IsTrue(validated.IsSuccess);
        Assert.AreEqual("Pass123!", validated.Value);
    }

    [TestMethod]
    public void Ensure_OrderValidation_ComplexBusinessRules()
    {
        // Arrange
        var order = new Order
        {
            Id = "ORD-001",
            Total = 150.00m,
            Items = 5,
            Status = "Pending"
        };
        var result = Result<Order>.Ok(order);

        // Act
        var validated = result.Ensure(
            (o => o.Total > 0, new Error("Order total must be positive")),
            (o => o.Items > 0, new Error("Order must have items")),
            (o => o.Total <= 10000, new Error("Order exceeds maximum amount")),
            (o => !string.IsNullOrEmpty(o.Status), new Error("Order status is required"))
        );

        // Assert
        Assert.IsTrue(validated.IsSuccess);
        Assert.AreEqual(150.00m, validated.Value!.Total);
    }

    [TestMethod]
    public void Ensure_ValidationPipeline_WithMapping()
    {
        // Arrange
        var input = "  john.doe@example.com  ";

        // Act
        var result = Result<string>.Ok(input)
            .Map(s => s.Trim())
            .EnsureNotNull("Email cannot be null")
            .Ensure(s => s.Length > 0, "Email cannot be empty")
            .Ensure(s => s.Contains("@"), "Invalid email format")
            .Map(s => s.ToLower())
            .Ensure(s => s.Length <= 100, "Email too long");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("john.doe@example.com", result.Value);
    }

    [TestMethod]
    public void Ensure_FailedValidation_CollectsAllErrors()
    {
        // Arrange
        var result = Result<string>.Ok("bad");

        // Act
        var validated = result.Ensure(
            (v => v.Length >= 8, new Error("Too short")),
            (v => v.Any(char.IsDigit), new Error("Missing digit")),
            (v => v.Any(char.IsUpper), new Error("Missing uppercase"))
        );

        // Assert
        Assert.IsTrue(validated.IsFailed);
        Assert.HasCount(3, validated.Errors);
        Assert.IsTrue(validated.Errors.Any(e => e.Message == "Too short"));
        Assert.IsTrue(validated.Errors.Any(e => e.Message == "Missing digit"));
        Assert.IsTrue(validated.Errors.Any(e => e.Message == "Missing uppercase"));
    }

    #endregion

    #region Helper Classes

    private class Person
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    private class Order
    {
        public string Id { get; set; } = "";
        public decimal Total { get; set; }
        public int Items { get; set; }
        public string Status { get; set; } = "";
    }

    #endregion
}
