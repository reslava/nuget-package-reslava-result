using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.AdvancedPatterns;
using static REslava.Result.AdvancedPatterns.OneOfExtensions;

namespace REslava.Result.Tests.AdvancedPatterns;

[TestClass]
public class OneOfExtensionsTests
{
    #region ToThreeWay Tests

    [TestMethod]
    public void ToThreeWay_WithT1_ShouldCreateT1Instance()
    {
        // Arrange
        var error = new TestError("Test error");
        var twoWay = OneOf<TestError, string>.FromT1(error);

        // Act
        var threeWay = OneOfExtensions.ToThreeWay(twoWay, 42);

        // Assert
        Assert.IsTrue(threeWay.IsT1);
        Assert.AreEqual(error, threeWay.AsT1);
    }

    [TestMethod]
    public void ToThreeWay_WithT2_ShouldCreateT2Instance()
    {
        // Arrange
        var value = "test value";
        var twoWay = OneOf<TestError, string>.FromT2(value);

        // Act
        var threeWay = OneOfExtensions.ToThreeWay(twoWay, 42);

        // Assert
        Assert.IsTrue(threeWay.IsT2);
        Assert.AreEqual(value, threeWay.AsT2);
    }

    #endregion

    #region ToTwoWay (Filter) Tests

    [TestMethod]
    public void ToTwoWay_WithT1_ShouldReturnT1Instance()
    {
        // Arrange
        var error = new TestError("Test error");
        var threeWay = OneOf<TestError, string, int>.FromT1(error);

        // Act
        var twoWay = threeWay.ToTwoWay<TestError, string, int>();

        // Assert
        Assert.IsTrue(twoWay.HasValue);
        Assert.IsTrue(twoWay.Value.IsT1);
        Assert.AreEqual(error, twoWay.Value.AsT1);
    }

    [TestMethod]
    public void ToTwoWay_WithT2_ShouldReturnT2Instance()
    {
        // Arrange
        var value = "test value";
        var threeWay = OneOf<TestError, string, int>.FromT2(value);

        // Act
        var twoWay = threeWay.ToTwoWay<TestError, string, int>();

        // Assert
        Assert.IsTrue(twoWay.HasValue);
        Assert.IsTrue(twoWay.Value.IsT2);
        Assert.AreEqual(value, twoWay.Value.AsT2);
    }

    [TestMethod]
    public void ToTwoWay_WithT3_ShouldReturnNull()
    {
        // Arrange
        var value = 42;
        var threeWay = OneOf<TestError, string, int>.FromT3(value);

        // Act
        var twoWay = threeWay.ToTwoWay<TestError, string, int>();

        // Assert
        Assert.IsFalse(twoWay.HasValue);
    }

    #endregion

    #region ToTwoWay (Mapping) Tests

    [TestMethod]
    public void ToTwoWay_WithMappingT3ToT1_ShouldMapT3ToT1()
    {
        // Arrange
        var value = 42;
        var threeWay = OneOf<TestError, string, int>.FromT3(value);

        // Act
        var twoWay = threeWay.ToTwoWay(
            t3ToT1: i => new TestError($"Mapped from int: {i}")
        );

        // Assert
        Assert.IsTrue(twoWay.IsT1);
        Assert.AreEqual("Mapped from int: 42", twoWay.AsT1.Message);
    }

    [TestMethod]
    public void ToTwoWay_WithMappingT3ToT2_ShouldMapT3ToT2()
    {
        // Arrange
        var value = 42;
        var threeWay = OneOf<TestError, string, int>.FromT3(value);

        // Act
        var twoWay = threeWay.ToTwoWay(
            t3ToT2: i => $"Mapped from int: {i}"
        );

        // Assert
        Assert.IsTrue(twoWay.IsT2);
        Assert.AreEqual("Mapped from int: 42", twoWay.AsT2);
    }

    [TestMethod]
    public void ToTwoWay_WithT1_ShouldPropagateT1()
    {
        // Arrange
        var error = new TestError("Test error");
        var threeWay = OneOf<TestError, string, int>.FromT1(error);

        // Act
        var twoWay = threeWay.ToTwoWay(
            t3ToT1: i => new TestError($"Mapped: {i}"),
            t3ToT2: i => $"Mapped: {i}"
        );

        // Assert
        Assert.IsTrue(twoWay.IsT1);
        Assert.AreEqual(error, twoWay.AsT1);
    }

    [TestMethod]
    public void ToTwoWay_WithT2_ShouldPropagateT2()
    {
        // Arrange
        var value = "test value";
        var threeWay = OneOf<TestError, string, int>.FromT2(value);

        // Act
        var twoWay = threeWay.ToTwoWay(
            t3ToT1: i => new TestError($"Mapped: {i}"),
            t3ToT2: i => $"Mapped: {i}"
        );

        // Assert
        Assert.IsTrue(twoWay.IsT2);
        Assert.AreEqual(value, twoWay.AsT2);
    }

    [TestMethod]
    public void ToTwoWay_WithBothMappings_ShouldPreferT3ToT1()
    {
        // Arrange
        var value = 42;
        var threeWay = OneOf<TestError, string, int>.FromT3(value);

        // Act
        var twoWay = threeWay.ToTwoWay(
            t3ToT1: i => new TestError($"T1 mapping: {i}"),
            t3ToT2: i => $"T2 mapping: {i}"
        );

        // Assert
        Assert.IsTrue(twoWay.IsT1);
        Assert.AreEqual("T1 mapping: 42", twoWay.AsT1.Message);
    }

    [TestMethod]
    public void ToTwoWay_WithNoMappings_ShouldThrowException()
    {
        // Arrange
        var threeWay = OneOf<TestError, string, int>.FromT3(42);

        // Act & Assert
        try
        {
            threeWay.ToTwoWay<TestError, string, int>(null, null);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    #endregion

    #region ToTwoWayWithFallback Tests

    [TestMethod]
    public void ToTwoWayWithFallback_WithT1_ShouldPropagateT1()
    {
        // Arrange
        var error = new TestError("Test error");
        var threeWay = OneOf<TestError, string, int>.FromT1(error);

        // Act
        var twoWay = threeWay.ToTwoWayWithFallback(
            fallbackT1: new TestError("Fallback T1"),
            fallbackT2: "Fallback T2"
        );

        // Assert
        Assert.IsTrue(twoWay.IsT1);
        Assert.AreEqual(error, twoWay.AsT1);
    }

    [TestMethod]
    public void ToTwoWayWithFallback_WithT2_ShouldPropagateT2()
    {
        // Arrange
        var value = "test value";
        var threeWay = OneOf<TestError, string, int>.FromT2(value);

        // Act
        var twoWay = threeWay.ToTwoWayWithFallback(
            fallbackT1: new TestError("Fallback T1"),
            fallbackT2: "Fallback T2"
        );

        // Assert
        Assert.IsTrue(twoWay.IsT2);
        Assert.AreEqual(value, twoWay.AsT2);
    }

    [TestMethod]
    public void ToTwoWayWithFallback_WithT3AndFallbackT1_ShouldUseFallbackT1()
    {
        // Arrange
        var value = 42;
        var threeWay = OneOf<TestError, string, int>.FromT3(value);
        var fallback = new TestError("Fallback error");

        // Act
        var twoWay = threeWay.ToTwoWayWithFallback(
            fallbackT1: fallback,
            fallbackT2: "Fallback T2"
        );

        // Assert
        Assert.IsTrue(twoWay.IsT1);
        Assert.AreEqual(fallback, twoWay.AsT1);
    }

    [TestMethod]
    public void ToTwoWayWithFallback_WithT3AndFallbackT2_ShouldUseFallbackT2()
    {
        // Arrange
        var value = 42;
        var threeWay = OneOf<TestError, string, int>.FromT3(value);
        var fallback = "Fallback string";

        // Act
        var twoWay = threeWay.ToTwoWayWithFallback(
            fallbackT2: fallback
        );

        // Assert
        Assert.IsTrue(twoWay.IsT2);
        Assert.AreEqual(fallback, twoWay.AsT2);
    }

    [TestMethod]
    public void ToTwoWayWithFallback_WithNoFallbacks_ShouldThrowException()
    {
        // Arrange
        var threeWay = OneOf<TestError, string, int>.FromT3(42);

        // Act & Assert
        try
        {
            threeWay.ToTwoWayWithFallback<TestError, string, int>();
            Assert.Fail("Expected ArgumentException was not thrown");
        }
        catch (ArgumentException)
        {
            // Expected exception
        }
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
