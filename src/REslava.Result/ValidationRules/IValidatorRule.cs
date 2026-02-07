namespace REslava.Result;

public interface IValidatorRule<T>
{
    /// <summary>
    /// Gets the name of the validation rule.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets the default error message for this validation rule.
    /// </summary>
    string ErrorMessage { get; }
}

public interface IValidatorRuleSync<T> : IValidatorRule<T>
{
    /// <summary>
    /// Validates the entity synchronously.
    /// </summary>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>A ValidationResult indicating success or failure.</returns>
    ValidationResult<T> Validate(T entity);
}

public interface IValidatorRuleAsync<T> : IValidatorRule<T>    
{
    /// <summary>
    /// Validates the entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>A ValidationResult indicating success or failure.</returns>
    Task<ValidationResult<T>> ValidateAsync(T entity);
}

// Combined interface for rules that support both
public interface IValidatorRuleCombined<T> : IValidatorRuleSync<T>, IValidatorRuleAsync<T>
{
}