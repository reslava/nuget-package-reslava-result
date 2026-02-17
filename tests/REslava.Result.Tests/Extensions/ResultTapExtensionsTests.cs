using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.Extensions;

namespace REslava.Result.Tests.Extensions;

[TestClass]
public sealed class ResultTapExtensionsTests
{
    #region TapOnFailure — Generic Result<T> (single error)

    [TestMethod]
    public void TapOnFailure_Generic_Failed_ShouldExecuteWithFirstError()
    {
        var result = Result<int>.Fail("First error").WithError("Second error");
        IError? captured = null;

        var returned = result.TapOnFailure(error => captured = error);

        Assert.IsTrue(returned.IsFailed);
        Assert.IsNotNull(captured);
        Assert.AreEqual("First error", captured!.Message);
    }

    [TestMethod]
    public void TapOnFailure_Generic_Success_ShouldNotExecute()
    {
        var result = Result<int>.Ok(42);
        var executed = false;

        var returned = result.TapOnFailure((IError _) => executed = true);

        Assert.IsTrue(returned.IsSuccess);
        Assert.IsFalse(executed);
    }

    [TestMethod]
    public void TapOnFailure_Generic_ShouldReturnSameResult()
    {
        var result = Result<int>.Fail("Error");

        var returned = result.TapOnFailure((IError _) => { });

        Assert.AreSame(result, returned);
    }

    #endregion

    #region TapOnFailure — Generic Result<T> (all errors)

    [TestMethod]
    public void TapOnFailure_AllErrors_Failed_ShouldExecuteWithAllErrors()
    {
        var result = Result<int>.Fail("Error 1").WithError("Error 2");
        ImmutableList<IError>? captured = null;

        var returned = result.TapOnFailure(errors => captured = errors);

        Assert.IsTrue(returned.IsFailed);
        Assert.IsNotNull(captured);
        Assert.AreEqual(2, captured!.Count);
    }

    [TestMethod]
    public void TapOnFailure_AllErrors_Success_ShouldNotExecute()
    {
        var result = Result<int>.Ok(42);
        var executed = false;

        var returned = result.TapOnFailure((ImmutableList<IError> _) => executed = true);

        Assert.IsFalse(executed);
    }

    #endregion

    #region TapOnFailureAsync — Generic Result<T>

    [TestMethod]
    public async Task TapOnFailureAsync_Generic_Failed_ShouldExecute()
    {
        var result = Result<int>.Fail("Error");
        IError? captured = null;

        var returned = await result.TapOnFailureAsync(error =>
        {
            captured = error;
            return Task.CompletedTask;
        });

        Assert.IsTrue(returned.IsFailed);
        Assert.IsNotNull(captured);
        Assert.AreEqual("Error", captured!.Message);
    }

    [TestMethod]
    public async Task TapOnFailureAsync_Generic_Success_ShouldNotExecute()
    {
        var result = Result<int>.Ok(42);
        var executed = false;

        var returned = await result.TapOnFailureAsync(_ =>
        {
            executed = true;
            return Task.CompletedTask;
        });

        Assert.IsTrue(returned.IsSuccess);
        Assert.IsFalse(executed);
    }

    #endregion

    #region TapOnFailure — Non-Generic Result

    [TestMethod]
    public void TapOnFailure_NonGeneric_Failed_ShouldExecute()
    {
        var result = Result.Fail("Error");
        IError? captured = null;

        var returned = result.TapOnFailure(error => captured = error);

        Assert.IsTrue(returned.IsFailed);
        Assert.IsNotNull(captured);
        Assert.AreEqual("Error", captured!.Message);
    }

    [TestMethod]
    public void TapOnFailure_NonGeneric_Success_ShouldThrow()
    {
        // Non-generic TapOnFailure validates Errors is not empty
        var result = Result.Ok();

        Assert.ThrowsExactly<ArgumentException>(
            () => result.TapOnFailure(_ => { }));
    }

    #endregion

    #region TapOnFailureAsync — Non-Generic Result

    [TestMethod]
    public async Task TapOnFailureAsync_NonGeneric_Failed_ShouldExecute()
    {
        var result = Result.Fail("Error");
        IError? captured = null;

        var returned = await result.TapOnFailureAsync(error =>
        {
            captured = error;
            return Task.CompletedTask;
        });

        Assert.IsTrue(returned.IsFailed);
        Assert.IsNotNull(captured);
    }

    #endregion

    #region TapOnFailureAsync — Task<Result<T>>

    [TestMethod]
    public async Task TapOnFailureAsync_TaskResult_Failed_ShouldExecute()
    {
        var resultTask = Task.FromResult(Result<int>.Fail("Error"));
        IError? captured = null;

        var returned = await resultTask.TapOnFailureAsync(error => captured = error);

        Assert.IsTrue(returned.IsFailed);
        Assert.IsNotNull(captured);
        Assert.AreEqual("Error", captured!.Message);
    }

    [TestMethod]
    public async Task TapOnFailureAsync_TaskResult_Success_ShouldNotExecute()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        var executed = false;

        var returned = await resultTask.TapOnFailureAsync(_ => executed = true);

        Assert.IsTrue(returned.IsSuccess);
        Assert.IsFalse(executed);
    }

    #endregion

    #region TapBoth

    [TestMethod]
    public void TapBoth_Success_ShouldExecuteWithResult()
    {
        var result = Result<int>.Ok(42);
        Result<int>? captured = null;

        var returned = result.TapBoth(r => captured = r);

        Assert.IsNotNull(captured);
        Assert.IsTrue(captured!.IsSuccess);
        Assert.AreEqual(42, captured.Value);
        Assert.AreSame(result, returned);
    }

    [TestMethod]
    public void TapBoth_Failed_ShouldExecuteWithResult()
    {
        var result = Result<int>.Fail("Error");
        Result<int>? captured = null;

        var returned = result.TapBoth(r => captured = r);

        Assert.IsNotNull(captured);
        Assert.IsTrue(captured!.IsFailed);
        Assert.AreSame(result, returned);
    }

    [TestMethod]
    public void TapBoth_ShouldAlwaysExecute()
    {
        var successResult = Result<string>.Ok("value");
        var failResult = Result<string>.Fail("error");
        var successExecuted = false;
        var failExecuted = false;

        successResult.TapBoth(_ => successExecuted = true);
        failResult.TapBoth(_ => failExecuted = true);

        Assert.IsTrue(successExecuted);
        Assert.IsTrue(failExecuted);
    }

    #endregion

    #region TapAsync — Task<Result> Non-Generic

    [TestMethod]
    public async Task TapAsync_NonGeneric_Success_ShouldExecuteAction()
    {
        var resultTask = Task.FromResult(Result.Ok());
        var executed = false;

        var returned = await resultTask.TapAsync(() => executed = true);

        Assert.IsTrue(returned.IsSuccess);
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public async Task TapAsync_NonGeneric_Failed_ShouldNotExecuteAction()
    {
        var resultTask = Task.FromResult(Result.Fail("Error"));
        var executed = false;

        var returned = await resultTask.TapAsync(() => executed = true);

        Assert.IsTrue(returned.IsFailed);
        Assert.IsFalse(executed);
    }

    [TestMethod]
    public async Task TapAsync_NonGeneric_AsyncAction_Success_ShouldExecute()
    {
        var resultTask = Task.FromResult(Result.Ok());
        var executed = false;

        await resultTask.TapAsync(async () =>
        {
            await Task.CompletedTask;
            executed = true;
        });

        Assert.IsTrue(executed);
    }

    #endregion

    #region TapAsync — Task<Result<T>> Generic

    [TestMethod]
    public async Task TapAsync_Generic_Success_ShouldExecuteWithValue()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        int? captured = null;

        var returned = await resultTask.TapAsync(v => captured = v);

        Assert.IsTrue(returned.IsSuccess);
        Assert.AreEqual(42, captured);
    }

    [TestMethod]
    public async Task TapAsync_Generic_Failed_ShouldNotExecute()
    {
        var resultTask = Task.FromResult(Result<int>.Fail("Error"));
        var executed = false;

        var returned = await resultTask.TapAsync(_ => executed = true);

        Assert.IsTrue(returned.IsFailed);
        Assert.IsFalse(executed);
    }

    [TestMethod]
    public async Task TapAsync_Generic_AsyncAction_Success_ShouldExecute()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(42));
        int? captured = null;

        var returned = await resultTask.TapAsync(async v =>
        {
            await Task.CompletedTask;
            captured = v;
        });

        Assert.IsTrue(returned.IsSuccess);
        Assert.AreEqual(42, captured);
    }

    #endregion
}
