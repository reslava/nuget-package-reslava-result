using System;
using System.Linq;
using System.Threading.Tasks;

namespace REslava.Result;

/// <summary>
/// A validation rule that uses an async predicate function to validate a property of an entity.
/// Supports async lambda expressions for asynchronous validation scenarios.
/// </summary>
/// <typeparam name="T">The type of entity to validate.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated.</typeparam>
/// <example>
/// <code>
/// // Create an async rule using lambda
/// var rule = new AsyncPredicateValidatorRule&lt;User, string&gt;(
///     u => u.Email,
///     "UniqueEmail",
///     "Email already exists",
///     async email => await EmailService.IsUniqueAsync(email)
/// );
/// 
/// // Use the rule
/// var result = await rule.ValidateAsync(user);
/// </code>
/// </example>
public class AsyncPredicateValidatorRule<T, TProperty> : IValidatorRuleAsync<T>
{
    private readonly Func<T, TProperty> _propertySelector;
    private readonly Func<TProperty, Task<bool>> _validator;
    
    /// <summary>
    /// Initializes a new instance of the AsyncPredicateValidatorRule.
    /// </summary>
    /// <param name="propertySelector">Function to select the property to validate.</param>
    /// <param name="ruleName">The name of the validation rule.</param>
    /// <param name="errorMessage">The error message if validation fails.</param>
    /// <param name="validator">The async validation predicate function.</param>
    /// <example>
    /// <code>
    /// var rule = new AsyncPredicateValidatorRule&lt;User, string&gt;(
    ///     u => u.Email,
    ///     "UniqueEmail",
    ///     "Email already exists",
    ///     async email => await EmailService.IsUniqueAsync(email)
    /// );
    /// </code>
    /// </example>
    public AsyncPredicateValidatorRule(
        Func<T, TProperty> propertySelector,
        string ruleName,
        string errorMessage,
        Func<TProperty, Task<bool>> validator)
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
    /// Validates the specified entity using the async predicate function.
    /// </summary>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>A ValidationResult indicating success or failure.</returns>
    /// <example>
    /// <code>
    /// var user = new User("test@example.com", 25);
    /// var result = await rule.ValidateAsync(user);
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
    public async Task<ValidationResult<T>> ValidateAsync(T entity)
    {
        try
        {
            var propertyValue = _propertySelector(entity);
            
            if (await _validator(propertyValue))
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
