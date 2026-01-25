using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.AdvancedPatterns;

namespace REslava.Result.Tests.AdvancedPatterns
{
    [TestClass]
    public class MaybeTests
    {
        #region Constructor and Factory Methods

        [TestMethod]
        public void Maybe_Some_ShouldCreateMaybeWithValue()
        {
            // Arrange
            var value = "test";

            // Act
            var maybe = Maybe<string>.Some(value);

            // Assert
            Assert.IsTrue(maybe.HasValue);
            Assert.AreEqual(value, maybe.Value);
            Assert.AreEqual("Some(test)", maybe.ToString());
        }

        [TestMethod]
        public void Maybe_None_ShouldCreateEmptyMaybe()
        {
            // Act
            var maybe = Maybe<string>.None;

            // Assert
            Assert.IsFalse(maybe.HasValue);
            Assert.AreEqual("None", maybe.ToString());
        }

        [TestMethod]
        public void Maybe_Value_WhenNoValue_ShouldThrowException()
        {
            // Arrange
            var maybe = Maybe<string>.None;

            // Act & Assert
            try
            {
                var value = maybe.Value;
                Assert.Fail("Expected InvalidOperationException was not thrown");
            }
            catch (InvalidOperationException)
            {
                // Expected exception - test passes
            }
        }

        #endregion

        #region Implicit Conversion

        [TestMethod]
        public void Maybe_ImplicitConversion_FromValue_ShouldCreateSome()
        {
            // Arrange
            string value = "test";

            // Act
            Maybe<string> maybe = value;

            // Assert
            Assert.IsTrue(maybe.HasValue);
            Assert.AreEqual(value, maybe.Value);
        }

        #endregion

        #region Map Operation

        [TestMethod]
        public void Maybe_Map_WithSome_ShouldTransformValue()
        {
            // Arrange
            var maybe = Maybe<int>.Some(5);

            // Act
            var result = maybe.Map(x => x * 2);

            // Assert
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(10, result.Value);
        }

        [TestMethod]
        public void Maybe_Map_WithNone_ShouldReturnNone()
        {
            // Arrange
            var maybe = Maybe<int>.None;

            // Act
            var result = maybe.Map(x => x * 2);

            // Assert
            Assert.IsFalse(result.HasValue);
        }

        [TestMethod]
        public void Maybe_Map_WithNullMapper_ShouldThrowException()
        {
            // Arrange
            var maybe = Maybe<int>.Some(5);

            // Act & Assert
            try
            {
                var result = maybe.Map<int>(null!);
                Assert.Fail("Expected ArgumentNullException was not thrown");
            }
            catch (ArgumentNullException)
            {
                // Expected exception - test passes
            }
        }

        #endregion

        #region Bind Operation

        [TestMethod]
        public void Maybe_Bind_WithSome_ShouldChainMaybeOperations()
        {
            // Arrange
            var maybe = Maybe<string>.Some("hello");

            // Act
            var result = maybe.Bind(s => Maybe<int>.Some(s.Length));

            // Assert
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(5, result.Value);
        }

        [TestMethod]
        public void Maybe_Bind_WithNone_ShouldReturnNone()
        {
            // Arrange
            var maybe = Maybe<string>.None;

            // Act
            var result = maybe.Bind(s => Maybe<int>.Some(s.Length));

            // Assert
            Assert.IsFalse(result.HasValue);
        }

        [TestMethod]
        public void Maybe_Bind_WithBinderReturningNone_ShouldReturnNone()
        {
            // Arrange
            var maybe = Maybe<string>.Some("test");

            // Act
            var result = maybe.Bind(s => Maybe<int>.None);

            // Assert
            Assert.IsFalse(result.HasValue);
        }

        #endregion

        #region Filter Operation

        [TestMethod]
        public void Maybe_Filter_WithMatchingPredicate_ShouldReturnSome()
        {
            // Arrange
            var maybe = Maybe<int>.Some(10);

            // Act
            var result = maybe.Filter(x => x > 5);

            // Assert
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(10, result.Value);
        }

        [TestMethod]
        public void Maybe_Filter_WithNonMatchingPredicate_ShouldReturnNone()
        {
            // Arrange
            var maybe = Maybe<int>.Some(3);

            // Act
            var result = maybe.Filter(x => x > 5);

            // Assert
            Assert.IsFalse(result.HasValue);
        }

        [TestMethod]
        public void Maybe_Filter_WithNone_ShouldReturnNone()
        {
            // Arrange
            var maybe = Maybe<int>.None;

            // Act
            var result = maybe.Filter(x => x > 5);

            // Assert
            Assert.IsFalse(result.HasValue);
        }

        #endregion

        #region ValueOrDefault

        [TestMethod]
        public void Maybe_ValueOrDefault_WithSome_ShouldReturnValue()
        {
            // Arrange
            var maybe = Maybe<string>.Some("test");

            // Act
            var result = maybe.ValueOrDefault("default");

            // Assert
            Assert.AreEqual("test", result);
        }

        [TestMethod]
        public void Maybe_ValueOrDefault_WithNone_ShouldReturnDefault()
        {
            // Arrange
            var maybe = Maybe<string>.None;

            // Act
            var result = maybe.ValueOrDefault("default");

            // Assert
            Assert.AreEqual("default", result);
        }

        #endregion

        #region Match Operation

        [TestMethod]
        public void Maybe_Match_WithSome_ShouldExecuteSomeFunction()
        {
            // Arrange
            var maybe = Maybe<int>.Some(5);

            // Act
            var result = maybe.Match(
                some: x => x * 2,
                none: () => 0
            );

            // Assert
            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public void Maybe_Match_WithNone_ShouldExecuteNoneFunction()
        {
            // Arrange
            var maybe = Maybe<int>.None;

            // Act
            var result = maybe.Match(
                some: x => x * 2,
                none: () => 0
            );

            // Assert
            Assert.AreEqual(0, result);
        }

        #endregion

        #region Tap Operations

        [TestMethod]
        public void Maybe_Tap_WithSome_ShouldExecuteAction()
        {
            // Arrange
            var maybe = Maybe<int>.Some(5);
            var executed = false;

            // Act
            var result = maybe.Tap(x => executed = true);

            // Assert
            Assert.IsTrue(executed);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(5, result.Value);
        }

        [TestMethod]
        public void Maybe_Tap_WithNone_ShouldNotExecuteAction()
        {
            // Arrange
            var maybe = Maybe<int>.None;
            var executed = false;

            // Act
            var result = maybe.Tap(x => executed = true);

            // Assert
            Assert.IsFalse(executed);
            Assert.IsFalse(result.HasValue);
        }

        [TestMethod]
        public void Maybe_TapNone_WithNone_ShouldExecuteAction()
        {
            // Arrange
            var maybe = Maybe<int>.None;
            var executed = false;

            // Act
            var result = maybe.TapNone(() => executed = true);

            // Assert
            Assert.IsTrue(executed);
            Assert.IsFalse(result.HasValue);
        }

        [TestMethod]
        public void Maybe_TapNone_WithSome_ShouldNotExecuteAction()
        {
            // Arrange
            var maybe = Maybe<int>.Some(5);
            var executed = false;

            // Act
            var result = maybe.TapNone(() => executed = true);

            // Assert
            Assert.IsFalse(executed);
            Assert.IsTrue(result.HasValue);
        }

        #endregion

        #region Equality

        [TestMethod]
        public void Maybe_Equals_SameValue_ShouldBeEqual()
        {
            // Arrange
            var maybe1 = Maybe<int>.Some(5);
            var maybe2 = Maybe<int>.Some(5);

            // Act & Assert
            Assert.IsTrue(maybe1.Equals(maybe2));
            Assert.IsTrue(maybe1 == maybe2);
            Assert.IsFalse(maybe1 != maybe2);
        }

        [TestMethod]
        public void Maybe_Equals_DifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            var maybe1 = Maybe<int>.Some(5);
            var maybe2 = Maybe<int>.Some(10);

            // Act & Assert
            Assert.IsFalse(maybe1.Equals(maybe2));
            Assert.IsFalse(maybe1 == maybe2);
            Assert.IsTrue(maybe1 != maybe2);
        }

        [TestMethod]
        public void Maybe_Equals_BothNone_ShouldBeEqual()
        {
            // Arrange
            var maybe1 = Maybe<int>.None;
            var maybe2 = Maybe<int>.None;

            // Act & Assert
            Assert.IsTrue(maybe1.Equals(maybe2));
            Assert.IsTrue(maybe1 == maybe2);
            Assert.IsFalse(maybe1 != maybe2);
        }

        [TestMethod]
        public void Maybe_Equals_OneNoneOneSome_ShouldNotBeEqual()
        {
            // Arrange
            var maybe1 = Maybe<int>.None;
            var maybe2 = Maybe<int>.Some(5);

            // Act & Assert
            Assert.IsFalse(maybe1.Equals(maybe2));
            Assert.IsFalse(maybe1 == maybe2);
            Assert.IsTrue(maybe1 != maybe2);
        }

        #endregion

        #region Complex Scenarios

        [TestMethod]
        public void Maybe_ComplexChaining_ShouldWorkCorrectly()
        {
            // Arrange
            var maybe = Maybe<string>.Some("  hello world  ");

            // Act
            var result = maybe
                .Map(s => s.Trim())
                .Filter(s => s.Length > 0)
                .Map(s => s.ToUpper())
                .Bind(s => s.StartsWith("H") ? Maybe<string>.Some(s) : Maybe<string>.None)
                .Map(s => $"{s}!")
                .ValueOrDefault("default");

            // Assert
            Assert.AreEqual("HELLO WORLD!", result);
        }

        [TestMethod]
        public void Maybe_ComplexChaining_WithFailure_ShouldReturnDefault()
        {
            // Arrange
            var maybe = Maybe<string>.Some("test");

            // Act
            var result = maybe
                .Map(s => s.Trim())
                .Filter(s => s.Length > 10) // This will fail
                .Map(s => s.ToUpper())
                .ValueOrDefault("default");

            // Assert
            Assert.AreEqual("default", result);
        }

        #endregion
    }
}
