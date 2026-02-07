using System;
using REslava.Result;

namespace REslava.Result.AdvancedPatterns
{
    /// <summary>
    /// Essential conversion methods between Result&lt;T&gt; and OneOf&lt;T1, T2&gt;.
    /// Focused on the most common use cases with explicit, clear conversions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This extension provides the two most essential conversion patterns:
    /// 1. Result → OneOf for migrating from Result to OneOf patterns
    /// 2. OneOf → Result for integrating OneOf with existing Result code
    /// </para>
    /// <para>
    /// All conversions require explicit error mapping functions to ensure
    /// type safety and clear intent. No "magic" default mapping is provided.
    /// </para>
    /// </remarks>
    public static class ResultOneOfExtensions
    {
        #region Result → OneOf

        /// <summary>
        /// Converts a Result&lt;T&gt; to OneOf&lt;TError, T&gt; using custom error mapping.
        /// This is the most common conversion pattern for migrating from Result to OneOf.
        /// </summary>
        /// <typeparam name="TError">The error type for OneOf.</typeparam>
        /// <typeparam name="T">The success type.</typeparam>
        /// <param name="result">The Result to convert.</param>
        /// <param name="errorMapper">Function to map Result error to TError.</param>
        /// <returns>A OneOf containing either the mapped error or the success value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when errorMapper is null.</exception>
        /// <example>
        /// <code>
        /// // Migrate from Result to OneOf
        /// Result&lt;User&gt; result = GetUser();
        /// OneOf&lt;ApiError, User&gt; oneOf = result.ToOneOf(reason => new ApiError(reason.Message));
        /// </code>
        /// </example>
        public static OneOf<TError, T> ToOneOf<TError, T>(
            this Result<T> result, 
            Func<IReason, TError> errorMapper)
        {
            if (errorMapper == null) throw new ArgumentNullException(nameof(errorMapper));
            
            return result.IsSuccess 
                ? OneOf<TError, T>.FromT2(result.Value!)
                : OneOf<TError, T>.FromT1(errorMapper(result.Reasons.First()));
        }

        #endregion

        #region OneOf → Result

        /// <summary>
        /// Converts a OneOf&lt;TError, T&gt; to Result&lt;T&gt; using custom error mapping.
        /// Essential for integrating OneOf with existing Result-based code.
        /// </summary>
        /// <typeparam name="TError">The error type in OneOf.</typeparam>
        /// <typeparam name="T">The success type.</typeparam>
        /// <param name="oneOf">The OneOf to convert.</param>
        /// <param name="errorMapper">Function to map TError to IError.</param>
        /// <returns>A Result containing either the mapped error or the success value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when errorMapper is null.</exception>
        /// <example>
        /// <code>
        /// // Integrate OneOf with existing Result infrastructure
        /// OneOf&lt;ApiError, User&gt; oneOf = GetUser();
        /// Result&lt;User&gt; result = oneOf.ToResult(error => Error.Create(error.Message, error.Code));
        /// 
        /// if (result.IsSuccess)
        /// {
        ///     await SaveToDatabase(result.Value);
        /// }
        /// </code>
        /// </example>
        public static Result<T> ToResult<TError, T>(
            this OneOf<TError, T> oneOf, 
            Func<TError, IError> errorMapper)
        {
            if (errorMapper == null) throw new ArgumentNullException(nameof(errorMapper));
            
            return oneOf.Match(
                case1: error => Result<T>.Fail(errorMapper(error)),
                case2: success => Result<T>.Ok(success)
            );
        }

        #endregion
    }
}
