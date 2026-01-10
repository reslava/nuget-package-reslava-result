using System.Runtime.CompilerServices;

namespace REslava.Result;

public partial class Result : IResult
{
    public static Result Ok()
    {
        return new Result();
    }       

    public static Result Fail(string message)
    {
        var result = new Result();
        result.Reasons.Add(new Error(message));
        return result;
    }

    public static Result Fail(Error error)
    {
        var result = new Result();
        result.Reasons.Add(error);
        return result;
    }

    public static Result Fail(IEnumerable<string> messages)
    {
        if (messages is null)
        { 
            throw new ArgumentNullException(nameof(messages), "The error messages list can not be null");
        }
        if (!messages.Any())
        { 
            throw new ArgumentException("The error messages list can not be empty", nameof(messages));
        }

        var result = new Result();
        result.Reasons.AddRange(messages.Select(errorMessage => new Error(errorMessage)));
        return result;
    }

    public static Result Fail(IEnumerable<Error> errors)
    {
        if (errors is null)
        {
            throw new ArgumentNullException(nameof(errors), "The errors list can not be null");
        }
        if (!errors.Any())
        {
            throw new ArgumentException("The errors list can not be empty", nameof(errors));
        }

        var result = new Result();
        result.Reasons.AddRange(errors);
        return result;
    }
}

public partial class Result<TValue> : Result, IResult<TValue>
{   
    public static Result<TValue> Ok(TValue value)
    {        
        var result = new Result<TValue>() { Value = value };
        return result;
    }
    
    public static Result<TValue> From(Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            throw new InvalidOperationException(
                "Cannot convert a successful Result to Result<TValue>. " +
                "Use Result<TValue>.Ok() with a value instead.");
        }

        var typedResult = new Result<TValue>();
        typedResult.Reasons.AddRange(result.Reasons);

        return typedResult;
    }    
    
    public static new Result<TValue> Fail(string message)
    {     
        return Result<TValue>.From(Result.Fail(message));
    }

    public static new Result<TValue> Fail(Error error)
    {
        return Result<TValue>.From(Result.Fail(error));
    }

    public static new Result<TValue> Fail(IEnumerable<string> messages)
    {
        return Result<TValue>.From(Result.Fail(messages));
    }

    public static new Result<TValue> Fail(IEnumerable<Error> errors)
    {
        return Result<TValue>.From(Result.Fail(errors));
    }
}
