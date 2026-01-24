using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// Fluent builder for creating validation rule sets for a specific entity type.
/// Provides a fluent API for defining validation rules with method chaining.
/// </summary>
/// <typeparam name="T">The type of entity to validate.</typeparam>
/// <example>
/// <code>
/// var rules = new ValidatorRuleBuilder&lt;User&gt;()
///     .Rule(u => u.Email, "Required", email => !string.IsNullOrEmpty(email))
///     .Rule(u => u.Email, "Format", email => email.Contains("@"))
///     .Rule(u => u.Age, "MinAge", age => age >= 18)
///     .Build();
/// 
/// var result = rules.Validate(user);
/// </code>
/// </example>
public class ValidatorRuleBuilder<T>
{
    private readonly List<IValidatorRule<T>> _rules = new();
    
    /// <summary>
    /// Adds a validation rule to the builder.
    /// </summary>
    /// <param name="rule">The validation rule to add.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// var builder = new ValidatorRuleBuilder&lt;User&gt;();
    /// var rule = new EmailValidatorRule();
    /// builder.AddRule(rule);
    /// </code>
    /// </example>
    public ValidatorRuleBuilder<T> AddRule(IValidatorRule<T> rule)
    {
        _rules.Add(rule);
        return this;
    }
    
    /// <summary>
    /// Adds a custom validation rule defined by a predicate function.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="propertySelector">Function to select the property to validate.</param>
    /// <param name="ruleName">The name of the validation rule.</param>
    /// <param name="errorMessage">The error message if validation fails.</param>
    /// <param name="validator">The validation predicate function.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// var builder = new ValidatorRuleBuilder&lt;User&gt;()
    ///     .Rule(u => u.Email, "Required", "Email is required", email => !string.IsNullOrEmpty(email))
    ///     .Rule(u => u.Age, "MinAge", "Must be 18+", age => age >= 18);
    /// </code>
    /// </example>
    public ValidatorRuleBuilder<T> Rule<TProperty>(
        Func<T, TProperty> propertySelector,
        string ruleName,
        string errorMessage,
        Func<TProperty, bool> validator)
    {
        var rule = new PredicateValidatorRule<T, TProperty>(propertySelector, ruleName, errorMessage, validator);
        _rules.Add(rule);
        return this;
    }
    
    /// <summary>
    /// Adds a custom async validation rule defined by an async predicate function.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="propertySelector">Function to select the property to validate.</param>
    /// <param name="ruleName">The name of the validation rule.</param>
    /// <param name="errorMessage">The error message if validation fails.</param>
    /// <param name="validator">The async validation predicate function.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// var builder = new ValidatorRuleBuilder&lt;User&gt;()
    ///     .RuleAsync(u => u.Email, "UniqueEmail", "Email already exists", async email => await EmailService.IsUniqueAsync(email))
    ///     .Rule(u => u.Age, "MinAge", "Must be 18+", age => age >= 18);
    /// </code>
    /// </example>
    public ValidatorRuleBuilder<T> RuleAsync<TProperty>(
        Func<T, TProperty> propertySelector,
        string ruleName,
        string errorMessage,
        Func<TProperty, Task<bool>> validator)
    {
        var rule = new AsyncPredicateValidatorRule<T, TProperty>(propertySelector, ruleName, errorMessage, validator);
        _rules.Add(rule);
        return this;
    }
    
    /// <summary>
    /// Builds an immutable validation rule set from the added rules.
    /// </summary>
    /// <returns>An immutable ValidatorRuleSet containing all added rules.</returns>
    /// <example>
    /// <code>
    /// var builder = new ValidatorRuleBuilder&lt;User&gt;()
    ///     .Rule(u => u.Email, "Required", "Email required", email => !string.IsNullOrEmpty(email));
    ///     
    /// var ruleSet = builder.Build();  // Returns ValidatorRuleSet&lt;User&gt;
    /// </code>
    /// </example>
    public ValidatorRuleSet<T> Build()
    {
        return new ValidatorRuleSet<T>(_rules.ToImmutableList());
    }
}