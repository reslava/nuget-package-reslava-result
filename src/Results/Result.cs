using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// Represents the result of an operation without a value.
/// Immutable by design - all operations return new instances.
/// </summary>
public partial class Result : IResult
{
    public bool IsSuccess => !IsFailed;
    public bool IsFailed => Reasons.OfType<IError>().Any();
    public ImmutableList<IReason> Reasons { get; private init; }
    public ImmutableList<IError> Errors => Reasons.OfType<IError>().ToImmutableList(); 
    public ImmutableList<ISuccess> Successes => Reasons.OfType<ISuccess>().ToImmutableList();

    // Protected constructors - only factory methods create Results
    /// <summary>
    /// Initializes a new instance of the Result class with no reasons.
    /// This constructor is protected and used internally for creating successful results.
    /// </summary>
    protected Result()
    {
        Reasons = [];
    }

    /// <summary>
    /// Initializes a new instance of the Result class with multiple reasons.
    /// This constructor is internal and used by factory methods.
    /// </summary>
    /// <param name="reasons">The reasons associated with this result.</param>
    internal Result(ImmutableList<IReason> reasons)
    {
        Reasons = reasons ?? [];
    }

    /// <summary>
    /// Initializes a new instance of the Result class with a single reason.
    /// This constructor is protected and used internally.
    /// </summary>
    /// <param name="reason">The reason associated with this result.</param>
    protected Result(IReason reason)
    {
        reason = reason.EnsureNotNull(nameof(reason));
        Reasons = ImmutableList.Create(reason);
    }

    // Fluent methods - all return NEW instances (immutability)
    /// <summary>
    /// Adds a reason to the result.
    /// </summary>
    /// <param name="reason">The reason to add.</param>
    /// <returns>A new Result with the added reason.</returns>
    /// <example>
    /// <code>
    /// var result = Result.Ok()
    ///     .WithReason(new Success("Operation completed"));
    /// </code>
    /// </example>
    public Result WithReason(IReason reason) 
    { 
        reason = reason.EnsureNotNull(nameof(reason));
        return new Result(Reasons.Add(reason)); 
    }

    /// <summary>
    /// Adds multiple reasons to the result.
    /// </summary>
    /// <param name="reasons">The reasons to add.</param>
    /// <returns>A new Result with the added reasons.</returns>
    /// <exception cref="ArgumentException">Thrown when the reasons list is empty.</exception>
    /// <example>
    /// <code>
    /// var reasons = ImmutableList.Create&lt;IReason&gt;(new Success("Validated"), new Success("Processed"));
    /// var result = Result.Ok().WithReasons(reasons);
    /// </code>
    /// </example>
    public Result WithReasons(ImmutableList<IReason> reasons) 
    { 
        reasons = reasons.EnsureNotNull(nameof(reasons));
        if (reasons.Count == 0) 
             throw new ArgumentException("The reasons list cannot be empty", nameof(reasons));        
        
        return new Result(Reasons.AddRange(reasons)); 
    }

    /// <summary>
    /// Adds a success reason with a message to the result.
    /// </summary>
    /// <param name="message">The success message.</param>
    /// <returns>A new Result with the added success reason.</returns>
    /// <exception cref="ArgumentException">Thrown when the message is null or empty.</exception>
    /// <example>
    /// <code>
    /// var result = Result.Ok().WithSuccess("User created successfully");
    /// </code>
    /// </example>
    public Result WithSuccess(string message) 
    { 
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));
        return new Result(Reasons.Add(new Success(message)));           
    }

    /// <summary>
    /// Adds a success reason to the result.
    /// </summary>
    /// <param name="success">The success reason to add.</param>
    /// <returns>A new Result with the added success reason.</returns>
    /// <example>
    /// <code>
    /// var success = new Success("Operation completed").WithTag("UserId", 123);
    /// var result = Result.Ok().WithSuccess(success);
    /// </code>
    /// </example>
    public Result WithSuccess(ISuccess success) 
    { 
        success = success.EnsureNotNull(nameof(success));
        return new Result(Reasons.Add(success)); 
    }

    /// <summary>
    /// Adds multiple success reasons to the result.
    /// </summary>
    /// <param name="successes">The success reasons to add.</param>
    /// <returns>A new Result with the added success reasons.</returns>
    /// <exception cref="ArgumentException">Thrown when the successes list is empty.</exception>
    /// <example>
    /// <code>
    /// var successes = new List&lt;ISuccess&gt; 
    /// {
    ///     new Success("Validated"),
    ///     new Success("Processed"),
    ///     new Success("Saved")
    /// };
    /// var result = Result.Ok().WithSuccesses(successes);
    /// </code>
    /// </example>
    public Result WithSuccesses(IEnumerable<ISuccess> successes) 
    { 
        successes = successes.EnsureNotNull(nameof(successes));
        var successList = successes.ToList();
        if (successList.Count == 0)
            throw new ArgumentException("The successes list cannot be empty", nameof(successes));
        return new Result(Reasons.AddRange(successList)); 
    }

    /// <summary>
    /// Adds an error reason with a message to the result.
    /// Note: This will make the result failed if it wasn't already.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A new Result with the added error reason.</returns>
    /// <exception cref="ArgumentException">Thrown when the message is null or empty.</exception>
    /// <example>
    /// <code>
    /// var result = Result.Ok().WithError("Validation failed");
    /// // result.IsFailed will be true
    /// </code>
    /// </example>
    public Result WithError(string message) 
    { 
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));
        return new Result(Reasons.Add(new Error(message))); 
    }

    /// <summary>
    /// Adds an error reason to the result.
    /// Note: This will make the result failed if it wasn't already.
    /// </summary>
    /// <param name="error">The error reason to add.</param>
    /// <returns>A new Result with the added error reason.</returns>
    /// <example>
    /// <code>
    /// var error = new Error("Database connection failed").WithTag("Database", "Users");
    /// var result = Result.Ok().WithError(error);
    /// // result.IsFailed will be true
    /// </code>
    /// </example>
    public Result WithError(IError error) 
    { 
        error = error.EnsureNotNull(nameof(error));
        return new Result(Reasons.Add(error)); 
    }

    /// <summary>
    /// Adds multiple error reasons to the result.
    /// Note: This will make the result failed if it wasn't already.
    /// </summary>
    /// <param name="errors">The error reasons to add.</param>
    /// <returns>A new Result with the added error reasons.</returns>
    /// <exception cref="ArgumentException">Thrown when the errors list is empty.</exception>
    /// <example>
    /// <code>
    /// var errors = new List&lt;IError&gt; 
    /// {
    ///     new Error("Invalid email"),
    ///     new Error("Password too short")
    /// };
    /// var result = Result.Ok().WithErrors(errors);
    /// // result.IsFailed will be true
    /// </code>
    /// </example>
    public Result WithErrors(IEnumerable<IError> errors) 
    { 
        errors = errors.EnsureNotNull(nameof(errors));
        var errorList = errors.ToList();
        if (errorList.Count == 0)
            throw new ArgumentException("The errors list cannot be empty", nameof(errors));
        return new Result(Reasons.AddRange(errorList)); 
    }

    /// <summary>
    /// Returns a string representation of the Result.
    /// </summary>
    /// <returns>A string containing the result state and reasons information.</returns>
    /// <example>
    /// <code>
    /// var result = Result.Ok();
    /// Console.WriteLine(result.ToString());
    /// // Output: Result: IsSuccess='True', Reasons=
    /// </code>
    /// </example>
    public override string ToString()
    {
        var reasonsString = Reasons.Any()
            ? $", Reasons={string.Join("; ", Reasons)}"
            : string.Empty;

        return $"Result: IsSuccess='{IsSuccess}'{reasonsString}";
    }
}