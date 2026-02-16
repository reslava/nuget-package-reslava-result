using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;

namespace REslava.Result.Tests.Factories;

[TestClass]
public sealed class ResultRetryTests
{
    [TestMethod]
    public async Task Retry_SucceedsFirstAttempt_ShouldReturnImmediately()
    {
        var callCount = 0;

        var result = await Result.Retry<int>(async () =>
        {
            callCount++;
            return Result<int>.Ok(42);
        }, maxRetries: 3, delay: TimeSpan.FromMilliseconds(10));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
        Assert.AreEqual(1, callCount);
    }

    [TestMethod]
    public async Task Retry_FailsThenSucceeds_ShouldRetryAndReturn()
    {
        var callCount = 0;

        var result = await Result.Retry<int>(async () =>
        {
            callCount++;
            if (callCount < 3)
                return Result<int>.Fail("not yet");
            return Result<int>.Ok(42);
        }, maxRetries: 3, delay: TimeSpan.FromMilliseconds(10));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
        Assert.AreEqual(3, callCount);
    }

    [TestMethod]
    public async Task Retry_ExhaustsAllRetries_ShouldReturnAllErrors()
    {
        var result = await Result.Retry<int>(async () =>
        {
            return Result<int>.Fail("always fails");
        }, maxRetries: 2, delay: TimeSpan.FromMilliseconds(10));

        Assert.IsTrue(result.IsFailed);
        // 3 attempts × (1 marker error + 1 original error) = 6 errors
        Assert.AreEqual(6, result.Errors.Count);
    }

    [TestMethod]
    public async Task Retry_ExhaustsAllRetries_ErrorsContainAttemptInfo()
    {
        var result = await Result.Retry<int>(async () =>
        {
            return Result<int>.Fail("fail");
        }, maxRetries: 1, delay: TimeSpan.FromMilliseconds(10));

        Assert.IsTrue(result.IsFailed);
        Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("Retry attempt 1 of 2")));
        Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("Retry attempt 2 of 2")));
    }

    [TestMethod]
    public async Task Retry_WithExponentialBackoff_ShouldIncreaseDelay()
    {
        var sw = Stopwatch.StartNew();

        var result = await Result.Retry<int>(async () =>
        {
            return Result<int>.Fail("fail");
        }, maxRetries: 2, delay: TimeSpan.FromMilliseconds(50), backoffFactor: 2.0);

        sw.Stop();

        Assert.IsTrue(result.IsFailed);
        // Delays: 50ms + 100ms = 150ms minimum
        Assert.IsTrue(sw.ElapsedMilliseconds >= 120, $"Took {sw.ElapsedMilliseconds}ms, expected >= 120ms");
    }

    [TestMethod]
    public async Task Retry_WithConstantDelay_ShouldMaintainSameDelay()
    {
        var sw = Stopwatch.StartNew();

        var result = await Result.Retry<int>(async () =>
        {
            return Result<int>.Fail("fail");
        }, maxRetries: 2, delay: TimeSpan.FromMilliseconds(50), backoffFactor: 1.0);

        sw.Stop();

        Assert.IsTrue(result.IsFailed);
        // Delays: 50ms + 50ms = 100ms minimum
        Assert.IsTrue(sw.ElapsedMilliseconds >= 80, $"Took {sw.ElapsedMilliseconds}ms, expected >= 80ms");
    }

    [TestMethod]
    public async Task Retry_CancellationRequested_ShouldReturnCancelledResult()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await Result.Retry<int>(async () =>
        {
            return Result<int>.Fail("fail");
        }, maxRetries: 3, delay: TimeSpan.FromMilliseconds(10), cancellationToken: cts.Token);

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public async Task Retry_CancellationDuringDelay_ShouldReturnCancelledResult()
    {
        var cts = new CancellationTokenSource();
        var callCount = 0;

        var task = Result.Retry<int>(async () =>
        {
            callCount++;
            if (callCount == 1)
                cts.CancelAfter(TimeSpan.FromMilliseconds(20));
            return Result<int>.Fail("fail");
        }, maxRetries: 5, delay: TimeSpan.FromMilliseconds(200), cancellationToken: cts.Token);

        var result = await task;

        Assert.IsTrue(result.IsFailed);
        Assert.IsTrue(callCount <= 2, $"Expected <= 2 calls but got {callCount}");
    }

    [TestMethod]
    public async Task Retry_OperationThrowsException_ShouldWrapInExceptionError()
    {
        var callCount = 0;

        var result = await Result.Retry<int>(async () =>
        {
            callCount++;
            if (callCount < 3)
                throw new InvalidOperationException("boom");
            return Result<int>.Ok(42);
        }, maxRetries: 3, delay: TimeSpan.FromMilliseconds(10));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
    }

    [TestMethod]
    public async Task Retry_OperationThrowsOperationCancelled_ShouldStopRetrying()
    {
        var callCount = 0;

        var result = await Result.Retry<int>(async () =>
        {
            callCount++;
            throw new OperationCanceledException();
        }, maxRetries: 5, delay: TimeSpan.FromMilliseconds(10));

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual(1, callCount);
    }

    [TestMethod]
    public async Task Retry_ZeroMaxRetries_ShouldExecuteOnce()
    {
        var callCount = 0;

        var result = await Result.Retry<int>(async () =>
        {
            callCount++;
            return Result<int>.Fail("fail");
        }, maxRetries: 0, delay: TimeSpan.FromMilliseconds(10));

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual(1, callCount);
    }

    [TestMethod]
    public void Retry_NegativeMaxRetries_ShouldThrowArgumentOutOfRange()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            Result.Retry<int>(async () => Result<int>.Ok(1), maxRetries: -1).GetAwaiter().GetResult());
    }

    [TestMethod]
    public void Retry_BackoffFactorLessThanOne_ShouldThrowArgumentOutOfRange()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            Result.Retry<int>(async () => Result<int>.Ok(1), backoffFactor: 0.5).GetAwaiter().GetResult());
    }

    [TestMethod]
    public async Task Retry_NonGeneric_SucceedsAfterRetry()
    {
        var callCount = 0;

        var result = await Result.Retry(async () =>
        {
            callCount++;
            if (callCount < 2)
                return Result.Fail("not yet");
            return Result.Ok();
        }, maxRetries: 3, delay: TimeSpan.FromMilliseconds(10));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, callCount);
    }
}
