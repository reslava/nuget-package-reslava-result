using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;

namespace REslava.Result.Tests.Results;

[TestClass]
public sealed class ResultMapErrorTests
{
    #region MapError — generic Result<T>

    [TestMethod]
    public void MapError_Success_PassesThroughUnchanged()
    {
        var result = Result<int>.Ok(42);

        var mapped = result.MapError(errors => errors);

        Assert.IsTrue(mapped.IsSuccess);
        Assert.AreEqual(42, mapped.Value);
    }

    [TestMethod]
    public void MapError_Failure_TransformsErrors()
    {
        var result = Result<int>.Fail("original");

        var mapped = result.MapError(errors =>
            errors.Select(e => (IError)new Error($"[ctx] {e.Message}")).ToImmutableList());

        Assert.IsTrue(mapped.IsFailure);
        Assert.AreEqual("[ctx] original", mapped.Errors[0].Message);
    }

    [TestMethod]
    public void MapError_Failure_DoesNotChangeToSuccess()
    {
        var result = Result<int>.Fail("err");

        var mapped = result.MapError(errors => errors);

        Assert.IsTrue(mapped.IsFailure);
    }

    [TestMethod]
    public void MapError_Success_MapperNotCalled()
    {
        var result = Result<int>.Ok(1);
        var called = false;

        result.MapError(errors => { called = true; return errors; });

        Assert.IsFalse(called);
    }

    [TestMethod]
    public void MapError_NullMapper_Throws()
    {
        var result = Result<int>.Ok(1);

        Assert.ThrowsExactly<ArgumentNullException>(() =>
            result.MapError(null!));
    }

    [TestMethod]
    public void MapError_Failure_MultipleErrors_AllMapped()
    {
        var result = Result<int>.Fail("e1").WithError("e2");

        var mapped = result.MapError(errors =>
            errors.Select(e => (IError)new Error($"wrapped: {e.Message}")).ToImmutableList());

        Assert.AreEqual(2, mapped.Errors.Count);
        Assert.AreEqual("wrapped: e1", mapped.Errors[0].Message);
        Assert.AreEqual("wrapped: e2", mapped.Errors[1].Message);
    }

    #endregion

    #region MapError — non-generic Result

    [TestMethod]
    public void MapError_NonGeneric_Success_PassesThrough()
    {
        var result = Result.Ok();

        var mapped = result.MapError(errors => errors);

        Assert.IsTrue(mapped.IsSuccess);
    }

    [TestMethod]
    public void MapError_NonGeneric_Failure_TransformsErrors()
    {
        var result = Result.Fail("problem");

        var mapped = result.MapError(errors =>
            errors.Select(e => (IError)new Error($"[svc] {e.Message}")).ToImmutableList());

        Assert.IsTrue(mapped.IsFailure);
        Assert.AreEqual("[svc] problem", mapped.Errors[0].Message);
    }

    #endregion

    #region MapErrorAsync — generic Result<T>

    [TestMethod]
    public async Task MapErrorAsync_Success_PassesThroughUnchanged()
    {
        var result = Result<string>.Ok("hello");

        var mapped = await result.MapErrorAsync(
            errors => Task.FromResult(errors));

        Assert.IsTrue(mapped.IsSuccess);
        Assert.AreEqual("hello", mapped.Value);
    }

    [TestMethod]
    public async Task MapErrorAsync_Failure_TransformsErrors()
    {
        var result = Result<string>.Fail("async-err");

        var mapped = await result.MapErrorAsync(
            errors => Task.FromResult(
                errors.Select(e => (IError)new Error($"async: {e.Message}")).ToImmutableList()));

        Assert.IsTrue(mapped.IsFailure);
        Assert.AreEqual("async: async-err", mapped.Errors[0].Message);
    }

    [TestMethod]
    public async Task MapErrorAsync_CancelledToken_Throws()
    {
        var result = Result<int>.Ok(1);
        var token = new CancellationToken(canceled: true);

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() =>
            result.MapErrorAsync(
                errors => Task.FromResult(errors),
                cancellationToken: token));
    }

    #endregion
}
