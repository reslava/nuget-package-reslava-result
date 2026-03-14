namespace REslava.Result;

/// <summary>
/// System/diagnostic metadata captured automatically when an error or success is created.
/// Separate from <see cref="IReason.Tags"/>, which holds user/business metadata.
/// </summary>
/// <remarks>
/// <para>Tags are for user/business metadata (field names, HTTP codes, custom context).</para>
/// <para>Metadata is for framework/diagnostic data (caller info, pipeline position).</para>
/// </remarks>
public sealed record ReasonMetadata
{
    /// <summary>Name of the method that created this reason (via <c>[CallerMemberName]</c>).</summary>
    public string? CallerMember { get; init; }

    /// <summary>Source file path of the creation site (via <c>[CallerFilePath]</c>).</summary>
    public string? CallerFile { get; init; }

    /// <summary>Source line number of the creation site (via <c>[CallerLineNumber]</c>).</summary>
    public int? CallerLine { get; init; }

    /// <summary>Singleton empty metadata instance.</summary>
    public static readonly ReasonMetadata Empty = new();

    /// <summary>
    /// Creates a <see cref="ReasonMetadata"/> from compiler-injected caller attributes.
    /// Returns <see cref="Empty"/> when <paramref name="callerMember"/> is null.
    /// </summary>
    internal static ReasonMetadata FromCaller(string? callerMember, string? callerFile, int callerLine)
        => callerMember is null
            ? Empty
            : new ReasonMetadata
            {
                CallerMember = callerMember,
                CallerFile   = callerFile,
                CallerLine   = callerLine > 0 ? callerLine : null
            };
}
