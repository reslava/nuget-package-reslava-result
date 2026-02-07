using System.Collections.Immutable;

namespace REslava.Result;
// Factory methods for Result<TValue>
public partial class Result<TValue>
{
    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <param name="value">The value to wrap in a successful result.</param>
    /// <returns>A successful Result&lt;TValue&gt; containing the specified value.</returns>
    /// <example>
    /// <code>
    /// var user = new User { Name = "John" };
    /// var result = Result&lt;User&gt;.Ok(user);
    /// Console.WriteLine(result.Value.Name); // John
    /// </code>
    /// </example>
    public static Result<TValue> Ok(TValue value)
    {        
        return new Result<TValue>(value, ImmutableList<IReason>.Empty);
    }

    /// <summary>
    /// Creates a successful result with a value and success message.
    /// </summary>
    /// <param name="value">The value to wrap in a successful result.</param>
    /// <param name="message">The success message to include.</param>
    /// <returns>A successful Result&lt;TValue&gt; with the value and message.</returns>
    /// <example>
    /// <code>
    /// var user = new User { Name = "John" };
    /// var result = Result&lt;User&gt;.Ok(user, "User created successfully");
    /// </code>
    /// </example>
    public static Result<TValue> Ok(TValue value, string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));
        return new Result<TValue>(value, new Success(message));
    }

    /// <summary>
    /// Creates a successful result with a value and success reason.
    /// </summary>
    /// <param name="value">The value to wrap in a successful result.</param>
    /// <param name="success">The success reason to include.</param>
    /// <returns>A successful Result&lt;TValue&gt; with the value and success reason.</returns>
    /// <example>
    /// <code>
    /// var user = new User { Name = "John" };
    /// var success = new Success("User created");
    /// var result = Result&lt;User&gt;.Ok(user, success);
    /// </code>
    /// </example>
    public static Result<TValue> Ok(TValue value, ISuccess success)
    {        
        success = success.EnsureNotNull(nameof(success));
        return new Result<TValue>(value, success);
    }

    /// <summary>
    /// Creates a successful result with a value and multiple success messages.
    /// </summary>
    /// <param name="value">The value to wrap in a successful result.</param>
    /// <param name="messages">Collection of success messages.</param>
    /// <returns>A successful Result&lt;TValue&gt; with the value and messages.</returns>
    /// <example>
    /// <code>
    /// var user = new User { Name = "John" };
    /// var messages = new[] { "User created", "Profile completed" };
    /// var result = Result&lt;User&gt;.Ok(user, messages);
    /// </code>
    /// </example>
    public static Result<TValue> Ok(TValue value, IEnumerable<string> messages)
    {
        messages = messages.EnsureNotNull(nameof(messages));
        var messageList = messages.ToList();
        if (messageList.Count == 0)
            throw new ArgumentException("The success messages list cannot be empty", nameof(messages));
        
        return new Result<TValue>(value, messageList.Select(m => new Success(m)).ToImmutableList<IReason>());
    }

    /// <summary>
    /// Creates a successful result with a value and multiple success reasons.
    /// </summary>
    /// <param name="value">The value to wrap in a successful result.</param>
    /// <param name="successes">Collection of success reasons.</param>
    /// <returns>A successful Result&lt;TValue&gt; with the value and success reasons.</returns>
    /// <example>
    /// <code>
    /// var user = new User { Name = "John" };
    /// var successes = new[] 
    /// { 
    ///     new Success("User created"), 
    ///     new Success("Welcome email sent") 
    /// };
    /// var result = Result&lt;User&gt;.Ok(user, successes);
    /// </code>
    /// </example>
    public static Result<TValue> Ok(TValue value, ImmutableList<ISuccess> successes)
    {
        successes = successes.EnsureNotNull(nameof(successes));
        if (successes.Count == 0)
            throw new ArgumentException("The successes list cannot be empty", nameof(successes));
        
        return new Result<TValue>(value, successes.ToImmutableList<IReason>());
    }

    /// <summary>
    /// Converts a non-generic failed Result to a generic Result&lt;TValue&gt;.
    /// Cannot be used with successful results - use Ok() methods instead.
    /// </summary>
    /// <param name="result">The non-generic result to convert.</param>
    /// <returns>A failed Result&lt;TValue&gt; with the same reasons.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the input result is successful.</exception>
    /// <example>
    /// <code>
    /// var nonGenericResult = Result.Fail("Operation failed");
    /// var genericResult = Result&lt;User&gt;.FromResult(nonGenericResult);
    /// // genericResult.IsFailed is true
    /// </code>
    /// </example>
    public static Result<TValue> FromResult(Result result)
    {
        result = result.EnsureNotNull(nameof(result));

        if (result.IsSuccess)
        {
            throw new InvalidOperationException(
                "Cannot convert a successful Result to Result<TValue>. " +
                "Use Result<TValue>.Ok() with a value instead.");
        }

        return new Result<TValue>(default, result.Reasons);
    }

    /// <summary>
    /// Creates a failed Result&lt;TValue&gt; with an error message.
    /// </summary>
    /// <param name="message">The error message to include.</param>
    /// <returns>A failed Result&lt;TValue&gt; with the specified error message.</returns>
    /// <example>
    /// <code>
    /// var result = Result&lt;User&gt;.Fail("User not found");
    /// Console.WriteLine(result.IsFailed); // true
    /// </code>
    /// </example>
    public static new Result<TValue> Fail(string message)
    {
        return Result<TValue>.FromResult(Result.Fail(message));
    }

    /// <summary>
    /// Creates a failed Result&lt;TValue&gt; with an error reason.
    /// </summary>
    /// <param name="error">The error reason to include.</param>
    /// <returns>A failed Result&lt;TValue&gt; with the specified error reason.</returns>
    /// <example>
    /// <code>
    /// var error = new ValidationError("Invalid email format");
    /// var result = Result&lt;User&gt;.Fail(error);
    /// </code>
    /// </example>
    public static new Result<TValue> Fail(IError error)
    {
        return Result<TValue>.FromResult(Result.Fail(error));
    }

    /// <summary>
    /// Creates a failed Result&lt;TValue&gt; with multiple error messages.
    /// </summary>
    /// <param name="messages">Collection of error messages.</param>
    /// <returns>A failed Result&lt;TValue&gt; with the specified error messages.</returns>
    /// <example>
    /// <code>
    /// var errors = new[] { "Name is required", "Email is invalid" };
    /// var result = Result&lt;User&gt;.Fail(errors);
    /// </code>
    /// </example>
    public static new Result<TValue> Fail(IEnumerable<string> messages)
    {
        return Result<TValue>.FromResult(Result.Fail(messages));
    }

    /// <summary>
    /// Creates a failed Result&lt;TValue&gt; with multiple error reasons.
    /// </summary>
    /// <param name="errors">Collection of error reasons.</param>
    /// <returns>A failed Result&lt;TValue&gt; with the specified error reasons.</returns>
    /// <example>
    /// <code>
    /// var errors = new[] 
    /// { 
    ///     new ValidationError("Name is required"), 
    ///     new ValidationError("Email is invalid") 
    /// };
    /// var result = Result&lt;User&gt;.Fail(errors);
    /// </code>
    /// </example>
    public static new Result<TValue> Fail(IEnumerable<IError> errors)
    {
        return Result<TValue>.FromResult(Result.Fail(errors));
    }
}