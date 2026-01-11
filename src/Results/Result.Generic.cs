namespace REslava.Result;

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

    public new Result<TValue> WithSuccess(string message) { return (Result<TValue>)base.WithSuccess(message); }
    public new Result<TValue> WithError(string message) { return (Result<TValue>)base.WithError(message); }
    public new Result<TValue> WithSuccess(ISuccess success) { return (Result<TValue>)base.WithSuccess(success); }
    public new Result<TValue> WithError(IError error) { return (Result<TValue>)base.WithError(error); }
    public new Result<TValue> WithSuccesses(IEnumerable<ISuccess> sucesses) { return (Result<TValue>)base.WithSuccesses(sucesses); }
    public new Result<TValue> WithErrors(IEnumerable<IError> errors) { return (Result<TValue>)base.WithErrors(errors); }
}
