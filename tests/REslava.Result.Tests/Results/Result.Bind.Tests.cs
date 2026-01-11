namespace REslava.Result.Tests;

/// <summary>
/// Comprehensive tests for Bind
/// </summary>
[TestClass]
public sealed class ResultMapTests
{
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
        Assert.HasCount(2, bound.Successes);
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
        Assert.IsTrue(bound.Errors.Any(e => e.GetType().Equals(typeof(ExceptionError))));
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
}
