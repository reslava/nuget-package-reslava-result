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
    public Result WithSuccess(ISuccess success) { Reasons.Add((IReason)success); return this; }
    public Result WithError(IError error) { Reasons.Add((IReason)error); return this; }
    public Result WithSuccesses(IEnumerable<ISuccess> sucesses) { Reasons.AddRange((IEnumerable<IReason>)sucesses); return this; }
    public Result WithErrors(IEnumerable<IError> errors) { Reasons.AddRange((IEnumerable<IReason>)errors); return this; }

    public override string ToString()
    {
        var reasonsString = Reasons.Any()
                                ? $", Reasons={string.Join("; ", Reasons)}"
                                : string.Empty;

        return $"Result: IsSuccess='{IsSuccess}'{reasonsString}";
    }    
}
