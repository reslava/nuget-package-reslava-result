using System.Collections.Immutable;
namespace REslava.Result;

public interface IResultResponse
{
    bool IsSuccess { get; }
    bool IsFailed { get; }
    ImmutableList<IReason> Reasons { get; }
    ImmutableList<IError> Errors { get; }
    ImmutableList<ISuccess> Successes { get; }
}

public interface IResultResponse<out TValue> : IResultResponse
{
    TValue? Value { get; }    
}
