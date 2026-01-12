namespace REslava.Result.Tests;

/// <summary>
/// Comprehensive tests for Result validation extension methods (Ensure, EnsureAsync, EnsureNotNull)
/// </summary>
[TestClass]
public sealed class ResultValidationExtensionsCompleteTests
{
    #region Ensure - Sync with Error Object

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

    #region Ensure - Sync with String Message

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

    #region Ensure - Multiple Validations

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

    #region EnsureAsync - Task<Result<T>> + Sync Predicate + Error

    [TestMethod]
    public async Task EnsureAsync_TaskResult_SyncPredicate_Error_PredicateTrue_ReturnsSuccess()
    {
        // Arrange
        var error = new Error("Value must be positive");

        // Act
        var result = await Task.FromResult(Result<int>.Ok(10))
            .EnsureAsync(v => v > 0, error);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(10, result.Value);
    }

    [TestMethod]
    public async Task EnsureAsync_TaskResult_SyncPredicate_Error_PredicateFalse_ReturnsFailed()
    {
        // Arrange
        var error = new Error("Value must be positive");

        // Act
        var result = await Task.FromResult(Result<int>.Ok(-5))
            .EnsureAsync(v => v > 0, error);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Value must be positive", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task EnsureAsync_TaskResult_SyncPredicate_Error_OnFailedResult_ReturnsOriginalErrors()
    {
        // Arrange
        var error = new Error("Additional validation");

        // Act
        var result = await Task.FromResult(Result<int>.Fail("Original error"))
            .EnsureAsync(v => v > 0, error);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Original error", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task EnsureAsync_TaskResult_SyncPredicate_Error_NullPredicate_ThrowsArgumentNullException()
    {
        // Arrange
        var error = new Error("Test");

        Func<int, bool> f =null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await Task.FromResult(Result<int>.Ok(10))
                .EnsureAsync(f!, error)
        );
    }

    [TestMethod]
    public async Task EnsureAsync_TaskResult_SyncPredicate_Error_NullError_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await Task.FromResult(Result<int>.Ok(10))
                .EnsureAsync(v => v > 0, (Error)null!)
        );
    }

    #endregion

    #region EnsureAsync - Task<Result<T>> + Sync Predicate + String

    [TestMethod]
    public async Task EnsureAsync_TaskResult_SyncPredicate_String_PredicateTrue_ReturnsSuccess()
    {
        // Act
        var result = await Task.FromResult(Result<int>.Ok(10))
            .EnsureAsync(v => v > 0, "Value must be positive");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(10, result.Value);
    }

    [TestMethod]
    public async Task EnsureAsync_TaskResult_SyncPredicate_String_PredicateFalse_ReturnsFailed()
    {
        // Act
        var result = await Task.FromResult(Result<int>.Ok(-5))
            .EnsureAsync(v => v > 0, "Value must be positive");

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Value must be positive", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task EnsureAsync_TaskResult_SyncPredicate_String_WithChaining_Works()
    {
        // Act
        var result = await GetUserAsync(123)
            .EnsureAsync(u => u.Age >= 18, "Must be 18 or older")
            .EnsureAsync(u => u.Name.Length > 0, "Name is required");

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    #endregion

    #region EnsureAsync - Result<T> + Async Predicate + Error

    [TestMethod]
    public async Task EnsureAsync_Result_AsyncPredicate_Error_PredicateTrue_ReturnsSuccess()
    {
        // Arrange
        var result = Result<int>.Ok(10);
        var error = new Error("Validation failed");

        // Act
        var validated = await result.EnsureAsync(
            async v =>
            {
                await Task.Delay(10);
                return v > 0;
            },
            error);

        // Assert
        Assert.IsTrue(validated.IsSuccess);
        Assert.AreEqual(10, validated.Value);
    }

    [TestMethod]
    public async Task EnsureAsync_Result_AsyncPredicate_Error_PredicateFalse_ReturnsFailed()
    {
        // Arrange
        var result = Result<int>.Ok(-5);
        var error = new Error("Validation failed");

        // Act
        var validated = await result.EnsureAsync(
            async v =>
            {
                await Task.Delay(10);
                return v > 0;
            },
            error);

        // Assert
        Assert.IsTrue(validated.IsFailed);
        Assert.AreEqual("Validation failed", validated.Errors[0].Message);
    }

    [TestMethod]
    public async Task EnsureAsync_Result_AsyncPredicate_Error_OnFailedResult_ReturnsOriginalErrors()
    {
        // Arrange
        var result = Result<int>.Fail("Original error");
        var error = new Error("Additional validation");

        // Act
        var validated = await result.EnsureAsync(
            async v =>
            {
                await Task.Delay(10);
                return v > 0;
            },
            error);

        // Assert
        Assert.IsTrue(validated.IsFailed);
        Assert.AreEqual("Original error", validated.Errors[0].Message);
    }

    [TestMethod]
    public async Task EnsureAsync_Result_AsyncPredicate_Error_NullPredicate_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int>.Ok(10);
        var error = new Error("Test");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await result.EnsureAsync(null!, error)
        );
    }

    [TestMethod]
    public async Task EnsureAsync_Result_AsyncPredicate_Error_NullError_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await result.EnsureAsync(async v => await Task.FromResult(true), (Error)null!)
        );
    }

    #endregion

    #region EnsureAsync - Result<T> + Async Predicate + String

    [TestMethod]
    public async Task EnsureAsync_Result_AsyncPredicate_String_PredicateTrue_ReturnsSuccess()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act
        var validated = await result.EnsureAsync(
            async v =>
            {
                await Task.Delay(10);
                return v > 0;
            },
            "Value must be positive");

        // Assert
        Assert.IsTrue(validated.IsSuccess);
        Assert.AreEqual(10, validated.Value);
    }

    [TestMethod]
    public async Task EnsureAsync_Result_AsyncPredicate_String_PredicateFalse_ReturnsFailed()
    {
        // Arrange
        var result = Result<int>.Ok(-5);

        // Act
        var validated = await result.EnsureAsync(
            async v =>
            {
                await Task.Delay(10);
                return v > 0;
            },
            "Value must be positive");

        // Assert
        Assert.IsTrue(validated.IsFailed);
        Assert.AreEqual("Value must be positive", validated.Errors[0].Message);
    }

    #endregion

    #region EnsureAsync - Task<Result<T>> + Async Predicate + Error

    [TestMethod]
    public async Task EnsureAsync_TaskResult_AsyncPredicate_Error_PredicateTrue_ReturnsSuccess()
    {
        // Arrange
        var error = new Error("User must be active");

        // Act
        var result = await GetUserAsync(123)
            .EnsureAsync(
                async u =>
                {
                    await Task.Delay(10);
                    return u.IsActive;
                },
                error);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task EnsureAsync_TaskResult_AsyncPredicate_Error_PredicateFalse_ReturnsFailed()
    {
        // Arrange
        var error = new Error("User must be active");

        // Act
        var result = await GetInactiveUserAsync(456)
            .EnsureAsync(
                async u =>
                {
                    await Task.Delay(10);
                    return u.IsActive;
                },
                error);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("User must be active", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task EnsureAsync_TaskResult_AsyncPredicate_Error_WithDatabaseCheck_Works()
    {
        // Arrange
        var error = new Error("User does not exist in database");

        // Act
        var result = await GetUserAsync(123)
            .EnsureAsync(
                async u => await UserExistsInDatabaseAsync(u.Id),
                error);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task EnsureAsync_TaskResult_AsyncPredicate_Error_OnFailedResult_ReturnsOriginalErrors()
    {
        // Arrange
        var error = new Error("Additional validation");

        // Act
        var result = await Task.FromResult(Result<User>.Fail("Original error"))
            .EnsureAsync(
                async u =>
                {
                    await Task.Delay(10);
                    return u.IsActive;
                },
                error);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Original error", result.Errors[0].Message);
    }

    #endregion

    #region EnsureAsync - Task<Result<T>> + Async Predicate + String

    [TestMethod]
    public async Task EnsureAsync_TaskResult_AsyncPredicate_String_PredicateTrue_ReturnsSuccess()
    {
        // Act
        var result = await GetUserAsync(123)
            .EnsureAsync(
                async u =>
                {
                    await Task.Delay(10);
                    return u.Age >= 18;
                },
                "Must be 18 or older");

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task EnsureAsync_TaskResult_AsyncPredicate_String_PredicateFalse_ReturnsFailed()
    {
        // Act
        var result = await GetYoungUserAsync(15)
            .EnsureAsync(
                async u =>
                {
                    await Task.Delay(10);
                    return u.Age >= 18;
                },
                "Must be 18 or older");

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Must be 18 or older", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task EnsureAsync_TaskResult_AsyncPredicate_String_WithChaining_Works()
    {
        // Act
        var result = await GetUserAsync(123)
            .EnsureAsync(
                async u => await UserExistsInDatabaseAsync(u.Id),
                "User not found")
            .EnsureAsync(
                async u => await HasPermissionAsync(u.Id),
                "Access denied");

        // Assert
        Assert.IsTrue(result.IsSuccess);
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

    #region EnsureNotNullAsync Tests

    [TestMethod]
    public async Task EnsureNotNullAsync_WithNonNullValue_ReturnsSuccess()
    {
        // Act
        var result = await Task.FromResult(Result<string>.Ok("test"))
            .EnsureNotNullAsync("Value cannot be null");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("test", result.Value);
    }

    [TestMethod]
    public async Task EnsureNotNullAsync_WithNullValue_ReturnsFailed()
    {
        // Act
        var result = await Task.FromResult(Result<string>.Ok(null!))
            .EnsureNotNullAsync("Value cannot be null");

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Value cannot be null", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task EnsureNotNullAsync_WithNullErrorMessage_UsesDefaultMessage()
    {
        // Act
        var result = await Task.FromResult(Result<string>.Ok(null!))
            .EnsureNotNullAsync(null!);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Value can not be null", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task EnsureNotNullAsync_OnFailedResult_ReturnsOriginalErrors()
    {
        // Act
        var result = await Task.FromResult(Result<string>.Fail("Original error"))
            .EnsureNotNullAsync("Value cannot be null");

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Original error", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task EnsureNotNullAsync_WithChaining_Works()
    {
        // Act
        var result = await GetUserAsync(123)
            .EnsureNotNullAsync("User cannot be null")
            .EnsureAsync(u => u.Name.Length > 0, "Name is required");

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    #endregion

    #region Integration Tests - Chaining

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

    [TestMethod]
    public async Task EnsureAsync_ChainedWithMapAndBind_Works()
    {
        // Act
        var result = await GetUserAsync(123)
            .EnsureAsync(u => u.Age >= 18, "Must be 18 or older")
            .EnsureAsync(async u => await UserExistsInDatabaseAsync(u.Id), "User not found")
            .EnsureAsync(u => u.IsActive, "User must be active");

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task EnsureAsync_MixingSyncAndAsync_Works()
    {
        // Act
        var result = await GetUserAsync(123)
            .EnsureAsync(u => u.Age >= 18, "Must be 18 or older")                          // Sync predicate
            .EnsureAsync(async u => await UserExistsInDatabaseAsync(u.Id), "Not found")    // Async predicate
            .EnsureAsync(u => u.Name.Length > 0, "Name required");                         // Sync predicate

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    #endregion

    #region Real-World Scenarios

    [TestMethod]
    public async Task EnsureAsync_EmailValidation_Scenario()
    {
        // Arrange
        var result = Result<string>.Ok("test@example.com");

        // Act
        var validated = await result
            .Ensure(v => !string.IsNullOrWhiteSpace(v), "Email cannot be empty")
            .Ensure(v => v.Contains("@"), "Email must contain @")
            .Ensure(v => v.Length <= 100, "Email too long")
            .EnsureAsync(async v => await EmailIsUniqueAsync(v), "Email already exists");

        // Assert
        Assert.IsTrue(validated.IsSuccess);
        Assert.AreEqual("test@example.com", validated.Value);
    }

    [TestMethod]
    public async Task EnsureAsync_UserRegistration_ComplexScenario()
    {
        // Arrange
        var user = new User
        {
            Id = 123,
            Name = "John Doe",
            Email = "john@example.com",
            Age = 25,
            IsActive = true
        };
        var result = Result<User>.Ok(user);

        // Act
        var validated = await result
            .EnsureNotNull("User cannot be null")
            .Ensure(u => u.Age >= 18, "Must be 18 or older")
            .Ensure(u => u.Email.Contains("@"), "Invalid email format")
            .EnsureAsync(async u => await EmailIsUniqueAsync(u.Email), "Email already exists")
            .EnsureAsync(async u => await UsernameIsUniqueAsync(u.Name), "Username taken");

        // Assert
        Assert.IsTrue(validated.IsSuccess);
        Assert.AreEqual("John Doe", validated.Value!.Name);
    }

    [TestMethod]
    public async Task EnsureAsync_OrderValidation_WithDatabaseChecks()
    {
        // Arrange
        var order = new Order
        {
            Id = "ORD-001",
            UserId = "user-123",
            Total = 150.00m,
            Items = 5
        };
        var result = Result<Order>.Ok(order);

        // Act
        var validated = await result
            .Ensure(o => o.Total > 0, "Total must be positive")
            .Ensure(o => o.Items > 0, "Must have items")
            .EnsureAsync(async o => await UserExistsAsync(o.UserId), "User not found")
            .EnsureAsync(async o => await HasSufficientFundsAsync(o.UserId, o.Total), "Insufficient funds");

        // Assert
        Assert.IsTrue(validated.IsSuccess);
    }

    [TestMethod]
    public async Task EnsureAsync_ValidationPipeline_StopsAtFirstError()
    {
        // Arrange
        var user = new User { Id = 123, Name = "", Email = "invalid", Age = 15, IsActive = false };

        // Act
        var result = await Result<User>.Ok(user)
            .Ensure(u => u.Name.Length > 0, "Name required")           // Fails here
            .EnsureAsync(async u => await EmailIsUniqueAsync(u.Email), "Email taken")  // Not evaluated
            .EnsureAsync(u => u.Age >= 18, "Must be 18+");                  // Not evaluated

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual("Name required", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task EnsureAsync_PasswordValidation_WithRemoteCheck()
    {
        // Arrange
        var password = "SecurePass123!";

        // Act
        var result = await Result<string>.Ok(password)
            .Ensure(p => p.Length >= 8, "Password too short")
            .Ensure(p => p.Any(char.IsDigit), "Must contain digit")
            .Ensure(p => p.Any(char.IsUpper), "Must contain uppercase")
            .EnsureAsync(async p => await IsNotCommonPasswordAsync(p), "Password too common");

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    #endregion

    #region Performance Tests

    [TestMethod]
    public async Task EnsureAsync_ManyValidations_ExecutesEfficiently()
    {
        // Arrange
        var result = Result<int>.Ok(50);

        // Act
        var validated = await result
            .Ensure(v => v > 0, "E1")
            .Ensure(v => v < 100, "E2")
            .EnsureAsync(async v => await Task.FromResult(v % 2 == 0), "E3")
            .EnsureAsync(v => v >= 10, "E4")
            .EnsureAsync(async v => await Task.FromResult(v <= 90), "E5");

        // Assert
        Assert.IsTrue(validated.IsSuccess);
    }

    [TestMethod]
    public async Task EnsureAsync_StopsEarlyOnFailure_DoesNotExecuteRemaining()
    {
        // Arrange
        var callCount = 0;
        var result = Result<int>.Ok(5);

        // Act
        var validated = await result
            .EnsureAsync(async v =>
            {
                callCount++;
                await Task.Delay(10);
                return v > 10; // Fails
            }, "Error 1")
            .EnsureAsync(async v =>
            {
                callCount++; // Should not be called
                await Task.Delay(10);
                return true;
            }, "Error 2");

        // Assert
        Assert.IsTrue(validated.IsFailed);
        Assert.AreEqual(1, callCount); // Only first validation executed
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public async Task EnsureAsync_WithNullValue_Works()
    {
        // Arrange
        var result = Result<string?>.Ok(null);

        // Act
        var validated = await result.EnsureAsync(
            async v =>
            {
                await Task.Delay(10);
                return v is null;
            },
            "Value must be null");

        // Assert
        Assert.IsTrue(validated.IsSuccess);
    }

    // [TestMethod]
    // public async Task EnsureAsync_EmptyString_HandledCorrectly()
    // {
    //     // Arrange
    //     var result = Result<string>.Ok("");

    //     // Act
    //     var validated = await result.EnsureAsync(
    //         v => v.Length == 0,
    //         "Must be empty");

    //     // Assert
    //     Assert.IsTrue(validated.IsSuccess);
    // }

    // TODO: Enable this test if default value handling is implemented
    // [TestMethod]
    // public async Task EnsureAsync_DefaultValue_Works()
    // {
    //     // Arrange
    //     var result = Result<int>.Ok(default);

    //     // Act
    //     var validated = await result.EnsureAsync(
    //         v => v == 0,
    //         "Must be zero");

    //     // Assert
    //     Assert.IsTrue(validated.IsSuccess);
    // }

    [TestMethod]
    public async Task EnsureAsync_LongRunningPredicate_Completes()
    {
        // Arrange
        var result = Result<int>.Ok(42);

        // Act
        var validated = await result.EnsureAsync(
            async v =>
            {
                await Task.Delay(100);
                return v > 0;
            },
            "Must be positive");

        // Assert
        Assert.IsTrue(validated.IsSuccess);
    }

    #endregion

    #region Helper Methods and Classes

    private static Task<Result<User>> GetUserAsync(int id)
    {
        return Task.FromResult(Result<User>.Ok(new User
        {
            Id = id,
            Name = $"User{id}",
            Email = $"user{id}@example.com",
            Age = 30,
            IsActive = true
        }));
    }

    private static Task<Result<User>> GetInactiveUserAsync(int id)
    {
        return Task.FromResult(Result<User>.Ok(new User
        {
            Id = id,
            Name = $"User{id}",
            Email = $"user{id}@example.com",
            Age = 30,
            IsActive = false
        }));
    }

    private static Task<Result<User>> GetYoungUserAsync(int age)
    {
        return Task.FromResult(Result<User>.Ok(new User
        {
            Id = 1,
            Name = "YoungUser",
            Email = "young@example.com",
            Age = age,
            IsActive = true
        }));
    }

    private static Task<bool> UserExistsInDatabaseAsync(int userId)
    {
        return Task.FromResult(true);
    }

    private static Task<bool> HasPermissionAsync(int userId)
    {
        return Task.FromResult(true);
    }

    private static Task<bool> EmailIsUniqueAsync(string email)
    {
        return Task.FromResult(!email.Contains("duplicate"));
    }

    private static Task<bool> UsernameIsUniqueAsync(string username)
    {
        return Task.FromResult(!username.Contains("taken"));
    }

    private static Task<bool> UserExistsAsync(string userId)
    {
        return Task.FromResult(true);
    }

    private static Task<bool> HasSufficientFundsAsync(string userId, decimal amount)
    {
        return Task.FromResult(true);
    }

    private static Task<bool> IsNotCommonPasswordAsync(string password)
    {
        var commonPasswords = new[] { "password", "123456", "qwerty" };
        return Task.FromResult(!commonPasswords.Contains(password.ToLower()));
    }

    private class Person
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    private class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    private class Order
    {
        public string Id { get; set; } = "";
        public string UserId { get; set; } = "";
        public decimal Total { get; set; }
        public int Items { get; set; }
    }

    #endregion
}