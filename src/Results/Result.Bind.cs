using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace REslava.Result;

/// <summary>
/// Chains another operation that returns a Result, allowing for sequential operations.
/// Also known as FlatMap or SelectMany.
/// </summary>
/// <typeparam name="TOut">The type of the output value.</typeparam>
/// <param name="binder">The function that returns a new Result.</param>
/// <returns>The result of the binder function or a failed result.</returns>
public partial class Result<TValue> : Result, IResult<TValue>
{    
    public Result<TOut> Bind<TOut>(Func<TValue, Result<TOut>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        if (IsFailed)
        {
            var result = new Result<TOut>();
            result.Reasons.AddRange(Reasons);
            return result;
        }

        try
        {
            var bindResult = binder(Value!);

            // Return original and new success reasons
            if (bindResult.IsSuccess && Successes.Any())
            {
                var newResult = new Result<TOut> { ValueOrDefault = bindResult.ValueOrDefault };
                newResult.Reasons.AddRange(Successes);
                newResult.Reasons.AddRange(bindResult.Reasons);

                return newResult;
            }

            // return errors from new result
            return bindResult;
        }
        catch (Exception ex)
        {
            // TODO ExceptionError
            return Result<TOut>.Fail($"Exception {ex.Message}");
        }
    }
}
