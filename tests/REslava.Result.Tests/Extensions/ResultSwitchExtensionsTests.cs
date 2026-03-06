using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.Extensions;

namespace REslava.Result.Tests.Extensions;

[TestClass]
public sealed class ResultSwitchExtensionsTests
{
    #region Switch — Task<Result> sync actions

    [TestMethod]
    public async Task Switch_TaskResult_Success_ExecutesOnSuccess()
    {
        var task = Task.FromResult(Result.Ok());
        var called = false;

        await task.Switch(
            onSuccess: () => called = true,
            onFailure: _ => { });

        Assert.IsTrue(called);
    }

    [TestMethod]
    public async Task Switch_TaskResult_Failure_ExecutesOnFailure()
    {
        var task = Task.FromResult(Result.Fail("err"));
        var called = false;

        await task.Switch(
            onSuccess: () => { },
            onFailure: _ => called = true);

        Assert.IsTrue(called);
    }

    #endregion

    #region Switch — Task<Result<T>> sync actions

    [TestMethod]
    public async Task Switch_TaskResultT_Success_PassesValue()
    {
        var task = Task.FromResult(Result<int>.Ok(99));
        int? captured = null;

        await task.Switch(
            onSuccess: v => captured = v,
            onFailure: _ => { });

        Assert.AreEqual(99, captured);
    }

    [TestMethod]
    public async Task Switch_TaskResultT_Failure_PassesErrors()
    {
        var task = Task.FromResult(Result<int>.Fail("oops"));
        ImmutableList<IError>? captured = null;

        await task.Switch(
            onSuccess: _ => { },
            onFailure: errors => captured = errors);

        Assert.IsNotNull(captured);
        Assert.AreEqual("oops", captured![0].Message);
    }

    #endregion

    #region SwitchAsync — Task<Result> async actions

    [TestMethod]
    public async Task SwitchAsync_TaskResult_Success_AwaitsOnSuccess()
    {
        var task = Task.FromResult(Result.Ok());
        var called = false;

        await task.SwitchAsync(
            onSuccess: () => { called = true; return Task.CompletedTask; },
            onFailure: _ => Task.CompletedTask);

        Assert.IsTrue(called);
    }

    [TestMethod]
    public async Task SwitchAsync_TaskResult_Failure_AwaitsOnFailure()
    {
        var task = Task.FromResult(Result.Fail("err"));
        var called = false;

        await task.SwitchAsync(
            onSuccess: () => Task.CompletedTask,
            onFailure: _ => { called = true; return Task.CompletedTask; });

        Assert.IsTrue(called);
    }

    #endregion

    #region SwitchAsync — Task<Result<T>> async actions

    [TestMethod]
    public async Task SwitchAsync_TaskResultT_Success_AwaitsOnSuccess()
    {
        var task = Task.FromResult(Result<string>.Ok("world"));
        string? captured = null;

        await task.SwitchAsync(
            onSuccess: v => { captured = v; return Task.CompletedTask; },
            onFailure: _ => Task.CompletedTask);

        Assert.AreEqual("world", captured);
    }

    [TestMethod]
    public async Task SwitchAsync_TaskResultT_Failure_AwaitsOnFailure()
    {
        var task = Task.FromResult(Result<string>.Fail("bad"));
        ImmutableList<IError>? captured = null;

        await task.SwitchAsync(
            onSuccess: _ => Task.CompletedTask,
            onFailure: errors => { captured = errors; return Task.CompletedTask; });

        Assert.IsNotNull(captured);
        Assert.AreEqual("bad", captured![0].Message);
    }

    #endregion

    #region CancellationToken

    [TestMethod]
    public async Task Switch_CancelledToken_Throws()
    {
        var task = Task.FromResult(Result<int>.Ok(1));
        var token = new CancellationToken(canceled: true);

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() =>
            task.Switch(
                onSuccess: _ => { },
                onFailure: _ => { },
                cancellationToken: token));
    }

    #endregion
}
