using REslava.Result;

public class ExceptionError : Reason<ExceptionError>, IError
{
    public Exception Exception { get; }

    // Public constructors
    public ExceptionError(Exception exception)
        : base(exception?.Message ?? "An exception occurred")
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    public ExceptionError(string message, Exception exception)
        : base(message)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    // Private constructor for creating copies (needed for fluent API)
    private ExceptionError(
        string message,
        Dictionary<string, object> tags,
        Exception exception)
        : base(message, tags)
    {
        Exception = exception;
    }

    // Override CreateNew to include Exception
    protected override ExceptionError CreateNew(
        string message,
        Dictionary<string, object> tags)
    {
        return new ExceptionError(message, tags, Exception);
    }

    // Custom fluent methods specific to ExceptionError
    public ExceptionError WithExceptionType(string type)
    {
        return WithTags("ExceptionType", type);
    }

    public ExceptionError WithStackTrace()
    {
        return WithTags("StackTrace", Exception.StackTrace ?? "(Stack trace unavailable)");
    }
}
