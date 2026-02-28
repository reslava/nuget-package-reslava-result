using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.AdvancedPatterns;

namespace REslava.Result.Tests.AdvancedPatterns
{
    [TestClass]
    public sealed class MaybeResultInteropTests
    {
        #region Maybe → Result (factory overload)

        [TestMethod]
        public void ToResult_Factory_Some_ReturnsOk()
        {
            var maybe = Maybe<int>.Some(42);

            var result = maybe.ToResult(() => new Error("not found"));

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(42, result.Value);
        }

        [TestMethod]
        public void ToResult_Factory_None_ReturnsFailWithFactoryError()
        {
            var maybe = Maybe<int>.None;

            var result = maybe.ToResult(() => new Error("not found"));

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual("not found", result.Errors[0].Message);
        }

        [TestMethod]
        public void ToResult_Factory_None_FactoryInvokedOnce()
        {
            var maybe = Maybe<int>.None;
            var callCount = 0;

            maybe.ToResult(() => { callCount++; return new Error("err"); });

            Assert.AreEqual(1, callCount);
        }

        [TestMethod]
        public void ToResult_Factory_Some_FactoryNotInvoked()
        {
            var maybe = Maybe<int>.Some(1);
            var callCount = 0;

            maybe.ToResult(() => { callCount++; return new Error("err"); });

            Assert.AreEqual(0, callCount);
        }

        [TestMethod]
        public void ToResult_Factory_NullFactory_ThrowsArgumentNullException()
        {
            var maybe = Maybe<int>.Some(1);

            Assert.ThrowsExactly<ArgumentNullException>(() =>
                maybe.ToResult((Func<IError>)null!));
        }

        #endregion

        #region Maybe → Result (IError overload)

        [TestMethod]
        public void ToResult_Error_Some_ReturnsOk()
        {
            var maybe = Maybe<string>.Some("hello");

            var result = maybe.ToResult(new Error("unused"));

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("hello", result.Value);
        }

        [TestMethod]
        public void ToResult_Error_None_ReturnsFailWithError()
        {
            var maybe = Maybe<string>.None;

            var result = maybe.ToResult(new Error("user not found"));

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual("user not found", result.Errors[0].Message);
        }

        [TestMethod]
        public void ToResult_Error_NullError_ThrowsArgumentNullException()
        {
            var maybe = Maybe<string>.None;

            Assert.ThrowsExactly<ArgumentNullException>(() =>
                maybe.ToResult((IError)null!));
        }

        #endregion

        #region Maybe → Result (string overload)

        [TestMethod]
        public void ToResult_String_Some_ReturnsOk()
        {
            var maybe = Maybe<int>.Some(99);

            var result = maybe.ToResult("not found");

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(99, result.Value);
        }

        [TestMethod]
        public void ToResult_String_None_ReturnsFailWithMessage()
        {
            var maybe = Maybe<int>.None;

            var result = maybe.ToResult("record missing");

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual("record missing", result.Errors[0].Message);
        }

        [TestMethod]
        public void ToResult_String_NullOrEmptyMessage_ThrowsArgumentException()
        {
            var maybe = Maybe<int>.None;

            Assert.ThrowsExactly<ArgumentException>(() => maybe.ToResult((string)null!));
            Assert.ThrowsExactly<ArgumentException>(() => maybe.ToResult(""));
        }

        #endregion

        #region Result → Maybe

        [TestMethod]
        public void ToMaybe_Success_ReturnsSome()
        {
            var result = Result<string>.Ok("alice");

            var maybe = result.ToMaybe();

            Assert.IsTrue(maybe.HasValue);
            Assert.AreEqual("alice", maybe.Value);
        }

        [TestMethod]
        public void ToMaybe_Failure_ReturnsNone()
        {
            var result = Result<string>.Fail("not found");

            var maybe = result.ToMaybe();

            Assert.IsFalse(maybe.HasValue);
        }

        #endregion
    }
}
