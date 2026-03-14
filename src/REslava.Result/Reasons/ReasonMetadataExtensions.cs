namespace REslava.Result;

/// <summary>
/// Extension methods for ergonomic access to <see cref="IReasonMetadata"/> from <see cref="IReason"/> references.
/// </summary>
public static class ReasonMetadataExtensions
{
    /// <summary>
    /// Returns the <see cref="ReasonMetadata"/> if the reason implements <see cref="IReasonMetadata"/>;
    /// otherwise returns <see langword="null"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// var caller = error.TryGetMetadata()?.CallerMember;
    /// </code>
    /// </example>
    public static ReasonMetadata? TryGetMetadata(this IReason reason)
        => reason is IReasonMetadata m ? m.Metadata : null;

    /// <summary>
    /// Returns <see langword="true"/> if the reason implements <see cref="IReasonMetadata"/>
    /// and has a non-null <see cref="ReasonMetadata.CallerMember"/>.
    /// </summary>
    public static bool HasCallerInfo(this IReason reason)
        => reason is IReasonMetadata { Metadata.CallerMember: not null };
}
