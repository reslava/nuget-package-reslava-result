namespace REslava.Result;

/// <summary>
/// Opt-in interface that enables generic error construction via a static factory method.
/// Implement on custom error types to participate in <c>Result.Fail&lt;TError&gt;(string)</c>.
/// </summary>
/// <typeparam name="TSelf">The implementing error type (CRTP pattern).</typeparam>
/// <example>
/// <code>
/// // Instead of: Result&lt;User&gt;.Fail(new NotFoundError("User not found"))
/// // You can write:
/// Result&lt;User&gt;.Fail&lt;NotFoundError&gt;("User not found");
///
/// // Useful in generic code:
/// Result&lt;T&gt; NotFound&lt;T, TError&gt;(string message) where TError : IErrorFactory&lt;TError&gt;
///     => Result&lt;T&gt;.Fail(TError.Create(message));
/// </code>
/// </example>
public interface IErrorFactory<TSelf> where TSelf : IErrorFactory<TSelf>
{
    /// <summary>Creates an instance of <typeparamref name="TSelf"/> with the given message.</summary>
    static abstract TSelf Create(string message);
}
