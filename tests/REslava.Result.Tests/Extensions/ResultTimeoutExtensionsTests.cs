using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.Extensions;

namespace REslava.Result.Tests.Extensions;

[TestClass]
public sealed class ResultTimeoutExtensionsTests
{
    [TestMethod]
    public async Task Timeout_CompletesWithinTimeout_ShouldReturnResult()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(42));

        var result = await resultTask.Timeout(TimeSpan.FromSeconds(5));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
    }

    [TestMethod]
    public async Task Timeout_ExceedsTimeout_ShouldReturnTimeoutError()
    {
        var resultTask = Task.Delay(TimeSpan.FromSeconds(5))
            .ContinueWith(_ => Result<int>.Ok(42));

        var result = await resultTask.Timeout(TimeSpan.FromMilliseconds(50));

        Assert.IsTrue(result.IsFailed);
        Assert.IsTrue(result.Errors[0].Message.Contains("timed out"));
    }

    [TestMethod]
    public async Task Timeout_ExceedsTimeout_ErrorHasTimeoutTag()
    {
        var resultTask = Task.Delay(TimeSpan.FromSeconds(5))
            .ContinueWith(_ => Result<int>.Ok(42));

        var result = await resultTask.Timeout(TimeSpan.FromMilliseconds(50));

        Assert.IsTrue(result.IsFailed);
        Assert.IsTrue(result.Errors[0].Tags.ContainsKey("TimeoutTag"));
        Assert.IsTrue(result.Errors[0].Tags.ContainsKey("Timeout"));
    }

    [TestMethod]
    public async Task Timeout_ExceedsTimeout_ErrorMessageFormatsSeconds()
    {
        var resultTask = Task.Delay(TimeSpan.FromSeconds(5))
            .ContinueWith(_ => Result<int>.Ok(42));

        var result = await resultTask.Timeout(TimeSpan.FromSeconds(3));

        Assert.IsTrue(result.IsFailed);
        Assert.IsTrue(result.Errors[0].Message.Contains("3s"));
    }

    [TestMethod]
    public async Task Timeout_ExceedsTimeout_ErrorMessageFormatsMilliseconds()
    {
        var resultTask = Task.Delay(TimeSpan.FromSeconds(5))
            .ContinueWith(_ => Result<int>.Ok(42));

        var result = await resultTask.Timeout(TimeSpan.FromMilliseconds(50));

        Assert.IsTrue(result.IsFailed);
        Assert.IsTrue(result.Errors[0].Message.Contains("50ms"));
    }

    [TestMethod]
    public async Task Timeout_TaskFails_WithinTimeout_ShouldReturnFailedResult()
    {
        var resultTask = Task.FromResult(Result<int>.Fail("original error"));

        var result = await resultTask.Timeout(TimeSpan.FromSeconds(5));

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("original error", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task Timeout_TaskThrowsException_ShouldReturnExceptionError()
    {
        var resultTask = Task.FromException<Result<int>>(new InvalidOperationException("boom"));

        var result = await resultTask.Timeout(TimeSpan.FromSeconds(5));

        Assert.IsTrue(result.IsFailed);
        Assert.IsTrue(result.Errors[0].Message.Contains("boom"));
    }

    [TestMethod]
    public async Task Timeout_CancellationRequested_ShouldReturnCancelledResult()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var resultTask = Task.Delay(TimeSpan.FromSeconds(5))
            .ContinueWith(_ => Result<int>.Ok(42));

        var result = await resultTask.Timeout(TimeSpan.FromSeconds(1), cts.Token);

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public void Timeout_ZeroTimeout_ShouldThrowArgumentOutOfRange()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(42));

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            resultTask.Timeout(TimeSpan.Zero).GetAwaiter().GetResult());
    }

    [TestMethod]
    public void Timeout_NegativeTimeout_ShouldThrowArgumentOutOfRange()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(42));

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            resultTask.Timeout(TimeSpan.FromSeconds(-1)).GetAwaiter().GetResult());
    }

    [TestMethod]
    public async Task Timeout_NonGeneric_CompletesWithinTimeout()
    {
        var resultTask = Task.FromResult(Result.Ok());

        var result = await resultTask.Timeout(TimeSpan.FromSeconds(5));

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task Timeout_NonGeneric_ExceedsTimeout()
    {
        var resultTask = Task.Delay(TimeSpan.FromSeconds(5))
            .ContinueWith(_ => Result.Ok());

        var result = await resultTask.Timeout(TimeSpan.FromMilliseconds(50));

        Assert.IsTrue(result.IsFailed);
        Assert.IsTrue(result.Errors[0].Message.Contains("timed out"));
    }
}
