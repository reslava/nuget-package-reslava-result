using System.Collections.Immutable;
namespace REslava.Result;

public partial class Result : IResult
{
    public bool IsSuccess => !IsFailed;
    public bool IsFailed => Reasons.OfType<IError>().Any();
    public ImmutableList<IReason> Reasons { get; private init; }
    public ImmutableList<IError> Errors =>
        Reasons.OfType<IError>().ToImmutableList(); 
    public ImmutableList<ISuccess> Successes => 
        Reasons.OfType<ISuccess>().ToImmutableList();

    public Result()
    {
        Reasons = []; // Empty immutable list
    }
    public Result(ImmutableList<IReason> reasons)
    {
        ArgumentNullException.ThrowIfNull(reasons);
        Reasons = reasons;
    }

    public Result WithReason(IReason reason) { Reasons.Add(reason); return this; }
    public Result WithReasons(IEnumerable<IReason> reasons) { Reasons.AddRange(reasons); return this; }
    public Result WithSuccess(string message) { Reasons.Add(new Success(message)); return this; }
    public Result WithError(string message) { Reasons.Add(new Error(message)); return this; }
    public Result WithSuccess(ISuccess success) { Reasons.Add((IReason)success); return this; }    
    public Result WithError(IError error) { Reasons.Add((IReason)error); return this; }
    public Result WithSuccesses(IEnumerable<ISuccess> successes) { Reasons.AddRange((IEnumerable<IReason>)successes); return this; }
    public Result WithErrors(IEnumerable<IError> errors) { Reasons.AddRange((IEnumerable<IReason>)errors); return this; }

    public override string ToString()
    {
        var reasonsString = Reasons.Any()
                                ? $", Reasons={string.Join("; ", Reasons)}"
                                : string.Empty;

        return $"Result: IsSuccess='{IsSuccess}'{reasonsString}";
    }    
}
