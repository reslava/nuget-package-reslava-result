using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// Immutable collection of validation rules for a specific entity type.
/// Provides methods to validate entities against all rules in the set.
/// </summary>
/// <typeparam name="T">The type of entity to validate.</typeparam>
/// <example>
/// <code>
/// var ruleSet = new ValidatorRuleBuilder&lt;User&gt;()
///     .Rule(u => u.Email, "Required", "Email required", email => !string.IsNullOrEmpty(email))
///     .Rule(u => u.Age, "MinAge", "Must be 18+", age => age >= 18)
///     .Build();
/// 
/// var result = ruleSet.Validate(user);
/// if (result.IsValid)
/// {
///     Console.WriteLine("User is valid");
/// }
/// else
/// {
///     foreach (var error in result.ValidationErrors)
///     {
///         Console.WriteLine($"Error: {error.Message}");
///     }
/// }
/// </code>
/// </example>
public class ValidatorRuleSet<T>
{
    private readonly ImmutableList<IValidatorRule<T>> _rules;
    
    /// <summary>
    /// Initializes a new instance of the ValidatorRuleSet with the specified rules.
    /// </summary>
    /// <param name="rules">The immutable collection of validation rules.</param>
    internal ValidatorRuleSet(ImmutableList<IValidatorRule<T>> rules)
    {
        _rules = rules ?? ImmutableList<IValidatorRule<T>>.Empty;
    }
    
    /// <summary>
    /// Gets the number of validation rules in this set.
    /// </summary>
    public int Count => _rules.Count;
    
    /// <summary>
    /// Gets a read-only collection of all validation rules in this set.
    /// </summary>
    public IReadOnlyList<IValidatorRule<T>> Rules => _rules;
    
    /// <summary>
    /// Validates the specified entity against all rules in this set.
    /// Stops at first failure (fail-fast approach).
    /// </summary>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>A ValidationResult indicating success or failure.</returns>
    /// <example>
    /// <code>
    /// var result = ruleSet.Validate(user);
    /// if (result.IsValid)
    /// {
    ///     Console.WriteLine("All validations passed");
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Validation failed: {result.ValidationErrors.First().Message}");
    /// }
    /// </code>
    /// </example>
    public ValidationResult<T> Validate(T entity)
    {
        foreach (var rule in _rules)
        {
            if (rule is IValidatorRuleSync<T> syncRule)
            {
                var result = syncRule.Validate(entity);
                if (!result.IsValid)
                    return result;
            }
        }
        
        return ValidationResult<T>.Success(entity);
    }
    
    /// <summary>
    /// Validates the specified entity against all rules in this set asynchronously.
    /// </summary>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>A ValidationResult indicating success or failure.</returns>
    /// <example>
    /// <code>
    /// var result = await ruleSet.ValidateAsync(user);
    /// if (result.IsValid)
    /// {
    ///     Console.WriteLine("All validations passed");
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Validation failed: {result.ValidationErrors.First().Message}");
    /// }
    /// </code>
    /// </example>
    public async Task<ValidationResult<T>> ValidateAsync(T entity)
    {
        foreach (var rule in _rules)
        {
            if (rule is IValidatorRuleAsync<T> asyncRule)
            {
                var result = await asyncRule.ValidateAsync(entity);
                if (!result.IsValid)
                    return result;
            }
            else if (rule is IValidatorRuleSync<T> syncRule)
            {
                var result = syncRule.Validate(entity);
                if (!result.IsValid)
                    return result;
            }
        }
        
        return ValidationResult<T>.Success(entity);
    }
    
    /// <summary>
    /// Validates the entity and collects all validation errors (not fail-fast).
    /// </summary>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>A ValidationResult with all validation errors.</returns>
    /// <example>
    /// <code>
    /// var result = ruleSet.ValidateAll(user);
    /// if (result.IsValid)
    /// {
    ///     Console.WriteLine("All validations passed");
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Found {result.ValidationErrors.Count} validation errors:");
    ///     foreach (var error in result.ValidationErrors)
    ///     {
    ///         Console.WriteLine($"- {error.Message}");
    ///     }
    /// }
    /// </code>
    /// </example>
    public ValidationResult<T> ValidateAll(T entity)
    {
        var errors = new List<IReason>();
        
        foreach (var rule in _rules)
        {
            if (rule is IValidatorRuleSync<T> syncRule)
            {
                var result = syncRule.Validate(entity);
                if (!result.IsValid)
                {
                    errors.AddRange(result.ValidationErrors);
                }
            }
        }
        
        return errors.Count > 0 
            ? ValidationResult<T>.Failure(errors.ToArray())
            : ValidationResult<T>.Success(entity);
    }
}