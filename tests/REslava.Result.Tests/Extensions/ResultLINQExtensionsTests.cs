using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.Extensions;
using REslava.Result.Reasons;

namespace REslava.Result.Tests.Extensions;

[TestClass]
public sealed class ResultLINQExtensionsTests
{
    #region SelectMany - Two Parameter Tests

    [TestMethod]
    public void SelectMany_WithSuccessResult_ShouldApplySelector()
    {
        // Arrange
        var source = Result<int>.Ok(42);
        
        // Act
        var result = source.SelectMany(x => Result<string>.Ok(x.ToString()));
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("42", result.Value);
    }

    [TestMethod]
    public void SelectMany_WithFailedResult_ShouldPropagateFailure()
    {
        // Arrange
        var error = new Error("Original error");
        var source = Result<int>.Fail(error);
        
        // Act
        var result = source.SelectMany(x => Result<string>.Ok("never called"));
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreSame(error, result.Errors[0]);
    }

    [TestMethod]
    public void SelectMany_WithSuccessResultAndSuccesses_ShouldPreserveSuccesses()
    {
        // Arrange
        var source = Result<int>.Ok(42, "Initial success");
        
        // Act
        var result = source.SelectMany(x => Result<string>.Ok(x.ToString(), "Mapped success"));
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("42", result.Value);
        Assert.HasCount(2, result.Successes);
        Assert.AreEqual("Initial success", result.Successes[0].Message);
        Assert.AreEqual("Mapped success", result.Successes[1].Message);
    }

    [TestMethod]
    public void SelectMany_WithSelectorThrowing_ShouldReturnExceptionError()
    {
        // Arrange
        var source = Result<int>.Ok(42);
        
        // Act
        var result = source.SelectMany(x => throw new InvalidOperationException("Selector error"));
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
        var exceptionError = (ExceptionError)result.Errors[0];
        Assert.AreEqual("Selector error", exceptionError.Exception.Message);
    }

    [TestMethod]
    public void SelectMany_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Arrange
        var source = Result<int>.Ok(42);
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => source.SelectMany<int, string>(null!));
    }

    [TestMethod]
    public void SelectMany_WithSelectorReturningFailure_ShouldPropagateFailure()
    {
        // Arrange
        var source = Result<int>.Ok(42);
        var selectorError = new Error("Selector error");
        
        // Act
        var result = source.SelectMany(x => Result<string>.Fail(selectorError));
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreSame(selectorError, result.Errors[0]);
    }

    [TestMethod]
    public void SelectMany_WithComplexQuerySyntax_ShouldWorkCorrectly()
    {
        // Arrange
        var initial = Result<int>.Ok(5);
        
        // Act
        var result = from x in initial
                    select x * 2;
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(10, result.Value);
    }

    #endregion

    #region SelectManyAsync - Two Parameter Tests

    [TestMethod]
    public async Task SelectManyAsync_WithSuccessResult_ShouldApplySelector()
    {
        // Arrange
        var source = Result<int>.Ok(42);
        
        // Act
        var result = await source.SelectManyAsync(x => Task.FromResult(Result<string>.Ok(x.ToString())));
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("42", result.Value);
    }

    [TestMethod]
    public async Task SelectManyAsync_WithFailedResult_ShouldPropagateFailure()
    {
        // Arrange
        var error = new Error("Original error");
        var source = Result<int>.Fail(error);
        
        // Act
        var result = await source.SelectManyAsync(x => Task.FromResult(Result<string>.Ok("never called")));
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreSame(error, result.Errors[0]);
    }

    [TestMethod]
    public async Task SelectManyAsync_WithSuccessResultAndSuccesses_ShouldPreserveSuccesses()
    {
        // Arrange
        var source = Result<int>.Ok(42, "Initial success");
        
        // Act
        var result = await source.SelectManyAsync(x => Task.FromResult(Result<string>.Ok(x.ToString(), "Mapped success")));
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("42", result.Value);
        Assert.HasCount(2, result.Successes);
        Assert.AreEqual("Initial success", result.Successes[0].Message);
        Assert.AreEqual("Mapped success", result.Successes[1].Message);
    }

    [TestMethod]
    public async Task SelectManyAsync_WithSelectorThrowing_ShouldReturnExceptionError()
    {
        // Arrange
        var source = Result<int>.Ok(42);
        
        // Act
        var result = await source.SelectManyAsync(x => throw new InvalidOperationException("Async selector error"));
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
        var exceptionError = (ExceptionError)result.Errors[0];
        Assert.AreEqual("Async selector error", exceptionError.Exception.Message);
    }

    [TestMethod]
    public async Task SelectManyAsync_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Arrange
        var source = Result<int>.Ok(42);
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await source.SelectManyAsync<int, string>(null!));
    }

    [TestMethod]
    public async Task SelectManyAsync_WithAsyncOperation_ShouldWorkCorrectly()
    {
        // Arrange
        var source = Result<int>.Ok(42);
        
        // Act
        var result = await source.SelectManyAsync(async x =>
        {
            await Task.Delay(10);
            return Result<string>.Ok($"Processed {x}");
        });
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Processed 42", result.Value);
    }

    #endregion

    #region SelectMany - Three Parameter Tests

    [TestMethod]
    public void SelectMany_ThreeParameter_WithSuccessResults_ShouldCombineResults()
    {
        // Arrange
        var source = Result<int>.Ok(42);
        
        // Act
        var result = source.SelectMany(
            x => Result<string>.Ok(x.ToString()),
            (x, y) => $"{y}_{x * 2}");
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("42_84", result.Value);
    }

    [TestMethod]
    public void SelectMany_ThreeParameter_WithFailedSource_ShouldPropagateFailure()
    {
        // Arrange
        var error = new Error("Source error");
        var source = Result<int>.Fail(error);
        
        // Act
        var result = source.SelectMany(
            x => Result<string>.Ok("never called"),
            (x, y) => "never reached");
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreSame(error, result.Errors[0]);
    }

    [TestMethod]
    public void SelectMany_ThreeParameter_WithFailedCollection_ShouldPropagateFailure()
    {
        // Arrange
        var source = Result<int>.Ok(42);
        var collectionError = new Error("Collection error");
        
        // Act
        var result = source.SelectMany(
            x => Result<string>.Fail(collectionError),
            (x, y) => "never reached");
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreSame(collectionError, result.Errors[0]);
    }

    [TestMethod]
    public void SelectMany_ThreeParameter_WithSuccesses_ShouldPreserveAllSuccesses()
    {
        // Arrange
        var source = Result<int>.Ok(42, "Source success");
        
        // Act
        var result = source.SelectMany(
            x => Result<string>.Ok(x.ToString(), "Collection success"),
            (x, y) => $"{y}_{x * 2}");
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("42_84", result.Value);
        Assert.HasCount(2, result.Successes);
        Assert.AreEqual("Source success", result.Successes[0].Message);
        Assert.AreEqual("Collection success", result.Successes[1].Message);
    }

    [TestMethod]
    public void SelectMany_ThreeParameter_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Arrange
        var source = Result<int>.Ok(42);
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => source.SelectMany<int, string, string>(
            null!,
            (x, y) => "result"));
    }

    [TestMethod]
    public void SelectMany_ThreeParameter_WithNullResultSelector_ShouldThrowArgumentNullException()
    {
        // Arrange
        var source = Result<int>.Ok(42);
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => source.SelectMany<int, string, string>(
            x => Result<string>.Ok(x.ToString()),
            null!));
    }

    [TestMethod]
    public void SelectMany_ThreeParameter_WithComplexQuerySyntax_ShouldWorkCorrectly()
    {
        // Arrange
        var numbers = Result<int>.Ok(5);
        var strings = Result<string>.Ok("hello");
        
        // Act
        var result = from x in numbers
                    from y in strings
                    select $"{x}_{y}";
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("5_hello", result.Value);
    }

    [TestMethod]
    public void SelectMany_ThreeParameter_WithSelectorThrowing_ShouldReturnExceptionError()
    {
        // Arrange
        var source = Result<int>.Ok(42);
        
        // Act
        var result = source.SelectMany(
            x => throw new InvalidOperationException("Collection selector error"),
            (x, y) => "never reached");
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
        var exceptionError = (ExceptionError)result.Errors[0];
        Assert.AreEqual("Collection selector error", exceptionError.Exception.Message);
    }

    [TestMethod]
    public void SelectMany_ThreeParameter_WithResultSelectorThrowing_ShouldReturnExceptionError()
    {
        // Arrange
        var source = Result<int>.Ok(42);
        
        // Act
        var result = source.SelectMany(
            x => Result<string>.Ok(x.ToString()),
            (x, y) => throw new InvalidOperationException("Result selector error"));
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
        var exceptionError = (ExceptionError)result.Errors[0];
        Assert.AreEqual("Result selector error", exceptionError.Exception.Message);
    }

    #endregion

    #region SelectManyAsync - Three Parameter Tests

    [TestMethod]
    public async Task SelectManyAsync_ThreeParameter_WithSuccessResults_ShouldCombineResults()
    {
        // Arrange
        var source = Result<int>.Ok(42);
        
        // Act
        var result = await source.SelectManyAsync(
            x => Task.FromResult(Result<string>.Ok(x.ToString())),
            (x, y) => $"{y}_{x * 2}");
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("42_84", result.Value);
    }

    [TestMethod]
    public async Task SelectManyAsync_ThreeParameter_WithFailedSource_ShouldPropagateFailure()
    {
        // Arrange
        var error = new Error("Source error");
        var source = Result<int>.Fail(error);
        
        // Act
        var result = await source.SelectManyAsync(
            x => Task.FromResult(Result<string>.Ok("never called")),
            (x, y) => "never reached");
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreSame(error, result.Errors[0]);
    }

    [TestMethod]
    public async Task SelectManyAsync_ThreeParameter_WithAsyncOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var source = Result<int>.Ok(42);
        
        // Act
        var result = await source.SelectManyAsync(
            async x =>
            {
                await Task.Delay(10);
                return Result<string>.Ok($"Async_{x}");
            },
            (x, y) => $"{y}_Processed");
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Async_42_Processed", result.Value);
    }

    [TestMethod]
    public async Task SelectManyAsync_ThreeParameter_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Arrange
        var source = Result<int>.Ok(42);
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(async () => await source.SelectManyAsync<int, string, string>(
            null!,
            (x, y) => "result"));
    }

    [TestMethod]
    public async Task SelectManyAsync_ThreeParameter_WithNullResultSelector_ShouldThrowArgumentNullException()
    {
        // Arrange
        var source = Result<int>.Ok(42);
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(async () => await source.SelectManyAsync<int, string, string>(
            x => Task.FromResult(Result<string>.Ok(x.ToString())),
            (x, y) => "result"));
    }

    [TestMethod]
    public async Task SelectManyAsync_ThreeParameter_WithSuccesses_ShouldPreserveAllSuccesses()
    {
        // Arrange
        var source = Result<int>.Ok(42, "Source success");
        
        // Act
        var result = await source.SelectManyAsync(
            x => Task.FromResult(Result<string>.Ok(x.ToString(), "Collection success")),
            (x, y) => $"{y}_{x * 2}");
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("42_84", result.Value);
        Assert.HasCount(2, result.Successes);
        Assert.AreEqual("Source success", result.Successes[0].Message);
        Assert.AreEqual("Collection success", result.Successes[1].Message);
    }

    #endregion

    #region Complex LINQ Query Syntax Tests

    [TestMethod]
    public void ComplexLinqQuery_WithMultipleFromClauses_ShouldWorkCorrectly()
    {
        // Arrange
        var initial = Result<int>.Ok(5);
        
        // Act
        var result = from x in initial
                    from y in Result<string>.Ok($"num_{x}")
                    from z in Result<int>.Ok(x * 2)
                    select $"{y}_{z}";
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("num_5_10", result.Value);
    }

    [TestMethod]
    public void ComplexLinqQuery_WithFailureInMiddle_ShouldPropagateFailure()
    {
        // Arrange
        var initial = Result<int>.Ok(5);
        var middleError = new Error("Middle failure");
        
        // Act
        var result = from x in initial
                    from y in Result<string>.Fail(middleError)
                    from z in Result<int>.Ok(x * 2)
                    select $"{y}_{z}";
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreSame(middleError, result.Errors[0]);
    }

    [TestMethod]
    public void ComplexLinqQuery_WithSuccesses_ShouldPreserveAllSuccesses()
    {
        // Arrange
        var initial = Result<int>.Ok(5, "Initial success");
        
        // Act
        var result = from x in initial
                    from y in Result<string>.Ok($"num_{x}", "String success")
                    from z in Result<int>.Ok(x * 2, "Double success")
                    select $"{y}_{z}";
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("num_5_10", result.Value);
        Assert.HasCount(3, result.Successes);
        Assert.AreEqual("Initial success", result.Successes[0].Message);
        Assert.AreEqual("String success", result.Successes[1].Message);
        Assert.AreEqual("Double success", result.Successes[2].Message);
    }

    #endregion

    #region Edge Cases and Error Handling

    [TestMethod]
    public void SelectMany_WithNullValueInSuccessResult_ShouldHandleCorrectly()
    {
        // Arrange
        var source = Result<string>.Ok(null!);
        
        // Act
        var result = source.SelectMany(x => Result<int>.Ok(x?.Length ?? 0));
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Value);
    }

    [TestMethod]
    public void SelectMany_WithValueTypeResult_ShouldWorkCorrectly()
    {
        // Arrange
        var source = Result<int>.Ok(42);
        
        // Act
        var result = source.SelectMany(x => Result<double>.Ok(x / 2.0));
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(21.0, result.Value);
    }

    [TestMethod]
    public void SelectMany_WithReferenceTypeResult_ShouldWorkCorrectly()
    {
        // Arrange
        var source = Result<string>.Ok("test");
        
        // Act
        var result = source.SelectMany(x => Result<char[]>.Ok(x.ToCharArray()));
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        CollectionAssert.AreEqual(new[] { 't', 'e', 's', 't' }, result.Value);
    }

    [TestMethod]
    public void SelectMany_WithChainedOperations_ShouldPreserveSuccessChain()
    {
        // Arrange
        var initial = Result<int>.Ok(5, "Start");
        
        // Act
        var result = initial
            .SelectMany(x => Result<int>.Ok(x * 2, "Double"))
            .SelectMany(x => Result<string>.Ok(x.ToString(), "ToString"))
            .SelectMany(x => Result<int>.Ok(int.Parse(x), "Parse"));
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(10, result.Value);
        Assert.HasCount(4, result.Successes);
        Assert.AreEqual("Start", result.Successes[0].Message);
        Assert.AreEqual("Double", result.Successes[1].Message);
        Assert.AreEqual("ToString", result.Successes[2].Message);
        Assert.AreEqual("Parse", result.Successes[3].Message);
    }

    #endregion
}
