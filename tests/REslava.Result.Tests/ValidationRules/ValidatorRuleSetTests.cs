using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using System.Threading.Tasks;

namespace REslava.Result.Tests.ValidationRules;

[TestClass]
public class ValidatorRuleSetTests
{
    private class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Email { get; set; } = string.Empty;
    }

    [TestMethod]
    public void Constructor_WithNullRules_ShouldCreateEmptyRuleSet()
    {
        // Act
        var ruleSet = new ValidatorRuleSet<TestEntity>(null!);

        // Assert
        Assert.IsNotNull(ruleSet);
        Assert.AreEqual(0, ruleSet.Count);
        Assert.IsEmpty(ruleSet.Rules);
    }

    [TestMethod]
    public void Constructor_WithEmptyRules_ShouldCreateEmptyRuleSet()
    {
        // Arrange
        var rules = System.Collections.Immutable.ImmutableList<IValidatorRule<TestEntity>>.Empty;

        // Act
        var ruleSet = new ValidatorRuleSet<TestEntity>(rules);

        // Assert
        Assert.IsNotNull(ruleSet);
        Assert.AreEqual(0, ruleSet.Count);
        Assert.IsEmpty(ruleSet.Rules);
    }

    [TestMethod]
    public void Constructor_WithRules_ShouldCreateRuleSetWithRules()
    {
        // Arrange
        var rules = System.Collections.Immutable.ImmutableList.Create<IValidatorRule<TestEntity>>(
            new PredicateValidatorRule<TestEntity, string>(
                e => e.Name,
                "NameRequired",
                "Name is required",
                name => !string.IsNullOrEmpty(name)),
            new PredicateValidatorRule<TestEntity, int>(
                e => e.Age,
                "MinAge",
                "Must be 18 or older",
                age => age >= 18));

        // Act
        var ruleSet = new ValidatorRuleSet<TestEntity>(rules);

        // Assert
        Assert.IsNotNull(ruleSet);
        Assert.AreEqual(2, ruleSet.Count);
        Assert.HasCount(2, ruleSet.Rules);
    }

    [TestMethod]
    public void Validate_WithValidEntityAndNoRules_ShouldReturnSuccess()
    {
        // Arrange
        var ruleSet = new ValidatorRuleSet<TestEntity>(System.Collections.Immutable.ImmutableList<IValidatorRule<TestEntity>>.Empty);
        var entity = new TestEntity { Name = "John", Age = 25 };

        // Act
        var result = ruleSet.Validate(entity);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(entity, result.Value);
    }

    [TestMethod]
    public void Validate_WithValidEntityAndValidRules_ShouldReturnSuccess()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();
        builder.Rule(e => e.Name, "NameRequired", "Name is required", name => !string.IsNullOrEmpty(name));
        builder.Rule(e => e.Age, "MinAge", "Must be 18 or older", age => age >= 18);
        var ruleSet = builder.Build();
        var entity = new TestEntity { Name = "John", Age = 25 };

        // Act
        var result = ruleSet.Validate(entity);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(entity, result.Value);
    }

    [TestMethod]
    public void Validate_WithInvalidEntityAndSingleRule_ShouldReturnFailure()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();
        builder.Rule(e => e.Name, "NameRequired", "Name is required", name => !string.IsNullOrEmpty(name));
        var ruleSet = builder.Build();
        var entity = new TestEntity { Name = "", Age = 25 };

        // Act
        var result = ruleSet.Validate(entity);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.ValidationErrors);
        Assert.AreEqual("Name is required", result.ValidationErrors[0].Message);
    }

    [TestMethod]
    public void Validate_WithInvalidEntityAndMultipleRules_ShouldReturnFailureOnFirstError()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();
        builder.Rule(e => e.Name, "NameRequired", "Name is required", name => !string.IsNullOrEmpty(name));
        builder.Rule(e => e.Age, "MinAge", "Must be 18 or older", age => age >= 18);
        var ruleSet = builder.Build();
        var entity = new TestEntity { Name = "", Age = 15 };

        // Act
        var result = ruleSet.Validate(entity);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.ValidationErrors);
        // Should fail on first rule (NameRequired)
        Assert.AreEqual("Name is required", result.ValidationErrors[0].Message);
    }

    [TestMethod]
    public async Task ValidateAsync_WithValidEntityAndValidRules_ShouldReturnSuccess()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();
        builder.Rule(e => e.Name, "NameRequired", "Name is required", name => !string.IsNullOrEmpty(name));
        builder.RuleAsync(e => e.Email, "UniqueEmail", "Email already exists", email => Task.FromResult(true));
        var ruleSet = builder.Build();
        var entity = new TestEntity { Name = "John", Age = 25, Email = "test@example.com" };

        // Act
        var result = await ruleSet.ValidateAsync(entity);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(entity, result.Value);
    }

    [TestMethod]
    public async Task ValidateAsync_WithInvalidEntityAndAsyncRule_ShouldReturnFailure()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();
        builder.Rule(e => e.Name, "NameRequired", "Name is required", name => !string.IsNullOrEmpty(name));
        builder.RuleAsync(e => e.Email, "UniqueEmail", "Email already exists", email => Task.FromResult(false));
        var ruleSet = builder.Build();
        var entity = new TestEntity { Name = "John", Age = 25, Email = "existing@example.com" };

        // Act
        var result = await ruleSet.ValidateAsync(entity);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.ValidationErrors);
        Assert.AreEqual("Email already exists", result.ValidationErrors[0].Message);
    }

    [TestMethod]
    public async Task ValidateAsync_WithMixedSyncAndAsyncRules_ShouldWorkCorrectly()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();
        builder.Rule(e => e.Name, "NameRequired", "Name is required", name => !string.IsNullOrEmpty(name));
        builder.Rule(e => e.Age, "MinAge", "Must be 18 or older", age => age >= 18);
        builder.RuleAsync(e => e.Email, "UniqueEmail", "Email already exists", email => Task.FromResult(true));
        var ruleSet = builder.Build();
        var entity = new TestEntity { Name = "John", Age = 25, Email = "test@example.com" };

        // Act
        var result = await ruleSet.ValidateAsync(entity);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(entity, result.Value);
    }

    [TestMethod]
    public void ValidateAll_WithValidEntityAndValidRules_ShouldReturnSuccess()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();
        builder.Rule(e => e.Name, "NameRequired", "Name is required", name => !string.IsNullOrEmpty(name));
        builder.Rule(e => e.Age, "MinAge", "Must be 18 or older", age => age >= 18);
        var ruleSet = builder.Build();
        var entity = new TestEntity { Name = "John", Age = 25 };

        // Act
        var result = ruleSet.ValidateAll(entity);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(entity, result.Value);
    }

    [TestMethod]
    public void ValidateAll_WithInvalidEntityAndMultipleRules_ShouldReturnAllErrors()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();
        builder.Rule(e => e.Name, "NameRequired", "Name is required", name => !string.IsNullOrEmpty(name));
        builder.Rule(e => e.Age, "MinAge", "Must be 18 or older", age => age >= 18);
        var ruleSet = builder.Build();
        var entity = new TestEntity { Name = "", Age = 15 };

        // Act
        var result = ruleSet.ValidateAll(entity);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(2, result.ValidationErrors);
        Assert.AreEqual("Name is required", result.ValidationErrors[0].Message);
        Assert.AreEqual("Must be 18 or older", result.ValidationErrors[1].Message);
    }

    [TestMethod]
    public void ValidateAll_WithSomeValidAndSomeInvalidRules_ShouldReturnOnlyErrors()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();
        builder.Rule(e => e.Name, "NameRequired", "Name is required", name => !string.IsNullOrEmpty(name));
        builder.Rule(e => e.Age, "MinAge", "Must be 18 or older", age => age >= 18);
        var ruleSet = builder.Build();
        var entity = new TestEntity { Name = "John", Age = 15 }; // Name valid, age invalid

        // Act
        var result = ruleSet.ValidateAll(entity);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.ValidationErrors);
        Assert.AreEqual("Must be 18 or older", result.ValidationErrors[0].Message);
    }

    [TestMethod]
    public void Validate_WithNoRules_ShouldReturnSuccess()
    {
        // Arrange
        var ruleSet = new ValidatorRuleSet<TestEntity>(System.Collections.Immutable.ImmutableList<IValidatorRule<TestEntity>>.Empty);
        var entity = new TestEntity { Name = "", Age = 0 }; // Invalid entity but no rules

        // Act
        var result = ruleSet.Validate(entity);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(entity, result.Value);
    }

    [TestMethod]
    public void ValidateAll_WithNoRules_ShouldReturnSuccess()
    {
        // Arrange
        var ruleSet = new ValidatorRuleSet<TestEntity>(System.Collections.Immutable.ImmutableList<IValidatorRule<TestEntity>>.Empty);
        var entity = new TestEntity { Name = "", Age = 0 }; // Invalid entity but no rules

        // Act
        var result = ruleSet.ValidateAll(entity);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(entity, result.Value);
    }

    [TestMethod]
    public async Task ValidateAsync_WithNoRules_ShouldReturnSuccess()
    {
        // Arrange
        var ruleSet = new ValidatorRuleSet<TestEntity>(System.Collections.Immutable.ImmutableList<IValidatorRule<TestEntity>>.Empty);
        var entity = new TestEntity { Name = "", Age = 0 }; // Invalid entity but no rules

        // Act
        var result = await ruleSet.ValidateAsync(entity);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(entity, result.Value);
    }

    [TestMethod]
    public void Rules_ShouldReturnReadOnlyList()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();
        builder.Rule(e => e.Name, "NameRequired", "Name is required", name => !string.IsNullOrEmpty(name));
        var ruleSet = builder.Build();

        // Act
        var rules = ruleSet.Rules;

        // Assert
        Assert.IsNotNull(rules);
        Assert.IsInstanceOfType(rules, typeof(System.Collections.Generic.IReadOnlyList<IValidatorRule<TestEntity>>));
        Assert.HasCount(1, rules);
    }
}
