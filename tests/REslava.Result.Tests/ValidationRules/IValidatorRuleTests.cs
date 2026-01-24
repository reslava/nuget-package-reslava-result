using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using System.Threading.Tasks;

namespace REslava.Result.Tests.ValidationRules;

[TestClass]
public class IValidatorRuleTests
{
    private class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Email { get; set; } = string.Empty;
    }

    [TestMethod]
    public void IValidatorRuleSync_Implementation_ShouldWorkCorrectly()
    {
        // Arrange
        var rule = new PredicateValidatorRule<TestEntity, string>(
            e => e.Name,
            "NameRequired",
            "Name is required",
            name => !string.IsNullOrEmpty(name));

        // Act & Assert
        Assert.IsInstanceOfType(rule, typeof(IValidatorRule<TestEntity>));
        Assert.IsInstanceOfType(rule, typeof(IValidatorRuleSync<TestEntity>));
        Assert.AreEqual("NameRequired", rule.Name);
        Assert.AreEqual("Name is required", rule.ErrorMessage);
    }

    [TestMethod]
    public void IValidatorRuleAsync_Implementation_ShouldWorkCorrectly()
    {
        // Arrange
        var rule = new AsyncPredicateValidatorRule<TestEntity, string>(
            e => e.Email,
            "UniqueEmail",
            "Email already exists",
            email => Task.FromResult(true));

        // Act & Assert
        Assert.IsInstanceOfType(rule, typeof(IValidatorRule<TestEntity>));
        Assert.IsInstanceOfType(rule, typeof(IValidatorRuleAsync<TestEntity>));
        Assert.AreEqual("UniqueEmail", rule.Name);
        Assert.AreEqual("Email already exists", rule.ErrorMessage);
    }

    [TestMethod]
    public void IValidatorRuleSync_ValidateMethod_ShouldReturnValidationResult()
    {
        // Arrange
        var rule = new PredicateValidatorRule<TestEntity, string>(
            e => e.Name,
            "NameRequired",
            "Name is required",
            name => !string.IsNullOrEmpty(name));
        var validEntity = new TestEntity { Name = "John", Age = 25 };
        var invalidEntity = new TestEntity { Name = "", Age = 25 };

        // Act
        var validResult = rule.Validate(validEntity);
        var invalidResult = rule.Validate(invalidEntity);

        // Assert
        Assert.IsInstanceOfType(validResult, typeof(ValidationResult<TestEntity>));
        Assert.IsInstanceOfType(invalidResult, typeof(ValidationResult<TestEntity>));
        Assert.IsTrue(validResult.IsValid);
        Assert.IsFalse(invalidResult.IsValid);
    }

    [TestMethod]
    public async Task IValidatorRuleAsync_ValidateAsyncMethod_ShouldReturnValidationResult()
    {
        // Arrange
        var rule = new AsyncPredicateValidatorRule<TestEntity, string>(
            e => e.Email,
            "UniqueEmail",
            "Email already exists",
            email => Task.FromResult(true));
        var entity = new TestEntity { Name = "John", Age = 25, Email = "test@example.com" };

        // Act
        var result = await rule.ValidateAsync(entity);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ValidationResult<TestEntity>));
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void IValidatorRuleSync_CanBeUsedInRuleSet()
    {
        // Arrange
        var syncRule = new PredicateValidatorRule<TestEntity, string>(
            e => e.Name,
            "NameRequired",
            "Name is required",
            name => !string.IsNullOrEmpty(name));

        var builder = new ValidatorRuleBuilder<TestEntity>();
        builder.AddRule(syncRule);
        var ruleSet = builder.Build();
        var entity = new TestEntity { Name = "John", Age = 25 };

        // Act
        var result = ruleSet.Validate(entity);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task IValidatorRuleAsync_CanBeUsedInRuleSet()
    {
        // Arrange
        var asyncRule = new AsyncPredicateValidatorRule<TestEntity, string>(
            e => e.Email,
            "UniqueEmail",
            "Email already exists",
            email => Task.FromResult(true));

        var builder = new ValidatorRuleBuilder<TestEntity>();
        builder.AddRule(asyncRule);
        var ruleSet = builder.Build();
        var entity = new TestEntity { Name = "John", Age = 25, Email = "test@example.com" };

        // Act
        var result = await ruleSet.ValidateAsync(entity);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void IValidatorRuleSync_CanBeUsedInFluentBuilder()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();
        var entity = new TestEntity { Name = "John", Age = 25 };

        // Act
        var ruleSet = builder
            .Rule(e => e.Name, "NameRequired", "Name is required", name => !string.IsNullOrEmpty(name))
            .Rule(e => e.Age, "MinAge", "Must be 18 or older", age => age >= 18)
            .Build();

        var result = ruleSet.Validate(entity);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task IValidatorRuleAsync_CanBeUsedInFluentBuilder()
    {
        // Arrange
        var builder = new ValidatorRuleBuilder<TestEntity>();
        var entity = new TestEntity { Name = "John", Age = 25, Email = "test@example.com" };

        // Act
        var ruleSet = builder
            .Rule(e => e.Name, "NameRequired", "Name is required", name => !string.IsNullOrEmpty(name))
            .RuleAsync(e => e.Email, "UniqueEmail", "Email already exists", email => Task.FromResult(true))
            .Build();

        var result = await ruleSet.ValidateAsync(entity);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void IValidatorRuleCombined_InterfaceShouldBeAvailable()
    {
        // Arrange & Act
        var syncRule = new PredicateValidatorRule<TestEntity, string>(
            e => e.Name,
            "NameRequired",
            "Name is required",
            name => !string.IsNullOrEmpty(name));

        var asyncRule = new AsyncPredicateValidatorRule<TestEntity, string>(
            e => e.Email,
            "UniqueEmail",
            "Email already exists",
            email => Task.FromResult(true));

        // Assert
        // These should compile without errors, proving the interfaces exist
        Assert.IsNotNull(syncRule);
        Assert.IsNotNull(asyncRule);
        
        // Verify the combined interface exists
        var combinedType = typeof(IValidatorRuleCombined<TestEntity>);
        Assert.IsNotNull(combinedType);
    }

    [TestMethod]
    public void IValidatorRule_PropertiesShouldBeAccessible()
    {
        // Arrange
        var syncRule = new PredicateValidatorRule<TestEntity, string>(
            e => e.Name,
            "NameRequired",
            "Name is required",
            name => !string.IsNullOrEmpty(name));

        var asyncRule = new AsyncPredicateValidatorRule<TestEntity, string>(
            e => e.Email,
            "UniqueEmail",
            "Email already exists",
            email => Task.FromResult(true));

        // Act & Assert
        Assert.AreEqual("NameRequired", ((IValidatorRule<TestEntity>)syncRule).Name);
        Assert.AreEqual("Name is required", ((IValidatorRule<TestEntity>)syncRule).ErrorMessage);
        Assert.AreEqual("UniqueEmail", ((IValidatorRule<TestEntity>)asyncRule).Name);
        Assert.AreEqual("Email already exists", ((IValidatorRule<TestEntity>)asyncRule).ErrorMessage);
    }

    [TestMethod]
    public void IValidatorRuleSync_CanBeCastToBaseInterface()
    {
        // Arrange
        var rule = new PredicateValidatorRule<TestEntity, string>(
            e => e.Name,
            "NameRequired",
            "Name is required",
            name => !string.IsNullOrEmpty(name));

        // Act
        IValidatorRule<TestEntity> baseRule = rule;
        IValidatorRuleSync<TestEntity> syncRule = rule;

        // Assert
        Assert.IsNotNull(baseRule);
        Assert.IsNotNull(syncRule);
        Assert.AreEqual("NameRequired", baseRule.Name);
        Assert.AreEqual("Name is required", baseRule.ErrorMessage);
        Assert.AreEqual("NameRequired", syncRule.Name);
        Assert.AreEqual("Name is required", syncRule.ErrorMessage);
    }

    [TestMethod]
    public void IValidatorRuleAsync_CanBeCastToBaseInterface()
    {
        // Arrange
        var rule = new AsyncPredicateValidatorRule<TestEntity, string>(
            e => e.Email,
            "UniqueEmail",
            "Email already exists",
            email => Task.FromResult(true));

        // Act
        IValidatorRule<TestEntity> baseRule = rule;
        IValidatorRuleAsync<TestEntity> asyncRule = rule;

        // Assert
        Assert.IsNotNull(baseRule);
        Assert.IsNotNull(asyncRule);
        Assert.AreEqual("UniqueEmail", baseRule.Name);
        Assert.AreEqual("Email already exists", baseRule.ErrorMessage);
        Assert.AreEqual("UniqueEmail", asyncRule.Name);
        Assert.AreEqual("Email already exists", asyncRule.ErrorMessage);
    }
}
