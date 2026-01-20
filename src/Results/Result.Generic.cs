using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// Represents the result of an operation with a value of type TValue.
/// Immutable by design - all operations return new instances.
/// </summary>
public partial class Result<TValue> : Result, IResult<TValue>
{   
    // Private backing field - used internally for creating new instances
    private readonly TValue? _value;         
    
    /// <summary>
    /// Gets the value if the result is successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the result is failed. Use IsSuccess to check first,
    /// or use GetValueOr() for a safe alternative.
    /// </exception>
    /// <example>
    /// <code>
    /// // Safe usage:
    /// if (result.IsSuccess)
    /// {
    ///     var value = result.Value;  // Won't throw
    /// }
    /// 
    /// // Or use Match:
    /// var output = result.Match(
    ///     onSuccess: value => DoSomething(value),
    ///     onFailure: errors => HandleErrors(errors)
    /// );
    /// </code>
    /// </example>
    public TValue Value
    {
        get
        {
            if (IsFailed)
            {
                var errorMessages = string.Join(", ", Errors.Select(e => e.Message));
                throw new InvalidOperationException(
                    $"Cannot access Value on a failed Result. " +
                    $"Errors: [{errorMessages}]. " +
                    $"Check IsSuccess first or use GetValueOr() for safe access.");
            }
            return _value!;
        }
    }    
    // ============================================
    // CONSTRUCTORS
    // ============================================
    protected Result() : base()
    {
        _value = default;
    }

    public Result(TValue? value, ImmutableList<IReason> reasons) : base(reasons)
    {
        _value = value;
    }   

    public Result(TValue? value, IReason reason) : base(reason)
    {
        _value = value;        
    }
    
    // ============================================
    // SAFE ALTERNATIVE METHODS
    // ============================================

    /// <summary>
    /// Gets the value if successful, otherwise returns the specified default value.
    /// This is the recommended way to safely extract values without checking IsSuccess.
    /// </summary>
    /// <param name="defaultValue">The value to return if the result is failed.</param>
    /// <returns>The result value if successful, otherwise the default value.</returns>
    /// <example>
    /// <code>
    /// var user = result.GetValueOr(new User { Name = "Guest" });
    /// Console.WriteLine(user.Name);  // Safe - always has a value
    /// </code>
    /// </example>
    public TValue GetValueOr(TValue defaultValue)
    {
        return IsSuccess ? _value! : defaultValue;
    }

    /// <summary>
    /// Gets the value if successful, otherwise computes a default value lazily.
    /// Use this when the default value is expensive to create.
    /// </summary>
    /// <param name="defaultValueFactory">Function to compute the default value.</param>
    /// <returns>The result value if successful, otherwise the computed default.</returns>
    /// <example>
    /// <code>
    /// var user = result.GetValueOr(() => _database.GetGuestUser());
    /// </code>
    /// </example>
    public TValue GetValueOr(Func<TValue> defaultValueFactory)
    {
        defaultValueFactory = defaultValueFactory.EnsureNotNull(nameof(defaultValueFactory));
        return IsSuccess ? _value! : defaultValueFactory();
    }

    /// <summary>
    /// Gets the value if successful, otherwise computes a default based on the errors.
    /// Use this when you need error context to determine the fallback value.
    /// </summary>
    /// <param name="errorHandler">Function to compute default from errors.</param>
    /// <returns>The result value if successful, otherwise the error-based default.</returns>
    /// <example>
    /// <code>
    /// var user = result.GetValueOr(errors => 
    ///     new User { Name = $"Error: {errors[0].Message}" });
    /// </code>
    /// </example>
    public TValue GetValueOr(Func<ImmutableList<IError>, TValue> errorHandler)
    {
        errorHandler = errorHandler.EnsureNotNull(nameof(errorHandler));
        return IsSuccess ? _value! : errorHandler(Errors);
    }

    /// <summary>
    /// Tries to get the value using the standard .NET try pattern.
    /// </summary>
    public bool TryGetValue(out TValue value)
    {
        if (IsSuccess)
        {
            value = _value!;
            return true;
        }
        value = default!;
        return false;
    }

    // ============================================
    // FLUENT METHODS - All use _value internally
    // ============================================
    public new Result<TValue> WithReason(IReason reason)
    {
        reason = reason.EnsureNotNull(nameof(reason));
        return new Result<TValue>(_value, Reasons.Add(reason));
    }

    public new Result<TValue> WithReasons(ImmutableList<IReason> reasons)
    {
        reasons = reasons.EnsureNotNull(nameof(reasons));
        if (reasons.Count == 0)
            throw new ArgumentException("The reasons list cannot be empty", nameof(reasons));
        return new Result<TValue>(_value, Reasons.AddRange(reasons));
    }

    public new Result<TValue> WithSuccess(string message) 
    { 
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));
        return new Result<TValue>(_value, Reasons.Add(new Success(message))); 
    }

    public new Result<TValue> WithSuccess(ISuccess success) 
    { 
        success = success.EnsureNotNull(nameof(success));
        return new Result<TValue>(_value, Reasons.Add(success)); 
    }

    public new Result<TValue> WithSuccesses(IEnumerable<ISuccess> successes) 
    { 
        successes = successes.EnsureNotNull(nameof(successes));
        var successList = successes.ToList();
        if (successList.Count == 0)
            throw new ArgumentException("The successes list cannot be empty", nameof(successes));
        // ✅ Use _value (private field) instead of ValueOrDefault
        return new Result<TValue>(_value, Reasons.AddRange(successList)); 
    }

    public new Result<TValue> WithError(string message) 
    { 
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));
        // ✅ Use _value (private field) instead of ValueOrDefault
        return new Result<TValue>(_value, Reasons.Add(new Error(message))); 
    }

    public new Result<TValue> WithError(IError error) 
    { 
        error = error.EnsureNotNull(nameof(error));
        return new Result<TValue>(_value, Reasons.Add(error)); 
    }

    public new Result<TValue> WithErrors(IEnumerable<IError> errors) 
    { 
        errors = errors.EnsureNotNull(nameof(errors));
        var errorList = errors.ToList();
        if (errorList.Count == 0)
            throw new ArgumentException("The errors list cannot be empty", nameof(errors));
        return new Result<TValue>(_value, Reasons.AddRange(errorList)); 
    }    
    
    // ============================================
    // TOSTRING
    // ============================================
    public override string ToString()
    {
        var baseString = base.ToString();
        var valueString = $"{nameof(Value)} = {_value}";
        return $"{baseString}, {valueString}";
    }
}