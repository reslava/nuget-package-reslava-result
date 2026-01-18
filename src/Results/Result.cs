using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// Represents the result of an operation without a value.
/// Immutable by design - all operations return new instances.
/// </summary>
public partial class Result : IResult
{
    public bool IsSuccess => !IsFailed;
    public bool IsFailed => Reasons.OfType<IError>().Any();
    public ImmutableList<IReason> Reasons { get; private init; }
    public ImmutableList<IError> Errors => Reasons.OfType<IError>().ToImmutableList(); 
    public ImmutableList<ISuccess> Successes => Reasons.OfType<ISuccess>().ToImmutableList();

    // Protected constructors - only factory methods create Results
    protected Result()
    {
        Reasons = [];
    }

    internal Result(ImmutableList<IReason> reasons)
    {
        Reasons = reasons ?? [];
    }

    protected Result(IReason reason)
    {
        ArgumentNullException.ThrowIfNull(reason, nameof(reason));
        Reasons = ImmutableList.Create(reason);
    }

    // Fluent methods - all return NEW instances (immutability)
    public Result WithReason(IReason reason) 
    { 
        ArgumentNullException.ThrowIfNull(reason, nameof(reason));
        return new Result(Reasons.Add(reason)); 
    }

    public Result WithReasons(ImmutableList<IReason> reasons) 
    { 
        ArgumentNullException.ThrowIfNull(reasons, nameof(reasons));
        if (reasons.Count == 0) 
             throw new ArgumentException("The reasons list cannot be empty", nameof(reasons));        
        
        return new Result(Reasons.AddRange(reasons)); 
    }

    public Result WithSuccess(string message) 
    { 
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));
        return new Result(Reasons.Add(new Success(message)));           
    }

    public Result WithSuccess(ISuccess success) 
    { 
        ArgumentNullException.ThrowIfNull(success, nameof(success));
        return new Result(Reasons.Add(success)); 
    }

    public Result WithSuccesses(IEnumerable<ISuccess> successes) 
    { 
        ArgumentNullException.ThrowIfNull(successes, nameof(successes));
        var successList = successes.ToList();
        if (successList.Count == 0)
            throw new ArgumentException("The successes list cannot be empty", nameof(successes));
        return new Result(Reasons.AddRange(successList)); 
    }

    public Result WithError(string message) 
    { 
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));
        return new Result(Reasons.Add(new Error(message))); 
    }

    public Result WithError(IError error) 
    { 
        ArgumentNullException.ThrowIfNull(error, nameof(error));
        return new Result(Reasons.Add(error)); 
    }

    public Result WithErrors(IEnumerable<IError> errors) 
    { 
        ArgumentNullException.ThrowIfNull(errors, nameof(errors));
        var errorList = errors.ToList();
        if (errorList.Count == 0)
            throw new ArgumentException("The errors list cannot be empty", nameof(errors));
        return new Result(Reasons.AddRange(errorList)); 
    }

    public override string ToString()
    {
        var reasonsString = Reasons.Any()
            ? $", Reasons={string.Join("; ", Reasons)}"
            : string.Empty;

        return $"Result: IsSuccess='{IsSuccess}'{reasonsString}";
    }
}