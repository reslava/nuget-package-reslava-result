using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.AdvancedPatterns;

namespace REslava.Result.Tests.AdvancedPatterns;

[TestClass]
public class OneOf3Tests
{
    #region Construction Tests

    [TestMethod]
    public void OneOf3_FromT1_ShouldCreateT1Instance()
    {
        // Arrange
        var error = new TestError("Test error");

        // Act
        var oneOf = OneOf<TestError, string, int>.FromT1(error);

        // Assert
        Assert.IsTrue(oneOf.IsT1);
        Assert.IsFalse(oneOf.IsT2);
        Assert.IsFalse(oneOf.IsT3);
        Assert.AreEqual(error, oneOf.AsT1);
    }

    [TestMethod]
    public void OneOf3_FromT2_ShouldCreateT2Instance()
    {
        // Arrange
        var value = "test value";

        // Act
        var oneOf = OneOf<TestError, string, int>.FromT2(value);

        // Assert
        Assert.IsFalse(oneOf.IsT1);
        Assert.IsTrue(oneOf.IsT2);
        Assert.IsFalse(oneOf.IsT3);
        Assert.AreEqual(value, oneOf.AsT2);
    }

    [TestMethod]
    public void OneOf3_FromT3_ShouldCreateT3Instance()
    {
        // Arrange
        var value = 42;

        // Act
        var oneOf = OneOf<TestError, string, int>.FromT3(value);

        // Assert
        Assert.IsFalse(oneOf.IsT1);
        Assert.IsFalse(oneOf.IsT2);
        Assert.IsTrue(oneOf.IsT3);
        Assert.AreEqual(value, oneOf.AsT3);
    }

    [TestMethod]
    public void OneOf3_ImplicitConversionT1_ShouldCreateT1Instance()
    {
        // Arrange
        var error = new TestError("Test error");

        // Act
        OneOf<TestError, string, int> oneOf = error;

        // Assert
        Assert.IsTrue(oneOf.IsT1);
        Assert.AreEqual(error, oneOf.AsT1);
    }

    [TestMethod]
    public void OneOf3_ImplicitConversionT2_ShouldCreateT2Instance()
    {
        // Arrange
        var value = "test value";

        // Act
        OneOf<TestError, string, int> oneOf = value;

        // Assert
        Assert.IsTrue(oneOf.IsT2);
        Assert.AreEqual(value, oneOf.AsT2);
    }

    [TestMethod]
    public void OneOf3_ImplicitConversionT3_ShouldCreateT3Instance()
    {
        // Arrange
        var value = 42;

        // Act
        OneOf<TestError, string, int> oneOf = value;

        // Assert
        Assert.IsTrue(oneOf.IsT3);
        Assert.AreEqual(value, oneOf.AsT3);
    }

    #endregion

    #region Property Access Tests

    [TestMethod]
    public void OneOf3_AsT1_WhenT1_ShouldReturnValue()
    {
        // Arrange
        var error = new TestError("Test error");
        var oneOf = OneOf<TestError, string, int>.FromT1(error);

        // Act & Assert
        Assert.AreEqual(error, oneOf.AsT1);
    }

    [TestMethod]
    public void OneOf3_AsT1_WhenT2_ShouldThrowException()
    {
        // Arrange
        var oneOf = OneOf<TestError, string, int>.FromT2("test");

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
    public void OneOf3_AsT1_WhenT3_ShouldThrowException()
    {
        // Arrange
        var oneOf = OneOf<TestError, string, int>.FromT3(42);

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
    public void OneOf3_AsT2_WhenT2_ShouldReturnValue()
    {
        // Arrange
        var value = "test value";
        var oneOf = OneOf<TestError, string, int>.FromT2(value);

        // Act & Assert
        Assert.AreEqual(value, oneOf.AsT2);
    }

    [TestMethod]
    public void OneOf3_AsT2_WhenT1_ShouldThrowException()
    {
        // Arrange
        var oneOf = OneOf<TestError, string, int>.FromT1(new TestError("test"));

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

    [TestMethod]
    public void OneOf3_AsT2_WhenT3_ShouldThrowException()
    {
        // Arrange
        var oneOf = OneOf<TestError, string, int>.FromT3(42);

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

    [TestMethod]
    public void OneOf3_AsT3_WhenT3_ShouldReturnValue()
    {
        // Arrange
        var value = 42;
        var oneOf = OneOf<TestError, string, int>.FromT3(value);

        // Act & Assert
        Assert.AreEqual(value, oneOf.AsT3);
    }

    [TestMethod]
    public void OneOf3_AsT3_WhenT1_ShouldThrowException()
    {
        // Arrange
        var oneOf = OneOf<TestError, string, int>.FromT1(new TestError("test"));

        // Act & Assert
        try
        {
            var result = oneOf.AsT3;
            Assert.Fail("Expected InvalidOperationException was not thrown");
        }
        catch (InvalidOperationException)
        {
            // Expected exception
        }
    }

    [TestMethod]
    public void OneOf3_AsT3_WhenT2_ShouldThrowException()
    {
        // Arrange
        var oneOf = OneOf<TestError, string, int>.FromT2("test");

        // Act & Assert
        try
        {
            var result = oneOf.AsT3;
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
    public void OneOf3_Match_WithT1_ShouldExecuteCase1()
    {
        // Arrange
        var error = new TestError("Test error");
        var oneOf = OneOf<TestError, string, int>.FromT1(error);

        // Act
        var result = oneOf.Match(
            case1: e => $"Error: {e.Message}",
            case2: s => $"String: {s}",
            case3: i => $"Int: {i}"
        );

        // Assert
        Assert.AreEqual("Error: Test error", result);
    }

    [TestMethod]
    public void OneOf3_Match_WithT2_ShouldExecuteCase2()
    {
        // Arrange
        var value = "test value";
        var oneOf = OneOf<TestError, string, int>.FromT2(value);

        // Act
        var result = oneOf.Match(
            case1: e => $"Error: {e.Message}",
            case2: s => $"String: {s}",
            case3: i => $"Int: {i}"
        );

        // Assert
        Assert.AreEqual("String: test value", result);
    }

    [TestMethod]
    public void OneOf3_Match_WithT3_ShouldExecuteCase3()
    {
        // Arrange
        var value = 42;
        var oneOf = OneOf<TestError, string, int>.FromT3(value);

        // Act
        var result = oneOf.Match(
            case1: e => $"Error: {e.Message}",
            case2: s => $"String: {s}",
            case3: i => $"Int: {i}"
        );

        // Assert
        Assert.AreEqual("Int: 42", result);
    }

    [TestMethod]
    public void OneOf3_Match_WithNullCase1_ShouldThrowException()
    {
        // Arrange
        var oneOf = OneOf<TestError, string, int>.FromT1(new TestError("test"));

        // Act & Assert
        try
        {
            oneOf.Match(
                case1: null!,
                case2: s => s,
                case3: i => i.ToString()
            );
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    [TestMethod]
    public void OneOf3_Match_WithNullCase2_ShouldThrowException()
    {
        // Arrange
        var oneOf = OneOf<TestError, string, int>.FromT1(new TestError("test"));

        // Act & Assert
        try
        {
            oneOf.Match(
                case1: e => e.Message,
                case2: null!,
                case3: i => i.ToString()
            );
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    [TestMethod]
    public void OneOf3_Match_WithNullCase3_ShouldThrowException()
    {
        // Arrange
        var oneOf = OneOf<TestError, string, int>.FromT1(new TestError("test"));

        // Act & Assert
        try
        {
            oneOf.Match(
                case1: e => e.Message,
                case2: s => s,
                case3: null!
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
    public void OneOf3_Switch_WithT1_ShouldExecuteCase1()
    {
        // Arrange
        var error = new TestError("Test error");
        var oneOf = OneOf<TestError, string, int>.FromT1(error);
        var executed = false;

        // Act
        oneOf.Switch(
            case1: e => { executed = true; },
            case2: s => { executed = false; },
            case3: i => { executed = false; }
        );

        // Assert
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public void OneOf3_Switch_WithT2_ShouldExecuteCase2()
    {
        // Arrange
        var value = "test value";
        var oneOf = OneOf<TestError, string, int>.FromT2(value);
        var executed = false;

        // Act
        oneOf.Switch(
            case1: e => { executed = false; },
            case2: s => { executed = true; },
            case3: i => { executed = false; }
        );

        // Assert
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public void OneOf3_Switch_WithT3_ShouldExecuteCase3()
    {
        // Arrange
        var value = 42;
        var oneOf = OneOf<TestError, string, int>.FromT3(value);
        var executed = false;

        // Act
        oneOf.Switch(
            case1: e => { executed = false; },
            case2: s => { executed = false; },
            case3: i => { executed = true; }
        );

        // Assert
        Assert.IsTrue(executed);
    }

    #endregion

    #region Map Tests

    [TestMethod]
    public void OneOf3_MapT2_WithT1_ShouldPropagateT1()
    {
        // Arrange
        var error = new TestError("Test error");
        var oneOf = OneOf<TestError, string, int>.FromT1(error);

        // Act
        var result = oneOf.MapT2(s => s.ToUpper());

        // Assert
        Assert.IsTrue(result.IsT1);
        Assert.AreEqual(error, result.AsT1);
    }

    [TestMethod]
    public void OneOf3_MapT2_WithT2_ShouldTransformT2()
    {
        // Arrange
        var value = "test value";
        var oneOf = OneOf<TestError, string, int>.FromT2(value);

        // Act
        var result = oneOf.MapT2(s => s.ToUpper());

        // Assert
        Assert.IsTrue(result.IsT2);
        Assert.AreEqual("TEST VALUE", result.AsT2);
    }

    [TestMethod]
    public void OneOf3_MapT2_WithT3_ShouldPropagateT3()
    {
        // Arrange
        var value = 42;
        var oneOf = OneOf<TestError, string, int>.FromT3(value);

        // Act
        var result = oneOf.MapT2(s => s.ToUpper());

        // Assert
        Assert.IsTrue(result.IsT3);
        Assert.AreEqual(value, result.AsT3);
    }

    [TestMethod]
    public void OneOf3_MapT3_WithT1_ShouldPropagateT1()
    {
        // Arrange
        var error = new TestError("Test error");
        var oneOf = OneOf<TestError, string, int>.FromT1(error);

        // Act
        var result = oneOf.MapT3(i => i * 2);

        // Assert
        Assert.IsTrue(result.IsT1);
        Assert.AreEqual(error, result.AsT1);
    }

    [TestMethod]
    public void OneOf3_MapT3_WithT2_ShouldPropagateT2()
    {
        // Arrange
        var value = "test value";
        var oneOf = OneOf<TestError, string, int>.FromT2(value);

        // Act
        var result = oneOf.MapT3(i => i * 2);

        // Assert
        Assert.IsTrue(result.IsT2);
        Assert.AreEqual(value, result.AsT2);
    }

    [TestMethod]
    public void OneOf3_MapT3_WithT3_ShouldTransformT3()
    {
        // Arrange
        var value = 42;
        var oneOf = OneOf<TestError, string, int>.FromT3(value);

        // Act
        var result = oneOf.MapT3(i => i * 2);

        // Assert
        Assert.IsTrue(result.IsT3);
        Assert.AreEqual(84, result.AsT3);
    }

    [TestMethod]
    public void OneOf3_MapT2_WithNullMapper_ShouldThrowException()
    {
        // Arrange
        var oneOf = OneOf<TestError, string, int>.FromT2("test");

        // Act & Assert
        try
        {
            oneOf.MapT2((Func<string, int>)null!);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    [TestMethod]
    public void OneOf3_MapT3_WithNullMapper_ShouldThrowException()
    {
        // Arrange
        var oneOf = OneOf<TestError, string, int>.FromT3(42);

        // Act & Assert
        try
        {
            oneOf.MapT3((Func<int, string>)null!);
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
    public void OneOf3_BindT2_WithT1_ShouldPropagateT1()
    {
        // Arrange
        var error = new TestError("Test error");
        var oneOf = OneOf<TestError, string, int>.FromT1(error);

        // Act
        var result = oneOf.BindT2(s => OneOf<TestError, int, int>.FromT2(s.Length));

        // Assert
        Assert.IsTrue(result.IsT1);
        Assert.AreEqual(error, result.AsT1);
    }

    [TestMethod]
    public void OneOf3_BindT2_WithT2_ShouldApplyBinder()
    {
        // Arrange
        var value = "test value";
        var oneOf = OneOf<TestError, string, int>.FromT2(value);

        // Act
        var result = oneOf.BindT2(s => OneOf<TestError, int, int>.FromT2(s.Length));

        // Assert
        Assert.IsTrue(result.IsT2);
        Assert.AreEqual(10, result.AsT2);
    }

    [TestMethod]
    public void OneOf3_BindT2_WithT3_ShouldPropagateT3()
    {
        // Arrange
        var value = 42;
        var oneOf = OneOf<TestError, string, int>.FromT3(value);

        // Act
        var result = oneOf.BindT2(s => OneOf<TestError, int, int>.FromT2(s.Length));

        // Assert
        Assert.IsTrue(result.IsT3);
        Assert.AreEqual(value, result.AsT3);
    }

    [TestMethod]
    public void OneOf3_BindT3_WithT1_ShouldPropagateT1()
    {
        // Arrange
        var error = new TestError("Test error");
        var oneOf = OneOf<TestError, string, int>.FromT1(error);

        // Act
        var result = oneOf.BindT3(i => OneOf<TestError, string, string>.FromT3(i.ToString()));

        // Assert
        Assert.IsTrue(result.IsT1);
        Assert.AreEqual(error, result.AsT1);
    }

    [TestMethod]
    public void OneOf3_BindT3_WithT2_ShouldPropagateT2()
    {
        // Arrange
        var value = "test value";
        var oneOf = OneOf<TestError, string, int>.FromT2(value);

        // Act
        var result = oneOf.BindT3(i => OneOf<TestError, string, string>.FromT3(i.ToString()));

        // Assert
        Assert.IsTrue(result.IsT2);
        Assert.AreEqual(value, result.AsT2);
    }

    [TestMethod]
    public void OneOf3_BindT3_WithT3_ShouldApplyBinder()
    {
        // Arrange
        var value = 42;
        var oneOf = OneOf<TestError, string, int>.FromT3(value);

        // Act
        var result = oneOf.BindT3(i => OneOf<TestError, string, string>.FromT3(i.ToString()));

        // Assert
        Assert.IsTrue(result.IsT3);
        Assert.AreEqual("42", result.AsT3);
    }

    #endregion

    #region Equality Tests

    [TestMethod]
    public void OneOf3_Equals_WithSameT1Values_ShouldBeEqual()
    {
        // Arrange
        var error1 = new TestError("Test error");
        var error2 = new TestError("Test error");
        var oneOf1 = OneOf<TestError, string, int>.FromT1(error1);
        var oneOf2 = OneOf<TestError, string, int>.FromT1(error2);

        // Act & Assert
        Assert.IsTrue(oneOf1.Equals(oneOf2));
        Assert.IsTrue(oneOf1 == oneOf2);
        Assert.IsFalse(oneOf1 != oneOf2);
    }

    [TestMethod]
    public void OneOf3_Equals_WithSameT2Values_ShouldBeEqual()
    {
        // Arrange
        var oneOf1 = OneOf<TestError, string, int>.FromT2("test");
        var oneOf2 = OneOf<TestError, string, int>.FromT2("test");

        // Act & Assert
        Assert.IsTrue(oneOf1.Equals(oneOf2));
        Assert.IsTrue(oneOf1 == oneOf2);
        Assert.IsFalse(oneOf1 != oneOf2);
    }

    [TestMethod]
    public void OneOf3_Equals_WithSameT3Values_ShouldBeEqual()
    {
        // Arrange
        var oneOf1 = OneOf<TestError, string, int>.FromT3(42);
        var oneOf2 = OneOf<TestError, string, int>.FromT3(42);

        // Act & Assert
        Assert.IsTrue(oneOf1.Equals(oneOf2));
        Assert.IsTrue(oneOf1 == oneOf2);
        Assert.IsFalse(oneOf1 != oneOf2);
    }

    [TestMethod]
    public void OneOf3_Equals_WithDifferentTypes_ShouldNotBeEqual()
    {
        // Arrange
        var error = new TestError("Test error");
        var oneOf1 = OneOf<TestError, string, int>.FromT1(error);
        var oneOf2 = OneOf<TestError, string, int>.FromT2("test");

        // Act & Assert
        Assert.IsFalse(oneOf1.Equals(oneOf2));
        Assert.IsFalse(oneOf1 == oneOf2);
        Assert.IsTrue(oneOf1 != oneOf2);
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void OneOf3_ToString_WithT1_ShouldReturnT1Representation()
    {
        // Arrange
        var error = new TestError("Test error");
        var oneOf = OneOf<TestError, string, int>.FromT1(error);

        // Act
        var result = oneOf.ToString();

        // Assert
        Assert.IsTrue(result.Contains("OneOf<"));
        Assert.IsTrue(result.Contains("T1:"));
        Assert.IsTrue(result.Contains("TestError"));
    }

    [TestMethod]
    public void OneOf3_ToString_WithT2_ShouldReturnT2Representation()
    {
        // Arrange
        var value = "test value";
        var oneOf = OneOf<TestError, string, int>.FromT2(value);

        // Act
        var result = oneOf.ToString();

        // Assert
        Assert.IsTrue(result.Contains("OneOf<"));
        Assert.IsTrue(result.Contains("T2:"));
        Assert.IsTrue(result.Contains("String"));
    }

    [TestMethod]
    public void OneOf3_ToString_WithT3_ShouldReturnT3Representation()
    {
        // Arrange
        var value = 42;
        var oneOf = OneOf<TestError, string, int>.FromT3(value);

        // Act
        var result = oneOf.ToString();

        // Assert
        Assert.IsTrue(result.Contains("OneOf<"));
        Assert.IsTrue(result.Contains("T3:"));
        Assert.IsTrue(result.Contains("Int32"));
    }

    #endregion

    #region GetHashCode Tests

    [TestMethod]
    public void OneOf3_GetHashCode_WithEqualValues_ShouldBeEqual()
    {
        // Arrange
        var oneOf1 = OneOf<TestError, string, int>.FromT2("test");
        var oneOf2 = OneOf<TestError, string, int>.FromT2("test");

        // Act
        var hash1 = oneOf1.GetHashCode();
        var hash2 = oneOf2.GetHashCode();

        // Assert
        Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void OneOf3_GetHashCode_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var oneOf1 = OneOf<TestError, string, int>.FromT2("test1");
        var oneOf2 = OneOf<TestError, string, int>.FromT2("test2");

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
