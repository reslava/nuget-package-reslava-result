using System.Collections.Immutable;

namespace REslava.Result;
// Factory methods for Result<TValue>
public partial class Result<TValue>
{
    public static Result<TValue> Ok(TValue value)
    {        
        return new Result<TValue>(value, ImmutableList<IReason>.Empty);
    }

    public static Result<TValue> Ok(TValue value, string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));
        return new Result<TValue>(value, new Success(message));
    }

    public static Result<TValue> Ok(TValue value, ISuccess success)
    {        
        ArgumentNullException.ThrowIfNull(success, nameof(success));
        return new Result<TValue>(value, success);
    }

    public static Result<TValue> Ok(TValue value, IEnumerable<string> messages)
    {
        ArgumentNullException.ThrowIfNull(messages, nameof(messages));
        var messageList = messages.ToList();
        if (messageList.Count == 0)
            throw new ArgumentException("The success messages list cannot be empty", nameof(messages));
        
        return new Result<TValue>(value, messageList.Select(m => new Success(m)).ToImmutableList<IReason>());
    }

    public static Result<TValue> Ok(TValue value, ImmutableList<ISuccess> successes)
    {
        ArgumentNullException.ThrowIfNull(successes, nameof(successes));
        if (successes.Count == 0)
            throw new ArgumentException("The successes list cannot be empty", nameof(successes));
        
        return new Result<TValue>(value, successes.ToImmutableList<IReason>());
    }

    public static Result<TValue> FromResult(Result result)
    {
        ArgumentNullException.ThrowIfNull(result, nameof(result));

        if (result.IsSuccess)
        {
            throw new InvalidOperationException(
                "Cannot convert a successful Result to Result<TValue>. " +
                "Use Result<TValue>.Ok() with a value instead.");
        }

        return new Result<TValue>(default, result.Reasons);
    }

    public static new Result<TValue> Fail(string message)
    {
        return Result<TValue>.FromResult(Result.Fail(message));
    }

    public static new Result<TValue> Fail(IError error)
    {
        return Result<TValue>.FromResult(Result.Fail(error));
    }

    public static new Result<TValue> Fail(IEnumerable<string> messages)
    {
        return Result<TValue>.FromResult(Result.Fail(messages));
    }

    public static new Result<TValue> Fail(IEnumerable<IError> errors)
    {
        return Result<TValue>.FromResult(Result.Fail(errors));
    }
}