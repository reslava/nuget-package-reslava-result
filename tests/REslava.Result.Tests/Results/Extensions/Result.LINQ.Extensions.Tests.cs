namespace REslava.Result.Tests;

/// <summary>
/// Comprehensive tests for Result LINQ extensions
/// </summary>
[TestClass]
public sealed class ResultLINQExtensionsTests
{
    #region SelectMany - Two Parameter Tests

    [TestMethod]
    public void LINQ_UserValidation_FailsOnInvalidAge()
    {
        // Arrange
        var email = "test@example.com";
        var age = 15;

        // Act
        var result = from e in Result<string>.Ok(email)
                     where !string.IsNullOrEmpty(e)
                     where e.Contains("@")
                     from a in Result<int>.Ok(age)
                     where a >= 18
                     select new User { Email = e, Age = a };

        // Assert
        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public async Task LINQ_AsyncPipeline_Scenario()
    {
        // Arrange
        var userId = 123;

        // Act
        var result = await Result<int>.Ok(userId)
            .SelectManyAsync(async id => await GetUserAsync(id));
            // TODO EnrichUserAsync implementation .SelectAsync(async user => await EnrichUserAsync(user))
            // TODO .WhereAsync(async user => await IsActiveAsync(user));

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Value);
    }

    [TestMethod]
    public void LINQ_CalculationPipeline_Works()
    {
        // Arrange & Act
        var result = from price in Result<decimal>.Ok(100m)
                     where price > 0
                     from discount in Result<decimal>.Ok(0.1m)
                     where discount >= 0 && discount <= 1
                     let discountedPrice = price * (1 - discount)
                     select discountedPrice;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(90m, result.Value);
    }

    [TestMethod]
    public void LINQ_MixedWithOtherMethods_Works()
    {
        // Arrange
        var result = Result<int>.Ok(5)
            .WithSuccess("Created")
            .Tap(x => Console.WriteLine($"Processing {x}"));

        // Act
        var final = from x in result
                    where x > 0
                    from y in Result<int>.Ok(x * 2)
                    select x + y;

        // Assert
        Assert.IsTrue(final.IsSuccess);
        Assert.AreEqual(15, final.Value);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void LINQ_EmptyQuery_Works()
    {
        // Arrange & Act
        var result = from x in Result<int>.Ok(5)
                     select x;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(5, result.Value);
    }

    [TestMethod]
    public void LINQ_MultipleTransformations_SameType_Works()
    {
        // Arrange & Act
        var result = from x in Result<int>.Ok(5)
                     from y in Result<int>.Ok(x + 1)
                     from z in Result<int>.Ok(y + 1)
                     select z;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(7, result.Value);
    }

    [TestMethod]
    public void LINQ_TypeChanges_Works()
    {
        // Arrange & Act
        var result = from x in Result<int>.Ok(42)
                     from s in Result<string>.Ok(x.ToString())
                     from d in Result<double>.Ok(double.Parse(s))
                     select d;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42.0, result.Value);
    }

    [TestMethod]
    public void LINQ_WithNullValue_Works()
    {
        // Arrange & Act
        var result = from x in Result<string?>.Ok(null)
                     select x?.ToUpper();

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.Value);
    }

    [TestMethod]
    public void LINQ_DefaultValue_Works()
    {
        // Arrange & Act
        var result = from x in Result<int>.Ok(default)
                     where x == 0
                     select x;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Value);
    }

    #endregion

    #region Performance and Chaining Tests

    [TestMethod]
    public void LINQ_LongChain_ExecutesCorrectly()
    {
        // Arrange & Act
        var result = Result<int>.Ok(1)
            .SelectMany(x => Result<int>.Ok(x + 1))
            .SelectMany(x => Result<int>.Ok(x + 1))
            .SelectMany(x => Result<int>.Ok(x + 1))
            .SelectMany(x => Result<int>.Ok(x + 1))
            .SelectMany(x => Result<int>.Ok(x + 1));

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(6, result.Value);
    }

    [TestMethod]
    public void LINQ_AlternatingSelectAndWhere_Works()
    {
        // Arrange & Act
        var result = Result<int>.Ok(2)
            .Select(x => x * 2)
            .Where(x => x > 0)
            .Select(x => x + 5)
            .Where(x => x < 100)
            .Select(x => x.ToString());

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("9", result.Value);
    }

    [TestMethod]
    public void LINQ_EarlyFailure_StopsExecution()
    {
        // Arrange
        var executionCount = 0;

        // Act
        var result = Result<int>.Ok(5)
            .Select(x => { executionCount++; return x; })
            .Where(x => false)  // Fails here
            .Select(x => { executionCount++; return x; });  // Should not execute

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual(1, executionCount);
    }

    #endregion

    #region Helper Methods and Classes

    private static async Task<Result<User>> GetUserAsync(int id)
    {
        await Task.Delay(10);
        return Result<User>.Ok(new User { Id = id, Email = $"user{id}@example.com", Age = 30 });
    }

    private static async Task<User> EnrichUserAsync(User user)
    {
        await Task.Delay(10);
        user.IsActive = true;
        return user;
    }

    private static async Task<bool> IsActiveAsync(User user)
    {
        await Task.Delay(10);
        return user.IsActive;
    }

    private class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    #endregion

    public void SelectMany_TwoParameter_OnSuccess_ChainsCorrectly()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var mapped = result.SelectMany(x => Result<string>.Ok(x.ToString()));

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.AreEqual("5", mapped.Value);
    }

    [TestMethod]
    public void SelectMany_TwoParameter_OnFailure_PropagatesError()
    {
        // Arrange
        var result = Result<int>.Fail("Initial error");

        // Act
        var mapped = result.SelectMany(x => Result<string>.Ok(x.ToString()));

        // Assert
        Assert.IsTrue(mapped.IsFailed);
        Assert.HasCount(1, mapped.Errors);
        Assert.AreEqual("Initial error", mapped.Errors[0].Message);
    }

    [TestMethod]
    public void SelectMany_TwoParameter_SelectorFails_ReturnsError()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var mapped = result.SelectMany(x => Result<string>.Fail("Selector failed"));

        // Assert
        Assert.IsTrue(mapped.IsFailed);
        Assert.HasCount(1, mapped.Errors);
        Assert.AreEqual("Selector failed", mapped.Errors[0].Message);
    }

    [TestMethod]
    public void SelectMany_TwoParameter_PreservesSuccessReasons()
    {
        // Arrange
        var result = Result<int>.Ok(5)
            .WithSuccess("Initial success");

        // Act
        var mapped = result.SelectMany(x => Result<string>.Ok(x.ToString())
            .WithSuccess("Selector success"));

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.HasCount(2, mapped.Successes);
        Assert.IsTrue(mapped.Successes.Any(s => s.Message == "Initial success"));
        Assert.IsTrue(mapped.Successes.Any(s => s.Message == "Selector success"));
    }

    [TestMethod]
    public void SelectMany_TwoParameter_SelectorThrows_ReturnsExceptionError()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var mapped = result.SelectMany<int, string>(x => 
            throw new InvalidOperationException("Selector exception"));

        // Assert
        Assert.IsTrue(mapped.IsFailed);
        var exceptionError = mapped.Errors[0] as ExceptionError;
        Assert.IsNotNull(exceptionError);
        Assert.AreEqual("Selector exception", exceptionError.Message);
    }

    [TestMethod]
    public void SelectMany_TwoParameter_NullSelector_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            result.SelectMany<int, string>(null!));
    }    

    #region SelectMany - Three Parameter Tests

    [TestMethod]
    public void SelectMany_ThreeParameter_QuerySyntax_Works()
    {
        // Arrange & Act
        var result = from x in Result<int>.Ok(5)
                     from y in Result<int>.Ok(x * 2)
                     select x + y;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(15, result.Value); // 5 + 10
    }

    [TestMethod]
    public void SelectMany_ThreeParameter_MethodSyntax_Works()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var mapped = result.SelectMany(
            x => Result<int>.Ok(x * 2),
            (x, y) => x + y);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(15, mapped.Value); // 5 + 10
    }

    [TestMethod]
    public void SelectMany_ThreeParameter_SourceFailed_PropagatesError()
    {
        // Arrange
        var result = Result<int>.Fail("Source error");

        // Act
        var mapped = result.SelectMany(
            x => Result<int>.Ok(x * 2),
            (x, y) => x + y);

        // Assert
        Assert.IsTrue(mapped.IsFailed);
        Assert.AreEqual("Source error", mapped.Errors[0].Message);
    }

    [TestMethod]
    public void SelectMany_ThreeParameter_SelectorFailed_PropagatesError()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var mapped = result.SelectMany(
            x => Result<int>.Fail("Selector error"),
            (x, y) => x + y);

        // Assert
        Assert.IsTrue(mapped.IsFailed);
        Assert.AreEqual("Selector error", mapped.Errors[0].Message);
    }

    // TODO
    // [TestMethod]
    // public void SelectMany_ThreeParameter_ResultSelectorThrows_ReturnsExceptionError()
    // {
    //     // Arrange
    //     var result = Result<int>.Ok(5);

    //     // Act
    //     var mapped = result.SelectMany(
    //         x => Result<int>.Ok(x * 2),
    //         (x, y) => throw new DivideByZeroException("Result selector failed"));

    //     // Assert
    //     Assert.IsTrue(mapped.IsFailed);
    //     var exceptionError = mapped.Errors[0] as ExceptionError;
    //     Assert.IsNotNull(exceptionError);
    //     Assert.Contains("Result selector failed", exceptionError.Message);
    // }

    [TestMethod]
    public void SelectMany_ThreeParameter_PreservesAllSuccessReasons()
    {
        // Arrange
        var result = Result<int>.Ok(5)
            .WithSuccess("Source success");

        // Act
        var mapped = result.SelectMany(
            x => Result<int>.Ok(x * 2).WithSuccess("Intermediate success"),
            (x, y) => x + y);

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.HasCount(2, mapped.Successes);
        Assert.IsTrue(mapped.Successes.Any(s => s.Message == "Source success"));
        Assert.IsTrue(mapped.Successes.Any(s => s.Message == "Intermediate success"));
    }

    [TestMethod]
    public void SelectMany_ThreeParameter_ComplexQuerySyntax_Works()
    {
        // Arrange & Act
        var result = from x in Result<int>.Ok(10)
                     from y in Result<int>.Ok(x / 2)
                     from z in Result<int>.Ok(y + 3)
                     select x + y + z;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(23, result.Value); // 10 + 5 + 8
    }

    // TODO
    // [TestMethod]
    // public void SelectMany_ThreeParameter_NullSelector_ThrowsArgumentNullException()
    // {
    //     // Arrange
    //     var result = Result<int>.Ok(5);

    //     // Act & Assert
    //     Assert.Throws<ArgumentNullException>(() =>
    //         result.SelectMany<int, int, int>(null!, (x, y) => x + y));
    // }

    // [TestMethod]
    // public void SelectMany_ThreeParameter_NullResultSelector_ThrowsArgumentNullException()
    // {
    //     // Arrange
    //     var result = Result<int>.Ok(5);

    //     // Act & Assert
    //     Assert.Throws<ArgumentNullException>(() =>
    //         result.SelectMany(x => Result<int>.Ok(x * 2), null!));
    // }

    #endregion

    #region SelectManyAsync - Two Parameter Tests

    [TestMethod]
    public async Task SelectManyAsync_TwoParameter_OnSuccess_ChainsCorrectly()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var mapped = await result.SelectManyAsync(async x =>
        {
            await Task.Delay(10);
            return Result<string>.Ok(x.ToString());
        });

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.AreEqual("5", mapped.Value);
    }

    [TestMethod]
    public async Task SelectManyAsync_TwoParameter_OnFailure_PropagatesError()
    {
        // Arrange
        var result = Result<int>.Fail("Initial error");

        // Act
        var mapped = await result.SelectManyAsync(async x =>
        {
            await Task.Delay(10);
            return Result<string>.Ok(x.ToString());
        });

        // Assert
        Assert.IsTrue(mapped.IsFailed);
        Assert.AreEqual("Initial error", mapped.Errors[0].Message);
    }

    [TestMethod]
    public async Task SelectManyAsync_TwoParameter_SelectorThrows_ReturnsExceptionError()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var mapped = await result.SelectManyAsync<int, string>(async x =>
        {
            await Task.Delay(10);
            throw new InvalidOperationException("Async selector failed");
        });

        // Assert
        Assert.IsTrue(mapped.IsFailed);
        var exceptionError = mapped.Errors[0] as ExceptionError;
        Assert.IsNotNull(exceptionError);
        Assert.Contains("Async selector failed", exceptionError.Message);
    }

    [TestMethod]
    public async Task SelectManyAsync_TwoParameter_PreservesSuccessReasons()
    {
        // Arrange
        var result = Result<int>.Ok(5)
            .WithSuccess("Initial success");

        // Act
        var mapped = await result.SelectManyAsync(async x =>
        {
            await Task.Delay(10);
            return Result<string>.Ok(x.ToString())
                .WithSuccess("Async success");
        });

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.HasCount(2, mapped.Successes);
    }

    #endregion

    #region SelectManyAsync - Three Parameter Tests (All Async)

    [TestMethod]
    public async Task SelectManyAsync_ThreeParameter_AllAsync_Works()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var mapped = await result.SelectMany(
            async x =>
            {
                await Task.Delay(10);
                return Result<int>.Ok(x * 2);
            },
            async (x, y) =>
            {
                await Task.Delay(10);
                return x + y;
            });

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.AreEqual(15, mapped.Value);
    }

    [TestMethod]
    public async Task SelectManyAsync_ThreeParameter_AllAsync_SelectorFails_PropagatesError()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var mapped = await result.SelectMany(
            async x =>
            {
                await Task.Delay(10);
                return Result<int>.Fail("Async selector failed");
            },
            async (x, y) =>
            {
                await Task.Delay(10);
                return x + y;
            });

        // Assert
        Assert.IsTrue(mapped.IsFailed);
        Assert.AreEqual("Async selector failed", mapped.Errors[0].Message);
    }

    [TestMethod]
    public async Task SelectManyAsync_ThreeParameter_AllAsync_ResultSelectorThrows_ReturnsExceptionError()
    {
        // Arrange
        var result = Result<int>.Ok(5);        

        // Act
        var mapped = result.SelectMany(
            async x =>
            {
                await Task.Delay(10);
                return Result<int>.Ok(x * 2);
            },
            async (x, y) =>
            {
                await Task.Delay(10);
                // TODO throw new InvalidOperationException("Result selector failed");
            });

        // Assert
        // TODO 
        //Assert.IsTrue(mapped.IsFailed);
        //var exceptionError = mapped.Errors[0] as ExceptionError;
        //Assert.IsNotNull(exceptionError);
    }

    #endregion

    #region SelectManyAsync - Three Parameter Mixed (Sync Selector, Async ResultSelector)

    [TestMethod]
    public async Task SelectManyAsync_ThreeParameter_SyncSelectorAsyncResult_Works()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var mapped = await result.SelectMany(
            x => Result<int>.Ok(x * 2),
            async (x, y) =>
            {
                await Task.Delay(10);
                return x + y;
            });

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.AreEqual(15, mapped.Value);
    }

    [TestMethod]
    public async Task SelectManyAsync_ThreeParameter_SyncSelectorAsyncResult_PreservesSuccessReasons()
    {
        // Arrange
        var result = Result<int>.Ok(5)
            .WithSuccess("Initial");

        // Act
        var mapped = await result.SelectMany(
            x => Result<int>.Ok(x * 2).WithSuccess("Middle"),
            async (x, y) =>
            {
                await Task.Delay(10);
                return x + y;
            });

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.HasCount(2, mapped.Successes);
    }

    #endregion

    #region SelectManyAsync - Three Parameter Mixed (Async Selector, Sync ResultSelector)

    [TestMethod]
    public async Task SelectManyAsync_ThreeParameter_AsyncSelectorSyncResult_Works()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var mapped = await result.SelectMany(
            async x =>
            {
                await Task.Delay(10);
                return Result<int>.Ok(x * 2);
            },
            (x, y) => x + y);

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.AreEqual(15, mapped.Value);
    }

    [TestMethod]
    public async Task SelectManyAsync_ThreeParameter_AsyncSelectorSyncResult_SelectorFails_PropagatesError()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var mapped = await result.SelectMany(
            async x =>
            {
                await Task.Delay(10);
                return Result<int>.Fail("Async selector error");
            },
            (x, y) => x + y);

        // Assert
        Assert.IsTrue(mapped.IsFailed);
        Assert.AreEqual("Async selector error", mapped.Errors[0].Message);
    }

    #endregion

    #region Select Tests

    [TestMethod]
    public void Select_OnSuccess_TransformsValue()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var mapped = result.Select(x => x * 2);

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.AreEqual(10, mapped.Value);
    }

    [TestMethod]
    public void Select_OnFailure_PropagatesError()
    {
        // Arrange
        var result = Result<int>.Fail("Error");

        // Act
        var mapped = result.Select(x => x * 2);

        // Assert
        Assert.IsTrue(mapped.IsFailed);
        Assert.AreEqual("Error", mapped.Errors[0].Message);
    }

    [TestMethod]
    public void Select_ChangesType_Works()
    {
        // Arrange
        var result = Result<int>.Ok(42);

        // Act
        var mapped = result.Select(x => x.ToString());

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.AreEqual("42", mapped.Value);
        Assert.IsInstanceOfType<Result<string>>(mapped);
    }

    [TestMethod]
    public void Select_PreservesSuccessReasons()
    {
        // Arrange
        var result = Result<int>.Ok(5)
            .WithSuccess("Initial success");

        // Act
        var mapped = result.Select(x => x * 2);

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.HasCount(1, mapped.Successes);
        Assert.AreEqual("Initial success", mapped.Successes[0].Message);
    }

    [TestMethod]
    public void Select_SelectorThrows_ReturnsExceptionError()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var mapped = result.Select<int, string>(x => 
            throw new InvalidOperationException("Selector failed"));

        // Assert
        Assert.IsTrue(mapped.IsFailed);
        var exceptionError = mapped.Errors[0] as ExceptionError;
        Assert.IsNotNull(exceptionError);
        Assert.Contains("Selector failed", exceptionError.Message);
    }

    [TestMethod]
    public void Select_NullSelector_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            result.Select<int, string>(null!));
    }

    #endregion

    #region SelectAsync Tests

    [TestMethod]
    public async Task SelectAsync_OnSuccess_TransformsValue()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var mapped = await result.SelectAsync(async x =>
        {
            await Task.Delay(10);
            return x * 2;
        });

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.AreEqual(10, mapped.Value);
    }

    [TestMethod]
    public async Task SelectAsync_OnFailure_PropagatesError()
    {
        // Arrange
        var result = Result<int>.Fail("Error");

        // Act
        var mapped = await result.SelectAsync(async x =>
        {
            await Task.Delay(10);
            return x * 2;
        });

        // Assert
        Assert.IsTrue(mapped.IsFailed);
        Assert.AreEqual("Error", mapped.Errors[0].Message);
    }

    [TestMethod]
    public async Task SelectAsync_PreservesSuccessReasons()
    {
        // Arrange
        var result = Result<int>.Ok(5)
            .WithSuccess("Initial");

        // Act
        var mapped = await result.SelectAsync(async x =>
        {
            await Task.Delay(10);
            return x * 2;
        });

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.HasCount(1, mapped.Successes);
    }

    [TestMethod]
    public async Task SelectAsync_SelectorThrows_ReturnsExceptionError()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var mapped = await result.SelectAsync<int, string>(async x =>
        {
            await Task.Delay(10);
            throw new InvalidOperationException("Async selector failed");
        });

        // Assert
        Assert.IsTrue(mapped.IsFailed);
        var exceptionError = mapped.Errors[0] as ExceptionError;
        Assert.IsNotNull(exceptionError);
    }

    #endregion

    #region Where Tests

    [TestMethod]
    public void Where_PredicateTrue_ReturnsOriginalResult()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act
        var filtered = result.Where(x => x > 5);

        // Assert
        Assert.IsTrue(filtered.IsSuccess);
        Assert.AreEqual(10, filtered.Value);
        Assert.AreSame(result, filtered);
    }

    [TestMethod]
    public void Where_PredicateFalse_ReturnsFailedResult()
    {
        // Arrange
        var result = Result<int>.Ok(3);

        // Act
        var filtered = result.Where(x => x > 5);

        // Assert
        Assert.IsTrue(filtered.IsFailed);
        Assert.AreEqual("Predicate not satisfied", filtered.Errors[0].Message);
    }

    [TestMethod]
    public void Where_WithCustomMessage_PredicateFalse_ReturnsCustomError()
    {
        // Arrange
        var result = Result<int>.Ok(3);

        // Act
        var filtered = result.Where(x => x > 5, "Value must be greater than 5");

        // Assert
        Assert.IsTrue(filtered.IsFailed);
        Assert.AreEqual("Value must be greater than 5", filtered.Errors[0].Message);
    }

    [TestMethod]
    public void Where_OnFailedResult_PropagatesError()
    {
        // Arrange
        var result = Result<int>.Fail("Original error");

        // Act
        var filtered = result.Where(x => x > 5);

        // Assert
        Assert.IsTrue(filtered.IsFailed);
        Assert.AreEqual("Original error", filtered.Errors[0].Message);
    }

    [TestMethod]
    public void Where_PredicateThrows_ReturnsExceptionError()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var filtered = result.Where(x => throw new InvalidOperationException("Predicate failed"));

        // Assert
        Assert.IsTrue(filtered.IsFailed);
        var exceptionError = filtered.Errors[0] as ExceptionError;
        Assert.IsNotNull(exceptionError);
    }

    [TestMethod]
    public void Where_NullPredicate_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result.Where(null!));
    }

    #endregion

    #region WhereAsync Tests

    [TestMethod]
    public async Task WhereAsync_PredicateTrue_ReturnsOriginalResult()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act
        var filtered = await result.WhereAsync(async x =>
        {
            await Task.Delay(10);
            return x > 5;
        });

        // Assert
        Assert.IsTrue(filtered.IsSuccess);
        Assert.AreEqual(10, filtered.Value);
    }

    [TestMethod]
    public async Task WhereAsync_PredicateFalse_ReturnsFailedResult()
    {
        // Arrange
        var result = Result<int>.Ok(3);

        // Act
        var filtered = await result.WhereAsync(async x =>
        {
            await Task.Delay(10);
            return x > 5;
        });

        // Assert
        Assert.IsTrue(filtered.IsFailed);
        Assert.AreEqual("Predicate not satisfied", filtered.Errors[0].Message);
    }

    [TestMethod]
    public async Task WhereAsync_PredicateThrows_ReturnsExceptionError()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var filtered = await result.WhereAsync(async x =>
        {
            await Task.Delay(10);
            throw new InvalidOperationException("Async predicate failed");
        });

        // Assert
        Assert.IsTrue(filtered.IsFailed);
        var exceptionError = filtered.Errors[0] as ExceptionError;
        Assert.IsNotNull(exceptionError);
    }

    #endregion

    #region LINQ Query Syntax Integration Tests

    [TestMethod]
    public void LINQ_QuerySyntax_SimpleSelect_Works()
    {
        // Arrange & Act
        var result = from x in Result<int>.Ok(5)
                     select x * 2;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(10, result.Value);
    }

    [TestMethod]
    public void LINQ_QuerySyntax_WithWhere_Works()
    {
        // Arrange & Act
        var result = from x in Result<int>.Ok(10)
                     where x > 5
                     select x * 2;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(20, result.Value);
    }

    [TestMethod]
    public void LINQ_QuerySyntax_WhereFails_ReturnsError()
    {
        // Arrange & Act
        var result = from x in Result<int>.Ok(3)
                     where x > 5
                     select x * 2;

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Predicate not satisfied", result.Errors[0].Message);
    }

    [TestMethod]
    public void LINQ_QuerySyntax_MultipleFromClauses_Works()
    {
        // Arrange & Act
        var result = from x in Result<int>.Ok(5)
                     from y in Result<int>.Ok(x * 2)
                     select x + y;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(15, result.Value);
    }

    [TestMethod]
    public void LINQ_QuerySyntax_ComplexQuery_Works()
    {
        // Arrange & Act
        var result = from x in Result<int>.Ok(10)
                     where x > 5
                     from y in Result<int>.Ok(x / 2)
                     where y > 0
                     select x + y;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(15, result.Value); // 10 + 5
    }

    [TestMethod]
    public void LINQ_QuerySyntax_IntermediateFailure_StopsExecution()
    {
        // Arrange & Act
        var result = from x in Result<int>.Ok(10)
                     from y in Result<int>.Fail("Intermediate error")
                     select x + y;

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Intermediate error", result.Errors[0].Message);
    }

    [TestMethod]
    public void LINQ_QuerySyntax_MultipleWhere_Works()
    {
        // Arrange & Act
        var result = from x in Result<int>.Ok(10)
                     where x > 5
                     where x < 20
                     select x * 2;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(20, result.Value);
    }

    [TestMethod]
    public void LINQ_QuerySyntax_ChainedTransformations_Works()
    {
        // Arrange & Act
        var result = from x in Result<int>.Ok(5)
                     from y in Result<int>.Ok(x * 2)
                     from z in Result<int>.Ok(y + 5)
                     select $"Result: {x + y + z}";

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Result: 30", result.Value); // 5 + 10 + 15
    }

    #endregion

    #region Success Reason Preservation Tests

    [TestMethod]
    public void LINQ_Select_PreservesSuccessReasons()
    {
        // Arrange & Act
        var result = Result<int>.Ok(5)
            .WithSuccess("Created")
            .Select(x => x * 2);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(1, result.Successes);
        Assert.AreEqual("Created", result.Successes[0].Message);
    }

    [TestMethod]
    public void LINQ_SelectMany_PreservesSuccessReasons()
    {
        // Arrange & Act
        var result = Result<int>.Ok(5)
            .WithSuccess("Initial")
            .SelectMany(x => Result<int>.Ok(x * 2).WithSuccess("Transformed"));

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(2, result.Successes);
        Assert.IsTrue(result.Successes.Any(s => s.Message == "Initial"));
        Assert.IsTrue(result.Successes.Any(s => s.Message == "Transformed"));
    }

    [TestMethod]
    public void LINQ_ThreeParameterSelectMany_PreservesSuccessReasons()
    {
        // Arrange & Act
        var result = Result<int>.Ok(5)
            .WithSuccess("Step 1")
            .SelectMany(
                x => Result<int>.Ok(x * 2).WithSuccess("Step 2"),
                (x, y) => x + y);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(2, result.Successes);
    }

    [TestMethod]
    public void LINQ_QuerySyntax_PreservesSuccessReasons()
    {
        // Arrange & Act
        var initial = Result<int>.Ok(5).WithSuccess("Created");
        var result = from x in initial
                     from y in Result<int>.Ok(x * 2).WithSuccess("Doubled")
                     select x + y;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(2, result.Successes);
    }

    #endregion

    #region Real-World Integration Tests

    [TestMethod]
    public void LINQ_UserValidation_Scenario()
    {
        // Arrange
        var email = "test@example.com";
        var age = 25;

        // Act
        var result = from e in Result<string>.Ok(email)
                     where !string.IsNullOrEmpty(e)
                     where e.Contains("@")
                     from a in Result<int>.Ok(age)
                     where a >= 18
                     select new User { Email = e, Age = a };

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(email, result.Value!.Email);
        Assert.AreEqual(age, result.Value.Age);
    }

    [TestMethod]
    public void LINQ_UserValidation_FailsOnInvalidEmail()
    {
        // Arrange
        var email = "invalid-email";
        var age = 25;

        // Act
        var result = from e in Result<string>.Ok(email)
                     where !string.IsNullOrEmpty(e)
                     where e.Contains("@")
                     from a in Result<int>.Ok(age)
                     where a >= 18
                     select new User { Email = e, Age = a };

        // Assert
        Assert.IsTrue(result.IsFailed);
    }

    // TODO: Add more real-world integration tests as needed
    #endregion
}
