using System.Collections.Immutable;

namespace REslava.Result;

public partial class Result
{
    /// <summary>
    /// Converts this Result to a Result with a value.
    /// </summary>
    public Result<TValue> ToResult<TValue>(TValue value)
    {
        if (IsFailed)
        {
            return new Result<TValue>(default, Reasons);
        }

        var result = Result<TValue>.Ok(value);

        // Preserve success reasons if any exist
        if (Successes.Count > 0)
        {
            return new Result<TValue>(value, Reasons);
        }

        return result;
    }
    /// <summary>
    /// Converts this Result to a Result with a value from a func valueFactory.
    /// </summary>    
    public Result<TValue> ToResult<TValue>(Func<TValue> valueFactory)
    {
        ValidationExtensions.EnsureNotNull(valueFactory, nameof(valueFactory));

        if (IsFailed)
        {
            return new Result<TValue>(default, Reasons);
        }

        try
        {
            var value = valueFactory();
            return Successes.Count > 0
                ? new Result<TValue>(value, Reasons)
                : Result<TValue>.Ok(value);
        }
        catch (Exception ex)
        {
            return Result<TValue>.Fail(new ExceptionError(ex));
        }
    }
}

public partial class Result<TValue>
{
    #region Implicit Conversions - Simple Types (Throws for null)

    /// <summary>
    /// Implicitly converts a value to a successful Result.
    /// </summary>
    public static implicit operator Result<TValue>(TValue value)
        => Ok(value);

    /// <summary>
    /// Implicitly converts an Error to a failed Result.
    /// Throws for null input to fail fast for programmer errors.
    /// </summary>
    public static implicit operator Result<TValue>(Error error)
    {
        ValidationExtensions.EnsureNotNull(error, nameof(error));
        return Fail(error);
    }

    /// <summary>
    /// Implicitly converts an ExceptionError to a failed Result.
    /// Throws for null input to fail fast for programmer errors.
    /// </summary>
    public static implicit operator Result<TValue>(ExceptionError error)
    {
        ValidationExtensions.EnsureNotNull(error, nameof(error));
        return Fail(error);
    }

    #endregion

    #region Implicit Conversions - Collections (Throws for null, handles empty gracefully)

    /// <summary>
    /// Implicitly converts an array of Errors to a failed Result.
    /// Throws for null input to fail fast for programmer errors.
    /// Returns a ConversionError if the array is empty.
    /// </summary>
    public static implicit operator Result<TValue>(Error[] errors)
    {
        ValidationExtensions.EnsureArrayNotNull(errors, nameof(errors));
        
        // ✅ EMPTY case: Return ConversionError
        if (errors.Length == 0)
        {
            var conversionError = new ConversionError("Empty error array provided")
                .WithConversionType("Error[]")
                .WithTag("ArrayLength", errors.Length);  // ✅ Use ArrayLength tag

            return Fail(conversionError);
        }

        // Valid array: use normal Fail
        return Fail(errors);
    }

    /// <summary>
    /// Implicitly converts a List of Errors to a failed Result.
    /// Throws for null input to fail fast for programmer errors.
    /// Returns a ConversionError if the list is empty.
    /// </summary>
    public static implicit operator Result<TValue>(List<Error> errors)
    {
        ValidationExtensions.EnsureNotNull(errors, nameof(errors));
        
        if (errors.Count == 0)
        {
            var conversionError = new ConversionError("Empty error list provided")
                .WithConversionType("List<Error>")
                .WithTag("ListCount", errors.Count);  // ✅ Use ListCount tag

            return Fail(conversionError);
        }

        return Fail(errors);
    }

    /// <summary>
    /// Implicitly converts an array of ExceptionErrors to a failed Result.
    /// Throws for null input to fail fast for programmer errors.
    /// Returns a ConversionError if the array is empty.
    /// </summary>
    public static implicit operator Result<TValue>(ExceptionError[] errors)
    {
        ValidationExtensions.EnsureArrayNotNull(errors, nameof(errors));
        
        if (errors.Length == 0)
        {
            var conversionError = new ConversionError("Empty exception error array provided")
                .WithConversionType("ExceptionError[]")
                .WithTag("ArrayLength", errors.Length);  // ✅ Use ArrayLength tag

            return Fail(conversionError);
        }

        return Fail(errors);
    }

    /// <summary>
    /// Implicitly converts a List of ExceptionErrors to a failed Result.
    /// Throws for null input to fail fast for programmer errors.
    /// Returns a ConversionError if the list is empty.
    /// </summary>
    public static implicit operator Result<TValue>(List<ExceptionError> errors)
    {
        ValidationExtensions.EnsureNotNull(errors, nameof(errors));
        
        if (errors.Count == 0)
        {
            var conversionError = new ConversionError("Empty exception error list provided")
                .WithConversionType("List<ExceptionError>")
                .WithTag("ListCount", errors.Count);  // ✅ Use ListCount tag

            return Fail(conversionError);
        }

        return Fail(errors);
    }

    #endregion

    #region Result-to-Result Conversion

    /// <summary>
    /// Converts this Result&lt;TValue&gt; to a non-generic Result, discarding the value.
    /// </summary>
    public Result ToResult()
    {
        // Use internal constructor to preserve all reasons
        return new Result(Reasons);
    }

    #endregion
}
