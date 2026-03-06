using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;

namespace REslava.Result.Tests.Results;

[TestClass]
public sealed class ResultOrTests
{
    #region Or — generic Result<T>

    [TestMethod]
    public void Or_Success_ReturnsSelf()
    {
        var result = Result<int>.Ok(1);
        var fallback = Result<int>.Ok(99);

        var returned = result.Or(fallback);

        Assert.IsTrue(returned.IsSuccess);
        Assert.AreEqual(1, returned.Value);
    }

    [TestMethod]
    public void Or_Failure_ReturnsFallback()
    {
        var result = Result<int>.Fail("err");
        var fallback = Result<int>.Ok(99);

        var returned = result.Or(fallback);

        Assert.IsTrue(returned.IsSuccess);
        Assert.AreEqual(99, returned.Value);
    }

    [TestMethod]
    public void Or_Failure_FallbackItself_CanBeFailed()
    {
        var result = Result<int>.Fail("err");
        var fallback = Result<int>.Fail("fallback-err");

        var returned = result.Or(fallback);

        Assert.IsTrue(returned.IsFailure);
        Assert.AreEqual("fallback-err", returned.Errors[0].Message);
    }

    #endregion

    #region OrElse — generic Result<T>

    [TestMethod]
    public void OrElse_Success_FactoryNotCalled()
    {
        var result = Result<int>.Ok(42);
        var called = false;

        var returned = result.OrElse(_ => { called = true; return Result<int>.Ok(0); });

        Assert.IsFalse(called);
        Assert.AreEqual(42, returned.Value);
    }

    [TestMethod]
    public void OrElse_Failure_FactoryCalledWithErrors()
    {
        var result = Result<int>.Fail("original");
        ImmutableList<IError>? captured = null;

        result.OrElse(errors => { captured = errors; return Result<int>.Ok(0); });

        Assert.IsNotNull(captured);
        Assert.AreEqual("original", captured![0].Message);
    }

    [TestMethod]
    public void OrElse_Failure_FactoryReturnsFailure_StillFails()
    {
        var result = Result<int>.Fail("first");

        var returned = result.OrElse(_ => Result<int>.Fail("second"));

        Assert.IsTrue(returned.IsFailure);
        Assert.AreEqual("second", returned.Errors[0].Message);
    }

    [TestMethod]
    public void OrElse_NullFactory_Throws()
    {
        var result = Result<int>.Ok(1);

        Assert.ThrowsExactly<ArgumentNullException>(() =>
            result.OrElse(null!));
    }

    #endregion

    #region OrElseAsync — generic Result<T>

    [TestMethod]
    public async Task OrElseAsync_Success_FactoryNotCalled()
    {
        var result = Result<string>.Ok("hello");
        var called = false;

        var returned = await result.OrElseAsync(_ =>
        {
            called = true;
            return Task.FromResult(Result<string>.Ok("fallback"));
        });

        Assert.IsFalse(called);
        Assert.AreEqual("hello", returned.Value);
    }

    [TestMethod]
    public async Task OrElseAsync_Failure_FactoryCalledWithErrors()
    {
        var result = Result<string>.Fail("async-err");
        ImmutableList<IError>? captured = null;

        await result.OrElseAsync(errors =>
        {
            captured = errors;
            return Task.FromResult(Result<string>.Ok("ok"));
        });

        Assert.IsNotNull(captured);
        Assert.AreEqual("async-err", captured![0].Message);
    }

    [TestMethod]
    public async Task OrElseAsync_CancelledToken_Throws()
    {
        var result = Result<int>.Ok(1);
        var token = new CancellationToken(canceled: true);

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() =>
            result.OrElseAsync(
                _ => Task.FromResult(Result<int>.Ok(0)),
                cancellationToken: token));
    }

    #endregion

    #region Or — non-generic Result

    [TestMethod]
    public void Or_NonGeneric_Success_ReturnsSelf()
    {
        var result = Result.Ok();
        var fallback = Result.Ok("fallback");

        var returned = result.Or(fallback);

        Assert.IsTrue(returned.IsSuccess);
    }

    [TestMethod]
    public void Or_NonGeneric_Failure_ReturnsFallback()
    {
        var result = Result.Fail("ng-err");
        var fallback = Result.Ok("recovered");

        var returned = result.Or(fallback);

        Assert.IsTrue(returned.IsSuccess);
    }

    #endregion
}
