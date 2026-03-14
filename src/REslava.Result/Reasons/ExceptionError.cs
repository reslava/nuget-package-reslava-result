using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace REslava.Result;

/// <summary>
/// Wraps a .NET <see cref="Exception"/> as a typed error reason.
/// Automatically captures the exception type, stack trace, and inner exception message as tags.
/// Produced automatically by <see cref="Result.Try"/> and <see cref="Result.TryAsync"/>.
/// </summary>
/// <example>
/// <code>
/// var result = Result&lt;string&gt;.Try(() => File.ReadAllText("config.json"));
/// if (result.IsFailure)
/// {
///     var ex = result.Errors.OfType&lt;ExceptionError&gt;().First();
///     Console.WriteLine(ex.Exception.Message);      // original exception message
///     Console.WriteLine(ex.Tags["ExceptionType"]);  // e.g. "FileNotFoundException"
/// }
/// </code>
/// </example>
public class ExceptionError : Reason<ExceptionError>, IError
{
    /// <summary>Gets the original .NET exception that caused this error.</summary>
    public Exception Exception { get; init; }

    /// <summary>Creates an error from an exception, using the exception's own message.</summary>
    public ExceptionError(
        Exception exception,
        [CallerMemberName] string? callerMember = null,
        [CallerFilePath]   string? callerFile   = null,
        [CallerLineNumber] int     callerLine   = 0)
        : base(
            exception?.Message ?? "An exception occurred",
            CreateExceptionTags(exception),
            ReasonMetadata.FromCaller(callerMember, callerFile, callerLine))
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    /// <summary>Creates an error with a custom message, preserving the original exception for diagnostics.</summary>
    public ExceptionError(
        string message,
        Exception exception,
        [CallerMemberName] string? callerMember = null,
        [CallerFilePath]   string? callerFile   = null,
        [CallerLineNumber] int     callerLine   = 0)
        : base(
            message,
            CreateExceptionTags(exception),
            ReasonMetadata.FromCaller(callerMember, callerFile, callerLine))
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    // ========================================================================
    // Private Constructor - For immutable copies (used by WithMessage/WithTags)
    // ========================================================================
    private ExceptionError(
        string message,
        ImmutableDictionary<string, object> tags,
        Exception exception)
        : base(message, tags)
    {
        Exception = exception;
    }

    // ========================================================================
    // Factory Method - Required by CRTP
    // ========================================================================
    protected override ExceptionError CreateNew(
        string message,
        ImmutableDictionary<string, object> tags)
    {
        // When creating new instances (via WithMessage/WithTags),
        // preserve the original Exception
        return new ExceptionError(message, tags, Exception);
    }

    // ========================================================================
    // Helper Method - Create tags from Exception (static for clarity)
    // ========================================================================
    private static ImmutableDictionary<string, object> CreateExceptionTags(Exception? exception)
    {
        exception = exception.EnsureNotNull("Exception cannot be null");        

        var tags = ImmutableDictionary<string, object>.Empty
            .Add("ExceptionType", exception.GetType().Name);

        if (exception.StackTrace != null)
        {
            tags = tags.Add("StackTrace", exception.StackTrace);
        }

        if (exception.InnerException != null)
        {
            tags = tags.Add("InnerException", exception.InnerException.Message);
        }

        return tags;
    }
}