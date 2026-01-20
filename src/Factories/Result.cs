using System.Collections.Immutable;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace REslava.Result;

// Factory methods for Result (non-generic)
public partial class Result
{
    /// <summary>
    /// Creates a successful result without any reasons.
    /// </summary>
    /// <returns>A successful Result instance.</returns>
    /// <example>
    /// <code>
    /// var result = Result.Ok();
    /// Console.WriteLine(result.IsSuccess); // true
    /// </code>
    /// </example>
    public static Result Ok() => new Result();
    
    /// <summary>
    /// Creates a successful result with a success message.
    /// </summary>
    /// <param name="message">The success message to include.</param>
    /// <returns>A successful Result with the specified message.</returns>
    /// <example>
    /// <code>
    /// var result = Result.Ok("Operation completed successfully");
    /// Console.WriteLine(string.Join(", ", result.Successes.Select(s => s.Message)));
    /// // Output: Operation completed successfully
    /// </code>
    /// </example>
    public static Result Ok(string message)
    {
        message = message.EnsureNotNullOrEmpty(nameof(message));
        return new Result(new Success(message));
    }
    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    /// <param name="message">The error message to include.</param>
    /// <returns>A failed Result with the specified error message.</returns>
    /// <example>
    /// <code>
    /// var result = Result.Fail("Invalid input");
    /// Console.WriteLine(string.Join(", ", result.Errors.Select(e => e.Message)));
    /// // Output: Invalid input
    /// </code>
    /// </example>
    public static Result Fail(string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));
        return new Result(new Error(message));
    }

    /// <summary>
    /// Creates a successful result with a success reason.
    /// </summary>
    /// <param name="success">The success reason to include.</param>
    /// <returns>A successful Result with the specified success reason.</returns>
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
    /// Creates a failed result with an error reason.
    /// </summary>
    /// <param name="error">The error reason to include.</param>
    /// <returns>A failed Result with the specified error reason.</returns>
    /// <example>
    /// <code>
    /// var error = new ValidationError("Email is required");
    /// var result = Result.Fail(error);
    /// </code>
    /// </example>
    public static Result Fail(IError error)
    {
        ArgumentNullException.ThrowIfNull(error, nameof(error));
        return new Result(error);
    }

    /// <summary>
    /// Creates a successful result with multiple success messages.
    /// </summary>
    /// <param name="messages">Collection of success messages.</param>
    /// <returns>A successful Result with the specified messages.</returns>
    /// <example>
    /// <code>
    /// var messages = new[] { "File uploaded", "Database updated" };
    /// var result = Result.Ok(messages);
    /// </code>
    /// </example>
    public static Result Ok(IEnumerable<string> messages)
    {
        messages = messages.EnsureNotNullOrEmpty(nameof(messages));
        return new Result(messages.Select(m => new Success(m)).ToImmutableList<IReason>());
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
        messages = messages.EnsureNotNullOrEmpty(nameof(messages));
        return new Result(messages.Select(m => new Error(m)).ToImmutableList<IReason>());
    }

    /// <summary>
    /// Creates a successful result with multiple success reasons.
    /// </summary>
    /// <param name="successes">Collection of success reasons.</param>
    /// <returns>A successful Result with the specified success reasons.</returns>
    /// <example>
    /// <code>
    /// var successes = new[] 
    /// { 
    ///     new Success("User created"), 
    ///     new Success("Welcome email sent") 
    /// };
    /// var result = Result.Ok(successes);
    /// </code>
    /// </example>
    public static Result Ok(IEnumerable<ISuccess> successes)
    {
        successes = successes.EnsureNotNullOrEmpty(nameof(successes));
        return new Result(successes.ToImmutableList<IReason>());
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
        errors = errors.EnsureNotNullOrEmpty(nameof(errors));
        return new Result(errors.ToImmutableList<IReason>());
    }
}

