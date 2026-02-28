using System;
using REslava.Result;

namespace REslava.Result.AdvancedPatterns
{
    /// <summary>
    /// Bidirectional bridge between <see cref="Maybe{T}"/> and <see cref="Result{TValue}"/>.
    /// </summary>
    public static class MaybeResultInterop
    {
        #region Maybe<T> → Result<T>

        /// <summary>
        /// Converts a Maybe to a Result using a lazy error factory when the Maybe is None.
        /// </summary>
        /// <example>
        /// <code>
        /// Maybe&lt;User&gt; maybe = FindUser(id);
        /// Result&lt;User&gt; result = maybe.ToResult(() => new NotFoundError("User", id));
        /// </code>
        /// </example>
        public static Result<T> ToResult<T>(this Maybe<T> maybe, Func<IError> errorFactory)
        {
            if (errorFactory == null) throw new ArgumentNullException(nameof(errorFactory));
            return maybe.HasValue
                ? Result<T>.Ok(maybe.Value)
                : Result<T>.Fail(errorFactory());
        }

        /// <summary>
        /// Converts a Maybe to a Result using a static error when the Maybe is None.
        /// </summary>
        /// <example>
        /// <code>
        /// Result&lt;User&gt; result = maybeUser.ToResult(new NotFoundError("User", id));
        /// </code>
        /// </example>
        public static Result<T> ToResult<T>(this Maybe<T> maybe, IError error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));
            return maybe.HasValue
                ? Result<T>.Ok(maybe.Value)
                : Result<T>.Fail(error);
        }

        /// <summary>
        /// Converts a Maybe to a Result using an error message when the Maybe is None.
        /// </summary>
        /// <example>
        /// <code>
        /// Result&lt;User&gt; result = maybeUser.ToResult("User not found");
        /// </code>
        /// </example>
        public static Result<T> ToResult<T>(this Maybe<T> maybe, string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage)) throw new ArgumentException("Error message cannot be null or empty.", nameof(errorMessage));
            return maybe.HasValue
                ? Result<T>.Ok(maybe.Value)
                : Result<T>.Fail(errorMessage);
        }

        #endregion

        #region Result<T> → Maybe<T>

        /// <summary>
        /// Converts a Result to a Maybe. Error information is discarded on failure —
        /// use when failure is representable as absence rather than error detail.
        /// </summary>
        /// <example>
        /// <code>
        /// Result&lt;User&gt; result = GetUser(id);
        /// Maybe&lt;User&gt; maybe = result.ToMaybe(); // None if failed, Some(user) if succeeded
        /// </code>
        /// </example>
        public static Maybe<T> ToMaybe<T>(this Result<T> result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.IsSuccess
                ? Maybe<T>.Some(result.Value!)
                : Maybe<T>.None;
        }

        #endregion
    }
}
