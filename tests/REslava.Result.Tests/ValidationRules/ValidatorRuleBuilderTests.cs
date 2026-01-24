using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using System.Threading.Tasks;

namespace REslava.Result.Tests.ValidationRules;

[TestClass]
public class ValidatorRuleBuilderTests
{
    private class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Email { get; set; } = string.Empty;
    }

    [TestMethod]
    public void Constructor_ShouldCreateEmptyBuilder()
    {
        // Act
        var builder = new ValidatorRuleBuilder<TestEntity>();

        // Assert
        Assert.IsNotNull(builder);
    }

    [TestMethod]
    public void AddRule_WithValidRule_ShouldAddRuleToBuilder()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();
        var rule = new PredicateValidatorRule<TestEntity, string>(
            e => e.Name,
            "NameRequired",
            "Name is required",
            name => !string.IsNullOrEmpty(name));

        // Act
        var result = builder.AddRule(rule);

        // Assert
        Assert.AreEqual(builder, result); // Should return same instance for chaining
    }

    [TestMethod]
    public void AddRule_WithNullRule_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddRule(null!));
    }

    [TestMethod]
    public void Rule_WithValidParameters_ShouldAddRuleToBuilder()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();

        // Act
        var result = builder.Rule(
            e => e.Name,
            "NameRequired",
            "Name is required",
            name => !string.IsNullOrEmpty(name));

        // Assert
        Assert.AreEqual(builder, result); // Should return same instance for chaining
    }

    [TestMethod]
    public void RuleAsync_WithValidParameters_ShouldAddAsyncRuleToBuilder()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();

        // Act
        var result = builder.RuleAsync(
            e => e.Email,
            "UniqueEmail",
            "Email already exists",
            email => Task.FromResult(true));

        // Assert
        Assert.AreEqual(builder, result); // Should return same instance for chaining
    }

    [TestMethod]
    public void Rule_WithNullPropertySelector_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.Rule<string>(null!, "Test", "Error", s => true));
    }

    [TestMethod]
    public void Rule_WithNullValidator_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.Rule(e => e.Name, "Test", "Error", null!));
    }

    [TestMethod]
    public void RuleAsync_WithNullPropertySelector_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.RuleAsync<string>(null!, "Test", "Error", s => Task.FromResult(true)));
    }

    [TestMethod]
    public void RuleAsync_WithNullValidator_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.RuleAsync(e => e.Name, "Test", "Error", null!));
    }

    [TestMethod]
    public void Build_WithNoRules_ShouldReturnEmptyRuleSet()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();

        // Act
        var ruleSet = builder.Build();

        // Assert
        Assert.IsNotNull(ruleSet);
        Assert.AreEqual(0, ruleSet.Count);
        Assert.IsEmpty(ruleSet.Rules);
    }

    [TestMethod]
    public void Build_WithSingleRule_ShouldReturnRuleSetWithOneRule()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();
        builder.Rule(
            e => e.Name,
            "NameRequired",
            "Name is required",
            name => !string.IsNullOrEmpty(name));

        // Act
        var ruleSet = builder.Build();

        // Assert
        Assert.IsNotNull(ruleSet);
        Assert.AreEqual(1, ruleSet.Count);
        Assert.HasCount(1, ruleSet.Rules);
        Assert.AreEqual("NameRequired", ruleSet.Rules[0].Name);
    }

    [TestMethod]
    public void Build_WithMultipleRules_ShouldReturnRuleSetWithAllRules()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();
        builder.Rule(
            e => e.Name,
            "NameRequired",
            "Name is required",
            name => !string.IsNullOrEmpty(name));
        builder.Rule(
            e => e.Age,
            "MinAge",
            "Must be 18 or older",
            age => age >= 18);
        builder.RuleAsync(
            e => e.Email,
            "UniqueEmail",
            "Email already exists",
            email => Task.FromResult(true));

        // Act
        var ruleSet = builder.Build();

        // Assert
        Assert.IsNotNull(ruleSet);
        Assert.AreEqual(3, ruleSet.Count);
        Assert.HasCount(3, ruleSet.Rules);
        Assert.AreEqual("NameRequired", ruleSet.Rules[0].Name);
        Assert.AreEqual("MinAge", ruleSet.Rules[1].Name);
        Assert.AreEqual("UniqueEmail", ruleSet.Rules[2].Name);
    }

    [TestMethod]
    public void FluentChaining_ShouldWorkCorrectly()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();

        // Act
        var ruleSet = builder
            .Rule(e => e.Name, "NameRequired", "Name is required", name => !string.IsNullOrEmpty(name))
            .Rule(e => e.Age, "MinAge", "Must be 18 or older", age => age >= 18)
            .RuleAsync(e => e.Email, "UniqueEmail", "Email already exists", email => Task.FromResult(true))
            .Build();

        // Assert
        Assert.IsNotNull(ruleSet);
        Assert.AreEqual(3, ruleSet.Count);
    }

    [TestMethod]
    public void Build_WithMixedSyncAndAsyncRules_ShouldCreateCorrectRuleSet()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();
        builder.Rule(e => e.Name, "NameRequired", "Name is required", name => !string.IsNullOrEmpty(name));
        builder.RuleAsync(e => e.Email, "UniqueEmail", "Email already exists", email => Task.FromResult(true));

        // Act
        var ruleSet = builder.Build();

        // Assert
        Assert.AreEqual(2, ruleSet.Count);
        Assert.HasCount(2, ruleSet.Rules);
        Assert.IsInstanceOfType(ruleSet.Rules[0], typeof(PredicateValidatorRule<TestEntity, string>));
        Assert.IsInstanceOfType(ruleSet.Rules[1], typeof(AsyncPredicateValidatorRule<TestEntity, string>));
    }

    [TestMethod]
    public void Rule_WithDifferentPropertyTypes_ShouldWorkCorrectly()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();

        // Act
        var ruleSet = builder
            .Rule(e => e.Name, "NameRequired", "Name is required", name => !string.IsNullOrEmpty(name))
            .Rule(e => e.Age, "MinAge", "Must be 18 or older", age => age >= 18)
            .Build();

        // Assert
        Assert.AreEqual(2, ruleSet.Count);
        Assert.HasCount(2, ruleSet.Rules);
        Assert.AreEqual("NameRequired", ruleSet.Rules[0].Name);
        Assert.AreEqual("MinAge", ruleSet.Rules[1].Name);
    }

    [TestMethod]
    public void AddRule_WithCustomRule_ShouldWorkCorrectly()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();
        var customRule = new PredicateValidatorRule<TestEntity, string>(
            e => e.Name,
            "CustomRule",
            "Custom error message",
            name => name.Length > 5);

        // Act
        var ruleSet = builder.AddRule(customRule).Build();

        // Assert
        Assert.AreEqual(1, ruleSet.Count);
        Assert.HasCount(1, ruleSet.Rules);
        Assert.AreEqual("CustomRule", ruleSet.Rules[0].Name);
        Assert.AreEqual("Custom error message", ruleSet.Rules[0].ErrorMessage);
    }
}
