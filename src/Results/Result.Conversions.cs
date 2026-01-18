using System.Collections.Immutable;

namespace REslava.Result;

public partial class Result
{
    /// <summary>
    /// Converts this Result to a Result with a value.
    /// </summary>
    /// <typeparam name="TValue">The type of value for the new Result.</typeparam>
    /// <param name="value">The value to include in the new Result.</param>
    /// <returns>A Result&lt;TValue&gt; with the same reasons as this Result.</returns>
    public Result<TValue> ToResult<TValue>(TValue value)
    {
        if (IsFailed)
        {
            // Use internal constructor to preserve all reasons
            return new Result<TValue>(default, Reasons);
        }

        var result = Result<TValue>.Ok(value);
        result.Reasons.AddRange(Successes);
        return result;
    }
}

public partial class Result<TValue>
{
    #region Implicit Conversions - Simple Types (Never Throw)

    /// <summary>
    /// Implicitly converts a value to a successful Result.
    /// </summary>
    /// <param name="value">The value to wrap in a Result.</param>
    /// <returns>A successful Result containing the value.</returns>
    /// <example>
    /// <code>
    /// Result&lt;int&gt; result = 42; // Implicit conversion
    /// </code>
    /// </example>
    public static implicit operator Result<TValue>(TValue value) 
        => Ok(value);

    /// <summary>
    /// Implicitly converts an Error to a failed Result.
    /// Returns a ConversionError if the error is null.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>A failed Result containing the error.</returns>
    /// <example>
    /// <code>
    /// Result&lt;User&gt; result = new Error("Not found"); // Implicit conversion
    /// </code>
    /// </example>
    public static implicit operator Result<TValue>(Error error)
    {
        // ✅ Graceful handling: null becomes ConversionError
        if (error == null)
        {
            return Fail(new ConversionError("Null error provided")
                .WithTags("ConversionType", "Error")
                .WithTags("ProvidedValue", "null"));
        }
        
        return Fail(error);
    }

    /// <summary>
    /// Implicitly converts an ExceptionError to a failed Result.
    /// Returns a ConversionError if the error is null.
    /// </summary>
    /// <param name="error">The exception error to convert.</param>
    /// <returns>A failed Result containing the exception error.</returns>
    public static implicit operator Result<TValue>(ExceptionError error)
    {
        // ✅ Graceful handling: null becomes ConversionError
        if (error == null)
        {
            return Fail(new ConversionError("Null exception error provided")
                .WithTags("ConversionType", "ExceptionError")
                .WithTags("ProvidedValue", "null"));
        }
        
        return Fail(error);
    }

    #endregion

    #region Implicit Conversions - Collections (Never Throw)

    /// <summary>
    /// Implicitly converts an array of Errors to a failed Result.
    /// Returns a ConversionError if the array is null or empty.
    /// </summary>
    /// <param name="errors">The array of errors to convert.</param>
    /// <returns>A failed Result containing the errors.</returns>
    /// <example>
    /// <code>
    /// Error[] errors = new[] { new Error("E1"), new Error("E2") };
    /// Result&lt;User&gt; result = errors; // Implicit conversion
    /// </code>
    /// </example>
    public static implicit operator Result<TValue>(Error[] errors)
    {
        // ✅ NULL case: Return ConversionError
        if (errors == null)
        {
            return Fail(new ConversionError("Null error array provided")
                .WithTags("ConversionType", "Error[]")
                .WithTags("ProvidedValue", "null"));
        }
        
        // ✅ EMPTY case: Return ConversionError
        if (errors.Length == 0)
        {
            return Fail(new ConversionError("Empty error array provided")
                .WithTags("ConversionType", "Error[]")
                .WithTags("ArrayLength", 0));
        }
        
        // Valid array: use normal Fail
        return Fail(errors);
    }

    /// <summary>
    /// Implicitly converts a List of Errors to a failed Result.
    /// Returns a ConversionError if the list is null or empty.
    /// </summary>
    /// <param name="errors">The list of errors to convert.</param>
    /// <returns>A failed Result containing the errors.</returns>
    public static implicit operator Result<TValue>(List<Error> errors)
    {
        // ✅ NULL case
        if (errors == null)
        {
            return Fail(new ConversionError("Null error list provided")
                .WithTags("ConversionType", "List<Error>")
                .WithTags("ProvidedValue", "null"));
        }
        
        // ✅ EMPTY case
        if (errors.Count == 0)
        {
            return Fail(new ConversionError("Empty error list provided")
                .WithTags("ConversionType", "List<Error>")
                .WithTags("ListCount", 0));
        }
        
        return Fail(errors);
    }

    /// <summary>
    /// Implicitly converts an array of ExceptionErrors to a failed Result.
    /// Returns a ConversionError if the array is null or empty.
    /// </summary>
    /// <param name="errors">The array of exception errors to convert.</param>
    /// <returns>A failed Result containing the exception errors.</returns>
    public static implicit operator Result<TValue>(ExceptionError[] errors)
    {
        if (errors == null)
        {
            return Fail(new ConversionError("Null exception error array provided")
                .WithTags("ConversionType", "ExceptionError[]")
                .WithTags("ProvidedValue", "null"));
        }
        
        if (errors.Length == 0)
        {
            return Fail(new ConversionError("Empty exception error array provided")
                .WithTags("ConversionType", "ExceptionError[]")
                .WithTags("ArrayLength", 0));
        }
        
        return Fail(errors);
    }

    /// <summary>
    /// Implicitly converts a List of ExceptionErrors to a failed Result.
    /// Returns a ConversionError if the list is null or empty.
    /// </summary>
    /// <param name="errors">The list of exception errors to convert.</param>
    /// <returns>A failed Result containing the exception errors.</returns>
    public static implicit operator Result<TValue>(List<ExceptionError> errors)
    {
        if (errors == null)
        {
            return Fail(new ConversionError("Null exception error list provided")
                .WithTags("ConversionType", "List<ExceptionError>")
                .WithTags("ProvidedValue", "null"));
        }
        
        if (errors.Count == 0)
        {
            return Fail(new ConversionError("Empty exception error list provided")
                .WithTags("ConversionType", "List<ExceptionError>")
                .WithTags("ListCount", 0));
        }
        
        return Fail(errors);
    }

    #endregion

    #region Result-to-Result Conversion

    /// <summary>
    /// Converts this Result&lt;TValue&gt; to a non-generic Result, discarding the value.
    /// </summary>
    /// <returns>A Result with the same reasons as this Result.</returns>
    /// <example>
    /// <code>
    /// Result&lt;int&gt; typedResult = Result&lt;int&gt;.Ok(42);
    /// Result baseResult = typedResult.ToResult(); // Value discarded
    /// </code>
    /// </example>
    public Result ToResult()
    {
        // Use internal constructor to preserve all reasons
        return new Result(Reasons);
    }

    #endregion
}
