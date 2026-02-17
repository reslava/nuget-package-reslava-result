using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.Extensions;

namespace REslava.Result.Tests.Extensions;

[TestClass]
public sealed class ResultLINQTaskExtensionsTests
{
    #region SelectManyAsync — Task<Result<S>> two-parameter (sync selector)

    [TestMethod]
    public async Task SelectManyAsync_Task_SyncSelector_Success_ShouldProject()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(5));

        var result = await resultTask.SelectManyAsync(
            x => Result<string>.Ok(x.ToString()));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("5", result.Value);
    }

    [TestMethod]
    public async Task SelectManyAsync_Task_SyncSelector_SourceFailed_ShouldPropagate()
    {
        var resultTask = Task.FromResult(Result<int>.Fail("Source error"));

        var result = await resultTask.SelectManyAsync(
            x => Result<string>.Ok(x.ToString()));

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Source error", result.Errors[0].Message);
    }

    #endregion

    #region SelectManyAsync — Task<Result<S>> two-parameter (async selector)

    [TestMethod]
    public async Task SelectManyAsync_Task_AsyncSelector_Success_ShouldProject()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(10));

        var result = await resultTask.SelectManyAsync(
            x => Task.FromResult(Result<string>.Ok($"val:{x}")));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("val:10", result.Value);
    }

    [TestMethod]
    public async Task SelectManyAsync_Task_AsyncSelector_SourceFailed_ShouldPropagate()
    {
        var resultTask = Task.FromResult(Result<int>.Fail("Failed"));

        var result = await resultTask.SelectManyAsync(
            x => Task.FromResult(Result<string>.Ok(x.ToString())));

        Assert.IsTrue(result.IsFailed);
    }

    #endregion

    #region SelectManyAsync — Task<Result<S>> three-parameter (sync/sync)

    [TestMethod]
    public async Task SelectManyAsync_Task_SyncSyncSelectors_Success_ShouldProject()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(5));

        var result = await resultTask.SelectManyAsync(
            x => Result<int>.Ok(x * 2),
            (original, intermediate) => $"{original}+{intermediate}");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("5+10", result.Value);
    }

    [TestMethod]
    public async Task SelectManyAsync_Task_SyncSyncSelectors_SelectorFails_ShouldPropagate()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(5));

        var result = await resultTask.SelectManyAsync(
            _ => Result<int>.Fail("Selector failed"),
            (original, intermediate) => $"{original}+{intermediate}");

        Assert.IsTrue(result.IsFailed);
    }

    #endregion

    #region SelectManyAsync — Task<Result<S>> three-parameter (async selector, sync resultSelector)

    [TestMethod]
    public async Task SelectManyAsync_Task_AsyncSyncSelectors_Success_ShouldProject()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(3));

        var result = await resultTask.SelectManyAsync(
            x => Task.FromResult(Result<int>.Ok(x * 3)),
            (original, intermediate) => original + intermediate);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(12, result.Value); // 3 + 9
    }

    #endregion

    #region SelectManyAsync — Task<Result<S>> three-parameter (sync selector, async resultSelector)

    [TestMethod]
    public async Task SelectManyAsync_Task_SyncAsyncSelectors_Success_ShouldProject()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(4));

        var result = await resultTask.SelectManyAsync(
            x => Result<int>.Ok(x * 2),
            async (original, intermediate) =>
            {
                await Task.CompletedTask;
                return original + intermediate;
            });

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(12, result.Value); // 4 + 8
    }

    #endregion

    #region SelectManyAsync — Task<Result<S>> three-parameter (async/async)

    [TestMethod]
    public async Task SelectManyAsync_Task_AsyncAsyncSelectors_Success_ShouldProject()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(2));

        var result = await resultTask.SelectManyAsync(
            x => Task.FromResult(Result<int>.Ok(x * 5)),
            async (original, intermediate) =>
            {
                await Task.CompletedTask;
                return $"{original}*5={intermediate}";
            });

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("2*5=10", result.Value);
    }

    [TestMethod]
    public async Task SelectManyAsync_Task_AsyncAsyncSelectors_SourceFailed_ShouldPropagate()
    {
        var resultTask = Task.FromResult(Result<int>.Fail("Source fail"));

        var result = await resultTask.SelectManyAsync(
            x => Task.FromResult(Result<int>.Ok(x * 5)),
            async (original, intermediate) =>
            {
                await Task.CompletedTask;
                return $"{original}*5={intermediate}";
            });

        Assert.IsTrue(result.IsFailed);
    }

    #endregion

    #region SelectAsync — Task<Result<S>>

    [TestMethod]
    public async Task SelectAsync_Task_SyncSelector_Success_ShouldProject()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(7));

        var result = await resultTask.SelectAsync(x => x * 2);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(14, result.Value);
    }

    [TestMethod]
    public async Task SelectAsync_Task_SyncSelector_Failed_ShouldPropagate()
    {
        var resultTask = Task.FromResult(Result<int>.Fail("Error"));

        var result = await resultTask.SelectAsync(x => x * 2);

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public async Task SelectAsync_Task_AsyncSelector_Success_ShouldProject()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(7));

        var result = await resultTask.SelectAsync(
            x => Task.FromResult(x.ToString()));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("7", result.Value);
    }

    #endregion

    #region WhereAsync — Task<Result<S>>

    [TestMethod]
    public async Task WhereAsync_Task_SyncPredicate_True_ShouldKeep()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(10));

        var result = await resultTask.WhereAsync(
            x => x > 5, "Must be > 5");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(10, result.Value);
    }

    [TestMethod]
    public async Task WhereAsync_Task_SyncPredicate_False_ShouldFail()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(3));

        var result = await resultTask.WhereAsync(
            x => x > 5, "Must be > 5");

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Must be > 5", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task WhereAsync_Task_AsyncPredicate_True_ShouldKeep()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(10));

        var result = await resultTask.WhereAsync(
            x => Task.FromResult(x > 5), "Must be > 5");

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task WhereAsync_Task_AsyncPredicate_False_ShouldFail()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(3));

        var result = await resultTask.WhereAsync(
            x => Task.FromResult(x > 5), "Must be > 5");

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public async Task WhereAsync_Task_SourceFailed_ShouldPropagate()
    {
        var resultTask = Task.FromResult(Result<int>.Fail("Source error"));

        var result = await resultTask.WhereAsync(
            x => x > 5, "Must be > 5");

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Source error", result.Errors[0].Message);
    }

    #endregion

    #region Chaining — Task<Result<S>> pipeline

    [TestMethod]
    public async Task Pipeline_SelectMany_Select_Where_ShouldChain()
    {
        var result = await Task.FromResult(Result<int>.Ok(5))
            .SelectManyAsync(x => Result<int>.Ok(x * 2))
            .SelectAsync(x => x + 1)
            .WhereAsync(x => x > 10, "Must be > 10");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(11, result.Value); // 5*2=10, +1=11
    }

    [TestMethod]
    public async Task Pipeline_FailMidway_ShouldPropagate()
    {
        var result = await Task.FromResult(Result<int>.Ok(5))
            .SelectManyAsync(_ => Result<int>.Fail("Mid fail"))
            .SelectAsync(x => x + 1);

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Mid fail", result.Errors[0].Message);
    }

    #endregion
}
