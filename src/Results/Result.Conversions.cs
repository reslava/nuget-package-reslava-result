namespace REslava.Result;

public partial class Result<TValue> : Result, IResult<TValue>
{
    public static implicit operator Result<TValue>(TValue value) => Ok(value);

    public static implicit operator Result<TValue>(Error error) => Fail(error);

    public static implicit operator Result<TValue>(ExceptionError error) => Fail(error);

    public static implicit operator Result<TValue>(Error[] errors) => Fail(errors);

    public static implicit operator Result<TValue>(List<Error> errors) => Fail(errors);

    public static implicit operator Result<TValue>(ExceptionError[] errors) => Fail(errors);  

    public static implicit operator Result<TValue>(List<ExceptionError> errors) => Fail(errors);  
}
