using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;

namespace REslava.Result.Tests.Factories;

[TestClass]
public sealed class ResultCombineTests
{
    #region Merge — Non-Generic

    [TestMethod]
    public void Merge_AllSuccess_ShouldReturnSuccess()
    {
        var result = Result.Merge(
            Result.Ok().WithSuccess("A"),
            Result.Ok().WithSuccess("B"));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, result.Successes.Count);
    }

    [TestMethod]
    public void Merge_MixedResults_ShouldReturnFailedWithAllReasons()
    {
        var result = Result.Merge(
            Result.Ok().WithSuccess("Step 1"),
            Result.Fail("Error in step 2"),
            Result.Ok().WithSuccess("Step 3"));

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.AreEqual(2, result.Successes.Count);
    }

    [TestMethod]
    public void Merge_Empty_ShouldReturnOk()
    {
        var result = Result.Merge(Array.Empty<Result>());

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public void Merge_AllFailed_ShouldReturnFailedWithAllErrors()
    {
        var result = Result.Merge(
            Result.Fail("Error 1"),
            Result.Fail("Error 2"));

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual(2, result.Errors.Count);
    }

    [TestMethod]
    public void Merge_Params_ShouldWork()
    {
        var result = Result.Merge(Result.Ok(), Result.Ok());

        Assert.IsTrue(result.IsSuccess);
    }

    #endregion

    #region Combine — Non-Generic

    [TestMethod]
    public void Combine_AllSuccess_ShouldReturnSuccess()
    {
        var result = Result.Combine(
            Result.Ok().WithSuccess("A"),
            Result.Ok().WithSuccess("B"));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, result.Successes.Count);
    }

    [TestMethod]
    public void Combine_AnyFailed_ShouldReturnFailedWithOnlyErrors()
    {
        var result = Result.Combine(
            Result.Ok().WithSuccess("Step 1"),
            Result.Fail("Error 1"),
            Result.Fail("Error 2"));

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual(2, result.Errors.Count);
        // Combine only keeps errors on failure, unlike Merge
    }

    [TestMethod]
    public void Combine_Empty_ShouldReturnOk()
    {
        var result = Result.Combine(Array.Empty<Result>());

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public void Combine_Params_ShouldWork()
    {
        var result = Result.Combine(
            Result.Ok(),
            Result.Fail("Error"));

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public void Combine_SingleFailed_ShouldReturnFailed()
    {
        var result = Result.Combine(Result.Fail("Only error"));

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Only error", result.Errors[0].Message);
    }

    [TestMethod]
    public void Combine_AllSuccessNoReasons_ShouldReturnOk()
    {
        var result = Result.Combine(Result.Ok(), Result.Ok());

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Successes.Count);
    }

    #endregion

    #region CombineParallelAsync — Non-Generic

    [TestMethod]
    public async Task CombineParallelAsync_AllSuccess_ShouldReturnSuccess()
    {
        var result = await Result.CombineParallelAsync(new[]
        {
            Task.FromResult(Result.Ok()),
            Task.FromResult(Result.Ok())
        });

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task CombineParallelAsync_AnyFailed_ShouldReturnFailed()
    {
        var result = await Result.CombineParallelAsync(new[]
        {
            Task.FromResult(Result.Ok()),
            Task.FromResult(Result.Fail("Error"))
        });

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public async Task CombineParallelAsync_Empty_ShouldReturnOk()
    {
        var result = await Result.CombineParallelAsync(
            Array.Empty<Task<Result>>());

        Assert.IsTrue(result.IsSuccess);
    }

    #endregion

    #region Combine — Generic Result<T>

    [TestMethod]
    public void Combine_Generic_AllSuccess_ShouldReturnValuesCollection()
    {
        var result = Result<int>.Combine(
            Result<int>.Ok(1),
            Result<int>.Ok(2),
            Result<int>.Ok(3));

        Assert.IsTrue(result.IsSuccess);
        CollectionAssert.AreEqual(
            new[] { 1, 2, 3 },
            result.Value!.ToList());
    }

    [TestMethod]
    public void Combine_Generic_AnyFailed_ShouldReturnFailedWithAllErrors()
    {
        var result = Result<int>.Combine(
            Result<int>.Ok(1),
            Result<int>.Fail("Error A"),
            Result<int>.Fail("Error B"));

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual(2, result.Errors.Count);
    }

    [TestMethod]
    public void Combine_Generic_Empty_ShouldReturnOkWithEmptyCollection()
    {
        var result = Result<int>.Combine(
            Array.Empty<Result<int>>());

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Value!.Count());
    }

    [TestMethod]
    public void Combine_Generic_WithSuccessReasons_ShouldPreserveReasons()
    {
        var result = Result<int>.Combine(
            Result<int>.Ok(1).WithSuccess("First"),
            Result<int>.Ok(2).WithSuccess("Second"));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, result.Successes.Count);
    }

    [TestMethod]
    public void Combine_Generic_Params_ShouldWork()
    {
        var result = Result<string>.Combine(
            Result<string>.Ok("a"),
            Result<string>.Ok("b"));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, result.Value!.Count());
    }

    #endregion

    #region CombineParallelAsync — Generic Result<T>

    [TestMethod]
    public async Task CombineParallelAsync_Generic_AllSuccess_ShouldReturnValues()
    {
        var result = await Result<int>.CombineParallelAsync(new[]
        {
            Task.FromResult(Result<int>.Ok(10)),
            Task.FromResult(Result<int>.Ok(20))
        });

        Assert.IsTrue(result.IsSuccess);
        CollectionAssert.AreEqual(
            new[] { 10, 20 },
            result.Value!.ToList());
    }

    [TestMethod]
    public async Task CombineParallelAsync_Generic_AnyFailed_ShouldReturnFailed()
    {
        var result = await Result<int>.CombineParallelAsync(new[]
        {
            Task.FromResult(Result<int>.Ok(10)),
            Task.FromResult(Result<int>.Fail("Error"))
        });

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public async Task CombineParallelAsync_Generic_Empty_ShouldReturnOk()
    {
        var result = await Result<int>.CombineParallelAsync(
            Array.Empty<Task<Result<int>>>());

        Assert.IsTrue(result.IsSuccess);
    }

    #endregion
}
