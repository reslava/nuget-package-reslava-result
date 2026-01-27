using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// Represents the result of an operation with a value of type TValue.
/// Immutable by design - all operations return new instances.
/// </summary>
public partial class Result<TValue> : Result, IResultResponse<TValue>
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
    public TValue? Value
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
    /// <summary>
    /// Initializes a new instance of the Result&lt;TValue&gt; class with a default value.
    /// This constructor is protected and used internally for creating failed results.
    /// </summary>
    protected Result() : base()
    {
        _value = default;
    }

    /// <summary>
    /// Initializes a new instance of the Result&lt;TValue&gt; class with a value and multiple reasons.
    /// </summary>
    /// <param name="value">The value to store.</param>
    /// <param name="reasons">The reasons associated with this result.</param>
    /// <example>
    /// <code>
    /// var reasons = ImmutableList.Create&lt;IReason&gt;(new Success("User created"));
    /// var result = new Result&lt;User&gt;(user, reasons);
    /// </code>
    /// </example>
    public Result(TValue? value, ImmutableList<IReason> reasons) : base(reasons)
    {
        _value = value;
    }   

    /// <summary>
    /// Initializes a new instance of the Result&lt;TValue&gt; class with a value and a single reason.
    /// </summary>
    /// <param name="value">The value to store.</param>
    /// <param name="reason">The reason associated with this result.</param>
    /// <example>
    /// <code>
    /// var result = new Result&lt;User&gt;(user, new Success("User found"));
    /// </code>
    /// </example>
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
    /// This is useful when you prefer the conventional TryGetValue pattern over GetValueOr.
    /// </summary>
    /// <param name="value">When this method returns, contains the value if the result is successful; otherwise, contains the default value.</param>
    /// <returns>true if the result is successful and value was obtained; false if the result is failed.</returns>
    /// <example>
    /// <code>
    /// if (result.TryGetValue(out var user))
    /// {
    ///     Console.WriteLine($"User: {user.Name}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Operation failed");
    /// }
    /// </code>
    /// </example>
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
    /// <summary>
    /// Adds a reason to the result while preserving the value.
    /// </summary>
    /// <param name="reason">The reason to add.</param>
    /// <returns>A new Result&lt;TValue&gt; with the added reason.</returns>
    /// <example>
    /// <code>
    /// var result = Result&lt;User&gt;.Ok(user)
    ///     .WithReason(new Success("User created successfully"));
    /// </code>
    /// </example>
    public new Result<TValue> WithReason(IReason reason)
    {
        reason = reason.EnsureNotNull(nameof(reason));
        return new Result<TValue>(_value, Reasons.Add(reason));
    }

    /// <summary>
    /// Adds multiple reasons to the result while preserving the value.
    /// </summary>
    /// <param name="reasons">The reasons to add.</param>
    /// <returns>A new Result&lt;TValue&gt; with the added reasons.</returns>
    /// <exception cref="ArgumentException">Thrown when the reasons list is empty.</exception>
    /// <example>
    /// <code>
    /// var reasons = new List&lt;IReason&gt; { new Success("Validated"), new Success("Processed") };
    /// var result = Result&lt;User&gt;.Ok(user).WithReasons(reasons);
    /// </code>
    /// </example>
    public new Result<TValue> WithReasons(ImmutableList<IReason> reasons)
    {
        reasons = reasons.EnsureNotNull(nameof(reasons));
        if (reasons.Count == 0)
            throw new ArgumentException("The reasons list cannot be empty", nameof(reasons));
        return new Result<TValue>(_value, Reasons.AddRange(reasons));
    }

    /// <summary>
    /// Adds a success reason with a message to the result while preserving the value.
    /// </summary>
    /// <param name="message">The success message.</param>
    /// <returns>A new Result&lt;TValue&gt; with the added success reason.</returns>
    /// <exception cref="ArgumentException">Thrown when the message is null or empty.</exception>
    /// <example>
    /// <code>
    /// var result = Result&lt;User&gt;.Ok(user)
    ///     .WithSuccess("User profile updated successfully");
    /// </code>
    /// </example>
    public new Result<TValue> WithSuccess(string message) 
    { 
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));
        return new Result<TValue>(_value, Reasons.Add(new Success(message))); 
    }

    /// <summary>
    /// Adds a success reason to the result while preserving the value.
    /// </summary>
    /// <param name="success">The success reason to add.</param>
    /// <returns>A new Result&lt;TValue&gt; with the added success reason.</returns>
    /// <example>
    /// <code>
    /// var success = new Success("Operation completed").WithTag("UserId", user.Id);
    /// var result = Result&lt;User&gt;.Ok(user).WithSuccess(success);
    /// </code>
    /// </example>
    public new Result<TValue> WithSuccess(ISuccess success) 
    { 
        success = success.EnsureNotNull(nameof(success));
        return new Result<TValue>(_value, Reasons.Add(success)); 
    }

    /// <summary>
    /// Adds multiple success reasons to the result while preserving the value.
    /// </summary>
    /// <param name="successes">The success reasons to add.</param>
    /// <returns>A new Result&lt;TValue&gt; with the added success reasons.</returns>
    /// <exception cref="ArgumentException">Thrown when the successes list is empty.</exception>
    /// <example>
    /// <code>
    /// var successes = new List&lt;ISuccess&gt; 
    /// {
    ///     new Success("Validated"),
    ///     new Success("Processed"),
    ///     new Success("Saved")
    /// };
    /// var result = Result&lt;User&gt;.Ok(user).WithSuccesses(successes);
    /// </code>
    /// </example>
    public new Result<TValue> WithSuccesses(IEnumerable<ISuccess> successes) 
    { 
        successes = successes.EnsureNotNull(nameof(successes));
        var successList = successes.ToList();
        if (successList.Count == 0)
            throw new ArgumentException("The successes list cannot be empty", nameof(successes));
        // ✅ Use _value (private field) instead of ValueOrDefault
        return new Result<TValue>(_value, Reasons.AddRange(successList)); 
    }

    /// <summary>
    /// Adds an error reason with a message to the result while preserving the value.
    /// Note: This will make the result failed if it wasn't already.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A new Result&lt;TValue&gt; with the added error reason.</returns>
    /// <exception cref="ArgumentException">Thrown when the message is null or empty.</exception>
    /// <example>
    /// <code>
    /// var result = Result&lt;User&gt;.Ok(user)
    ///     .WithError("User validation failed");
    /// // result.IsFailed will be true
    /// </code>
    /// </example>
    public new Result<TValue> WithError(string message) 
    { 
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));
        // ✅ Use _value (private field) instead of ValueOrDefault
        return new Result<TValue>(_value, Reasons.Add(new Error(message))); 
    }

    /// <summary>
    /// Adds an error reason to the result while preserving the value.
    /// Note: This will make the result failed if it wasn't already.
    /// </summary>
    /// <param name="error">The error reason to add.</param>
    /// <returns>A new Result&lt;TValue&gt; with the added error reason.</returns>
    /// <example>
    /// <code>
    /// var error = new Error("Database connection failed")
    ///     .WithTag("Database", "Users");
    /// var result = Result&lt;User&gt;.Ok(user).WithError(error);
    /// // result.IsFailed will be true
    /// </code>
    /// </example>
    public new Result<TValue> WithError(IError error) 
    { 
        error = error.EnsureNotNull(nameof(error));
        return new Result<TValue>(_value, Reasons.Add(error)); 
    }

    /// <summary>
    /// Adds multiple error reasons to the result while preserving the value.
    /// Note: This will make the result failed if it wasn't already.
    /// </summary>
    /// <param name="errors">The error reasons to add.</param>
    /// <returns>A new Result&lt;TValue&gt; with the added error reasons.</returns>
    /// <exception cref="ArgumentException">Thrown when the errors list is empty.</exception>
    /// <example>
    /// <code>
    /// var errors = new List&lt;IError&gt; 
    /// {
    ///     new Error("Invalid email"),
    ///     new Error("Password too short")
    /// };
    /// var result = Result&lt;User&gt;.Ok(user).WithErrors(errors);
    /// // result.IsFailed will be true
    /// </code>
    /// </example>
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
    /// <summary>
    /// Returns a string representation of the Result including the value.
    /// </summary>
    /// <returns>A string containing the result state and value information.</returns>
    /// <example>
    /// <code>
    /// var result = Result&lt;string&gt;.Ok("Hello World");
    /// Console.WriteLine(result.ToString());
    /// // Output: Result: IsSuccess='True', Reasons=, Value = Hello World
    /// </code>
    /// </example>
    public override string ToString()
    {
        var baseString = base.ToString();
        var valueString = $"{nameof(Value)} = {_value}";
        return $"{baseString}, {valueString}";
    }
}