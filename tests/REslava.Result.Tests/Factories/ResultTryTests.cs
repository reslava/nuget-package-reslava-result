using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;

namespace REslava.Result.Tests.Factories;

[TestClass]
public sealed class ResultTryTests
{
    #region Try — Non-Generic

    [TestMethod]
    public void Try_SuccessfulAction_ShouldReturnOk()
    {
        var executed = false;
        var result = Result.Try(() => { executed = true; });

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public void Try_ThrowingAction_ShouldReturnFailWithExceptionError()
    {
        var result = Result.Try(() => throw new InvalidOperationException("boom"));

        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
        Assert.AreEqual("boom", result.Errors[0].Message);
    }

    [TestMethod]
    public void Try_ThrowingAction_WithCustomErrorHandler_ShouldUseCustomError()
    {
        var result = Result.Try(
            () => throw new InvalidOperationException("boom"),
            ex => new Error($"Custom: {ex.Message}"));

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Custom: boom", result.Errors[0].Message);
    }

    [TestMethod]
    public void Try_NullAction_ShouldThrow()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => Result.Try((Action)null!));
    }

    #endregion

    #region TryAsync — Non-Generic

    [TestMethod]
    public async Task TryAsync_SuccessfulOperation_ShouldReturnOk()
    {
        var executed = false;
        var result = await Result.TryAsync(async () =>
        {
            await Task.CompletedTask;
            executed = true;
        });

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public async Task TryAsync_ThrowingOperation_ShouldReturnFailWithExceptionError()
    {
        var result = await Result.TryAsync(
            () => throw new InvalidOperationException("async boom"));

        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
        Assert.AreEqual("async boom", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task TryAsync_ThrowingOperation_WithCustomHandler_ShouldUseCustomError()
    {
        var result = await Result.TryAsync(
            () => throw new InvalidOperationException("async boom"),
            ex => new Error($"Custom: {ex.Message}"));

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Custom: async boom", result.Errors[0].Message);
    }

    [TestMethod]
    public void TryAsync_CancelledToken_ShouldThrowOperationCancelled()
    {
        var ct = new CancellationToken(canceled: true);

        Assert.ThrowsExactly<OperationCanceledException>(
            () => Result.TryAsync(() => Task.CompletedTask, cancellationToken: ct)
                .GetAwaiter().GetResult());
    }

    #endregion

    #region Try — Generic Result<T>

    [TestMethod]
    public void Try_Generic_SuccessfulOperation_ShouldReturnOkWithValue()
    {
        var result = Result<int>.Try(() => 42);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
    }

    [TestMethod]
    public void Try_Generic_ThrowingOperation_ShouldReturnFail()
    {
        var result = Result<int>.Try(
            () => throw new InvalidOperationException("boom"));

        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
    }

    [TestMethod]
    public void Try_Generic_ThrowingOperation_WithCustomHandler_ShouldUseCustomError()
    {
        var result = Result<int>.Try(
            () => throw new FormatException("bad format"),
            ex => new Error($"Parse failed: {ex.Message}"));

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Parse failed: bad format", result.Errors[0].Message);
    }

    [TestMethod]
    public void Try_Generic_NullOperation_ShouldThrow()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => Result<int>.Try((Func<int>)null!));
    }

    [TestMethod]
    public void Try_Generic_ReturnsNull_ShouldReturnOkWithNull()
    {
        var result = Result<string?>.Try(() => (string?)null);

        Assert.IsTrue(result.IsSuccess);
    }

    #endregion

    #region TryAsync — Generic Result<T>

    [TestMethod]
    public async Task TryAsync_Generic_SuccessfulOperation_ShouldReturnOkWithValue()
    {
        var result = await Result<int>.TryAsync(
            () => Task.FromResult(42));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
    }

    [TestMethod]
    public async Task TryAsync_Generic_ThrowingOperation_ShouldReturnFail()
    {
        var result = await Result<int>.TryAsync(
            () => throw new InvalidOperationException("async boom"));

        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
    }

    [TestMethod]
    public async Task TryAsync_Generic_WithCustomHandler_ShouldUseCustomError()
    {
        var result = await Result<int>.TryAsync(
            () => throw new FormatException("bad"),
            ex => new Error($"Custom: {ex.Message}"));

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Custom: bad", result.Errors[0].Message);
    }

    [TestMethod]
    public void TryAsync_Generic_CancelledToken_ShouldThrowOperationCancelled()
    {
        var ct = new CancellationToken(canceled: true);

        Assert.ThrowsExactly<OperationCanceledException>(
            () => Result<int>.TryAsync(() => Task.FromResult(42), cancellationToken: ct)
                .GetAwaiter().GetResult());
    }

    [TestMethod]
    public async Task TryAsync_Generic_OperationThrowsAfterAwait_ShouldReturnFail()
    {
        var result = await Result<int>.TryAsync(async () =>
        {
            await Task.Delay(1);
            throw new InvalidOperationException("delayed boom");
        });

        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
    }

    #endregion
}
