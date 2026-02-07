namespace REslava.Result;

public partial class Result
{
    /// <summary>
    /// Executes an operation and wraps the result in a Result.
    /// If the operation throws an exception, returns a failed Result with an ExceptionError.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="errorHandler">Optional custom error handler. If null, creates an ExceptionError.</param>
    /// <returns>A successful Result, or a failed Result with the error.</returns>
    /// <example>
    /// <code>
    /// // Simple usage
    /// var result = Result.Try(() => File.WriteAllText("file.txt", "content"));
    /// 
    /// // With custom error handler
    /// var result = Result.Try(
    ///     () => DeleteFile(path),
    ///     ex => new Error($"Failed to delete file: {ex.Message}")
    ///         .WithTags("FilePath", path)
    /// );
    /// </code>
    /// </example>
    public static Result Try(
        Action operation,
        Func<Exception, IError>? errorHandler = null)
    {
        operation = operation.EnsureNotNull(nameof(operation));

        try
        {
            operation();
            return Ok();
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
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="errorHandler">Optional custom error handler. If null, creates an ExceptionError.</param>
    /// <returns>A task containing a successful Result, or a failed Result with the error.</returns>
    /// <example>
    /// <code>
    /// // Simple usage
    /// var result = await Result.TryAsync(
    ///     async () => await File.WriteAllTextAsync("file.txt", "content")
    /// );
    /// 
    /// // With custom error handler
    /// var result = await Result.TryAsync(
    ///     async () => await DeleteFileAsync(path),
    ///     ex => new Error($"Failed to delete file: {ex.Message}")
    ///         .WithTags("FilePath", path)
    /// );
    /// </code>
    /// </example>
    public static async Task<Result> TryAsync(
        Func<Task> operation,
        Func<Exception, IError>? errorHandler = null)
    {
        operation = operation.EnsureNotNull(nameof(operation));

        try
        {
            await operation();
            return Ok();
        }
        catch (Exception ex)
        {
            var error = errorHandler?.Invoke(ex) ?? new ExceptionError(ex);
            return Fail(error);
        }
    }
}
