namespace REslava.Result;

/// <summary>
/// Cross-TFM argument validation helpers.
/// On net6+, delegates to BCL ThrowIfNullOrEmpty / ThrowIfNull.
/// On netstandard2.0, uses inline checks (BCL methods not available).
/// </summary>
internal static class Throw
{
    internal static void IfNullOrEmpty(string? argument, string? paramName = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(argument, paramName);
#else
        if (string.IsNullOrEmpty(argument))
            throw new System.ArgumentException("The value cannot be null or empty.", paramName);
#endif
    }

    internal static void IfNull(object? argument, string? paramName = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(argument, paramName);
#else
        if (argument is null)
            throw new System.ArgumentNullException(paramName);
#endif
    }
}
