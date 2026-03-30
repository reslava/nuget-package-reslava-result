namespace REslava.Result.Observers
{
    public sealed record PipelineStartContext(
        string PipelineId,
        string MethodName,
        string? InputValue,
        System.DateTimeOffset StartedAt
    );

    public sealed record PipelineEndContext(
        string PipelineId,
        string MethodName,
        bool IsSuccess,
        string? ErrorType,
        string? OutputValue,
        long ElapsedMs,
        System.DateTimeOffset EndedAt
    );

    public sealed record NodeEnterContext(
        string PipelineId,
        string NodeId,
        string StepName,
        string? InputValue,
        int NodeIndex
    );

    public sealed record NodeExitContext(
        string PipelineId,
        string NodeId,
        string StepName,
        bool IsSuccess,
        string? OutputValue,
        string? ErrorType,
        string? ErrorMessage,
        long ElapsedMs,
        int NodeIndex
    );
}
