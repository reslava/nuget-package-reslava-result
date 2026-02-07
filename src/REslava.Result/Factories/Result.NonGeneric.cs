using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// Factory methods for non-generic Result.
/// </summary>
public partial class Result
{
    /// <summary>
    /// Creates a successful result with no value.
    /// </summary>
    /// <returns>A successful Result.</returns>
    /// <example>
    /// <code>
    /// var result = Result.Ok();
    /// Console.WriteLine(result.IsSuccess); // true
    /// </code>
    /// </example>
    public static Result Ok()
    {
        return new Result();
    }

    /// <summary>
    /// Creates a successful result with a success message.
    /// </summary>
    /// <param name="message">The success message to include.</param>
    /// <returns>A successful Result with the message.</returns>
    /// <example>
    /// <code>
    /// var result = Result.Ok("Operation completed successfully");
    /// </code>
    /// </example>
    public static Result Ok(string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));
        return new Result(new Success(message));
    }

    /// <summary>
    /// Creates a successful result with a success reason.
    /// </summary>
    /// <param name="success">The success reason to include.</param>
    /// <returns>A successful Result with the success reason.</returns>
    /// <example>
    /// <code>
    /// var success = new Success("User created");
    /// var result = Result.Ok(success);
    /// </code>
    /// </example>
    public static Result Ok(ISuccess success)
    {
        success = success.EnsureNotNull(nameof(success));
        return new Result(success);
    }

    /// <summary>
    /// Creates a successful result with multiple success messages.
    /// </summary>
    /// <param name="messages">Collection of success messages.</param>
    /// <returns>A successful Result with the messages.</returns>
    /// <example>
    /// <code>
    /// var messages = new[] { "Validated", "Processed" };
    /// var result = Result.Ok(messages);
    /// </code>
    /// </example>
    public static Result Ok(IEnumerable<string> messages)
    {
        messages = messages.EnsureNotNull(nameof(messages));
        var messageList = messages.ToList();
        if (messageList.Count == 0)
            throw new ArgumentException("The success messages list cannot be empty", nameof(messages));
        
        return new Result(messageList.Select(m => new Success(m)).ToImmutableList<IReason>());
    }

    /// <summary>
    /// Creates a successful result with multiple success reasons.
    /// </summary>
    /// <param name="successes">Collection of success reasons.</param>
    /// <returns>A successful Result with the success reasons.</returns>
    /// <example>
    /// <code>
    /// var successes = new[] 
    /// { 
    ///     new Success("Validated"), 
    ///     new Success("Processed") 
    /// };
    /// var result = Result.Ok(successes);
    /// </code>
    /// </example>
    public static Result Ok(ImmutableList<ISuccess> successes)
    {
        successes = successes.EnsureNotNull(nameof(successes));
        if (successes.Count == 0)
            throw new ArgumentException("The successes list cannot be empty", nameof(successes));
        
        return new Result(successes.ToImmutableList<IReason>());
    }

    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    /// <param name="message">The error message to include.</param>
    /// <returns>A failed Result with the specified error message.</returns>
    /// <example>
    /// <code>
    /// var result = Result.Fail("Operation failed");
    /// Console.WriteLine(result.IsFailed); // true
    /// </code>
    /// </example>
    public static Result Fail(string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));
        return new Result(new Error(message));
    }

    /// <summary>
    /// Creates a failed result with an error reason.
    /// </summary>
    /// <param name="error">The error reason to include.</param>
    /// <returns>A failed Result with the specified error reason.</returns>
    /// <example>
    /// <code>
    /// var error = new ValidationError("Invalid email format");
    /// var result = Result.Fail(error);
    /// </code>
    /// </example>
    public static Result Fail(IError error)
    {
        error = error.EnsureNotNull(nameof(error));
        return new Result(error);
    }

    /// <summary>
    /// Creates a failed result with multiple error messages.
    /// </summary>
    /// <param name="messages">Collection of error messages.</param>
    /// <returns>A failed Result with the specified error messages.</returns>
    /// <example>
    /// <code>
    /// var errors = new[] { "Name is required", "Email is invalid" };
    /// var result = Result.Fail(errors);
    /// </code>
    /// </example>
    public static Result Fail(IEnumerable<string> messages)
    {
        messages = messages.EnsureNotNull(nameof(messages));
        var messageList = messages.ToList();
        if (messageList.Count == 0)
            throw new ArgumentException("The error messages list cannot be empty", nameof(messages));
        
        return new Result(messageList.Select(m => new Error(m)).ToImmutableList<IReason>());
    }

    /// <summary>
    /// Creates a failed result with multiple error reasons.
    /// </summary>
    /// <param name="errors">Collection of error reasons.</param>
    /// <returns>A failed Result with the specified error reasons.</returns>
    /// <example>
    /// <code>
    /// var errors = new[] 
    /// { 
    ///     new ValidationError("Name is required"), 
    ///     new ValidationError("Email is invalid") 
    /// };
    /// var result = Result.Fail(errors);
    /// </code>
    /// </example>
    public static Result Fail(IEnumerable<IError> errors)
    {
        errors = errors.EnsureNotNull(nameof(errors));
        var errorList = errors.ToList();
        if (errorList.Count == 0)
            throw new ArgumentException("The errors list cannot be empty", nameof(errors));
        
        return new Result(errorList.ToImmutableList<IReason>());
    }

    /// <summary>
    /// Adds a success reason with a message to the result.
    /// </summary>
    /// <param name="message">The success message.</param>
    /// <returns>A new Result with the added success reason.</returns>
    /// <exception cref="ArgumentException">Thrown when the message is null or empty.</exception>
    /// <example>
    /// <code>
    /// var result = Result.Ok()
    ///     .WithSuccess("Operation completed successfully");
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
    /// var success = new Success("Operation completed");
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
    ///     new Success("Processed")
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
    /// var reasons = new List&lt;IReason&gt; { new Success("Validated"), new Success("Processed") };
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
    /// Adds an error reason with a message to the result.
    /// Note: This will make the result failed if it wasn't already.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A new Result with the added error reason.</returns>
    /// <exception cref="ArgumentException">Thrown when the message is null or empty.</exception>
    /// <example>
    /// <code>
    /// var result = Result.Ok()
    ///     .WithError("Operation failed");
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
    /// var error = new Error("Database connection failed");
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
}
