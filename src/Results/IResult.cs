namespace REslava.Result;

public interface IResult
{
    bool IsSuccess { get; }
    bool IsFailed { get; }
    List<IReason> Reasons { get; }
    IReadOnlyList<IError> Errors { get; }
    IReadOnlyList<ISuccess> Successes { get; }
}

public interface IResult<out TValue> : IResult
{
    TValue? Value { get; }
    TValue? ValueOrDefault { get; }
}
