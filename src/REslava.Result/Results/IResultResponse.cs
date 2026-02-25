using System.Collections.Immutable;
namespace REslava.Result;

/// <summary>
/// Non-generic contract for a result — exposes status flags and reason lists
/// without requiring knowledge of the success value type.
/// Useful for middleware, logging pipelines, and generic result-handling utilities.
/// </summary>
public interface IResultResponse
{
    /// <summary>Gets a value indicating whether this result represents a successful outcome.</summary>
    bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether this result represents a failed outcome.</summary>
    bool IsFailure { get; }

    /// <summary>Gets all reasons (both errors and successes) attached to this result.</summary>
    ImmutableList<IReason> Reasons { get; }

    /// <summary>Gets the error reasons. Empty when <see cref="IsSuccess"/> is <c>true</c>.</summary>
    ImmutableList<IError> Errors { get; }

    /// <summary>Gets the success reasons. Empty when <see cref="IsFailure"/> is <c>true</c>.</summary>
    ImmutableList<ISuccess> Successes { get; }
}

/// <summary>
/// Generic contract for a typed result — extends <see cref="IResultResponse"/>
/// with access to the successful value.
/// </summary>
/// <typeparam name="TValue">The type of the value carried on success.</typeparam>
public interface IResultResponse<out TValue> : IResultResponse
{
    /// <summary>
    /// Gets the successful value, or <c>null</c> if the result has failed.
    /// Always check <see cref="IResultResponse.IsSuccess"/> before accessing,
    /// or use <c>GetValueOr()</c> for a null-safe alternative.
    /// </summary>
    TValue? Value { get; }
}
