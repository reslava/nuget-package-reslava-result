using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using System.Threading.Tasks;

namespace REslava.Result.Tests.ValidationRules;

[TestClass]
public class AsyncPredicateValidatorRuleTests
{
    private class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Email { get; set; } = string.Empty;
    }

    [TestMethod]
    public void Constructor_WithValidParameters_ShouldCreateRule()
    {
        // Arrange
        Func<TestEntity, string> propertySelector = e => e.Name;
        string ruleName = "NameRequired";
        string errorMessage = "Name is required";
        Func<string, Task<bool>> validator = name => Task.FromResult(!string.IsNullOrEmpty(name));

        // Act
        var rule = new AsyncPredicateValidatorRule<TestEntity, string>(propertySelector, ruleName, errorMessage, validator);

        // Assert
        Assert.AreEqual(ruleName, rule.Name);
        Assert.AreEqual(errorMessage, rule.ErrorMessage);
    }

    [TestMethod]
    public void Constructor_WithNullPropertySelector_ShouldThrowArgumentNullException()
    {
        // Arrange
        Func<TestEntity, string> propertySelector = null!;
        string ruleName = "Test";
        string errorMessage = "Error";
        
        // Suppress unused variable warnings - these are for constructor parameter validation
        _ = propertySelector;
        _ = ruleName;
        _ = errorMessage;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AsyncPredicateValidatorRule<TestEntity, string>(null!, "Test", "Error", async s => true));
    }

    [TestMethod]
    public void Constructor_WithNullValidator_ShouldThrowArgumentNullException()
    {
        // Arrange
        Func<TestEntity, string> propertySelector = e => e.Name;
        string ruleName = "Test";
        string errorMessage = "Error";
        Func<string, Task<bool>> validator = null!;
        
        // Suppress unused variable warnings
        _ = propertySelector;
        _ = ruleName;
        _ = errorMessage;
        _ = validator;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AsyncPredicateValidatorRule<TestEntity, string>(propertySelector, ruleName, errorMessage, null!));
    }

    [TestMethod]
    public async Task ValidateAsync_WithValidEntity_ShouldReturnSuccessResult()
    {
        // Arrange
        var entity = new TestEntity { Name = "John", Age = 25 };
        var rule = new AsyncPredicateValidatorRule<TestEntity, string>(
            e => e.Name,
            "NameRequired",
            "Name is required",
            name => Task.FromResult(!string.IsNullOrEmpty(name)));

        // Act
        var result = await rule.ValidateAsync(entity);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(entity, result.Value);
    }

    [TestMethod]
    public async Task ValidateAsync_WithInvalidEntity_ShouldReturnFailureResult()
    {
        // Arrange
        var entity = new TestEntity { Name = "", Age = 25 };
        var rule = new AsyncPredicateValidatorRule<TestEntity, string>(
            e => e.Name,
            "NameRequired",
            "Name is required",
            name => Task.FromResult(!string.IsNullOrEmpty(name)));

        // Act
        var result = await rule.ValidateAsync(entity);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.ValidationErrors);
        Assert.AreEqual("Name is required", result.ValidationErrors[0].Message);
    }

    [TestMethod]
    public async Task ValidateAsync_WithAgeValidation_ShouldWorkCorrectly()
    {
        // Arrange
        var entity = new TestEntity { Name = "John", Age = 15 };
        var rule = new AsyncPredicateValidatorRule<TestEntity, int>(
            e => e.Age,
            "MinAge",
            "Must be 18 or older",
            age => Task.FromResult(age >= 18));

        // Act
        var result = await rule.ValidateAsync(entity);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("Must be 18 or older", result.ValidationErrors[0].Message);
    }

    [TestMethod]
    public async Task ValidateAsync_WithEmailValidation_ShouldWorkCorrectly()
    {
        // Arrange
        var entity = new TestEntity { Name = "John", Age = 25, Email = "test@example.com" };
        var rule = new AsyncPredicateValidatorRule<TestEntity, string>(
            e => e.Email,
            "EmailFormat",
            "Invalid email format",
            email => Task.FromResult(email.Contains("@")));

        // Act
        var result = await rule.ValidateAsync(entity);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task ValidateAsync_WithSimulatedDatabaseCheck_ShouldWorkCorrectly()
    {
        // Arrange
        var entity = new TestEntity { Name = "John", Age = 25, Email = "existing@example.com" };
        var rule = new AsyncPredicateValidatorRule<TestEntity, string>(
            e => e.Email,
            "UniqueEmail",
            "Email already exists",
            async email => await SimulateEmailExistsCheck(email));

        // Act
        var result = await rule.ValidateAsync(entity);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("Email already exists", result.ValidationErrors[0].Message);
    }

    [TestMethod]
    public async Task ValidateAsync_WithPropertySelectorException_ShouldReturnFailureWithError()
    {
        // Arrange
        var entity = new TestEntity { Name = "John", Age = 25 };
        var rule = new AsyncPredicateValidatorRule<TestEntity, string>(
            e => e.Name,
            "NameRequired",
            "Name is required",
            name => Task.FromResult(!string.IsNullOrEmpty(name)));

        // Simulate null reference exception in property selector
        Func<TestEntity, string> faultySelector = e => e.Name!.ToUpper();

        var faultyRule = new AsyncPredicateValidatorRule<TestEntity, string>(
            faultySelector,
            "NameRequired",
            "Name is required",
            name => Task.FromResult(!string.IsNullOrEmpty(name)));

        // Act
        var result = await faultyRule.ValidateAsync(null!);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.ValidationErrors);
        Assert.Contains("Validation error in rule 'NameRequired'", result.ValidationErrors[0].Message);
    }

    [TestMethod]
    public async Task ValidateAsync_WithValidatorException_ShouldReturnFailureWithError()
    {
        // Arrange
        var entity = new TestEntity { Name = "John", Age = 25 };
        var rule = new AsyncPredicateValidatorRule<TestEntity, string>(
            e => e.Name,
            "NameRequired",
            "Name is required",
            name => throw new InvalidOperationException("Async validator error"));

        // Act
        var result = await rule.ValidateAsync(entity);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.ValidationErrors);
        Assert.Contains("Validation error in rule 'NameRequired'", result.ValidationErrors[0].Message);
        Assert.Contains("Async validator error", result.ValidationErrors[0].Message);
    }

    [TestMethod]
    public async Task ValidateAsync_WithComplexProperty_ShouldWorkCorrectly()
    {
        // Arrange
        var entity = new TestEntity { Name = "John", Age = 25, Email = "test@example.com" };
        var rule = new AsyncPredicateValidatorRule<TestEntity, string>(
            e => $"{e.Name}_{e.Age}",
            "CompositeFormat",
            "Invalid composite format",
            composite => Task.FromResult(composite.Contains("_")));

        // Act
        var result = await rule.ValidateAsync(entity);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task ValidateAsync_WithDelayedValidation_ShouldWorkCorrectly()
    {
        // Arrange
        var entity = new TestEntity { Name = "John", Age = 25 };
        var rule = new AsyncPredicateValidatorRule<TestEntity, string>(
            e => e.Name,
            "NameRequired",
            "Name is required",
            async name =>
            {
                await Task.Delay(10); // Simulate async operation
                return !string.IsNullOrEmpty(name);
            });

        // Act
        var result = await rule.ValidateAsync(entity);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    // Helper method to simulate database email check
    private async Task<bool> SimulateEmailExistsCheck(string email)
    {
        await Task.Delay(5); // Simulate database latency
        return email == "existing@example.com";
    }
}
