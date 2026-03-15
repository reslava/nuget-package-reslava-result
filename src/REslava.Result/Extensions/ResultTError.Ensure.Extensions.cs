using System;
using System.Threading.Tasks;
using REslava.Result.AdvancedPatterns;

namespace REslava.Result.Extensions;

/// <summary>
/// <c>Ensure</c> and <c>EnsureAsync</c> overloads for <see cref="Result{TValue,TError}"/> typed pipelines.
/// Each overload widens the error union by one slot when the predicate fails — same growth
/// pattern as <c>Bind</c>:
/// <list type="bullet">
///   <item><description>Overload 1 — first guard on a single-error result: <c>Result&lt;T,T1&gt; + T2 → Result&lt;T, ErrorsOf&lt;T1,T2&gt;&gt;</c></description></item>
///   <item><description>Overloads 2–7 — grow an existing <c>ErrorsOf</c> union by one slot per guard.</description></item>
/// </list>
/// </summary>
public static class ResultTErrorEnsureExtensions
{
    // =========================================================================
    // Ensure (sync predicate)
    // =========================================================================

    #region Ensure 1→2

    /// <summary>
    /// Adds a guard to a single-error result, widening to a 2-slot error union when the guard fails.
    /// Use this as the first <c>Ensure</c> in a typed pipeline.
    /// </summary>
    /// <example>
    /// <code>
    /// Result&lt;Order, ValidationError&gt; result = Validate(req);
    ///
    /// // Guard: widen to ErrorsOf&lt;ValidationError, CreditLimitError&gt;
    /// result.Ensure(o => o.Amount > 0, new CreditLimitError("Credit limit exceeded"));
    /// </code>
    /// </example>
    public static Result<TValue, ErrorsOf<T1, T2>> Ensure<TValue, T1, T2>(
        this Result<TValue, T1> result,
        Func<TValue, bool> predicate,
        T2 error)
        where T1 : IError
        where T2 : IError
    {
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));

        if (result.IsFailure)
        {
            var fail = Result<TValue, ErrorsOf<T1, T2>>.Fail(result.Error); // T1 → ErrorsOf<T1,T2> implicit
            fail.Context = result.Context;
            return fail;
        }

        if (!predicate(result.Value))
        {
            var fail = Result<TValue, ErrorsOf<T1, T2>>.Fail(error);        // T2 → ErrorsOf<T1,T2> implicit
            fail.Context = result.Context;
            return fail;
        }

        var ok = Result<TValue, ErrorsOf<T1, T2>>.Ok(result.Value);
        ok.Context = result.Context;
        return ok;
    }

    #endregion

    #region Ensure 2→3

    /// <summary>
    /// Grows a 2-slot error union by one slot when the guard fails.
    /// </summary>
    public static Result<TValue, ErrorsOf<T1, T2, T3>> Ensure<TValue, T1, T2, T3>(
        this Result<TValue, ErrorsOf<T1, T2>> result,
        Func<TValue, bool> predicate,
        T3 error)
        where T1 : IError
        where T2 : IError
        where T3 : IError
    {
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));

        if (result.IsFailure)
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3>>.Fail(
                result.Error.Match(
                    e1 => ErrorsOf<T1, T2, T3>.FromT1(e1),
                    e2 => ErrorsOf<T1, T2, T3>.FromT2(e2)));
            fail.Context = result.Context;
            return fail;
        }

        if (!predicate(result.Value))
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3>>.Fail(error);   // T3 → ErrorsOf implicit
            fail.Context = result.Context;
            return fail;
        }

        var ok = Result<TValue, ErrorsOf<T1, T2, T3>>.Ok(result.Value);
        ok.Context = result.Context;
        return ok;
    }

    #endregion

    #region Ensure 3→4

    /// <summary>
    /// Grows a 3-slot error union by one slot when the guard fails.
    /// </summary>
    public static Result<TValue, ErrorsOf<T1, T2, T3, T4>> Ensure<TValue, T1, T2, T3, T4>(
        this Result<TValue, ErrorsOf<T1, T2, T3>> result,
        Func<TValue, bool> predicate,
        T4 error)
        where T1 : IError
        where T2 : IError
        where T3 : IError
        where T4 : IError
    {
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));

        if (result.IsFailure)
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3, T4>>.Fail(
                result.Error.Match(
                    e1 => ErrorsOf<T1, T2, T3, T4>.FromT1(e1),
                    e2 => ErrorsOf<T1, T2, T3, T4>.FromT2(e2),
                    e3 => ErrorsOf<T1, T2, T3, T4>.FromT3(e3)));
            fail.Context = result.Context;
            return fail;
        }

        if (!predicate(result.Value))
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3, T4>>.Fail(error);
            fail.Context = result.Context;
            return fail;
        }

        var ok = Result<TValue, ErrorsOf<T1, T2, T3, T4>>.Ok(result.Value);
        ok.Context = result.Context;
        return ok;
    }

    #endregion

    #region Ensure 4→5

    /// <summary>
    /// Grows a 4-slot error union by one slot when the guard fails.
    /// </summary>
    public static Result<TValue, ErrorsOf<T1, T2, T3, T4, T5>> Ensure<TValue, T1, T2, T3, T4, T5>(
        this Result<TValue, ErrorsOf<T1, T2, T3, T4>> result,
        Func<TValue, bool> predicate,
        T5 error)
        where T1 : IError
        where T2 : IError
        where T3 : IError
        where T4 : IError
        where T5 : IError
    {
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));

        if (result.IsFailure)
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5>>.Fail(
                result.Error.Match(
                    e1 => ErrorsOf<T1, T2, T3, T4, T5>.FromT1(e1),
                    e2 => ErrorsOf<T1, T2, T3, T4, T5>.FromT2(e2),
                    e3 => ErrorsOf<T1, T2, T3, T4, T5>.FromT3(e3),
                    e4 => ErrorsOf<T1, T2, T3, T4, T5>.FromT4(e4)));
            fail.Context = result.Context;
            return fail;
        }

        if (!predicate(result.Value))
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5>>.Fail(error);
            fail.Context = result.Context;
            return fail;
        }

        var ok = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5>>.Ok(result.Value);
        ok.Context = result.Context;
        return ok;
    }

    #endregion

    #region Ensure 5→6

    /// <summary>
    /// Grows a 5-slot error union by one slot when the guard fails.
    /// </summary>
    public static Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6>> Ensure<TValue, T1, T2, T3, T4, T5, T6>(
        this Result<TValue, ErrorsOf<T1, T2, T3, T4, T5>> result,
        Func<TValue, bool> predicate,
        T6 error)
        where T1 : IError
        where T2 : IError
        where T3 : IError
        where T4 : IError
        where T5 : IError
        where T6 : IError
    {
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));

        if (result.IsFailure)
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6>>.Fail(
                result.Error.Match(
                    e1 => ErrorsOf<T1, T2, T3, T4, T5, T6>.FromT1(e1),
                    e2 => ErrorsOf<T1, T2, T3, T4, T5, T6>.FromT2(e2),
                    e3 => ErrorsOf<T1, T2, T3, T4, T5, T6>.FromT3(e3),
                    e4 => ErrorsOf<T1, T2, T3, T4, T5, T6>.FromT4(e4),
                    e5 => ErrorsOf<T1, T2, T3, T4, T5, T6>.FromT5(e5)));
            fail.Context = result.Context;
            return fail;
        }

        if (!predicate(result.Value))
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6>>.Fail(error);
            fail.Context = result.Context;
            return fail;
        }

        var ok = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6>>.Ok(result.Value);
        ok.Context = result.Context;
        return ok;
    }

    #endregion

    #region Ensure 6→7

    /// <summary>
    /// Grows a 6-slot error union by one slot when the guard fails.
    /// </summary>
    public static Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6, T7>> Ensure<TValue, T1, T2, T3, T4, T5, T6, T7>(
        this Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6>> result,
        Func<TValue, bool> predicate,
        T7 error)
        where T1 : IError
        where T2 : IError
        where T3 : IError
        where T4 : IError
        where T5 : IError
        where T6 : IError
        where T7 : IError
    {
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));

        if (result.IsFailure)
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6, T7>>.Fail(
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

        if (!predicate(result.Value))
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6, T7>>.Fail(error);
            fail.Context = result.Context;
            return fail;
        }

        var ok = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6, T7>>.Ok(result.Value);
        ok.Context = result.Context;
        return ok;
    }

    #endregion

    #region Ensure 7→8

    /// <summary>
    /// Grows a 7-slot error union by one slot when the guard fails — maximum union size (T8).
    /// </summary>
    public static Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6, T7, T8>> Ensure<TValue, T1, T2, T3, T4, T5, T6, T7, T8>(
        this Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6, T7>> result,
        Func<TValue, bool> predicate,
        T8 error)
        where T1 : IError
        where T2 : IError
        where T3 : IError
        where T4 : IError
        where T5 : IError
        where T6 : IError
        where T7 : IError
        where T8 : IError
    {
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));

        if (result.IsFailure)
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6, T7, T8>>.Fail(
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

        if (!predicate(result.Value))
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6, T7, T8>>.Fail(error);
            fail.Context = result.Context;
            return fail;
        }

        var ok = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6, T7, T8>>.Ok(result.Value);
        ok.Context = result.Context;
        return ok;
    }

    #endregion

    // =========================================================================
    // EnsureAsync (async predicate, sync result)
    // =========================================================================

    #region EnsureAsync 1→2

    /// <summary>
    /// Async variant of <see cref="Ensure{TValue,T1,T2}(Result{TValue,T1},Func{TValue,bool},T2)"/>.
    /// The predicate is awaited; the result itself is evaluated synchronously.
    /// </summary>
    public static async Task<Result<TValue, ErrorsOf<T1, T2>>> EnsureAsync<TValue, T1, T2>(
        this Result<TValue, T1> result,
        Func<TValue, Task<bool>> predicate,
        T2 error)
        where T1 : IError
        where T2 : IError
    {
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));

        if (result.IsFailure)
        {
            var fail = Result<TValue, ErrorsOf<T1, T2>>.Fail(result.Error);
            fail.Context = result.Context;
            return fail;
        }

        if (!await predicate(result.Value).ConfigureAwait(false))
        {
            var fail = Result<TValue, ErrorsOf<T1, T2>>.Fail(error);
            fail.Context = result.Context;
            return fail;
        }

        var ok = Result<TValue, ErrorsOf<T1, T2>>.Ok(result.Value);
        ok.Context = result.Context;
        return ok;
    }

    #endregion

    #region EnsureAsync 2→3

    /// <summary>
    /// Async variant that grows a 2-slot error union by one slot.
    /// </summary>
    public static async Task<Result<TValue, ErrorsOf<T1, T2, T3>>> EnsureAsync<TValue, T1, T2, T3>(
        this Result<TValue, ErrorsOf<T1, T2>> result,
        Func<TValue, Task<bool>> predicate,
        T3 error)
        where T1 : IError
        where T2 : IError
        where T3 : IError
    {
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));

        if (result.IsFailure)
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3>>.Fail(
                result.Error.Match(
                    e1 => ErrorsOf<T1, T2, T3>.FromT1(e1),
                    e2 => ErrorsOf<T1, T2, T3>.FromT2(e2)));
            fail.Context = result.Context;
            return fail;
        }

        if (!await predicate(result.Value).ConfigureAwait(false))
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3>>.Fail(error);
            fail.Context = result.Context;
            return fail;
        }

        var ok = Result<TValue, ErrorsOf<T1, T2, T3>>.Ok(result.Value);
        ok.Context = result.Context;
        return ok;
    }

    #endregion

    #region EnsureAsync 3→4

    /// <summary>
    /// Async variant that grows a 3-slot error union by one slot.
    /// </summary>
    public static async Task<Result<TValue, ErrorsOf<T1, T2, T3, T4>>> EnsureAsync<TValue, T1, T2, T3, T4>(
        this Result<TValue, ErrorsOf<T1, T2, T3>> result,
        Func<TValue, Task<bool>> predicate,
        T4 error)
        where T1 : IError
        where T2 : IError
        where T3 : IError
        where T4 : IError
    {
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));

        if (result.IsFailure)
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3, T4>>.Fail(
                result.Error.Match(
                    e1 => ErrorsOf<T1, T2, T3, T4>.FromT1(e1),
                    e2 => ErrorsOf<T1, T2, T3, T4>.FromT2(e2),
                    e3 => ErrorsOf<T1, T2, T3, T4>.FromT3(e3)));
            fail.Context = result.Context;
            return fail;
        }

        if (!await predicate(result.Value).ConfigureAwait(false))
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3, T4>>.Fail(error);
            fail.Context = result.Context;
            return fail;
        }

        var ok = Result<TValue, ErrorsOf<T1, T2, T3, T4>>.Ok(result.Value);
        ok.Context = result.Context;
        return ok;
    }

    #endregion

    #region EnsureAsync 4→5

    /// <summary>
    /// Async variant that grows a 4-slot error union by one slot.
    /// </summary>
    public static async Task<Result<TValue, ErrorsOf<T1, T2, T3, T4, T5>>> EnsureAsync<TValue, T1, T2, T3, T4, T5>(
        this Result<TValue, ErrorsOf<T1, T2, T3, T4>> result,
        Func<TValue, Task<bool>> predicate,
        T5 error)
        where T1 : IError
        where T2 : IError
        where T3 : IError
        where T4 : IError
        where T5 : IError
    {
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));

        if (result.IsFailure)
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5>>.Fail(
                result.Error.Match(
                    e1 => ErrorsOf<T1, T2, T3, T4, T5>.FromT1(e1),
                    e2 => ErrorsOf<T1, T2, T3, T4, T5>.FromT2(e2),
                    e3 => ErrorsOf<T1, T2, T3, T4, T5>.FromT3(e3),
                    e4 => ErrorsOf<T1, T2, T3, T4, T5>.FromT4(e4)));
            fail.Context = result.Context;
            return fail;
        }

        if (!await predicate(result.Value).ConfigureAwait(false))
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5>>.Fail(error);
            fail.Context = result.Context;
            return fail;
        }

        var ok = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5>>.Ok(result.Value);
        ok.Context = result.Context;
        return ok;
    }

    #endregion

    #region EnsureAsync 5→6

    /// <summary>
    /// Async variant that grows a 5-slot error union by one slot.
    /// </summary>
    public static async Task<Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6>>> EnsureAsync<TValue, T1, T2, T3, T4, T5, T6>(
        this Result<TValue, ErrorsOf<T1, T2, T3, T4, T5>> result,
        Func<TValue, Task<bool>> predicate,
        T6 error)
        where T1 : IError
        where T2 : IError
        where T3 : IError
        where T4 : IError
        where T5 : IError
        where T6 : IError
    {
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));

        if (result.IsFailure)
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6>>.Fail(
                result.Error.Match(
                    e1 => ErrorsOf<T1, T2, T3, T4, T5, T6>.FromT1(e1),
                    e2 => ErrorsOf<T1, T2, T3, T4, T5, T6>.FromT2(e2),
                    e3 => ErrorsOf<T1, T2, T3, T4, T5, T6>.FromT3(e3),
                    e4 => ErrorsOf<T1, T2, T3, T4, T5, T6>.FromT4(e4),
                    e5 => ErrorsOf<T1, T2, T3, T4, T5, T6>.FromT5(e5)));
            fail.Context = result.Context;
            return fail;
        }

        if (!await predicate(result.Value).ConfigureAwait(false))
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6>>.Fail(error);
            fail.Context = result.Context;
            return fail;
        }

        var ok = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6>>.Ok(result.Value);
        ok.Context = result.Context;
        return ok;
    }

    #endregion

    #region EnsureAsync 6→7

    /// <summary>
    /// Async variant that grows a 6-slot error union by one slot.
    /// </summary>
    public static async Task<Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6, T7>>> EnsureAsync<TValue, T1, T2, T3, T4, T5, T6, T7>(
        this Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6>> result,
        Func<TValue, Task<bool>> predicate,
        T7 error)
        where T1 : IError
        where T2 : IError
        where T3 : IError
        where T4 : IError
        where T5 : IError
        where T6 : IError
        where T7 : IError
    {
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));

        if (result.IsFailure)
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6, T7>>.Fail(
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

        if (!await predicate(result.Value).ConfigureAwait(false))
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6, T7>>.Fail(error);
            fail.Context = result.Context;
            return fail;
        }

        var ok = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6, T7>>.Ok(result.Value);
        ok.Context = result.Context;
        return ok;
    }

    #endregion

    #region EnsureAsync 7→8

    /// <summary>
    /// Async variant that grows a 7-slot error union by one slot — maximum union size (T8).
    /// </summary>
    public static async Task<Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6, T7, T8>>> EnsureAsync<TValue, T1, T2, T3, T4, T5, T6, T7, T8>(
        this Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6, T7>> result,
        Func<TValue, Task<bool>> predicate,
        T8 error)
        where T1 : IError
        where T2 : IError
        where T3 : IError
        where T4 : IError
        where T5 : IError
        where T6 : IError
        where T7 : IError
        where T8 : IError
    {
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));

        if (result.IsFailure)
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6, T7, T8>>.Fail(
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

        if (!await predicate(result.Value).ConfigureAwait(false))
        {
            var fail = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6, T7, T8>>.Fail(error);
            fail.Context = result.Context;
            return fail;
        }

        var ok = Result<TValue, ErrorsOf<T1, T2, T3, T4, T5, T6, T7, T8>>.Ok(result.Value);
        ok.Context = result.Context;
        return ok;
    }

    #endregion
}
