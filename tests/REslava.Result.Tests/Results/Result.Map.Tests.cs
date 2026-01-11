namespace REslava.Result.Tests;

/// <summary>
/// Comprehensive tests for Map
/// </summary>
[TestClass]
public sealed class ResultBindTests
{
    #region Map Methods

    [TestMethod]
    public void Map_OnSuccess_TransformsValue()
    {
        // Arrange
        var result = Result<int>.Ok(7);

        // Act
        var mapped = result.Map(x => x * 2);

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.AreEqual(14, mapped.Value);
    }

    [TestMethod]
    public void Map_OnSuccess_ChangesType()
    {
        // Arrange
        var result = Result<int>.Ok(42);

        // Act
        var mapped = result.Map(x => x.ToString());

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.AreEqual("42", mapped.Value);
        Assert.IsInstanceOfType<Result<string>>(mapped);
    }

    [TestMethod]
    public void Map_OnFailure_ReturnsSameErrors()
    {
        // Arrange
        var result = Result<int>.Fail("Original error");

        // Act
        var mapped = result.Map(x => x.ToString());

        // Assert
        Assert.IsTrue(mapped.IsFailed);
        Assert.HasCount(1, mapped.Errors);
        Assert.AreEqual("Original error", mapped.Errors[0].Message);
    }

    [TestMethod]
    public void Map_WithNullMapper_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result.Map<string>(null!));
    }

    [TestMethod]
    public void Map_WhenMapperThrows_ReturnsFailedResult()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act
        var mapped = result.Map<string>(x => throw new InvalidOperationException("Mapper failed"));

        // Assert
        Assert.IsTrue(mapped.IsFailed);
        Assert.IsTrue(mapped.Errors.Any(e => e.GetType().Equals(typeof(ExceptionError))));
        Assert.IsTrue(mapped.Errors.Any(e => e.Message.Contains("Mapper failed")));
    }

    [TestMethod]
    public void Map_ComplexTransformation_Works()
    {
        // Arrange
        var result = Result<Person>.Ok(new Person { Name = "John", Age = 30 });

        // Act
        var mapped = result.Map(p => $"{p.Name} ({p.Age})");

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.AreEqual("John (30)", mapped.Value);
    }

    [TestMethod]
    public void Map_Chaining_Works()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var mapped = result
            .Map(x => x * 2)
            .Map(x => x + 10)
            .Map(x => x.ToString());

        // Assert
        Assert.IsTrue(mapped.IsSuccess);
        Assert.AreEqual("20", mapped.Value);
    }

    #endregion

    #region Helper Classes

    private class Person
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }    
    #endregion
}
