using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.Extensions;
using REslava.Result.Reasons;

namespace REslava.Result.Tests.Extensions;

[TestClass]
public sealed class ResultMapExtensionsTests
{
    #region MapAsync - Sync Mapper Tests

    [TestMethod]
    public async Task MapAsync_WithSuccessResult_ShouldApplyMapper()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        
        // Act
        var result = await resultTask.MapAsync(x => x.ToString());
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("42", result.Value);
    }

    [TestMethod]
    public async Task MapAsync_WithFailedResult_ShouldPropagateFailure()
    {
        // Arrange
        var error = new Error("Original error");
        var resultTask = Task.FromResult(Result<int>.Fail(error));
        
        // Act
        var result = await resultTask.MapAsync(x => x.ToString());
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreSame(error, result.Errors[0]);
    }

    [TestMethod]
    public async Task MapAsync_WithSuccessResultAndSuccesses_ShouldPreserveSuccesses()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42, "Initial success"));
        
        // Act
        var result = await resultTask.MapAsync(x => x.ToString());
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("42", result.Value);
        Assert.HasCount(1, result.Successes);
        Assert.AreEqual("Initial success", result.Successes[0].Message);
    }

    [TestMethod]
    public async Task MapAsync_WithMapperThrowing_ShouldReturnExceptionError()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        
        // Act
        var result = await resultTask.MapAsync(x => throw new InvalidOperationException("Mapper error"));
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
        var exceptionError = (ExceptionError)result.Errors[0];
        Assert.AreEqual("Mapper error", exceptionError.Exception.Message);
    }

    [TestMethod]
    public async Task MapAsync_WithNullResultTask_ShouldThrowArgumentNullException()
    {
        // Arrange
        Task<Result<int>> resultTask = null!;
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await resultTask.MapAsync(x => x.ToString()));
    }

    [TestMethod]
    public async Task MapAsync_WithNullMapper_ShouldThrowArgumentNullException()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await resultTask.MapAsync<int, string>((Func<int, string>)null!));
    }

    [TestMethod]
    public async Task MapAsync_WithComplexMapping_ShouldWorkCorrectly()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<string>.Ok("hello"));
        
        // Act
        var result = await resultTask.MapAsync(s => s.ToUpper());
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("HELLO", result.Value);
    }

    [TestMethod]
    public async Task MapAsync_WithNullValue_ShouldHandleCorrectly()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<string>.Ok(null!));
        
        // Act
        var result = await resultTask.MapAsync(s => s?.Length ?? 0);
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Value);
    }

    #endregion

    #region MapAsync - Async Mapper Tests

    [TestMethod]
    public async Task MapAsync_WithAsyncMapper_ShouldApplyMapper()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        
        // Act
        var result = await resultTask.MapAsync(async x =>
        {
            await Task.Delay(10);
            return x.ToString();
        });
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("42", result.Value);
    }

    [TestMethod]
    public async Task MapAsync_WithFailedResultAndAsyncMapper_ShouldPropagateFailure()
    {
        // Arrange
        var error = new Error("Original error");
        var resultTask = Task.FromResult(Result<int>.Fail(error));
        
        // Act
        var result = await resultTask.MapAsync(async x =>
        {
            await Task.Delay(10);
            return x.ToString();
        });
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreSame(error, result.Errors[0]);
    }

    [TestMethod]
    public async Task MapAsync_WithAsyncMapperThrowing_ShouldReturnExceptionError()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        
        // Act
        var result = await resultTask.MapAsync(async x =>
        {
            await Task.Delay(10);
            throw new InvalidOperationException("Async mapper error");
        });
        
        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
        var exceptionError = (ExceptionError)result.Errors[0];
        Assert.AreEqual("Async mapper error", exceptionError.Exception.Message);
    }

    [TestMethod]
    public async Task MapAsync_WithNullAsyncMapper_ShouldThrowArgumentNullException()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await resultTask.MapAsync<int, string>((Func<int, string>)null!));
    }

    [TestMethod]
    public async Task MapAsync_WithAsyncMapperReturningNull_ShouldHandleCorrectly()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<string>.Ok("test"));
        
        // Act
        var result = await resultTask.MapAsync(async s =>
        {
            await Task.Delay(10);
            return null;
        });
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.Value);
    }

    [TestMethod]
    public async Task MapAsync_WithChainedAsyncOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42, "Initial"));
        
        // Act
        var result = await resultTask
            .MapAsync(async x =>
            {
                await Task.Delay(5);
                return x * 2;
            })
            .MapAsync(async x =>
            {
                await Task.Delay(5);
                return x.ToString();
            });
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("84", result.Value);
        Assert.HasCount(1, result.Successes);
        Assert.AreEqual("Initial", result.Successes[0].Message);
    }

    #endregion

    #region Edge Cases and Complex Scenarios

    [TestMethod]
    public async Task MapAsync_WithValueTypeMapping_ShouldWorkCorrectly()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        
        // Act
        var result = await resultTask.MapAsync(x => x / 2.0);
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(21.0, result.Value);
    }

    [TestMethod]
    public async Task MapAsync_WithReferenceTypeMapping_ShouldWorkCorrectly()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<string>.Ok("test"));
        
        // Act
        var result = await resultTask.MapAsync(s => s.ToCharArray());
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        CollectionAssert.AreEqual(new[] { 't', 'e', 's', 't' }, result.Value);
    }

    [TestMethod]
    public async Task MapAsync_WithComplexObjectMapping_ShouldWorkCorrectly()
    {
        // Arrange
        var user = new User { Id = 1, Name = "John" };
        var resultTask = Task.FromResult(Result<User>.Ok(user));
        
        // Act
        var result = await resultTask.MapAsync(u => new UserDto { Id = u.Id, Name = u.Name });
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Value.Id);
        Assert.AreEqual("John", result.Value.Name);
    }

    [TestMethod]
    public async Task MapAsync_WithFailedTask_ShouldHandleTaskException()
    {
        // Arrange
        var resultTask = Task.FromException<Result<int>>(new InvalidOperationException("Task failed"));
        
        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await resultTask.MapAsync(x => x.ToString()));
    }

    [TestMethod]
    public async Task MapAsync_WithCancelledTask_ShouldHandleCancellation()
    {
        // Arrange
        var cts = new System.Threading.CancellationTokenSource();
        cts.Cancel();
        var resultTask = Task.FromCanceled<Result<int>>(cts.Token);
        
        // Act & Assert
        await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () => await resultTask.MapAsync(x => x.ToString()));
    }

    [TestMethod]
    public async Task MapAsync_WithNestedTaskResults_ShouldWorkCorrectly()
    {
        // Arrange
        var innerResult = Result<int>.Ok(42);
        var resultTask = Task.FromResult(innerResult);
        
        // Act
        var result = await resultTask.MapAsync(x => Task.FromResult(Result<string>.Ok(x.ToString())));
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("42", result.Value);
    }

    #endregion

    #region Performance and Stress Tests

    [TestMethod]
    public async Task MapAsync_WithMultipleConcurrentOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var tasks = new List<Task<Result<string>>>();
        for (int i = 0; i < 10; i++)
        {
            var resultTask = Task.FromResult(Result<int>.Ok(i));
            tasks.Add(resultTask.MapAsync(x => x.ToString()));
        }
        
        // Act
        var results = await Task.WhenAll(tasks);
        
        // Assert
        Assert.HasCount(10, results);
        for (int i = 0; i < 10; i++)
        {
            Assert.IsTrue(results[i].IsSuccess);
            Assert.AreEqual(i.ToString(), results[i].Value);
        }
    }

    [TestMethod]
    public async Task MapAsync_WithLongRunningAsyncOperation_ShouldWorkCorrectly()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        
        // Act
        var startTime = DateTime.UtcNow;
        var result = await resultTask.MapAsync(async x =>
        {
            await Task.Delay(100);
            return x * 2;
        });
        var endTime = DateTime.UtcNow;
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(84, result.Value);
        Assert.IsTrue(endTime - startTime >= TimeSpan.FromMilliseconds(100));
    }

    #endregion

    #region Helper Classes

    private class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}
