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
}

public partial class Result<TValue>
{
    #region Implicit Conversions - Simple Types (Never Throw)

    /// <summary>
    /// Implicitly converts a value to a successful Result.
    /// </summary>
    public static implicit operator Result<TValue>(TValue value)
        => Ok(value);

    /// <summary>
    /// Implicitly converts an Error to a failed Result.
    /// Returns a ConversionError if the error is null.
    /// </summary>
    public static implicit operator Result<TValue>(Error error)
    {
        // ✅ Graceful handling: null becomes ConversionError
        if (error == null)
        {
            var conversionError = new ConversionError("Null error provided")
                .WithConversionType("Error")
                .WithProvidedValue("null");

            return Fail(conversionError);
        }

        return Fail(error);
    }

    /// <summary>
    /// Implicitly converts an ExceptionError to a failed Result.
    /// Returns a ConversionError if the error is null.
    /// </summary>
    public static implicit operator Result<TValue>(ExceptionError error)
    {
        // ✅ Graceful handling: null becomes ConversionError
        if (error == null)
        {
            var conversionError = new ConversionError("Null exception error provided")
                .WithConversionType("ExceptionError")
                .WithProvidedValue("null");

            return Fail(conversionError);
        }

        return Fail(error);
    }

    #endregion

    #region Implicit Conversions - Collections (Never Throw)

    /// <summary>
    /// Implicitly converts an array of Errors to a failed Result.
    /// Returns a ConversionError if the array is null or empty.
    /// </summary>
    public static implicit operator Result<TValue>(Error[] errors)
    {
        // ✅ NULL case: Return ConversionError
        if (errors == null)
        {
            var conversionError = new ConversionError("Null error array provided")
                .WithConversionType("Error[]")
                .WithProvidedValue("null");

            return Fail(conversionError);
        }

        // ✅ EMPTY case: Return ConversionError
        if (errors.Length == 0)
        {
            var conversionError = new ConversionError("Empty error array provided")
                .WithConversionType("Error[]")
                .WithTags("ArrayLength", errors.Length);  // ✅ Use ArrayLength tag

            return Fail(conversionError);
        }

        // Valid array: use normal Fail
        return Fail(errors);
    }

    /// <summary>
    /// Implicitly converts a List of Errors to a failed Result.
    /// Returns a ConversionError if the list is null or empty.
    /// </summary>
    public static implicit operator Result<TValue>(List<Error> errors)
    {
        if (errors == null)
        {
            var conversionError = new ConversionError("Null error list provided")
                .WithConversionType("List<Error>")
                .WithProvidedValue("null");

            return Fail(conversionError);
        }

        if (errors.Count == 0)
        {
            var conversionError = new ConversionError("Empty error list provided")
                .WithConversionType("List<Error>")
                .WithTags("ListCount", errors.Count);  // ✅ Use ListCount tag

            return Fail(conversionError);
        }

        return Fail(errors);
    }

    /// <summary>
    /// Implicitly converts an array of ExceptionErrors to a failed Result.
    /// Returns a ConversionError if the array is null or empty.
    /// </summary>
    public static implicit operator Result<TValue>(ExceptionError[] errors)
    {
        if (errors == null)
        {
            var conversionError = new ConversionError("Null exception error array provided")
                .WithConversionType("ExceptionError[]")
                .WithProvidedValue("null");

            return Fail(conversionError);
        }

        if (errors.Length == 0)
        {
            var conversionError = new ConversionError("Empty exception error array provided")
                .WithConversionType("ExceptionError[]")
                .WithTags("ArrayLength", errors.Length);  // ✅ Use ArrayLength tag

            return Fail(conversionError);
        }

        return Fail(errors);
    }

    /// <summary>
    /// Implicitly converts a List of ExceptionErrors to a failed Result.
    /// Returns a ConversionError if the list is null or empty.
    /// </summary>
    public static implicit operator Result<TValue>(List<ExceptionError> errors)
    {
        if (errors == null)
        {
            var conversionError = new ConversionError("Null exception error list provided")
                .WithConversionType("List<ExceptionError>")
                .WithProvidedValue("null");

            return Fail(conversionError);
        }

        if (errors.Count == 0)
        {
            var conversionError = new ConversionError("Empty exception error list provided")
                .WithConversionType("List<ExceptionError>")
                .WithTags("ListCount", errors.Count);  // ✅ Use ListCount tag

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
