using System;
using REslava.Result.AdvancedPatterns;

namespace REslava.Result.Extensions;

/// <summary>
/// <c>Bind</c> overloads for <see cref="Result{TValue,TError}"/> typed pipelines.
/// Each overload grows the error union by one slot:
/// <list type="bullet">
///   <item><description>Overload 1 — merges two single-error Results: <c>Result&lt;TIn,T1&gt; + Result&lt;TOut,T2&gt; → Result&lt;TOut, ErrorsOf&lt;T1,T2&gt;&gt;</c></description></item>
///   <item><description>Overloads 2–7 — grow an existing <c>ErrorsOf</c> union by one slot per step.</description></item>
/// </list>
/// </summary>
public static class ResultTErrorBindExtensions
{
    #region Bind 1→2

    /// <summary>
    /// Chains two single-error Results into a 2-slot error union.
    /// Use this as the first <c>Bind</c> in a typed pipeline.
    /// </summary>
    /// <example>
    /// <code>
    /// // Steps declare single concrete errors
    /// Result&lt;Order, ValidationError&gt; Validate(Req r) => ...
    /// Result&lt;Order, InventoryError&gt;  Reserve(Order o)  => ...
    ///
    /// // First Bind grows to ErrorsOf&lt;ValidationError, InventoryError&gt;
    /// Validate(req).Bind(Reserve);
    /// </code>
    /// </example>
    public static Result<TOut, ErrorsOf<T1, T2>> Bind<TIn, TOut, T1, T2>(
        this Result<TIn, T1> result,
        Func<TIn, Result<TOut, T2>> next)
        where T1 : IError
        where T2 : IError
    {
        if (next is null) throw new ArgumentNullException(nameof(next));

        if (result.IsFailure)
        {
            var fail = Result<TOut, ErrorsOf<T1, T2>>.Fail(result.Error); // T1 → ErrorsOf<T1,T2> implicit
            fail.Context = result.Context;
            return fail;
        }

        var n = next(result.Value);
        if (n.IsFailure)
        {
            var fail = Result<TOut, ErrorsOf<T1, T2>>.Fail(n.Error);      // T2 → ErrorsOf<T1,T2> implicit
            fail.Context = result.Context;
            return fail;
        }

        var ok = Result<TOut, ErrorsOf<T1, T2>>.Ok(n.Value);
        ok.Context = result.Context;
        return ok;
    }

    #endregion

    #region Bind 2→3

    /// <summary>
    /// Grows a 2-slot error union by one slot.
    /// </summary>
    public static Result<TOut, ErrorsOf<T1, T2, T3>> Bind<TIn, TOut, T1, T2, T3>(
        this Result<TIn, ErrorsOf<T1, T2>> result,
        Func<TIn, Result<TOut, T3>> next)
        where T1 : IError
        where T2 : IError
        where T3 : IError
    {
        if (next is null) throw new ArgumentNullException(nameof(next));

        if (result.IsFailure)
        {
            var fail = Result<TOut, ErrorsOf<T1, T2, T3>>.Fail(
                result.Error.Match(
                    e1 => ErrorsOf<T1, T2, T3>.FromT1(e1),
                    e2 => ErrorsOf<T1, T2, T3>.FromT2(e2)));
            fail.Context = result.Context;
            return fail;
        }

        var n = next(result.Value);
        if (n.IsFailure)
        {
            var fail = Result<TOut, ErrorsOf<T1, T2, T3>>.Fail(n.Error);  // T3 → ErrorsOf<T1,T2,T3> implicit
            fail.Context = result.Context;
            return fail;
        }

        var ok = Result<TOut, ErrorsOf<T1, T2, T3>>.Ok(n.Value);
        ok.Context = result.Context;
        return ok;
    }

    #endregion

    #region Bind 3→4

    /// <summary>
    /// Grows a 3-slot error union by one slot.
    /// </summary>
    public static Result<TOut, ErrorsOf<T1, T2, T3, T4>> Bind<TIn, TOut, T1, T2, T3, T4>(
        this Result<TIn, ErrorsOf<T1, T2, T3>> result,
        Func<TIn, Result<TOut, T4>> next)
        where T1 : IError
        where T2 : IError
        where T3 : IError
        where T4 : IError
    {
        if (next is null) throw new ArgumentNullException(nameof(next));

        if (result.IsFailure)
        {
            var fail = Result<TOut, ErrorsOf<T1, T2, T3, T4>>.Fail(
                result.Error.Match(
                    e1 => ErrorsOf<T1, T2, T3, T4>.FromT1(e1),
                    e2 => ErrorsOf<T1, T2, T3, T4>.FromT2(e2),
                    e3 => ErrorsOf<T1, T2, T3, T4>.FromT3(e3)));
            fail.Context = result.Context;
            return fail;
        }

        var n = next(result.Value);
        if (n.IsFailure)
        {
            var fail = Result<TOut, ErrorsOf<T1, T2, T3, T4>>.Fail(n.Error);
            fail.Context = result.Context;
            return fail;
        }

        var ok = Result<TOut, ErrorsOf<T1, T2, T3, T4>>.Ok(n.Value);
        ok.Context = result.Context;
        return ok;
    }

    #endregion

    #region Bind 4→5

    /// <summary>
    /// Grows a 4-slot error union by one slot.
    /// </summary>
    public static Result<TOut, ErrorsOf<T1, T2, T3, T4, T5>> Bind<TIn, TOut, T1, T2, T3, T4, T5>(
        this Result<TIn, ErrorsOf<T1, T2, T3, T4>> result,
        Func<TIn, Result<TOut, T5>> next)
        where T1 : IError
        where T2 : IError
        where T3 : IError
        where T4 : IError
        where T5 : IError
    {
        if (next is null) throw new ArgumentNullException(nameof(next));

        if (result.IsFailure)
        {
            var fail = Result<TOut, ErrorsOf<T1, T2, T3, T4, T5>>.Fail(
                result.Error.Match(
                    e1 => ErrorsOf<T1, T2, T3, T4, T5>.FromT1(e1),
                    e2 => ErrorsOf<T1, T2, T3, T4, T5>.FromT2(e2),
                    e3 => ErrorsOf<T1, T2, T3, T4, T5>.FromT3(e3),
                    e4 => ErrorsOf<T1, T2, T3, T4, T5>.FromT4(e4)));
            fail.Context = result.Context;
            return fail;
        }

        var n = next(result.Value);
        if (n.IsFailure)
        {
            var fail = Result<TOut, ErrorsOf<T1, T2, T3, T4, T5>>.Fail(n.Error);
            fail.Context = result.Context;
            return fail;
        }

        var ok = Result<TOut, ErrorsOf<T1, T2, T3, T4, T5>>.Ok(n.Value);
        ok.Context = result.Context;
        return ok;
    }

    #endregion

    #region Bind 5→6

    /// <summary>
    /// Grows a 5-slot error union by one slot.
    /// </summary>
    public static Result<TOut, ErrorsOf<T1, T2, T3, T4, T5, T6>> Bind<TIn, TOut, T1, T2, T3, T4, T5, T6>(
        this Result<TIn, ErrorsOf<T1, T2, T3, T4, T5>> result,
        Func<TIn, Result<TOut, T6>> next)
        where T1 : IError
        where T2 : IError
        where T3 : IError
        where T4 : IError
        where T5 : IError
        where T6 : IError
    {
        if (next is null) throw new ArgumentNullException(nameof(next));

        if (result.IsFailure)
        {
            var fail = Result<TOut, ErrorsOf<T1, T2, T3, T4, T5, T6>>.Fail(
                result.Error.Match(
                    e1 => ErrorsOf<T1, T2, T3, T4, T5, T6>.FromT1(e1),
                    e2 => ErrorsOf<T1, T2, T3, T4, T5, T6>.FromT2(e2),
                    e3 => ErrorsOf<T1, T2, T3, T4, T5, T6>.FromT3(e3),
                    e4 => ErrorsOf<T1, T2, T3, T4, T5, T6>.FromT4(e4),
                    e5 => ErrorsOf<T1, T2, T3, T4, T5, T6>.FromT5(e5)));
            fail.Context = result.Context;
            return fail;
        }

        var n = next(result.Value);
        if (n.IsFailure)
        {
            var fail = Result<TOut, ErrorsOf<T1, T2, T3, T4, T5, T6>>.Fail(n.Error);
            fail.Context = result.Context;
            return fail;
        }

        var ok = Result<TOut, ErrorsOf<T1, T2, T3, T4, T5, T6>>.Ok(n.Value);
        ok.Context = result.Context;
        return ok;
    }

    #endregion

    #region Bind 6→7

    /// <summary>
    /// Grows a 6-slot error union by one slot.
    /// </summary>
    public static Result<TOut, ErrorsOf<T1, T2, T3, T4, T5, T6, T7>> Bind<TIn, TOut, T1, T2, T3, T4, T5, T6, T7>(
        this Result<TIn, ErrorsOf<T1, T2, T3, T4, T5, T6>> result,
        Func<TIn, Result<TOut, T7>> next)
        where T1 : IError
        where T2 : IError
        where T3 : IError
        where T4 : IError
        where T5 : IError
        where T6 : IError
        where T7 : IError
    {
        if (next is null) throw new ArgumentNullException(nameof(next));

        if (result.IsFailure)
        {
            var fail = Result<TOut, ErrorsOf<T1, T2, T3, T4, T5, T6, T7>>.Fail(
                result.Error.Match(
                    e1 => ErrorsOf<T1, T2, T3, T4, T5, T6, T7>.FromT1(e1),
                    e2 => ErrorsOf<T1, T2, T3, T4, T5, T6, T7>.FromT2(e2),
                    e3 => ErrorsOf<T1, T2, T3, T4, T5, T6, T7>.FromT3(e3),
                    e4 => ErrorsOf<T1, T2, T3, T4, T5, T6, T7>.FromT4(e4),
                    e5 => ErrorsOf<T1, T2, T3, T4, T5, T6, T7>.FromT5(e5),
                    e6 => ErrorsOf<T1, T2, T3, T4, T5, T6, T7>.FromT6(e6)));
            fail.Context = result.Context;
            return fail;
        }

        var n = next(result.Value);
        if (n.IsFailure)
        {
            var fail = Result<TOut, ErrorsOf<T1, T2, T3, T4, T5, T6, T7>>.Fail(n.Error);
            fail.Context = result.Context;
            return fail;
        }

        var ok = Result<TOut, ErrorsOf<T1, T2, T3, T4, T5, T6, T7>>.Ok(n.Value);
        ok.Context = result.Context;
        return ok;
    }

    #endregion

    #region Bind 7→8

    /// <summary>
    /// Grows a 7-slot error union by one slot — maximum union size (T8).
    /// </summary>
    public static Result<TOut, ErrorsOf<T1, T2, T3, T4, T5, T6, T7, T8>> Bind<TIn, TOut, T1, T2, T3, T4, T5, T6, T7, T8>(
        this Result<TIn, ErrorsOf<T1, T2, T3, T4, T5, T6, T7>> result,
        Func<TIn, Result<TOut, T8>> next)
        where T1 : IError
        where T2 : IError
        where T3 : IError
        where T4 : IError
        where T5 : IError
        where T6 : IError
        where T7 : IError
        where T8 : IError
    {
        if (next is null) throw new ArgumentNullException(nameof(next));

        if (result.IsFailure)
        {
            var fail = Result<TOut, ErrorsOf<T1, T2, T3, T4, T5, T6, T7, T8>>.Fail(
                result.Error.Match(
                    e1 => ErrorsOf<T1, T2, T3, T4, T5, T6, T7, T8>.FromT1(e1),
                    e2 => ErrorsOf<T1, T2, T3, T4, T5, T6, T7, T8>.FromT2(e2),
                    e3 => ErrorsOf<T1, T2, T3, T4, T5, T6, T7, T8>.FromT3(e3),
                    e4 => ErrorsOf<T1, T2, T3, T4, T5, T6, T7, T8>.FromT4(e4),
                    e5 => ErrorsOf<T1, T2, T3, T4, T5, T6, T7, T8>.FromT5(e5),
                    e6 => ErrorsOf<T1, T2, T3, T4, T5, T6, T7, T8>.FromT6(e6),
                    e7 => ErrorsOf<T1, T2, T3, T4, T5, T6, T7, T8>.FromT7(e7)));
            fail.Context = result.Context;
            return fail;
        }

        var n = next(result.Value);
        if (n.IsFailure)
        {
            var fail = Result<TOut, ErrorsOf<T1, T2, T3, T4, T5, T6, T7, T8>>.Fail(n.Error);
            fail.Context = result.Context;
            return fail;
        }

        var ok = Result<TOut, ErrorsOf<T1, T2, T3, T4, T5, T6, T7, T8>>.Ok(n.Value);
        ok.Context = result.Context;
        return ok;
    }

    #endregion
}
