namespace REslava.Result;

/// <summary>
/// Secondary capability interface for reasons that expose system/diagnostic metadata.
/// Implement alongside <see cref="IReason"/> to opt-in to metadata support.
/// </summary>
/// <remarks>
/// <para>
/// All built-in <see cref="Reason"/> subclasses implement this interface.
/// External <see cref="IReason"/> implementations can also implement it to participate
/// in diagnostic tooling, pipeline diagrams, and structured logging.
/// </para>
/// <para>
/// Use pattern matching to access metadata from <see cref="IReason"/>-typed references:
/// <code>
/// if (reason is IReasonMetadata m)
///     Console.WriteLine(m.Metadata.CallerMember);
/// </code>
/// Or use the ergonomic extension method:
/// <code>
/// var caller = reason.TryGetMetadata()?.CallerMember;
/// </code>
/// </para>
/// </remarks>
public interface IReasonMetadata
{
    /// <summary>
    /// System/diagnostic metadata captured at the reason's creation site.
    /// Never null — defaults to <see cref="ReasonMetadata.Empty"/> when no metadata is captured.
    /// </summary>
    ReasonMetadata Metadata { get; }
}
