using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;

namespace REslava.Result.Tests.Factories;

[TestClass]
public sealed class ResultConditionalTests
{
    #region OkIf — Non-Generic

    [TestMethod]
    public void OkIf_ConditionTrue_WithMessage_ShouldReturnOk()
    {
        var result = Result.OkIf(true, "Error message");

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public void OkIf_ConditionFalse_WithMessage_ShouldReturnFail()
    {
        var result = Result.OkIf(false, "Must be valid");

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Must be valid", result.Errors[0].Message);
    }

    [TestMethod]
    public void OkIf_ConditionTrue_WithIError_ShouldReturnOk()
    {
        var error = new Error("Custom error");
        var result = Result.OkIf(true, error);

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public void OkIf_ConditionFalse_WithIError_ShouldReturnFailWithError()
    {
        var error = new Error("Custom error");
        var result = Result.OkIf(false, error);

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Custom error", result.Errors[0].Message);
    }

    [TestMethod]
    public void OkIf_ConditionTrue_WithSuccessMessage_ShouldReturnOkWithSuccess()
    {
        var result = Result.OkIf(true, "Error", "All good");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Successes.Count);
        Assert.AreEqual("All good", result.Successes[0].Message);
    }

    [TestMethod]
    public void OkIf_ConditionFalse_WithSuccessMessage_ShouldReturnFail()
    {
        var result = Result.OkIf(false, "Bad", "All good");

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Bad", result.Errors[0].Message);
    }

    [TestMethod]
    public void OkIf_LazyPredicate_True_ShouldReturnOk()
    {
        var result = Result.OkIf(() => true, "Error");

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public void OkIf_LazyPredicate_False_ShouldReturnFail()
    {
        var result = Result.OkIf(() => false, "Lazy failed");

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Lazy failed", result.Errors[0].Message);
    }

    [TestMethod]
    public void OkIf_LazyPredicate_Throws_ShouldReturnFailWithExceptionError()
    {
        var result = Result.OkIf(
            () => throw new InvalidOperationException("boom"),
            "Error");

        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
        Assert.AreEqual("boom", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task OkIfAsync_PredicateTrue_ShouldReturnOk()
    {
        var result = await Result.OkIfAsync(
            () => Task.FromResult(true),
            "Error");

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task OkIfAsync_PredicateFalse_ShouldReturnFail()
    {
        var result = await Result.OkIfAsync(
            () => Task.FromResult(false),
            "Async failed");

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Async failed", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task OkIfAsync_PredicateThrows_ShouldReturnFailWithExceptionError()
    {
        var result = await Result.OkIfAsync(
            () => throw new InvalidOperationException("async boom"),
            "Error");

        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
    }

    #endregion

    #region FailIf — Non-Generic

    [TestMethod]
    public void FailIf_ConditionTrue_ShouldReturnFail()
    {
        var result = Result.FailIf(true, "Should fail");

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Should fail", result.Errors[0].Message);
    }

    [TestMethod]
    public void FailIf_ConditionFalse_ShouldReturnOk()
    {
        var result = Result.FailIf(false, "Should fail");

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public void FailIf_ConditionTrue_WithIError_ShouldReturnFailWithError()
    {
        var error = new Error("Custom fail");
        var result = Result.FailIf(true, error);

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Custom fail", result.Errors[0].Message);
    }

    [TestMethod]
    public void FailIf_ConditionFalse_WithIError_ShouldReturnOk()
    {
        var error = new Error("Custom fail");
        var result = Result.FailIf(false, error);

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public void FailIf_ConditionFalse_WithSuccessMessage_ShouldReturnOkWithSuccess()
    {
        var result = Result.FailIf(false, "Error", "Passed");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Passed", result.Successes[0].Message);
    }

    [TestMethod]
    public void FailIf_ConditionTrue_WithSuccessMessage_ShouldReturnFail()
    {
        var result = Result.FailIf(true, "Error", "Passed");

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public void FailIf_LazyPredicate_True_ShouldReturnFail()
    {
        var result = Result.FailIf(() => true, "Lazy fail");

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public void FailIf_LazyPredicate_False_ShouldReturnOk()
    {
        var result = Result.FailIf(() => false, "Lazy fail");

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public void FailIf_LazyPredicate_Throws_ShouldReturnFailWithExceptionError()
    {
        var result = Result.FailIf(
            () => throw new InvalidOperationException("boom"),
            "Error");

        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
    }

    [TestMethod]
    public async Task FailIfAsync_PredicateTrue_ShouldReturnFail()
    {
        var result = await Result.FailIfAsync(
            () => Task.FromResult(true),
            "Async fail");

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public async Task FailIfAsync_PredicateFalse_ShouldReturnOk()
    {
        var result = await Result.FailIfAsync(
            () => Task.FromResult(false),
            "Async fail");

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task FailIfAsync_PredicateThrows_ShouldReturnFailWithExceptionError()
    {
        var result = await Result.FailIfAsync(
            () => throw new InvalidOperationException("async boom"),
            "Error");

        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
    }

    #endregion

    #region OkIf — Generic Result<T>

    [TestMethod]
    public void OkIf_Generic_ConditionTrue_ShouldReturnOkWithValue()
    {
        var result = Result<int>.OkIf(true, 42, "Error");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
    }

    [TestMethod]
    public void OkIf_Generic_ConditionFalse_ShouldReturnFail()
    {
        var result = Result<int>.OkIf(false, 42, "Not valid");

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Not valid", result.Errors[0].Message);
    }

    [TestMethod]
    public void OkIf_Generic_ConditionTrue_WithIError_ShouldReturnOkWithValue()
    {
        var result = Result<string>.OkIf(true, "hello", new Error("err"));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("hello", result.Value);
    }

    [TestMethod]
    public void OkIf_Generic_ConditionFalse_WithIError_ShouldReturnFail()
    {
        var result = Result<string>.OkIf(false, "hello", new Error("err"));

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("err", result.Errors[0].Message);
    }

    [TestMethod]
    public void OkIf_Generic_LazyValue_ConditionTrue_ShouldCreateValue()
    {
        var called = false;
        var result = Result<int>.OkIf(true, () => { called = true; return 99; }, "Error");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(99, result.Value);
        Assert.IsTrue(called);
    }

    [TestMethod]
    public void OkIf_Generic_LazyValue_ConditionFalse_ShouldNotCreateValue()
    {
        var called = false;
        var result = Result<int>.OkIf(false, () => { called = true; return 99; }, "Error");

        Assert.IsTrue(result.IsFailed);
        Assert.IsFalse(called);
    }

    [TestMethod]
    public void OkIf_Generic_LazyValue_Throws_ShouldReturnFail()
    {
        var result = Result<int>.OkIf(
            true,
            () => throw new InvalidOperationException("factory boom"),
            "Error");

        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
    }

    [TestMethod]
    public async Task OkIfAsync_Generic_ConditionTrue_ShouldReturnOkWithValue()
    {
        var result = await Result<int>.OkIfAsync(
            true,
            () => Task.FromResult(42),
            "Error");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
    }

    [TestMethod]
    public async Task OkIfAsync_Generic_ConditionFalse_ShouldNotCallFactory()
    {
        var called = false;
        var result = await Result<int>.OkIfAsync(
            false,
            () => { called = true; return Task.FromResult(42); },
            "Error");

        Assert.IsTrue(result.IsFailed);
        Assert.IsFalse(called);
    }

    [TestMethod]
    public void OkIf_Generic_LazyPredicateAndValue_BothTrue_ShouldReturnOk()
    {
        var result = Result<int>.OkIf(
            () => true,
            () => 100,
            "Error");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(100, result.Value);
    }

    [TestMethod]
    public void OkIf_Generic_LazyPredicateAndValue_PredicateFalse_ShouldReturnFail()
    {
        var valueCalled = false;
        var result = Result<int>.OkIf(
            () => false,
            () => { valueCalled = true; return 100; },
            "Error");

        Assert.IsTrue(result.IsFailed);
        Assert.IsFalse(valueCalled);
    }

    [TestMethod]
    public async Task OkIfAsync_Generic_LazyPredicateAndValue_ShouldWork()
    {
        var result = await Result<int>.OkIfAsync(
            () => Task.FromResult(true),
            () => Task.FromResult(77),
            "Error");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(77, result.Value);
    }

    [TestMethod]
    public async Task OkIfAsync_Generic_LazyPredicateThrows_ShouldReturnFail()
    {
        var result = await Result<int>.OkIfAsync(
            () => throw new InvalidOperationException("pred boom"),
            () => Task.FromResult(77),
            "Error");

        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
    }

    #endregion

    #region FailIf — Generic Result<T>

    [TestMethod]
    public void FailIf_Generic_ConditionTrue_ShouldReturnFail()
    {
        var result = Result<int>.FailIf(true, "Bad", 42);

        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Bad", result.Errors[0].Message);
    }

    [TestMethod]
    public void FailIf_Generic_ConditionFalse_ShouldReturnOkWithValue()
    {
        var result = Result<int>.FailIf(false, "Bad", 42);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
    }

    [TestMethod]
    public void FailIf_Generic_ConditionTrue_WithIError_ShouldReturnFail()
    {
        var result = Result<int>.FailIf(true, new Error("err"), 42);

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public void FailIf_Generic_ConditionFalse_WithIError_ShouldReturnOk()
    {
        var result = Result<int>.FailIf(false, new Error("err"), 42);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
    }

    [TestMethod]
    public void FailIf_Generic_LazyValue_ConditionFalse_ShouldCreateValue()
    {
        var result = Result<int>.FailIf(false, "Error", () => 99);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(99, result.Value);
    }

    [TestMethod]
    public void FailIf_Generic_LazyValue_ConditionTrue_ShouldNotCreateValue()
    {
        var called = false;
        var result = Result<int>.FailIf(true, "Error", () => { called = true; return 99; });

        Assert.IsTrue(result.IsFailed);
        Assert.IsFalse(called);
    }

    [TestMethod]
    public void FailIf_Generic_LazyPredicateAndValue_PredicateFalse_ShouldReturnOk()
    {
        var result = Result<int>.FailIf(
            () => false,
            "Error",
            () => 55);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(55, result.Value);
    }

    [TestMethod]
    public void FailIf_Generic_LazyPredicateAndValue_PredicateTrue_ShouldReturnFail()
    {
        var result = Result<int>.FailIf(
            () => true,
            "Failed",
            () => 55);

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public async Task FailIfAsync_Generic_PredicateTrue_ShouldReturnFail()
    {
        var result = await Result<int>.FailIfAsync(
            () => Task.FromResult(true),
            "Async fail",
            () => Task.FromResult(42));

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public async Task FailIfAsync_Generic_PredicateFalse_ShouldReturnOkWithValue()
    {
        var result = await Result<int>.FailIfAsync(
            () => Task.FromResult(false),
            "Async fail",
            () => Task.FromResult(42));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
    }

    [TestMethod]
    public async Task FailIfAsync_Generic_FactoryThrows_ShouldReturnFail()
    {
        var result = await Result<int>.FailIfAsync(
            () => Task.FromResult(false),
            "Error",
            () => throw new InvalidOperationException("factory boom"));

        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
    }

    #endregion

    #region Guard Validation

    [TestMethod]
    public void OkIf_NullErrorMessage_ShouldThrow()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => Result.OkIf(true, (string)null!));
    }

    [TestMethod]
    public void OkIf_EmptyErrorMessage_ShouldThrow()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => Result.OkIf(true, ""));
    }

    [TestMethod]
    public void FailIf_NullErrorMessage_ShouldThrow()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => Result.FailIf(true, (string)null!));
    }

    [TestMethod]
    public void OkIf_NullPredicate_ShouldThrow()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => Result.OkIf((Func<bool>)null!, "Error"));
    }

    #endregion
}
