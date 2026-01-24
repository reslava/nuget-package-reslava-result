using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using System.Collections.Immutable;

namespace REslava.Result.Tests.ValidationRules;

[TestClass]
public class ValidationResultTests
{
    private class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    [TestMethod]
    public void Success_WithValue_ShouldReturnValidResult()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test", Age = 25 };

        // Act
        var result = ValidationResult<TestEntity>.Success(entity);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(entity, result.Value);
        Assert.IsEmpty(result.ValidationErrors);
    }

    [TestMethod]
    public void Success_WithValueAndSuccesses_ShouldReturnValidResultWithSuccesses()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test", Age = 25 };
        var successes = new[] { new Success("Validation passed"), new Success("All good") };

        // Act
        var result = ValidationResult<TestEntity>.Success(entity, successes);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(entity, result.Value);
        Assert.HasCount(2, result.Successes);
        CollectionAssert.Contains(result.Successes, successes[0]);
        CollectionAssert.Contains(result.Successes, successes[1]);
    }

    [TestMethod]
    public void Failure_WithStringError_ShouldReturnInvalidResult()
    {
        // Arrange
        var errorMessage = "Validation failed";

        // Act
        var result = ValidationResult<TestEntity>.Failure(errorMessage);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.ValidationErrors);
        Assert.AreEqual(errorMessage, result.ValidationErrors[0].Message);
    }

    [TestMethod]
    public void Failure_WithIReasonError_ShouldReturnInvalidResult()
    {
        // Arrange
        var error = new Error("Custom error");

        // Act
        var result = ValidationResult<TestEntity>.Failure(error);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.ValidationErrors);
        Assert.AreEqual(error, result.ValidationErrors[0]);
    }

    [TestMethod]
    public void Failure_WithMultipleErrors_ShouldReturnInvalidResultWithAllErrors()
    {
        // Arrange
        var errors = new IReason[]
        {
            new Error("First error"),
            new Error("Second error"),
            new Error("Third error")
        };

        // Act
        var result = ValidationResult<TestEntity>.Failure(errors);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(3, result.ValidationErrors);
        CollectionAssert.AreEqual(errors, result.ValidationErrors.ToArray());
    }

    [TestMethod]
    public void Failure_WithNullErrors_ShouldReturnInvalidResultWithEmptyErrors()
    {
        // Act
        var result = ValidationResult<TestEntity>.Failure((IReason[])null!);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsEmpty(result.ValidationErrors);
    }

    [TestMethod]
    public void IsValid_ShouldReturnTrueForSuccessResult()
    {
        // Arrange
        var result = ValidationResult<TestEntity>.Success(new TestEntity());

        // Act & Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void IsValid_ShouldReturnFalseForFailureResult()
    {
        // Arrange
        var result = ValidationResult<TestEntity>.Failure("Error");

        // Act & Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void ValidationErrors_ShouldReturnEmptyForSuccessResult()
    {
        // Arrange
        var result = ValidationResult<TestEntity>.Success(new TestEntity());

        // Act & Assert
        Assert.IsEmpty(result.ValidationErrors);
    }

    [TestMethod]
    public void ValidationErrors_ShouldReturnErrorsForFailureResult()
    {
        // Arrange
        var result = ValidationResult<TestEntity>.Failure("Test error");

        // Act & Assert
        Assert.HasCount(1, result.ValidationErrors);
        Assert.AreEqual("Test error", result.ValidationErrors[0].Message);
    }

    [TestMethod]
    public void ValidationResult_ShouldInheritFromResult()
    {
        // Arrange
        var entity = new TestEntity();
        var result = ValidationResult<TestEntity>.Success(entity);

        // Act & Assert
        Assert.IsInstanceOfType(result, typeof(Result<TestEntity>));
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.IsFailed);
    }

    [TestMethod]
    public void Success_WithNullEntity_ShouldReturnValidResult()
    {
        // Act
        var result = ValidationResult<TestEntity>.Success(null!);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.IsNull(result.Value);
        Assert.IsEmpty(result.ValidationErrors);
    }

    [TestMethod]
    public void Success_WithNullSuccesses_ShouldReturnValidResultWithoutSuccesses()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test", Age = 25 };

        // Act
        var result = ValidationResult<TestEntity>.Success(entity, null!);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(entity, result.Value);
        Assert.IsEmpty(result.Successes);
    }

    [TestMethod]
    public void Success_WithEmptySuccesses_ShouldReturnValidResultWithoutSuccesses()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test", Age = 25 };
        var emptySuccesses = new Success[0];

        // Act
        var result = ValidationResult<TestEntity>.Success(entity, emptySuccesses);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(entity, result.Value);
        Assert.IsEmpty(result.Successes);
    }

    [TestMethod]
    public void Failure_WithEmptyErrors_ShouldReturnInvalidResultWithEmptyErrors()
    {
        // Arrange
        var emptyErrors = new IReason[0];

        // Act
        var result = ValidationResult<TestEntity>.Failure(emptyErrors);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsEmpty(result.ValidationErrors);
    }

    [TestMethod]
    public void Failure_WithNullStringError_ShouldReturnInvalidResult()
    {
        // Arrange
        string? errorMessage = null;

        // Act
        var result = ValidationResult<TestEntity>.Failure(errorMessage!);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.ValidationErrors);
        Assert.AreEqual(string.Empty, result.ValidationErrors[0].Message);
    }

    [TestMethod]
    public void Failure_WithEmptyStringError_ShouldReturnInvalidResult()
    {
        // Arrange
        var errorMessage = string.Empty;

        // Act
        var result = ValidationResult<TestEntity>.Failure(errorMessage);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.ValidationErrors);
        Assert.AreEqual(string.Empty, result.ValidationErrors[0].Message);
    }

    [TestMethod]
    public void ValidationErrors_ShouldBeReadOnly()
    {
        // Arrange
        var result = ValidationResult<TestEntity>.Failure("Test error");

        // Act & Assert
        Assert.IsInstanceOfType(result.ValidationErrors, typeof(System.Collections.Generic.IReadOnlyList<IReason>));
    }

    [TestMethod]
    public void Successes_ShouldBeReadOnly()
    {
        // Arrange
        var successes = new[] { new Success("Test success") };
        var result = ValidationResult<TestEntity>.Success(new TestEntity(), successes);

        // Act & Assert
        Assert.IsInstanceOfType(result.Successes, typeof(System.Collections.Generic.IReadOnlyList<ISuccess>));
    }

    [TestMethod]
    public void ValidationResult_Failure_ShouldNotHaveSuccesses()
    {
        // Arrange
        var result = ValidationResult<TestEntity>.Failure("Test error");

        // Act & Assert
        Assert.IsEmpty(result.Successes);
    }

    [TestMethod]
    public void ValidationResult_Success_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var result = ValidationResult<TestEntity>.Success(new TestEntity());

        // Act & Assert
        Assert.IsEmpty(result.ValidationErrors);
    }
}
