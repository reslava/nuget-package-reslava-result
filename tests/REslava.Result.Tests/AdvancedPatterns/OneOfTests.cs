using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.AdvancedPatterns;

namespace REslava.Result.Tests.AdvancedPatterns;

[TestClass]
public class OneOfTests
{
    #region Construction Tests

    [TestMethod]
    public void OneOf_FromT1_ShouldCreateT1Instance()
    {
        // Arrange
        var error = new TestError("Test error");

        // Act
        var oneOf = OneOf<TestError, string>.FromT1(error);

        // Assert
        Assert.IsTrue(oneOf.IsT1);
        Assert.IsFalse(oneOf.IsT2);
        Assert.AreEqual(error, oneOf.AsT1);
    }

    [TestMethod]
    public void OneOf_FromT2_ShouldCreateT2Instance()
    {
        // Arrange
        var value = "test value";

        // Act
        var oneOf = OneOf<TestError, string>.FromT2(value);

        // Assert
        Assert.IsFalse(oneOf.IsT1);
        Assert.IsTrue(oneOf.IsT2);
        Assert.AreEqual(value, oneOf.AsT2);
    }

    [TestMethod]
    public void OneOf_ImplicitConversionT1_ShouldCreateT1Instance()
    {
        // Arrange
        var error = new TestError("Test error");

        // Act
        OneOf<TestError, string> oneOf = error;

        // Assert
        Assert.IsTrue(oneOf.IsT1);
        Assert.AreEqual(error, oneOf.AsT1);
    }

    [TestMethod]
    public void OneOf_ImplicitConversionT2_ShouldCreateT2Instance()
    {
        // Arrange
        var value = "test value";

        // Act
        OneOf<TestError, string> oneOf = value;

        // Assert
        Assert.IsTrue(oneOf.IsT2);
        Assert.AreEqual(value, oneOf.AsT2);
    }

    #endregion

    #region Property Access Tests

    [TestMethod]
    public void OneOf_AsT1_WhenT1_ShouldReturnValue()
    {
        // Arrange
        var error = new TestError("Test error");
        var oneOf = OneOf<TestError, string>.FromT1(error);

        // Act & Assert
        Assert.AreEqual(error, oneOf.AsT1);
    }

    [TestMethod]
    public void OneOf_AsT1_WhenT2_ShouldThrowException()
    {
        // Arrange
        var oneOf = OneOf<TestError, string>.FromT2("test");

        // Act & Assert
        try
        {
            var result = oneOf.AsT1;
            Assert.Fail("Expected InvalidOperationException was not thrown");
        }
        catch (InvalidOperationException)
        {
            // Expected exception
        }
    }

    [TestMethod]
    public void OneOf_AsT2_WhenT2_ShouldReturnValue()
    {
        // Arrange
        var value = "test value";
        var oneOf = OneOf<TestError, string>.FromT2(value);

        // Act & Assert
        Assert.AreEqual(value, oneOf.AsT2);
    }

    [TestMethod]
    public void OneOf_AsT2_WhenT1_ShouldThrowException()
    {
        // Arrange
        var oneOf = OneOf<TestError, string>.FromT1(new TestError("test"));

        // Act & Assert
        try
        {
            var result = oneOf.AsT2;
            Assert.Fail("Expected InvalidOperationException was not thrown");
        }
        catch (InvalidOperationException)
        {
            // Expected exception
        }
    }

    #endregion

    #region Match Tests

    [TestMethod]
    public void OneOf_Match_WithT1_ShouldExecuteCase1()
    {
        // Arrange
        var error = new TestError("Test error");
        var oneOf = OneOf<TestError, string>.FromT1(error);

        // Act
        var result = oneOf.Match(
            case1: e => $"Error: {e.Message}",
            case2: s => $"Value: {s}"
        );

        // Assert
        Assert.AreEqual("Error: Test error", result);
    }

    [TestMethod]
    public void OneOf_Match_WithT2_ShouldExecuteCase2()
    {
        // Arrange
        var value = "test value";
        var oneOf = OneOf<TestError, string>.FromT2(value);

        // Act
        var result = oneOf.Match(
            case1: e => $"Error: {e.Message}",
            case2: s => $"Value: {s}"
        );

        // Assert
        Assert.AreEqual("Value: test value", result);
    }

    [TestMethod]
    public void OneOf_Match_WithNullCase1_ShouldThrowException()
    {
        // Arrange
        var oneOf = OneOf<TestError, string>.FromT1(new TestError("test"));

        // Act & Assert
        try
        {
            oneOf.Match(
                case1: null!,
                case2: s => s
            );
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    [TestMethod]
    public void OneOf_Match_WithNullCase2_ShouldThrowException()
    {
        // Arrange
        var oneOf = OneOf<TestError, string>.FromT1(new TestError("test"));

        // Act & Assert
        try
        {
            oneOf.Match(
                case1: e => e.Message,
                case2: null!
            );
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    #endregion

    #region Switch Tests

    [TestMethod]
    public void OneOf_Switch_WithT1_ShouldExecuteCase1()
    {
        // Arrange
        var error = new TestError("Test error");
        var oneOf = OneOf<TestError, string>.FromT1(error);
        var executed = false;

        // Act
        oneOf.Switch(
            case1: e => { executed = true; },
            case2: s => { executed = false; }
        );

        // Assert
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public void OneOf_Switch_WithT2_ShouldExecuteCase2()
    {
        // Arrange
        var value = "test value";
        var oneOf = OneOf<TestError, string>.FromT2(value);
        var executed = false;

        // Act
        oneOf.Switch(
            case1: e => { executed = false; },
            case2: s => { executed = true; }
        );

        // Assert
        Assert.IsTrue(executed);
    }

    #endregion

    #region Map Tests

    [TestMethod]
    public void OneOf_Map_WithT1_ShouldPropagateT1()
    {
        // Arrange
        var error = new TestError("Test error");
        var oneOf = OneOf<TestError, string>.FromT1(error);

        // Act
        var result = oneOf.Map(s => s.ToUpper());

        // Assert
        Assert.IsTrue(result.IsT1);
        Assert.AreEqual(error, result.AsT1);
    }

    [TestMethod]
    public void OneOf_Map_WithT2_ShouldTransformT2()
    {
        // Arrange
        var value = "test value";
        var oneOf = OneOf<TestError, string>.FromT2(value);

        // Act
        var result = oneOf.Map(s => s.ToUpper());

        // Assert
        Assert.IsTrue(result.IsT2);
        Assert.AreEqual("TEST VALUE", result.AsT2);
    }

    [TestMethod]
    public void OneOf_Map_WithNullMapper_ShouldThrowException()
    {
        // Arrange
        var oneOf = OneOf<TestError, string>.FromT2("test");

        // Act & Assert
        try
        {
            oneOf.Map((Func<string, int>)null!);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    #endregion

    #region Bind Tests

    [TestMethod]
    public void OneOf_Bind_WithT1_ShouldPropagateT1()
    {
        // Arrange
        var error = new TestError("Test error");
        var oneOf = OneOf<TestError, string>.FromT1(error);

        // Act
        var result = oneOf.Bind(s => OneOf<TestError, int>.FromT2(s.Length));

        // Assert
        Assert.IsTrue(result.IsT1);
        Assert.AreEqual(error, result.AsT1);
    }

    [TestMethod]
    public void OneOf_Bind_WithT2_ShouldApplyBinder()
    {
        // Arrange
        var value = "test value";
        var oneOf = OneOf<TestError, string>.FromT2(value);

        // Act
        var result = oneOf.Bind(s => OneOf<TestError, int>.FromT2(s.Length));

        // Assert
        Assert.IsTrue(result.IsT2);
        Assert.AreEqual(10, result.AsT2);
    }

    [TestMethod]
    public void OneOf_Bind_WithT2ReturningT1_ShouldReturnT1()
    {
        // Arrange
        var value = "test value";
        var oneOf = OneOf<TestError, string>.FromT2(value);
        var error = new TestError("Bind error");

        // Act
        var result = oneOf.Bind(s => OneOf<TestError, int>.FromT1(error));

        // Assert
        Assert.IsTrue(result.IsT1);
        Assert.AreEqual(error, result.AsT1);
    }

    [TestMethod]
    public void OneOf_Bind_WithNullBinder_ShouldThrowException()
    {
        // Arrange
        var oneOf = OneOf<TestError, string>.FromT2("test");

        // Act & Assert
        try
        {
            oneOf.Bind((Func<string, OneOf<TestError, int>>)null!);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    #endregion

    #region Filter Tests

    [TestMethod]
    public void OneOf_Filter_WithT1_ShouldReturnT1()
    {
        // Arrange
        var error = new TestError("Test error");
        var oneOf = OneOf<TestError, string>.FromT1(error);
        var fallback = new TestError("Fallback error");

        // Act
        var result = oneOf.Filter(s => s.Length > 5, fallback);

        // Assert
        Assert.IsTrue(result.IsT1);
        Assert.AreEqual(error, result.AsT1);
    }

    [TestMethod]
    public void OneOf_Filter_WithT2AndPredicateTrue_ShouldReturnT2()
    {
        // Arrange
        var value = "test value";
        var oneOf = OneOf<TestError, string>.FromT2(value);
        var fallback = new TestError("Fallback error");

        // Act
        var result = oneOf.Filter(s => s.Length > 5, fallback);

        // Assert
        Assert.IsTrue(result.IsT2);
        Assert.AreEqual(value, result.AsT2);
    }

    [TestMethod]
    public void OneOf_Filter_WithT2AndPredicateFalse_ShouldReturnFallbackT1()
    {
        // Arrange
        var value = "test";
        var oneOf = OneOf<TestError, string>.FromT2(value);
        var fallback = new TestError("Fallback error");

        // Act
        var result = oneOf.Filter(s => s.Length > 5, fallback);

        // Assert
        Assert.IsTrue(result.IsT1);
        Assert.AreEqual(fallback, result.AsT1);
    }

    [TestMethod]
    public void OneOf_Filter_WithNullPredicate_ShouldThrowException()
    {
        // Arrange
        var oneOf = OneOf<TestError, string>.FromT2("test");

        // Act & Assert
        try
        {
            oneOf.Filter((Func<string, bool>)null!, new TestError("fallback"));
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    #endregion

    #region Equality Tests

    [TestMethod]
    public void OneOf_Equals_WithSameT1Values_ShouldBeEqual()
    {
        // Arrange
        var error1 = new TestError("Test error");
        var error2 = new TestError("Test error");
        var oneOf1 = OneOf<TestError, string>.FromT1(error1);
        var oneOf2 = OneOf<TestError, string>.FromT1(error2);

        // Act & Assert
        Assert.IsTrue(oneOf1.Equals(oneOf2));
        Assert.IsTrue(oneOf1 == oneOf2);
        Assert.IsFalse(oneOf1 != oneOf2);
    }

    [TestMethod]
    public void OneOf_Equals_WithSameT2Values_ShouldBeEqual()
    {
        // Arrange
        var oneOf1 = OneOf<TestError, string>.FromT2("test");
        var oneOf2 = OneOf<TestError, string>.FromT2("test");

        // Act & Assert
        Assert.IsTrue(oneOf1.Equals(oneOf2));
        Assert.IsTrue(oneOf1 == oneOf2);
        Assert.IsFalse(oneOf1 != oneOf2);
    }

    [TestMethod]
    public void OneOf_Equals_WithDifferentTypes_ShouldNotBeEqual()
    {
        // Arrange
        var error = new TestError("Test error");
        var oneOf1 = OneOf<TestError, string>.FromT1(error);
        var oneOf2 = OneOf<TestError, string>.FromT2("test");

        // Act & Assert
        Assert.IsFalse(oneOf1.Equals(oneOf2));
        Assert.IsFalse(oneOf1 == oneOf2);
        Assert.IsTrue(oneOf1 != oneOf2);
    }

    [TestMethod]
    public void OneOf_Equals_WithDifferentT1Values_ShouldNotBeEqual()
    {
        // Arrange
        var error1 = new TestError("Error 1");
        var error2 = new TestError("Error 2");
        var oneOf1 = OneOf<TestError, string>.FromT1(error1);
        var oneOf2 = OneOf<TestError, string>.FromT1(error2);

        // Act & Assert
        Assert.IsFalse(oneOf1.Equals(oneOf2));
        Assert.IsFalse(oneOf1 == oneOf2);
        Assert.IsTrue(oneOf1 != oneOf2);
    }

    [TestMethod]
    public void OneOf_Equals_WithDifferentT2Values_ShouldNotBeEqual()
    {
        // Arrange
        var oneOf1 = OneOf<TestError, string>.FromT2("test1");
        var oneOf2 = OneOf<TestError, string>.FromT2("test2");

        // Act & Assert
        Assert.IsFalse(oneOf1.Equals(oneOf2));
        Assert.IsFalse(oneOf1 == oneOf2);
        Assert.IsTrue(oneOf1 != oneOf2);
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void OneOf_ToString_WithT1_ShouldReturnT1Representation()
    {
        // Arrange
        var error = new TestError("Test error");
        var oneOf = OneOf<TestError, string>.FromT1(error);

        // Act
        var result = oneOf.ToString();

        // Assert
        Assert.IsTrue(result.Contains("OneOf<"));
        Assert.IsTrue(result.Contains("T1:"));
        Assert.IsTrue(result.Contains("TestError"));
    }

    [TestMethod]
    public void OneOf_ToString_WithT2_ShouldReturnT2Representation()
    {
        // Arrange
        var value = "test value";
        var oneOf = OneOf<TestError, string>.FromT2(value);

        // Act
        var result = oneOf.ToString();

        // Assert
        Assert.IsTrue(result.Contains("OneOf<"));
        Assert.IsTrue(result.Contains("T2:"));
        Assert.IsTrue(result.Contains("String"));
    }

    #endregion

    #region GetHashCode Tests

    [TestMethod]
    public void OneOf_GetHashCode_WithEqualValues_ShouldBeEqual()
    {
        // Arrange
        var oneOf1 = OneOf<TestError, string>.FromT2("test");
        var oneOf2 = OneOf<TestError, string>.FromT2("test");

        // Act
        var hash1 = oneOf1.GetHashCode();
        var hash2 = oneOf2.GetHashCode();

        // Assert
        Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void OneOf_GetHashCode_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var oneOf1 = OneOf<TestError, string>.FromT2("test1");
        var oneOf2 = OneOf<TestError, string>.FromT2("test2");

        // Act
        var hash1 = oneOf1.GetHashCode();
        var hash2 = oneOf2.GetHashCode();

        // Assert
        Assert.AreNotEqual(hash1, hash2);
    }

    #endregion

    #region Helper Classes

    private class TestError : IEquatable<TestError>
    {
        public string Message { get; }

        public TestError(string message)
        {
            Message = message;
        }

        public bool Equals(TestError? other)
        {
            return other != null && Message == other.Message;
        }

        public override bool Equals(object? obj)
        {
            return obj is TestError other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Message.GetHashCode();
        }

        public override string ToString()
        {
            return $"TestError: {Message}";
        }
    }

    #endregion
}
