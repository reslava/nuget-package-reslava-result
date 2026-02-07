using System;
using REslava.Result;

namespace REslava.Result.AdvancedPatterns
{
    /// <summary>
    /// Pipeline integration extensions for seamless OneOf and Result interoperability.
    /// Enables mixed workflows where both patterns can work together naturally.
    /// </summary>
    public static class OneOfResultIntegrationExtensions
    {
        #region OneOf → Result Transformations

        /// <summary>
        /// Transforms the success value of a OneOf using a selector function, returning a Result.
        /// Primary method with custom error mapping for maximum flexibility.
        /// </summary>
        public static Result<TResult> SelectToResult<T1, T2, TResult>(
            this OneOf<T1, T2> oneOf, 
            Func<T2, TResult> selector,
            Func<T1, IError>? errorMapper = null)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            
            return oneOf.Match(
                case1: error => Result<TResult>.Fail(errorMapper?.Invoke(error) ?? new Error(error?.ToString() ?? "Unknown error")),
                case2: success => Result<TResult>.Ok(selector(success))
            );
        }

        /// <summary>
        /// Transforms the success value of a OneOf when T1 is an IError type.
        /// No error mapping needed - uses the IError directly.
        /// </summary>
        public static Result<TResult> SelectToResult<T1, T2, TResult>(
            this OneOf<T1, T2> oneOf, 
            Func<T2, TResult> selector)
            where T1 : IError
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            
            return oneOf.Match(
                case1: error => Result<TResult>.Fail(error),
                case2: success => Result<TResult>.Ok(selector(success))
            );
        }

        /// <summary>
        /// Binds the success value of a OneOf to a Result-producing function.
        /// Enables chaining OneOf values into Result workflows.
        /// </summary>
        public static Result<TResult> BindToResult<T1, T2, TResult>(
            this OneOf<T1, T2> oneOf, 
            Func<T2, Result<TResult>> binder,
            Func<T1, IError>? errorMapper = null)
        {
            if (binder == null) throw new ArgumentNullException(nameof(binder));
            
            return oneOf.Match(
                case1: error => Result<TResult>.Fail(errorMapper?.Invoke(error) ?? new Error(error?.ToString() ?? "Unknown error")),
                case2: success => binder(success)
            );
        }

        /// <summary>
        /// Binds the success value of a OneOf when T1 is an IError type.
        /// No error mapping needed - uses the IError directly.
        /// </summary>
        public static Result<TResult> BindToResult<T1, T2, TResult>(
            this OneOf<T1, T2> oneOf, 
            Func<T2, Result<TResult>> binder)
            where T1 : IError
        {
            if (binder == null) throw new ArgumentNullException(nameof(binder));
            
            return oneOf.Match(
                case1: error => Result<TResult>.Fail(error),
                case2: success => binder(success)
            );
        }

        #endregion

        #region OneOf Filtering

        /// <summary>
        /// Filters a OneOf based on a predicate, returning the original OneOf if the predicate passes.
        /// Enables conditional processing in OneOf pipelines.
        /// </summary>
        public static OneOf<T1, T2> Filter<T1, T2>(
            this OneOf<T1, T2> oneOf, 
            Func<T2, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            
            return oneOf.Match(
                case1: error => oneOf,
                case2: success => predicate(success) ? oneOf : throw new InvalidOperationException("Predicate not satisfied")
            );
        }

        #endregion

        #region Result → OneOf Transformations

        /// <summary>
        /// Converts a Result to OneOf using custom error mapping.
        /// Alternative to the basic ToOneOf method with more flexible error handling.
        /// </summary>
        public static OneOf<T1, T2> ToOneOfCustom<T1, T2>(
            this Result<T2> result, 
            Func<IReason, T1> errorMapper)
        {
            if (errorMapper == null) throw new ArgumentNullException(nameof(errorMapper));
            
            if (result.IsSuccess)
                return OneOf<T1, T2>.FromT2(result.Value!);
            
            return OneOf<T1, T2>.FromT1(errorMapper(result.Reasons.First()));
        }

        #endregion
    }
}