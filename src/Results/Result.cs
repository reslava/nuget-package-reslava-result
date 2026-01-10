using System.Collections.Generic;
using static REslava.Result.Result;

namespace REslava.Result;

public partial class Result : IResult
{
    public bool IsSuccess => !IsFailed;
    public bool IsFailed => Reasons.OfType<IError>().Any();
    public List<IReason> Reasons { get; }
    public IReadOnlyList<IError> Errors => Reasons.OfType<IError>().ToList();
    public IReadOnlyList<ISuccess> Successes => Reasons.OfType<ISuccess>().ToList();

    public Result()
    {
        Reasons = [];
    }

    public IResult WithSuccess(string message) { Reasons.Add(new Success(message)); return this; }
    public IResult WithError(string message) { Reasons.Add(new Error(message)); return this; }
    public IResult WithSuccess(Success success) { Reasons.Add((IReason)success); return this; }
    public IResult WithError(Error error) { Reasons.Add((IReason)error); return this; }
    public IResult WithSuccesses(IEnumerable<Success> sucesses) { Reasons.AddRange((IEnumerable<IReason>)sucesses); return this; }
    public IResult WithErrors(IEnumerable<Error> errors) { Reasons.AddRange((IEnumerable<IReason>)errors); return this; }

    public override string ToString()
    {
        var reasonsString = Reasons.Any()
                                ? $", Reasons={string.Join("; ", Reasons)}"
                                : string.Empty;

        return $"Result: IsSuccess='{IsSuccess}'{reasonsString}";
    }    
}

public partial class Result<TValue> : Result, IResult<TValue>
{
    public TValue? ValueOrDefault { get; private set; }
    public TValue? Value
    {
        get
        {
            ThrowIfFailed();
            return ValueOrDefault;
        }

        private set => ValueOrDefault = value;
    }

    public Result() { }

    private void ThrowIfFailed()
    {
        if (IsFailed)
        { 
            throw new InvalidOperationException($"Result is in status failed. Value is not set.");
        }
    }    

    public override string ToString()
    {
        var baseString = base.ToString();
        var valueString = $"{nameof(Value)} = {ValueOrDefault}";
        return $"{baseString}, {valueString}";
    }    
    
    public new IResult<TValue> WithSuccess(string message) { return (IResult<TValue>)base.WithSuccess(message); }
    public new IResult<TValue> WithError(string message) { return (IResult<TValue>)base.WithError(message); }
    public new IResult<TValue> WithSuccess(Success success) { return (IResult<TValue>)base.WithSuccess(success); }
    public new IResult<TValue> WithError(Error error) { return (IResult<TValue>)base.WithError(error); }
    public new IResult<TValue> WithSuccesses(IEnumerable<Success> sucesses) { return (IResult<TValue>)base.WithSuccesses(sucesses); }
    public new IResult<TValue> WithErrors(IEnumerable<Error> errors) { return (IResult<TValue>)base.WithErrors(errors); }
}
