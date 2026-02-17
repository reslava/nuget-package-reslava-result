using System.Collections.Immutable;

namespace REslava.Result;

// ============================================================================
// ExceptionError (Immutable)
// ============================================================================
public class ExceptionError : Reason<ExceptionError>, IError
{    
    public Exception Exception { get; init; }

    // ========================================================================
    // Public Constructor - Exception message becomes Error message
    // ========================================================================
    public ExceptionError(Exception exception)
        : base(
            exception?.Message ?? "An exception occurred",
            CreateExceptionTags(exception))
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    // ========================================================================
    // Public Constructor - Custom message
    // ========================================================================
    public ExceptionError(string message, Exception exception)
        : base(
            message,
            CreateExceptionTags(exception))
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