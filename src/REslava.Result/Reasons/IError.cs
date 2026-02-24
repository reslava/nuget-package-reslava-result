namespace REslava.Result;

/// <summary>
/// Marker interface for error reasons. Implement or extend to create custom error types
/// that integrate with <see cref="Result{T}"/> failure semantics and HTTP status mapping.
/// </summary>
/// <seealso cref="Error"/>
/// <seealso cref="ValidationError"/>
/// <seealso cref="NotFoundError"/>
public interface IError : IReason { }
