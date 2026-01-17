namespace REslava.Result;

public partial class Result<TValue> : Result, IResult<TValue>
{
    public static Result<TValue> Ok(TValue value)
    {        
        return new Result<TValue>(value, []);        
    }

    public static Result<TValue> FromResult(Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            throw new InvalidOperationException(
                "Cannot convert a successful Result to Result<TValue>. " +
                "Use Result<TValue>.Ok() with a value instead.");
        }

        var typedResult = new Result<TValue>(default(TValue), []);
        typedResult.Reasons.AddRange(result.Reasons);

        return typedResult;
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
