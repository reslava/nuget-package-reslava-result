using System.Collections.Immutable;
namespace REslava.Result;

public interface IResult
{
    bool IsSuccess { get; }
    bool IsFailed { get; }
    ImmutableList<IReason> Reasons { get; }
    ImmutableList<IError> Errors { get; }
    ImmutableList<ISuccess> Successes { get; }
}

public interface IResult<out TValue> : IResult
{
    TValue? Value { get; }    
}
