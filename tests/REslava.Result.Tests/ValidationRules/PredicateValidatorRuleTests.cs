using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;

namespace REslava.Result.Tests.ValidationRules;

[TestClass]
public class PredicateValidatorRuleTests
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
        Func<string, bool> validator = name => !string.IsNullOrEmpty(name);

        // Act
        var rule = new PredicateValidatorRule<TestEntity, string>(propertySelector, ruleName, errorMessage, validator);

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
        Func<string, bool> validator = s => true;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PredicateValidatorRule<TestEntity, string>(null!, "Test", "Error", s => true));
    }

    [TestMethod]
    public void Constructor_WithNullValidator_ShouldThrowArgumentNullException()
    {
        // Arrange
        Func<TestEntity, string> propertySelector = e => e.Name;
        string ruleName = "Test";
        string errorMessage = "Error";
        Func<string, bool> validator = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PredicateValidatorRule<TestEntity, string>(propertySelector, ruleName, errorMessage, validator));
    }

    [TestMethod]
    public void Constructor_WithNullRuleName_ShouldThrowArgumentNullException()
    {
        // Arrange
        Func<TestEntity, string> propertySelector = e => e.Name;
        string ruleName = null!;
        string errorMessage = "Error";
        Func<string, bool> validator = s => true;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PredicateValidatorRule<TestEntity, string>(propertySelector, ruleName, errorMessage, validator));
    }

    [TestMethod]
    public void Constructor_WithNullErrorMessage_ShouldThrowArgumentNullException()
    {
        // Arrange
        Func<TestEntity, string> propertySelector = e => e.Name;
        string ruleName = "Test";
        string errorMessage = null!;
        Func<string, bool> validator = s => true;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PredicateValidatorRule<TestEntity, string>(propertySelector, ruleName, errorMessage, validator));
    }

    [TestMethod]
    public void Validate_WithValidEntity_ShouldReturnSuccessResult()
    {
        // Arrange
        var entity = new TestEntity { Name = "John", Age = 25 };
        var rule = new PredicateValidatorRule<TestEntity, string>(
            e => e.Name,
            "NameRequired",
            "Name is required",
            name => !string.IsNullOrEmpty(name));

        // Act
        var result = rule.Validate(entity);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(entity, result.Value);
    }

    [TestMethod]
    public void Validate_WithInvalidEntity_ShouldReturnFailureResult()
    {
        // Arrange
        var entity = new TestEntity { Name = "", Age = 25 };
        var rule = new PredicateValidatorRule<TestEntity, string>(
            e => e.Name,
            "NameRequired",
            "Name is required",
            name => !string.IsNullOrEmpty(name));

        // Act
        var result = rule.Validate(entity);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.ValidationErrors);
        Assert.AreEqual("Name is required", result.ValidationErrors[0].Message);
    }

    [TestMethod]
    public void Validate_WithAgeValidation_ShouldWorkCorrectly()
    {
        // Arrange
        var entity = new TestEntity { Name = "John", Age = 15 };
        var rule = new PredicateValidatorRule<TestEntity, int>(
            e => e.Age,
            "MinAge",
            "Must be 18 or older",
            age => age >= 18);

        // Act
        var result = rule.Validate(entity);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("Must be 18 or older", result.ValidationErrors[0].Message);
    }

    [TestMethod]
    public void Validate_WithEmailValidation_ShouldWorkCorrectly()
    {
        // Arrange
        var entity = new TestEntity { Name = "John", Age = 25, Email = "test@example.com" };
        var rule = new PredicateValidatorRule<TestEntity, string>(
            e => e.Email,
            "EmailFormat",
            "Invalid email format",
            email => email.Contains("@"));

        // Act
        var result = rule.Validate(entity);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_WithPropertySelectorException_ShouldReturnFailureWithError()
    {
        // Arrange
        var entity = new TestEntity { Name = "John", Age = 25 };
        var rule = new PredicateValidatorRule<TestEntity, string>(
            e => e.Name,
            "NameRequired",
            "Name is required",
            name => !string.IsNullOrEmpty(name));

        // Simulate null reference exception in property selector
        Func<TestEntity, string> faultySelector = e => e.Name!.ToUpper();

        var faultyRule = new PredicateValidatorRule<TestEntity, string>(
            faultySelector,
            "NameRequired",
            "Name is required",
            name => !string.IsNullOrEmpty(name));

        // Act
        var result = faultyRule.Validate(null!);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.ValidationErrors);
        Assert.Contains("Validation error in rule 'NameRequired'", result.ValidationErrors[0].Message);
    }

    [TestMethod]
    public void Validate_WithValidatorException_ShouldReturnFailureWithError()
    {
        // Arrange
        var entity = new TestEntity { Name = "John", Age = 25 };
        var rule = new PredicateValidatorRule<TestEntity, string>(
            e => e.Name,
            "NameRequired",
            "Name is required",
            name => throw new InvalidOperationException("Validator error"));

        // Act
        var result = rule.Validate(entity);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.ValidationErrors);
        Assert.Contains("Validation error in rule 'NameRequired'", result.ValidationErrors[0].Message);
        Assert.Contains("Validator error", result.ValidationErrors[0].Message);
    }

    [TestMethod]
    public void Validate_WithComplexProperty_ShouldWorkCorrectly()
    {
        // Arrange
        var entity = new TestEntity { Name = "John", Age = 25, Email = "test@example.com" };
        var rule = new PredicateValidatorRule<TestEntity, string>(
            e => $"{e.Name}_{e.Age}",
            "CompositeFormat",
            "Invalid composite format",
            composite => composite.Contains("_"));

        // Act
        var result = rule.Validate(entity);

        // Assert
        Assert.IsTrue(result.IsValid);
    }
}
