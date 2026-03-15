using System;
using REslava.Result.AdvancedPatterns;

namespace REslava.Result;

/// <summary>
/// Represents the result of an operation with a typed value and a strongly-typed error.
/// The error type is constrained to <see cref="IError"/>, enabling exhaustive error matching
/// when combined with <see cref="ErrorsOf{T1,T2}"/> error unions.
/// </summary>
/// <typeparam name="TValue">The type of the success value.</typeparam>
/// <typeparam name="TError">The concrete error type. Must implement <see cref="IError"/>.</typeparam>
/// <remarks>
/// Unlike <see cref="Result{TValue}"/> which accumulates errors in a list, this type
/// carries exactly one concrete error. Use with <c>Bind</c> to grow the error union one
/// slot at a time, enabling compile-time exhaustive matching at the call site:
/// <code>
/// Result&lt;Order, ValidationError&gt; Validate(CheckoutRequest req) => ...
/// Result&lt;Order, InventoryError&gt;  ReserveInventory(Order order) => ...
///
/// // Pipeline — union grows automatically
/// Result&lt;Order, ErrorsOf&lt;ValidationError, InventoryError&gt;&gt;
/// Process(CheckoutRequest req) =>
///     Validate(req).Bind(ReserveInventory);
/// </code>
/// </remarks>
public sealed class Result<TValue, TError> where TError : IError
{
    private readonly TValue? _value;
    private readonly TError? _error;

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the success value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the result is a failure. Check <see cref="IsSuccess"/> first.
    /// </exception>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(
            "Cannot access Value on a failed Result. Check IsSuccess first.");

    /// <summary>
    /// Gets the error value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the result is a success. Check <see cref="IsFailure"/> first.
    /// </exception>
    public TError Error => IsFailure
        ? _error!
        : throw new InvalidOperationException(
            "Cannot access Error on a successful Result. Check IsFailure first.");

    /// <summary>
    /// Ambient context carried through this pipeline — entity type, runtime identity,
    /// correlation, operation name, and tenant. Seeded automatically by Ok/Fail and
    /// propagated by all pipeline operators (parent-wins).
    /// </summary>
    public ResultContext? Context { get; internal set; }

    private Result(TValue value)  { _value = value; IsSuccess = true; }
    private Result(TError error)  { _error = error; IsSuccess = false; }

    /// <summary>Creates a successful result with the given value.</summary>
    public static Result<TValue, TError> Ok(TValue value) => new(value)
    {
        Context = new ResultContext { Entity = typeof(TValue).Name }
    };

    /// <summary>Creates a failed result with the given error.</summary>
    public static Result<TValue, TError> Fail(TError error) => new(error)
    {
        Context = new ResultContext { Entity = typeof(TValue).Name }
    };

    /// <summary>
    /// Returns a copy of this result with additional context values merged in.
    /// Only non-null arguments overwrite existing context fields.
    /// </summary>
    public Result<TValue, TError> WithContext(
        string? entityId = null,
        string? correlationId = null,
        string? operationName = null,
        string? tenantId = null)
    {
        var current = Context ?? ResultContext.Empty;
        var merged = current with
        {
            EntityId      = entityId      ?? current.EntityId,
            CorrelationId = correlationId ?? current.CorrelationId,
            OperationName = operationName ?? current.OperationName,
            TenantId      = tenantId      ?? current.TenantId,
        };
        var copy = IsSuccess ? new Result<TValue, TError>(_value!) : new Result<TValue, TError>(_error!);
        copy.Context = merged;
        return copy;
    }

    /// <inheritdoc/>
    public override string ToString() =>
        IsSuccess ? $"Ok({_value})" : $"Fail({_error})";
}
