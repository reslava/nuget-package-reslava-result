using System.Collections.Immutable;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace REslava.Result;

// Factory methods for Result (non-generic)
public partial class Result
{
    public static Result Ok() => new Result();
    
    public static Result Ok(string message)
    {
        message = message.EnsureNotNullOrEmpty(nameof(message));
        return new Result(new Success(message));
    }
    public static Result Fail(string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));
        return new Result(new Error(message));
    }

    public static Result Ok(ISuccess success) 
    {
        success = success.EnsureNotNull(nameof(success));
        return new Result(success);
    }
    public static Result Fail(IError error)
    {
        ArgumentNullException.ThrowIfNull(error, nameof(error));
        return new Result(error);
    }

    public static Result Ok(IEnumerable<string> messages)
    {
        messages = messages.EnsureNotNullOrEmpty(nameof(messages));
        return new Result(messages.Select(m => new Success(m)).ToImmutableList<IReason>());
    }
    public static Result Fail(IEnumerable<string> messages)
    {
        messages = messages.EnsureNotNullOrEmpty(nameof(messages));
        return new Result(messages.Select(m => new Error(m)).ToImmutableList<IReason>());
    }

    public static Result Ok(IEnumerable<ISuccess> successes)
    {
        successes = successes.EnsureNotNullOrEmpty(nameof(successes));
        return new Result(successes.ToImmutableList<IReason>());
    } 
    public static Result Fail(IEnumerable<IError> errors)
    {        
        errors = errors.EnsureNotNullOrEmpty(nameof(errors));
        return new Result(errors.ToImmutableList<IReason>());
    }
}

