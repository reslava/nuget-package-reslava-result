using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.Extensions;

namespace REslava.Result.Tests.Extensions;

[TestClass]
public sealed class ResultMapErrorExtensionsTests
{
    #region Task<Result<T>> — sync mapper

    [TestMethod]
    public async Task MapErrorAsync_TaskResultT_Success_PassesThrough()
    {
        var task = Task.FromResult(Result<int>.Ok(99));

        var mapped = await task.MapErrorAsync(errors => errors);

        Assert.IsTrue(mapped.IsSuccess);
        Assert.AreEqual(99, mapped.Value);
    }

    [TestMethod]
    public async Task MapErrorAsync_TaskResultT_Failure_TransformsErrors()
    {
        var task = Task.FromResult(Result<int>.Fail("task-err"));

        var mapped = await task.MapErrorAsync(errors =>
            errors.Select(e => (IError)new Error($"task: {e.Message}")).ToImmutableList());

        Assert.IsTrue(mapped.IsFailure);
        Assert.AreEqual("task: task-err", mapped.Errors[0].Message);
    }

    #endregion

    #region Task<Result> — sync mapper

    [TestMethod]
    public async Task MapErrorAsync_TaskResult_Success_PassesThrough()
    {
        var task = Task.FromResult(Result.Ok());

        var mapped = await task.MapErrorAsync(errors => errors);

        Assert.IsTrue(mapped.IsSuccess);
    }

    [TestMethod]
    public async Task MapErrorAsync_TaskResult_Failure_TransformsErrors()
    {
        var task = Task.FromResult(Result.Fail("ng-err"));

        var mapped = await task.MapErrorAsync(errors =>
            errors.Select(e => (IError)new Error($"ng: {e.Message}")).ToImmutableList());

        Assert.IsTrue(mapped.IsFailure);
        Assert.AreEqual("ng: ng-err", mapped.Errors[0].Message);
    }

    #endregion

    #region Task<Result<T>> — async mapper

    [TestMethod]
    public async Task MapErrorAsync_AsyncMapper_TaskResultT_Failure_TransformsErrors()
    {
        var task = Task.FromResult(Result<string>.Fail("async"));

        var mapped = await task.MapErrorAsync(
            errors => Task.FromResult(
                errors.Select(e => (IError)new Error($"a: {e.Message}")).ToImmutableList()));

        Assert.AreEqual("a: async", mapped.Errors[0].Message);
    }

    #endregion

    #region CancellationToken

    [TestMethod]
    public async Task MapErrorAsync_CancelledToken_Throws()
    {
        var task = Task.FromResult(Result<int>.Ok(1));
        var token = new CancellationToken(canceled: true);

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() =>
            task.MapErrorAsync(errors => errors, cancellationToken: token));
    }

    #endregion
}
