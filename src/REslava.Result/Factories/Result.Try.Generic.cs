// In Factories/Result.Generic.cs or create new Factories/Result.Try.cs
namespace REslava.Result;

public partial class Result<TValue> 
{
    /// <summary>
    /// Executes an operation and wraps the result in a Result.
    /// If the operation throws an exception, returns a failed Result with an ExceptionError.
    /// </summary>
    /// <param name="operation">The operation to execute that returns TValue.</param>
    /// <param name="errorHandler">Optional custom error handler. If null, creates an ExceptionError.</param>
    /// <returns>A successful Result with the operation's return value, or a failed Result with the error.</returns>
    /// <example>
    /// <code>
    /// // Simple usage
    /// var result = Result&lt;int&gt;.Try(() => int.Parse("42"));
    /// 
    /// // With custom error handler
    /// var result = Result&lt;User&gt;.Try(
    ///     () => GetUser(id),
    ///     ex => new Error($"Failed to get user: {ex.Message}")
    /// );
    /// </code>
    /// </example>
    public static Result<TValue> Try(
        Func<TValue> operation,
        Func<Exception, IError>? errorHandler = null)
    {
        operation = operation.EnsureNotNull(nameof(operation));

        try
        {
            var value = operation();
            return Ok(value);
        }
        catch (Exception ex)
        {
            var error = errorHandler?.Invoke(ex) ?? new ExceptionError(ex);
            return Fail(error);
        }
    }

    /// <summary>
    /// Asynchronously executes an operation and wraps the result in a Result.
    /// If the operation throws an exception, returns a failed Result with an ExceptionError.
    /// </summary>
    /// <param name="operation">The async operation to execute that returns TValue.</param>
    /// <param name="errorHandler">Optional custom error handler. If null, creates an ExceptionError.</param>
    /// <returns>A task containing a successful Result with the operation's return value, or a failed Result with the error.</returns>
    /// <example>
    /// <code>
    /// var result = await Result&lt;User&gt;.TryAsync(
    ///     async () => await GetUserAsync(id)
    /// );
    /// </code>
    /// </example>
    public static async Task<Result<TValue>> TryAsync(
        Func<Task<TValue>> operation,
        Func<Exception, IError>? errorHandler = null)
    {
        operation = operation.EnsureNotNull(nameof(operation));

        try
        {
            var value = await operation();
            return Ok(value);
        }
        catch (Exception ex)
        {
            var error = errorHandler?.Invoke(ex) ?? new ExceptionError(ex);
            return Fail(error);
        }
    }
}
