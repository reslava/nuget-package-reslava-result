using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.Extensions;

namespace REslava.Result.Tests.Extensions;

[TestClass]
public sealed class ResultAsyncExtensionsTests
{
    #region MapAsync Tests

    [TestMethod]
    public async Task MapAsync_TaskResult_WithSyncMapper_ShouldMapValue()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        
        // Act
        var result = await resultTask.MapAsync(x => x * 2);
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(84, result.Value);
    }

    [TestMethod]
    public async Task MapAsync_TaskResult_WithAsyncMapper_ShouldMapValue()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        
        // Act
        var result = await resultTask.MapAsync(async x =>
        {
            await Task.Delay(10);
            return x * 2;
        });
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(84, result.Value);
    }

    [TestMethod]
    public async Task MapAsync_TaskResult_FailedResult_ShouldPropagateFailure()
    {
        // Arrange
        var error = new Error("Original error");
        var resultTask = Task.FromResult(Result<int>.Fail(error));
        
        // Act
        var result = await resultTask.MapAsync(x => x * 2);
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreSame(error, result.Errors[0]);
    }

    #endregion

    #region WithSuccessAsync Tests

    [TestMethod]
    public async Task WithSuccessAsync_TaskResult_WithStringMessage_ShouldAddSuccessReason()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        
        // Act
        var result = await resultTask.WithSuccessAsync("Operation completed");
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
        Assert.HasCount(1, result.Successes);
        Assert.AreEqual("Operation completed", result.Successes[0].Message);
    }

    [TestMethod]
    public async Task WithSuccessAsync_TaskResult_WithISuccess_ShouldAddSuccessReason()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        var success = new Success("Custom success");
        
        // Act
        var result = await resultTask.WithSuccessAsync(success);
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
        Assert.HasCount(1, result.Successes);
        Assert.AreSame(success, result.Successes[0]);
    }

    [TestMethod]
    public async Task WithSuccessAsync_TaskResult_FailedResult_ShouldPropagateFailure()
    {
        // Arrange
        var error = new Error("Original error");
        var resultTask = Task.FromResult(Result<int>.Fail(error));
        
        // Act
        var result = await resultTask.WithSuccessAsync("Operation completed");
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreSame(error, result.Errors[0]);
    }

    #endregion

    #region EnsureAsync Tests

    [TestMethod]
    public async Task EnsureAsync_TaskResult_WithSyncPredicate_ShouldPassWhenPredicateTrue()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        
        // Act
        var result = await resultTask.EnsureAsync(x => x > 0, "Value must be positive");
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
    }

    [TestMethod]
    public async Task EnsureAsync_TaskResult_WithSyncPredicate_ShouldFailWhenPredicateFalse()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        
        // Act
        var result = await resultTask.EnsureAsync(x => x > 100, "Value must be greater than 100");
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Value must be greater than 100", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task EnsureAsync_TaskResult_WithAsyncPredicate_ShouldPassWhenPredicateTrue()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        
        // Act
        var result = await resultTask.EnsureAsync(async x =>
        {
            await Task.Delay(10);
            return x > 0;
        }, "Value must be positive");
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
    }

    [TestMethod]
    public async Task EnsureAsync_TaskResult_FailedResult_ShouldPropagateFailure()
    {
        // Arrange
        var originalError = new Error("Original error");
        var resultTask = Task.FromResult(Result<int>.Fail(originalError));
        
        // Act
        var result = await resultTask.EnsureAsync(x => x > 0, "Value must be positive");
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreSame(originalError, result.Errors[0]);
    }

    #endregion

    #region BindAsync Tests

    [TestMethod]
    public async Task BindAsync_TaskResult_WithAsyncBinder_ShouldBindToNewResult()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        
        // Act
        var result = await resultTask.BindAsync(async x =>
        {
            await Task.Delay(10);
            return Result<string>.Ok(x.ToString());
        });
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("42", result.Value);
    }

    [TestMethod]
    public async Task BindAsync_TaskResult_FailedResult_ShouldPropagateFailure()
    {
        // Arrange
        var error = new Error("Original error");
        var resultTask = Task.FromResult(Result<int>.Fail(error));
        
        // Act
        var result = await resultTask.BindAsync(async x =>
        {
            await Task.Delay(10);
            return Result<string>.Ok(x.ToString());
        });
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreSame(error, result.Errors[0]);
    }

    [TestMethod]
    public async Task BindAsync_TaskResult_BinderReturnsFailure_ShouldReturnBinderFailure()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        var binderError = new Error("Binder failed");
        
        // Act
        var result = await resultTask.BindAsync(async x =>
        {
            await Task.Delay(10);
            return Result<string>.Fail(binderError);
        });
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreSame(binderError, result.Errors[0]);
    }

    #endregion

    #region TapAsync Tests

    [TestMethod]
    public async Task TapAsync_TaskGenericResult_WithSyncAction_ShouldExecuteAction()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        var capturedValue = 0;
        
        // Act
        var result = await resultTask.TapAsync(x => capturedValue = x);
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
        Assert.AreEqual(42, capturedValue);
    }

    [TestMethod]
    public async Task TapAsync_TaskGenericResult_WithAsyncAction_ShouldExecuteAction()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        var capturedValue = 0;
        
        // Act
        var result = await resultTask.TapAsync(async x =>
        {
            await Task.Delay(10);
            capturedValue = x;
        });
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
        Assert.AreEqual(42, capturedValue);
    }

    [TestMethod]
    public async Task TapAsync_TaskGenericResult_FailedResult_ShouldNotExecuteAction()
    {
        // Arrange
        var error = new Error("Failed");
        var resultTask = Task.FromResult(Result<int>.Fail(error));
        var capturedValue = 0;
        
        // Act
        var result = await resultTask.TapAsync(x => capturedValue = x);
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreSame(error, result.Errors[0]);
        Assert.AreEqual(0, capturedValue);
    }

    #endregion

    #region Chaining Operations Tests

    [TestMethod]
    public async Task ChainedAsyncOperations_ShouldPreserveSuccessReasons()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42, "Initial success"));
        
        // Act
        var result = await resultTask
            .WithSuccessAsync("First operation")
            .MapAsync(x => x * 2)
            .EnsureAsync(x => x > 0, "Value must be positive")
            .TapAsync(x => { /* side effect */ })
            .BindAsync(async x => Result<string>.Ok(x.ToString(), "Final conversion"));
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("84", result.Value);
        Assert.HasCount(3, result.Successes);
        Assert.AreEqual("Initial success", result.Successes[0].Message);
        Assert.AreEqual("First operation", result.Successes[1].Message);
    }

    [TestMethod]
    public async Task ChainedAsyncOperations_WithFailure_ShouldStopAtFirstFailure()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42, "Initial success"));
        
        // Act
        var result = await resultTask
            .WithSuccessAsync("First operation")
            .MapAsync(x => x * 2)
            .EnsureAsync(x => x > 100, "Value must be greater than 100")
            .WithSuccessAsync("This should not be added");
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Value must be greater than 100", result.Errors[0].Message);
        Assert.HasCount(1, result.Successes); // Only initial success
    }

    #endregion

    #region Exception Handling Tests

    [TestMethod]
    public async Task MapAsync_WithSyncMapperThatThrows_ShouldReturnExceptionError()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        
        // Act
        Func<int, string> throwingMapper = x => throw new InvalidOperationException("Mapper failed");
        var result = await resultTask.MapAsync(throwingMapper);
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
        Assert.AreEqual("Mapper failed", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task MapAsync_WithAsyncMapperThatThrows_ShouldReturnExceptionError()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        
        // Act
        Func<int, Task<string>> throwingAsyncMapper = async x =>
        {
            await Task.Delay(10);
            throw new InvalidOperationException("Async mapper failed");
        };
        var result = await resultTask.MapAsync(throwingAsyncMapper);
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
        Assert.AreEqual("Async mapper failed", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task BindAsync_WithAsyncBinderThatThrows_ShouldReturnExceptionError()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        
        // Act
        var result = await resultTask.BindAsync<int, string>(async x =>
        {
            await Task.Delay(10);
            throw new InvalidOperationException("Binder failed");
        });
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
        Assert.AreEqual("Binder failed", result.Errors[0].Message);
    }

    #endregion

    #region Null Handling Tests

    [TestMethod]
    public async Task MapAsync_NullTask_ShouldThrowArgumentNullException()
    {
        // Arrange
        Task<Result<int>> nullTask = null!;
        
        // Act & Assert
        try
        {
            await nullTask.MapAsync(x => x * 2);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException ex)
        {
            Assert.IsNotNull(ex);
        }
    }

    [TestMethod]
    public async Task WithSuccessAsync_NullTask_ShouldThrowArgumentNullException()
    {
        // Arrange
        Task<Result<int>> nullTask = null!;
        
        // Act & Assert
        try
        {
            await nullTask.WithSuccessAsync("Success message");
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException ex)
        {
            Assert.IsNotNull(ex);
        }
    }

    #endregion
}
