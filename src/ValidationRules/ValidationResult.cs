using System.Collections.Immutable;
namespace REslava.Result;

/// <summary>
/// Represents the result of a validation operation with a value of type T.
/// Inherits from Result&lt;T&gt; and provides validation-specific convenience methods.
/// </summary>
/// <typeparam name="T">The type of the value being validated.</typeparam>
/// <example>
/// <code>
/// // Create a successful validation result
/// var result = ValidationResult&lt;User&gt;.Success(user, new Success("User is valid"));
/// 
/// // Create a failed validation result
/// var result = ValidationResult&lt;User&gt;.Failure("Email is required");
/// 
/// // Check validation status
/// if (result.IsValid)
/// {
///     Console.WriteLine($"Validation passed: {result.Value}");
/// }
/// else
/// {
///     Console.WriteLine($"Validation failed: {string.Join(", ", result.ValidationErrors)}");
/// }
/// </code>
/// </example>
public class ValidationResult<T> : Result<T>
{
    /// <summary>
    /// Private constructor for successful validation results.
    /// </summary>
    /// <param name="value">The validated value.</param>
    /// <param name="reasons">The reasons associated with this validation result.</param>
    private ValidationResult(T value, ImmutableList<IReason> reasons) 
        : base(value, reasons) { }
    
    /// <summary>
    /// Private constructor for failed validation results.
    /// Uses default(T) for the value since validation failed.
    /// </summary>
    /// <param name="reasons">The validation errors.</param>
    private ValidationResult(ImmutableList<IReason> reasons) 
        : base(default, reasons) { }  
    
    /// <summary>
    /// Creates a successful validation result with the specified value and optional success reasons.
    /// </summary>
    /// <param name="value">The value that passed validation.</param>
    /// <param name="successes">Optional success reasons describing what was validated.</param>
    /// <returns>A successful ValidationResult&lt;T&gt;.</returns>
    /// <example>
    /// <code>
    /// var user = new User("test@example.com", 25);
    /// var result = ValidationResult&lt;User&gt;.Success(user, 
    ///     new Success("Email format is valid"),
    ///     new Success("Age meets requirements"));
    /// // result.IsValid == true
    /// // result.Value == user
    /// // result.Successes contains the success reasons
    /// </code>
    /// </example>
    public static ValidationResult<T> Success(T value, params Success[] successes)
    {
        var allReasons = successes?.Length > 0 
            ? successes.ToImmutableList<IReason>()
            : ImmutableList<IReason>.Empty;
        return new ValidationResult<T>(value, allReasons);
    }
    
    /// <summary>
    /// Creates a successful validation result with the specified value and no success reasons.
    /// </summary>
    /// <param name="value">The value that passed validation.</param>
    /// <returns>A successful ValidationResult&lt;T&gt;.</returns>
    /// <example>
    /// <code>
    /// var user = new User("test@example.com", 25);
    /// var result = ValidationResult&lt;User&gt;.Success(user);
    /// // result.IsValid == true
    /// // result.Value == user
    /// // result.Successes is empty
    /// </code>
    /// </example>
    public static ValidationResult<T> Success(T value)
    {
        return new ValidationResult<T>(value, ImmutableList<IReason>.Empty);
    }
    
    /// <summary>
    /// Creates a failed validation result with a simple error message.
    /// </summary>
    /// <param name="error">The error message describing why validation failed.</param>
    /// <returns>A failed ValidationResult&lt;T&gt;.</returns>
    /// <example>
    /// <code>
    /// var result = ValidationResult&lt;User&gt;.Failure("Email is required");
    /// // result.IsValid == false
    /// // result.ValidationErrors contains one Error with message "Email is required"
    /// </code>
    /// </example>
    public static ValidationResult<T> Failure(string error)
    {
        return new ValidationResult<T>(ImmutableList.Create<IReason>(new Error(error)));
    }
    
    /// <summary>
    /// Creates a failed validation result with a specific error reason.
    /// </summary>
    /// <param name="error">The error reason describing why validation failed.</param>
    /// <returns>A failed ValidationResult&lt;T&gt;.</returns>
    /// <example>
    /// <code>
    /// var error = new ValidationError("Email", "Invalid email format")
    ///     .WithTag("Field", "Email")
    ///     .WithTag("Value", "invalid-email");
    ///     
    /// var result = ValidationResult&lt;User&gt;.Failure(error);
    /// // result.IsValid == false
    /// // result.ValidationErrors contains the ValidationError with rich context
    /// </code>
    /// </example>
    public static ValidationResult<T> Failure(IReason error)
    {
        return new ValidationResult<T>(ImmutableList.Create(error));
    }
    
    /// <summary>
    /// Creates a failed validation result with multiple error reasons.
    /// </summary>
    /// <param name="errors">The error reasons describing why validation failed.</param>
    /// <returns>A failed ValidationResult&lt;T&gt;.</returns>
    /// <example>
    /// <code>
    /// var errors = new IReason[]
    /// {
    ///     new ValidationError("Email", "Email is required"),
    ///     new ValidationError("Age", "Must be 18 or older"),
    ///     new ValidationError("Password", "Password too short")
    /// };
    /// 
    /// var result = ValidationResult&lt;User&gt;.Failure(errors);
    /// // result.IsValid == false
    /// // result.ValidationErrors contains all three validation errors
    /// // Each error can have its own context and tags
    /// </code>
    /// </example>
    public static ValidationResult<T> Failure(params IReason[] errors)
    {
        return new ValidationResult<T>(errors?.ToImmutableList() ?? ImmutableList<IReason>.Empty);
    }
    
    /// <summary>
    /// Gets a value indicating whether the validation was successful.
    /// This is a convenience property that returns the opposite of IsFailed.
    /// </summary>
    /// <value>
    /// <c>true</c> if the validation passed; otherwise, <c>false</c>.
    /// </value>
    /// <example>
    /// <code>
    /// var result = ValidateUser(user);
    /// if (result.IsValid)
    /// {
    ///     Console.WriteLine("User is valid");
    ///     // Access validated value
    ///     var validUser = result.Value;
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"User validation failed: {result.ValidationErrors.Count} errors");
    /// }
    /// </code>
    /// </example>
    public bool IsValid => IsSuccess;
    
    /// <summary>
    /// Gets the validation errors that caused the validation to fail.
    /// This is a convenience property that filters the Reasons to only include errors.
    /// </summary>
    /// <value>
    /// A read-only list of validation errors. Empty if validation passed.
    /// </value>
    /// <example>
    /// <code>
    /// var result = ValidateUser(user);
    /// if (!result.IsValid)
    /// {
    ///     foreach (var error in result.ValidationErrors)
    ///     {
    ///         Console.WriteLine($"Error: {error.Message}");
    ///         
    ///         // Access error context if available
    ///         if (error.Tags?.Any() == true)
    ///         {
    ///             Console.WriteLine($"Context: {string.Join(", ", error.Tags)}");
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public IReadOnlyList<IReason> ValidationErrors => Errors;
}