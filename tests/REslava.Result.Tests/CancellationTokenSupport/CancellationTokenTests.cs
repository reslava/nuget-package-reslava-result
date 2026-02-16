using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.Extensions;

namespace REslava.Result.Tests.CancellationTokenSupport;

[TestClass]
public sealed class CancellationTokenTests
{
    private static readonly System.Threading.CancellationToken CancelledToken = new System.Threading.CancellationToken(canceled: true);

    [TestMethod]
    public void MapAsync_WithCancelledToken_ShouldThrow()
    {
        var result = Result<int>.Ok(42);

        Assert.ThrowsExactly<OperationCanceledException>(
            () => result.MapAsync(async x => x * 2, CancelledToken).GetAwaiter().GetResult());
    }

    [TestMethod]
    public void BindAsync_WithCancelledToken_ShouldThrow()
    {
        var result = Result<int>.Ok(42);

        Assert.ThrowsExactly<OperationCanceledException>(
            () => result.BindAsync(async x => Result<string>.Ok(x.ToString()), CancelledToken).GetAwaiter().GetResult());
    }

    [TestMethod]
    public void TapAsync_WithCancelledToken_ShouldThrow()
    {
        var result = Result<int>.Ok(42);

        Assert.ThrowsExactly<OperationCanceledException>(
            () => result.TapAsync(async x => { }, CancelledToken).GetAwaiter().GetResult());
    }

    [TestMethod]
    public void MatchAsync_WithCancelledToken_ShouldThrow()
    {
        var result = Result.Ok();

        Assert.ThrowsExactly<OperationCanceledException>(
            () => result.MatchAsync(
                async () => { },
                async errors => { },
                CancelledToken).GetAwaiter().GetResult());
    }

    [TestMethod]
    public void TryAsync_WithCancelledToken_ShouldThrow()
    {
        Assert.ThrowsExactly<OperationCanceledException>(
            () => Result.TryAsync(async () => { }, cancellationToken: CancelledToken).GetAwaiter().GetResult());
    }

    [TestMethod]
    public void TryAsyncGeneric_WithCancelledToken_ShouldThrow()
    {
        Assert.ThrowsExactly<OperationCanceledException>(
            () => Result<int>.TryAsync(async () => 42, cancellationToken: CancelledToken).GetAwaiter().GetResult());
    }

    [TestMethod]
    public void ExtensionMapAsync_WithCancelledToken_ShouldThrow()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(42));

        Assert.ThrowsExactly<OperationCanceledException>(
            () => resultTask.MapAsync(x => x * 2, CancelledToken).GetAwaiter().GetResult());
    }

    [TestMethod]
    public void ExtensionBindAsync_WithCancelledToken_ShouldThrow()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(42));

        Assert.ThrowsExactly<OperationCanceledException>(
            () => resultTask.BindAsync(async x => Result<string>.Ok(x.ToString()), CancelledToken).GetAwaiter().GetResult());
    }

    [TestMethod]
    public void EnsureAsync_WithCancelledToken_ShouldThrow()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(42));

        Assert.ThrowsExactly<OperationCanceledException>(
            () => resultTask.EnsureAsync(x => x > 0, new Error("fail"), CancelledToken).GetAwaiter().GetResult());
    }

    [TestMethod]
    public void SelectManyAsync_WithCancelledToken_ShouldThrow()
    {
        var source = Result<int>.Ok(5);

        Assert.ThrowsExactly<OperationCanceledException>(
            () => source.SelectManyAsync(async x => Result<string>.Ok(x.ToString()), CancelledToken).GetAwaiter().GetResult());
    }

    [TestMethod]
    public async Task MapAsync_WithDefaultToken_ShouldWork()
    {
        var result = Result<int>.Ok(42);

        var mapped = await result.MapAsync(async x => x * 2);

        Assert.IsTrue(mapped.IsSuccess);
        Assert.AreEqual(84, mapped.Value);
    }

    [TestMethod]
    public void ExtensionTapAsync_WithCancelledToken_ShouldThrow()
    {
        var resultTask = Task.FromResult(Result<int>.Ok(42));

        Assert.ThrowsExactly<OperationCanceledException>(
            () => resultTask.TapAsync(x => { }, CancelledToken).GetAwaiter().GetResult());
    }

    [TestMethod]
    public void MatchAsyncExtension_WithCancelledToken_ShouldThrow()
    {
        var result = Result<int>.Ok(42);

        Assert.ThrowsExactly<OperationCanceledException>(
            () => result.MatchAsync<int, string>(
                async v => v.ToString(),
                async errors => "fail",
                CancelledToken).GetAwaiter().GetResult());
    }
}
