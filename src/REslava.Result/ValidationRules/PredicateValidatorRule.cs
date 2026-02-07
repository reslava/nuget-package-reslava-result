using System;
using System.Linq;

namespace REslava.Result;

/// <summary>
/// A validation rule that uses a predicate function to validate a property of an entity.
/// Supports lambda expressions for concise rule definitions.
/// </summary>
/// <typeparam name="T">The type of entity to validate.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated.</typeparam>
/// <example>
/// <code>
/// // Create a rule using lambda
/// var rule = new PredicateValidatorRule&lt;User, string&gt;(
///     u => u.Email,
///     "EmailRequired",
///     "Email is required",
///     email => !string.IsNullOrEmpty(email)
/// );
/// 
/// // Use the rule
/// var result = rule.Validate(user);
/// </code>
/// </example>
public class PredicateValidatorRule<T, TProperty> : IValidatorRuleSync<T>
{
    private readonly Func<T, TProperty> _propertySelector;
    private readonly Func<TProperty, bool> _validator;
    
    /// <summary>
    /// Initializes a new instance of the PredicateValidatorRule.
    /// </summary>
    /// <param name="propertySelector">Function to select the property to validate.</param>
    /// <param name="ruleName">The name of the validation rule.</param>
    /// <param name="errorMessage">The error message if validation fails.</param>
    /// <param name="validator">The validation predicate function.</param>
    /// <example>
    /// <code>
    /// var rule = new PredicateValidatorRule&lt;User, string&gt;(
    ///     u => u.Email,
    ///     "EmailRequired",
    ///     "Email is required",
    ///     email => !string.IsNullOrEmpty(email)
    /// );
    /// </code>
    /// </example>
    public PredicateValidatorRule(
        Func<T, TProperty> propertySelector,
        string ruleName,
        string errorMessage,
        Func<TProperty, bool> validator)
    {
        _propertySelector = propertySelector ?? throw new ArgumentNullException(nameof(propertySelector));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        Name = ruleName ?? throw new ArgumentNullException(nameof(ruleName));
        ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
    }
    
    /// <summary>
    /// Gets the name of the validation rule.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the default error message for this validation rule.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Validates the specified entity using the predicate function.
    /// </summary>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>A ValidationResult indicating success or failure.</returns>
    /// <example>
    /// <code>
    /// var user = new User("", 25);
    /// var result = rule.Validate(user);
    /// 
    /// if (result.IsValid)
    /// {
    ///     Console.WriteLine("Validation passed");
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Validation failed: {result.ValidationErrors.First().Message}");
    /// }
    /// </code>
    /// </example>
    public ValidationResult<T> Validate(T entity)
    {
        try
        {
            var propertyValue = _propertySelector(entity);
            
            if (_validator(propertyValue))
            {
                return ValidationResult<T>.Success(entity);
            }
            else
            {
                return ValidationResult<T>.Failure(ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            return ValidationResult<T>.Failure($"Validation error in rule '{Name}': {ex.Message}");
        }
    }
}