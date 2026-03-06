using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.Extensions;

namespace REslava.Result.Tests.Extensions;

[TestClass]
public sealed class ResultOrExtensionsTests
{
    #region Or — Task<Result<T>>

    [TestMethod]
    public async Task Or_TaskResultT_Success_ReturnsSelf()
    {
        var task = Task.FromResult(Result<int>.Ok(5));
        var fallback = Result<int>.Ok(99);

        var returned = await task.Or(fallback);

        Assert.IsTrue(returned.IsSuccess);
        Assert.AreEqual(5, returned.Value);
    }

    [TestMethod]
    public async Task Or_TaskResultT_Failure_ReturnsFallback()
    {
        var task = Task.FromResult(Result<int>.Fail("err"));
        var fallback = Result<int>.Ok(99);

        var returned = await task.Or(fallback);

        Assert.IsTrue(returned.IsSuccess);
        Assert.AreEqual(99, returned.Value);
    }

    #endregion

    #region OrElse — Task<Result<T>>

    [TestMethod]
    public async Task OrElse_TaskResultT_Failure_FactoryCalledWithErrors()
    {
        var task = Task.FromResult(Result<int>.Fail("task-err"));
        ImmutableList<IError>? captured = null;

        await task.OrElse(errors => { captured = errors; return Result<int>.Ok(0); });

        Assert.IsNotNull(captured);
        Assert.AreEqual("task-err", captured![0].Message);
    }

    [TestMethod]
    public async Task OrElse_TaskResultT_Success_FactoryNotCalled()
    {
        var task = Task.FromResult(Result<int>.Ok(7));
        var called = false;

        var returned = await task.OrElse(_ => { called = true; return Result<int>.Ok(0); });

        Assert.IsFalse(called);
        Assert.AreEqual(7, returned.Value);
    }

    #endregion

    #region OrElseAsync — Task<Result<T>>

    [TestMethod]
    public async Task OrElseAsync_TaskResultT_Failure_AwaitsFactory()
    {
        var task = Task.FromResult(Result<string>.Fail("bad"));

        var returned = await task.OrElseAsync(
            _ => Task.FromResult(Result<string>.Ok("recovered")));

        Assert.IsTrue(returned.IsSuccess);
        Assert.AreEqual("recovered", returned.Value);
    }

    #endregion

    #region Or — Task<Result>

    [TestMethod]
    public async Task Or_TaskResult_Success_ReturnsSelf()
    {
        var task = Task.FromResult(Result.Ok());

        var returned = await task.Or(Result.Ok("fallback"));

        Assert.IsTrue(returned.IsSuccess);
    }

    [TestMethod]
    public async Task Or_TaskResult_Failure_ReturnsFallback()
    {
        var task = Task.FromResult(Result.Fail("ng"));

        var returned = await task.Or(Result.Ok("recovered"));

        Assert.IsTrue(returned.IsSuccess);
    }

    #endregion

    #region CancellationToken

    [TestMethod]
    public async Task Or_CancelledToken_Throws()
    {
        var task = Task.FromResult(Result<int>.Ok(1));
        var token = new CancellationToken(canceled: true);

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() =>
            task.Or(Result<int>.Ok(0), cancellationToken: token));
    }

    #endregion
}
