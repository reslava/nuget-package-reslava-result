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

    public Result WithSuccess(string message) { Reasons.Add(new Success(message)); return this; }
    public Result WithError(string message) { Reasons.Add(new Error(message)); return this; }
    public Result WithSuccess(Success success) { Reasons.Add((IReason)success); return this; }
    public Result WithError(Error error) { Reasons.Add((IReason)error); return this; }
    public Result WithSuccesses(IEnumerable<Success> sucesses) { Reasons.AddRange((IEnumerable<IReason>)sucesses); return this; }
    public Result WithErrors(IEnumerable<Error> errors) { Reasons.AddRange((IEnumerable<IReason>)errors); return this; }

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
    public Result<TValue> WithValue(TValue value) { Value = value; return this; }

    public override string ToString()
    {
        var baseString = base.ToString();
        var valueString = $"{nameof(Value)} = {ValueOrDefault}";
        return $"{baseString}, {valueString}";
    }
}
