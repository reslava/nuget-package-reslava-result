using System.Collections.Immutable;

namespace REslava.Result;

// Factory methods for Result (non-generic)
public partial class Result
{
    public static Result Ok() => new Result();
    
    public static Result Ok(string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));
        return new Result(new Success(message));
    }

    public static Result Ok(ISuccess success) 
    {
        ArgumentNullException.ThrowIfNull(success, nameof(success));
        return new Result(success);
    }

    public static Result Ok(IEnumerable<string> messages)
    {
        ArgumentNullException.ThrowIfNull(messages, nameof(messages));
        var messageList = messages.ToList();
        if (messageList.Count == 0)
            throw new ArgumentException("The success messages list cannot be empty", nameof(messages));
        
        return new Result(messageList.Select(m => new Success(m)).ToImmutableList<IReason>());
    }

    public static Result Ok(IEnumerable<ISuccess> successes)
    {
        ArgumentNullException.ThrowIfNull(successes, nameof(successes));
        var successList = successes.ToList();
        if (successList.Count == 0)
            throw new ArgumentException("The successes list cannot be empty", nameof(successes));
        
        return new Result(successList.ToImmutableList<IReason>());
    }

    public static Result Fail(string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));
        return new Result(new Error(message));
    }

    public static Result Fail(IError error) 
    {        
        ArgumentNullException.ThrowIfNull(error, nameof(error));
        return new Result(error);
    }

    public static Result Fail(IEnumerable<string> messages)
    {        
        ArgumentNullException.ThrowIfNull(messages, nameof(messages));
        var messageList = messages.ToList();
        if (messageList.Count == 0)
            throw new ArgumentException("The error messages list cannot be empty", nameof(messages));
        
        return new Result(messageList.Select(m => new Error(m)).ToImmutableList<IReason>());
    }

    public static Result Fail(IEnumerable<IError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors, nameof(errors));
        var errorList = errors.ToList();
        if (errorList.Count == 0)
            throw new ArgumentException("The errors list cannot be empty", nameof(errors));
        
        return new Result(errorList.ToImmutableList<IReason>());
    }
}

