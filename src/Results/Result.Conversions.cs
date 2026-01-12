namespace REslava.Result;

public partial class Result
{
    /// <summary>
    /// Converts this Result to a Result with a value.
    /// </summary>
    public Result<TValue> ToResult<TValue>(TValue value)
    {
        if (IsFailed)
        {
            return Result<TValue>.Fail(Errors.Cast<Error>().ToList());
        }

        var result = Result<TValue>.Ok(value);
        result.Reasons.AddRange(Successes);
        return result;
    }
}

public partial class Result<TValue> 
{
    public static implicit operator Result<TValue>(TValue value) => Ok(value);

    public static implicit operator Result<TValue>(Error error) => Fail(error);

    public static implicit operator Result<TValue>(ExceptionError error) => Fail(error);

    public static implicit operator Result<TValue>(Error[] errors) => Fail(errors);

    public static implicit operator Result<TValue>(List<Error> errors) => Fail(errors);

    public static implicit operator Result<TValue>(ExceptionError[] errors) => Fail(errors);  

    public static implicit operator Result<TValue>(List<ExceptionError> errors) => Fail(errors);  

    /// <summary>
    /// Converts this Result to a non-generic Result, discarding the value.
    /// </summary>
    public Result ToResult()
    {
        var result = new Result();
        result.Reasons.AddRange(Reasons);
        return result;
    }
}