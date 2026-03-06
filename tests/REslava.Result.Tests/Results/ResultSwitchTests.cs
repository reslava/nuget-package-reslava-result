using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;

namespace REslava.Result.Tests.Results;

[TestClass]
public sealed class ResultSwitchTests
{
    #region Switch — non-generic Result

    [TestMethod]
    public void Switch_Result_Success_ExecutesOnSuccess()
    {
        var result = Result.Ok();
        var called = false;

        result.Switch(
            onSuccess: () => called = true,
            onFailure: _ => { });

        Assert.IsTrue(called);
    }

    [TestMethod]
    public void Switch_Result_Failure_ExecutesOnFailure()
    {
        var result = Result.Fail("err");
        var called = false;

        result.Switch(
            onSuccess: () => { },
            onFailure: _ => called = true);

        Assert.IsTrue(called);
    }

    [TestMethod]
    public void Switch_Result_Success_DoesNotCallOnFailure()
    {
        var result = Result.Ok();
        var failureCalled = false;

        result.Switch(
            onSuccess: () => { },
            onFailure: _ => failureCalled = true);

        Assert.IsFalse(failureCalled);
    }

    [TestMethod]
    public void Switch_Result_Failure_DoesNotCallOnSuccess()
    {
        var result = Result.Fail("err");
        var successCalled = false;

        result.Switch(
            onSuccess: () => successCalled = true,
            onFailure: _ => { });

        Assert.IsFalse(successCalled);
    }

    #endregion

    #region Switch — generic Result<T>

    [TestMethod]
    public void Switch_ResultT_Success_PassesValue()
    {
        var result = Result<int>.Ok(42);
        int? captured = null;

        result.Switch(
            onSuccess: v => captured = v,
            onFailure: _ => { });

        Assert.AreEqual(42, captured);
    }

    [TestMethod]
    public void Switch_ResultT_Failure_PassesErrors()
    {
        var result = Result<int>.Fail("oops");
        ImmutableList<IError>? captured = null;

        result.Switch(
            onSuccess: _ => { },
            onFailure: errors => captured = errors);

        Assert.IsNotNull(captured);
        Assert.AreEqual("oops", captured![0].Message);
    }

    [TestMethod]
    public void Switch_NullOnSuccess_Throws()
    {
        var result = Result<int>.Ok(1);

        Assert.ThrowsExactly<ArgumentNullException>(() =>
            result.Switch(onSuccess: null!, onFailure: _ => { }));
    }

    [TestMethod]
    public void Switch_NullOnFailure_Throws()
    {
        var result = Result<int>.Ok(1);

        Assert.ThrowsExactly<ArgumentNullException>(() =>
            result.Switch(onSuccess: _ => { }, onFailure: null!));
    }

    #endregion

    #region SwitchAsync — non-generic Result

    [TestMethod]
    public async Task SwitchAsync_Result_Success_AwaitsOnSuccess()
    {
        var result = Result.Ok();
        var called = false;

        await result.SwitchAsync(
            onSuccess: () => { called = true; return Task.CompletedTask; },
            onFailure: _ => Task.CompletedTask);

        Assert.IsTrue(called);
    }

    [TestMethod]
    public async Task SwitchAsync_Result_Failure_AwaitsOnFailure()
    {
        var result = Result.Fail("err");
        var called = false;

        await result.SwitchAsync(
            onSuccess: () => Task.CompletedTask,
            onFailure: _ => { called = true; return Task.CompletedTask; });

        Assert.IsTrue(called);
    }

    #endregion

    #region SwitchAsync — generic Result<T>

    [TestMethod]
    public async Task SwitchAsync_ResultT_Success_AwaitsOnSuccess()
    {
        var result = Result<string>.Ok("hello");
        string? captured = null;

        await result.SwitchAsync(
            onSuccess: v => { captured = v; return Task.CompletedTask; },
            onFailure: _ => Task.CompletedTask);

        Assert.AreEqual("hello", captured);
    }

    [TestMethod]
    public async Task SwitchAsync_ResultT_Failure_AwaitsOnFailure()
    {
        var result = Result<string>.Fail("bad");
        ImmutableList<IError>? captured = null;

        await result.SwitchAsync(
            onSuccess: _ => Task.CompletedTask,
            onFailure: errors => { captured = errors; return Task.CompletedTask; });

        Assert.IsNotNull(captured);
        Assert.AreEqual("bad", captured![0].Message);
    }

    [TestMethod]
    public async Task SwitchAsync_CancelledToken_Throws()
    {
        var result = Result<int>.Ok(1);
        var token = new CancellationToken(canceled: true);

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() =>
            result.SwitchAsync(
                onSuccess: _ => Task.CompletedTask,
                onFailure: _ => Task.CompletedTask,
                cancellationToken: token));
    }

    #endregion
}
