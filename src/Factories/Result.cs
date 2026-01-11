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

    public static Result Fail(IError error)
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

    public static Result Fail(IEnumerable<IError> errors)
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
