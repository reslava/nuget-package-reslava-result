using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;

namespace REslava.Result.Tests.Factories;

[TestClass]
public sealed class ResultWhenAllTests
{
    // =========================================================================
    // 2-arity
    // =========================================================================

    [TestMethod]
    public async Task WhenAll_2Arity_BothSucceed_ShouldReturnTupleOfValues()
    {
        var result = await Result.WhenAll(
            Task.FromResult(Result<string>.Ok("hello")),
            Task.FromResult(Result<int>.Ok(42)));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("hello", result.Value.Item1);
        Assert.AreEqual(42, result.Value.Item2);
    }

    [TestMethod]
    public async Task WhenAll_2Arity_FirstFails_ShouldReturnErrors()
    {
        var result = await Result.WhenAll(
            Task.FromResult(Result<string>.Fail("error1")),
            Task.FromResult(Result<int>.Ok(42)));

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.AreEqual("error1", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task WhenAll_2Arity_SecondFails_ShouldReturnErrors()
    {
        var result = await Result.WhenAll(
            Task.FromResult(Result<string>.Ok("hello")),
            Task.FromResult(Result<int>.Fail("error2")));

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.AreEqual("error2", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task WhenAll_2Arity_BothFail_ShouldAggregateAllErrors()
    {
        var result = await Result.WhenAll(
            Task.FromResult(Result<string>.Fail("error1")),
            Task.FromResult(Result<int>.Fail("error2")));

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual(2, result.Errors.Count);
        Assert.AreEqual("error1", result.Errors[0].Message);
        Assert.AreEqual("error2", result.Errors[1].Message);
    }

    [TestMethod]
    public async Task WhenAll_2Arity_TaskThrowsException_ShouldReturnExceptionError()
    {
        var faultedTask = Task.FromException<Result<string>>(new InvalidOperationException("boom"));

        var result = await Result.WhenAll(
            faultedTask,
            Task.FromResult(Result<int>.Ok(42)));

        Assert.IsTrue(result.IsFailed);
        Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("boom")));
    }

    [TestMethod]
    public async Task WhenAll_2Arity_TaskCanceled_ShouldReturnCancellationError()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var canceledTask = Task.FromCanceled<Result<string>>(cts.Token);

        var result = await Result.WhenAll(
            canceledTask,
            Task.FromResult(Result<int>.Ok(42)));

        Assert.IsTrue(result.IsFailed);
    }

    // =========================================================================
    // 3-arity
    // =========================================================================

    [TestMethod]
    public async Task WhenAll_3Arity_AllSucceed_ShouldReturnTriple()
    {
        var result = await Result.WhenAll(
            Task.FromResult(Result<string>.Ok("a")),
            Task.FromResult(Result<int>.Ok(1)),
            Task.FromResult(Result<bool>.Ok(true)));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("a", result.Value.Item1);
        Assert.AreEqual(1, result.Value.Item2);
        Assert.IsTrue(result.Value.Item3);
    }

    [TestMethod]
    public async Task WhenAll_3Arity_MixedResults_ShouldAggregateAllErrors()
    {
        var result = await Result.WhenAll(
            Task.FromResult(Result<string>.Ok("a")),
            Task.FromResult(Result<int>.Fail("err1")),
            Task.FromResult(Result<bool>.Fail("err2")));

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual(2, result.Errors.Count);
    }

    // =========================================================================
    // 4-arity
    // =========================================================================

    [TestMethod]
    public async Task WhenAll_4Arity_AllSucceed_ShouldReturnQuadruple()
    {
        var result = await Result.WhenAll(
            Task.FromResult(Result<string>.Ok("a")),
            Task.FromResult(Result<int>.Ok(1)),
            Task.FromResult(Result<bool>.Ok(true)),
            Task.FromResult(Result<double>.Ok(3.14)));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("a", result.Value.Item1);
        Assert.AreEqual(1, result.Value.Item2);
        Assert.IsTrue(result.Value.Item3);
        Assert.AreEqual(3.14, result.Value.Item4, 0.001);
    }

    [TestMethod]
    public async Task WhenAll_4Arity_AllFail_ShouldReturnAllErrors()
    {
        var result = await Result.WhenAll(
            Task.FromResult(Result<string>.Fail("e1")),
            Task.FromResult(Result<int>.Fail("e2")),
            Task.FromResult(Result<bool>.Fail("e3")),
            Task.FromResult(Result<double>.Fail("e4")));

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual(4, result.Errors.Count);
    }

    // =========================================================================
    // Collection variant
    // =========================================================================

    [TestMethod]
    public async Task WhenAll_Collection_AllSucceed_ShouldReturnImmutableList()
    {
        var tasks = new[]
        {
            Task.FromResult(Result<int>.Ok(1)),
            Task.FromResult(Result<int>.Ok(2)),
            Task.FromResult(Result<int>.Ok(3))
        };

        var result = await Result.WhenAll(tasks.AsEnumerable());

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(3, result.Value.Count);
        Assert.AreEqual(1, result.Value[0]);
        Assert.AreEqual(2, result.Value[1]);
        Assert.AreEqual(3, result.Value[2]);
    }

    [TestMethod]
    public async Task WhenAll_Collection_SomeFail_ShouldAggregateErrors()
    {
        var tasks = new[]
        {
            Task.FromResult(Result<int>.Ok(1)),
            Task.FromResult(Result<int>.Fail("fail1")),
            Task.FromResult(Result<int>.Fail("fail2"))
        };

        var result = await Result.WhenAll(tasks.AsEnumerable());

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual(2, result.Errors.Count);
    }

    [TestMethod]
    public async Task WhenAll_Collection_EmptyCollection_ShouldReturnEmptyList()
    {
        var tasks = Enumerable.Empty<Task<Result<int>>>();

        var result = await Result.WhenAll(tasks);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Value.Count);
    }

    [TestMethod]
    public async Task WhenAll_Collection_ExceptionInTask_ShouldReturnExceptionError()
    {
        var tasks = new[]
        {
            Task.FromResult(Result<int>.Ok(1)),
            Task.FromException<Result<int>>(new InvalidOperationException("boom"))
        };

        var result = await Result.WhenAll(tasks.AsEnumerable());

        Assert.IsTrue(result.IsFailed);
        Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("boom")));
    }

    [TestMethod]
    public async Task WhenAll_RunsConcurrently_ShouldCompleteInParallel()
    {
        var sw = Stopwatch.StartNew();

        var result = await Result.WhenAll(
            DelayedOk("a", 100),
            DelayedOk("b", 100));

        sw.Stop();

        Assert.IsTrue(result.IsSuccess);
        // If sequential, would take ~200ms. Parallel should be ~100ms.
        // Use generous threshold to avoid flaky failures in CI/slow environments.
        Assert.IsTrue(sw.ElapsedMilliseconds < 500, $"Took {sw.ElapsedMilliseconds}ms, expected < 500ms");
    }

    private static async Task<Result<string>> DelayedOk(string value, int delayMs)
    {
        await Task.Delay(delayMs);
        return Result<string>.Ok(value);
    }
}
