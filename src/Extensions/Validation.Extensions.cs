

namespace REslava.Result;
public static class ValidationExtensions
{
    #region Public Constants
    /// <summary>
    /// Default message for null arguments.
    /// </summary>
    public const string DefaultNullMessage = "cannot be null.";

    /// <summary>
    /// Default message for empty collections.
    /// </summary>
    public const string DefaultEmptyMessage = "cannot be empty.";

    /// <summary>
    /// Default message for null or empty collections.
    /// </summary>
    public const string DefaultNullOrEmptyMessage = "cannot be null or empty.";

    /// <summary>
    /// Default message for null or whitespace strings.
    /// </summary>
    public const string DefaultNullOrWhitespaceMessage = "cannot be null or whitespace.";

    /// <summary>
    /// Default message for duplicate keys in dictionaries.
    /// </summary>
    public const string DefaultKeyExistsMessage = "Key '{0}' already exists.";    
    #endregion


    #region String Validations
    /// <summary>
    /// Throws an ArgumentException if the string is null or whitespace.
    /// </summary>
    public static void ThrowIfNullOrWhiteSpace(string? value, string paramName, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(message ?? $"{paramName} {DefaultNullOrWhitespaceMessage}", paramName);
    }

    /// <summary>
    /// Throws an ArgumentException if the string is null or empty.
    /// </summary>
    public static void ThrowIfNullOrEmpty(string? value, string paramName, string? message = null)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException(message ?? $"{paramName} {DefaultNullOrEmptyMessage}", paramName);
    }

    /// <summary>
    /// Throws an ArgumentException if the string length exceeds the specified maximum.
    /// </summary>
    public static void ThrowIfLongerThan(string? value, int maxLength, string paramName, string? message = null)
    {
        ThrowIfNullOrEmpty(value, paramName);
        if (value!.Length > maxLength)
            throw new ArgumentException(message ?? $"{paramName} cannot exceed {maxLength} characters.", paramName);
    }

    /// <summary>
    /// Throws an ArgumentException if the string length is less than the specified minimum.
    /// </summary>
    public static void ThrowIfShorterThan(string? value, int minLength, string paramName, string? message = null)
    {
        ThrowIfNullOrEmpty(value, paramName);
        if (value!.Length < minLength)
            throw new ArgumentException(message ?? $"{paramName} must be at least {minLength} characters.", paramName);
    }
    #endregion

    #region Array/Collection Validations
    /// <summary>
    /// Throws an ArgumentException if the array is null.
    /// </summary>
    public static void ThrowIfNull<T>(T[]? value, string paramName, string? message = null)
    {
        if (value is null)
            throw new ArgumentException(message ?? $"{paramName} {DefaultNullMessage}", paramName);
    }

    /// <summary>
    /// Throws an ArgumentException if the array is empty.
    /// </summary>
    public static void ThrowIfEmpty<T>(T[]? value, string paramName, string? message = null)
    {
        ThrowIfNull(value, paramName, message);
        if (value!.Length == 0)
            throw new ArgumentException(message ?? $"{paramName} {DefaultEmptyMessage}", paramName);
    }

    /// <summary>
    /// Throws an ArgumentException if the array is null or empty.
    /// </summary>
    public static void ThrowIfNullOrEmpty<T>(T[]? value, string paramName, string? message = null)
    {
        if (value is null || value.Length == 0)
            throw new ArgumentException(message ?? $"{paramName} {DefaultNullOrEmptyMessage}", paramName);
    }

    /// <summary>
    /// Throws an ArgumentException if the collection is null or empty.
    /// </summary>
    public static void ThrowIfNullOrEmpty<T>(ICollection<T>? value, string paramName, string? message = null)
    {
        if (value is null || value.Count == 0)
            throw new ArgumentException(message ?? $"{paramName} {DefaultNullOrEmptyMessage}", paramName);
    }

    /// <summary>
    /// Throws an ArgumentException if the enumerable is null or empty.
    /// </summary>
    public static void ThrowIfNullOrEmpty<T>(IEnumerable<T>? value, string paramName, string? message = null)
    {
        if (value is null || !value.Any())
            throw new ArgumentException(message ?? $"{paramName} {DefaultNullOrEmptyMessage}", paramName);
    }
    #endregion

    #region Dictionary
    /// <summary>
    /// Throws an ArgumentException if the dictionary is null.
    /// </summary>
    public static void ThrowIfNull(IDictionary<string, object>? value, string paramName, string? message = null)
    {
        if (value is null)
            throw new ArgumentException(message ?? $"{paramName} {DefaultNullMessage}", paramName);

    }
    /// <summary>
    /// Throws an ArgumentException if the dictionary is null or the key already exists.
    /// </summary>
    public static void ThrowIfKeyAlreadyExists(IDictionary<string, object>? value, string key, string paramName, string? message = null)
    {
        ThrowIfNull(value, paramName, message);
        ThrowIfNullOrWhiteSpace(key, paramName, message);

        if (value!.ContainsKey(key))
            throw new ArgumentException(message ?? string.Format(DefaultKeyExistsMessage, key), paramName);

    }
    /// <summary>
    /// Throws an ArgumentException if the dictionary is null or the key does not exist.
    /// </summary>
    public static void ThrowIfKeyNotExist(IDictionary<string, object>? value, string key, string paramName, string? message = null)
    {
        ThrowIfNull(value, paramName, message);
        ThrowIfNullOrWhiteSpace(key, paramName, message);

        if (!value!.ContainsKey(key))
            throw new ArgumentException(message ?? string.Format(DefaultKeyExistsMessage, key), paramName);
    }
    #endregion
}

// Usage
// ValidationExtensions.ThrowIfNullOrWhiteSpace(myString, nameof(myString), "Custom error message");
