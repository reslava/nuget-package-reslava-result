using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// Provides consistent validation extension methods with standardized error messages.
/// All methods throw ArgumentException (or ArgumentNullException) with consistent formatting.
/// Use these to validate inputs and maintain consistent error messages across the library.
/// </summary>
public static class ValidationExtensions
{
    #region Public Constants - For Testing and Consistency

    /// <summary>
    /// Default message for null arguments.
    /// </summary>
    public const string DefaultNullMessage = "cannot be null";

    /// <summary>
    /// Default message for empty collections.
    /// </summary>
    public const string DefaultEmptyMessage = "cannot be empty";

    /// <summary>
    /// Default message for null or empty collections.
    /// </summary>
    public const string DefaultNullOrEmptyMessage = "cannot be null or empty";

    /// <summary>
    /// Default message for null or whitespace strings.
    /// </summary>
    public const string DefaultNullOrWhitespaceMessage = "cannot be null or whitespace";

    /// <summary>
    /// Default message for duplicate keys in dictionaries.
    /// </summary>
    public const string DefaultKeyExistsMessage = "already exists";

    /// <summary>
    /// Default message for missing keys in dictionaries.
    /// </summary>
    public const string DefaultKeyNotFoundMessage = "does not exist";

    #endregion

    #region String Extension Methods

    /// <summary>
    /// Ensures the string is not null or whitespace, returning the validated string.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="paramName">The parameter name (use nameof()).</param>
    /// <param name="message">Optional custom message. If null, uses default message.</param>
    /// <returns>The validated string value.</returns>
    /// <exception cref="ArgumentException">Thrown when value is null or whitespace.</exception>
    /// <example>
    /// <code>
    /// public void SetEmail(string email)
    /// {
    ///     email = email.EnsureNotNullOrWhiteSpace(nameof(email));
    ///     // email is guaranteed to be non-null and non-whitespace here
    /// }
    /// </code>
    /// </example>
    public static string EnsureNotNullOrWhiteSpace(
        this string? value,
        string paramName,
        string? message = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            var errorMessage = message ?? $"{paramName} {DefaultNullOrWhitespaceMessage}";
            throw new ArgumentException(errorMessage, paramName);
        }
        return value;
    }

    /// <summary>
    /// Ensures the string is not null or empty, returning the validated string.
    /// </summary>
    public static string EnsureNotNullOrEmpty(
        this string? value,
        string paramName,
        string? message = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            var errorMessage = message ?? $"{paramName} {DefaultNullOrEmptyMessage}";
            throw new ArgumentException(errorMessage, paramName);
        }
        return value;
    }

    /// <summary>
    /// Ensures the string length does not exceed the specified maximum.
    /// </summary>
    public static string EnsureMaxLength(
        this string value,
        int maxLength,
        string paramName,
        string? message = null)
    {
        if (value.Length > maxLength)
        {
            var errorMessage = message ?? $"{paramName} cannot exceed {maxLength} characters";
            throw new ArgumentException(errorMessage, paramName);
        }
        return value;
    }

    /// <summary>
    /// Ensures the string length is at least the specified minimum.
    /// </summary>
    public static string EnsureMinLength(
        this string value,
        int minLength,
        string paramName,
        string? message = null)
    {
        if (value.Length < minLength)
        {
            var errorMessage = message ?? $"{paramName} must be at least {minLength} characters";
            throw new ArgumentException(errorMessage, paramName);
        }
        return value;
    }

    /// <summary>
    /// Ensures the string matches a pattern, allowing for fluent validation chains.
    /// </summary>
    /// <example>
    /// <code>
    /// email.EnsureNotNullOrWhiteSpace(nameof(email))
    ///      .EnsureMatches(@"^[^@]+@[^@]+$", nameof(email), "Invalid email format");
    /// </code>
    /// </example>
    public static string EnsureMatches(
        this string value,
        string pattern,
        string paramName,
        string? message = null)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(value, pattern))
        {
            var errorMessage = message ?? $"{paramName} does not match required pattern";
            throw new ArgumentException(errorMessage, paramName);
        }
        return value;
    }

    #endregion

    #region Array Extension Methods

    /// <summary>
    /// Ensures the array is not null, returning the validated array.
    /// </summary>
    public static T[] EnsureNotNull<T>(
        this T[]? value,
        string paramName,
        string? message = null)
    {
        if (value is null)
        {
            var errorMessage = message ?? $"{paramName} {DefaultNullMessage}";
            throw new ArgumentNullException(paramName, errorMessage);
        }
        return value;
    }

    /// <summary>
    /// Ensures the array is not empty, returning the validated array.
    /// </summary>
    public static T[] EnsureNotEmpty<T>(
        this T[] value,
        string paramName,
        string? message = null)
    {
        if (value.Length == 0)
        {
            var errorMessage = message ?? $"{paramName} {DefaultEmptyMessage}";
            throw new ArgumentException(errorMessage, paramName);
        }
        return value;
    }

    /// <summary>
    /// Ensures the array is not null or empty, returning the validated array.
    /// </summary>
    public static T[] EnsureNotNullOrEmpty<T>(
        this T[]? value,
        string paramName,
        string? message = null)
    {
        if (value is null || value.Length == 0)
        {
            var errorMessage = message ?? $"{paramName} {DefaultNullOrEmptyMessage}";
            throw new ArgumentException(errorMessage, paramName);
        }
        return value;
    }

    #endregion

    #region Collection Extension Methods

    /// <summary>
    /// Ensures the collection is not null, returning the validated collection.
    /// </summary>
    public static TCollection EnsureNotNull<TCollection>(
        this TCollection? value,
        string paramName,
        string? message = null)
        where TCollection : class
    {
        if (value is null)
        {
            var errorMessage = message ?? $"{paramName} {DefaultNullMessage}";
            throw new ArgumentNullException(paramName, errorMessage);
        }
        return value;
    }

    /// <summary>
    /// Ensures the collection is not null or empty, returning the validated collection.
    /// </summary>
    public static ICollection<T> EnsureNotNullOrEmpty<T>(
        this ICollection<T>? value,
        string paramName,
        string? message = null)
    {
        if (value is null || value.Count == 0)
        {
            var errorMessage = message ?? $"{paramName} {DefaultNullOrEmptyMessage}";
            throw new ArgumentException(errorMessage, paramName);
        }
        return value;
    }

    /// <summary>
    /// Ensures the enumerable is not null or empty, returning the validated enumerable.
    /// Note: This enumerates the collection to check if it's empty.
    /// </summary>
    public static IEnumerable<T> EnsureNotNullOrEmpty<T>(
        this IEnumerable<T>? value,
        string paramName,
        string? message = null)
    {
        if (value is null || !value.Any())
        {
            var errorMessage = message ?? $"{paramName} {DefaultNullOrEmptyMessage}";
            throw new ArgumentException(errorMessage, paramName);
        }
        return value;
    }

    #endregion

    #region Dictionary Extension Methods - Generic

    /// <summary>
    /// Ensures the dictionary is not null, returning the validated dictionary.
    /// </summary>
    public static IDictionary<TKey, TValue> EnsureNotNull<TKey, TValue>(
        this IDictionary<TKey, TValue>? dictionary,
        string paramName,
        string? message = null)
        where TKey : notnull
    {
        if (dictionary is null)
        {
            var errorMessage = message ?? $"{paramName} {DefaultNullMessage}";
            throw new ArgumentNullException(paramName, errorMessage);
        }
        return dictionary;
    }

    /// <summary>
    /// Ensures the dictionary is not null or empty, returning the validated dictionary.
    /// </summary>
    public static IDictionary<TKey, TValue> EnsureNotNullOrEmpty<TKey, TValue>(
        this IDictionary<TKey, TValue>? dictionary,
        string paramName,
        string? message = null)
        where TKey : notnull
    {
        if (dictionary is null || dictionary.Count == 0)
        {
            var errorMessage = message ?? $"{paramName} {DefaultNullOrEmptyMessage}";
            throw new ArgumentException(errorMessage, paramName);
        }
        return dictionary;
    }

    /// <summary>
    /// Ensures the key does not already exist in the dictionary.
    /// Returns the dictionary for fluent chaining.
    /// </summary>
    /// <example>
    /// <code>
    /// tags.EnsureNotNull(nameof(tags))
    ///     .EnsureKeyNotExists("UserId", nameof(tags));
    /// tags.Add("UserId", 123);
    /// </code>
    /// </example>
    public static IDictionary<TKey, TValue> EnsureKeyNotExists<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        string paramName,
        string? message = null)
        where TKey : notnull
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key), $"Key {DefaultNullMessage}");
        }

        if (dictionary.ContainsKey(key))
        {
            var errorMessage = message ?? $"Key '{key}' {DefaultKeyExistsMessage}";
            throw new ArgumentException(errorMessage, paramName);
        }

        return dictionary;
    }

    /// <summary>
    /// Ensures the key exists in the dictionary.
    /// Returns the dictionary for fluent chaining.
    /// </summary>
    public static IDictionary<TKey, TValue> EnsureKeyExists<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        string paramName,
        string? message = null)
        where TKey : notnull
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key), $"Key {DefaultNullMessage}");
        }

        if (!dictionary.ContainsKey(key))
        {
            var errorMessage = message ?? $"Key '{key}' {DefaultKeyNotFoundMessage}";
            throw new ArgumentException(errorMessage, paramName);
        }

        return dictionary;
    }

    #endregion

    #region ImmutableDictionary Extension Methods

    /// <summary>
    /// Ensures the key does not already exist in the immutable dictionary.
    /// Returns the dictionary for fluent chaining.
    /// </summary>
    public static ImmutableDictionary<TKey, TValue> EnsureKeyNotExists<TKey, TValue>(
        this ImmutableDictionary<TKey, TValue> dictionary,
        TKey key,
        string paramName,
        string? message = null)
        where TKey : notnull
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key), $"Key {DefaultNullMessage}");
        }

        if (dictionary.ContainsKey(key))
        {
            var errorMessage = message ?? $"Key '{key}' {DefaultKeyExistsMessage}";
            throw new ArgumentException(errorMessage, paramName);
        }

        return dictionary;
    }

    /// <summary>
    /// Ensures the key exists in the immutable dictionary.
    /// Returns the dictionary for fluent chaining.
    /// </summary>
    public static ImmutableDictionary<TKey, TValue> EnsureKeyExists<TKey, TValue>(
        this ImmutableDictionary<TKey, TValue> dictionary,
        TKey key,
        string paramName,
        string? message = null)
        where TKey : notnull
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key), $"Key {DefaultNullMessage}");
        }

        if (!dictionary.ContainsKey(key))
        {
            var errorMessage = message ?? $"Key '{key}' {DefaultKeyNotFoundMessage}";
            throw new ArgumentException(errorMessage, paramName);
        }

        return dictionary;
    }

    #endregion

    #region Specialized String Key Validations

    /// <summary>
    /// Validates a string key for dictionary use: ensures it's not null/whitespace and doesn't exist.
    /// Returns the validated key for use.
    /// </summary>
    /// <example>
    /// <code>
    /// var validKey = key.EnsureValidDictionaryKey(Tags, nameof(key));
    /// Tags = Tags.Add(validKey, value);
    /// </code>
    /// </example>
    public static string EnsureValidDictionaryKey<TValue>(
        this string? key,
        ImmutableDictionary<string, TValue> dictionary,
        string paramName,
        string? message = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            var errorMessage = message ?? $"Key {DefaultNullOrWhitespaceMessage}";
            throw new ArgumentException(errorMessage, paramName);
        }

        if (dictionary.ContainsKey(key))
        {
            var errorMessage = message ?? $"Key '{key}' {DefaultKeyExistsMessage}";
            throw new ArgumentException(errorMessage, paramName);
        }

        return key;
    }

    /// <summary>
    /// Validates a string key for mutable dictionary use.
    /// </summary>
    public static string EnsureValidDictionaryKey<TValue>(
        this string? key,
        IDictionary<string, TValue> dictionary,
        string paramName,
        string? message = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            var errorMessage = message ?? $"Key {DefaultNullOrWhitespaceMessage}";
            throw new ArgumentException(errorMessage, paramName);
        }

        if (dictionary.ContainsKey(key))
        {
            var errorMessage = message ?? $"Key '{key}' {DefaultKeyExistsMessage}";
            throw new ArgumentException(errorMessage, paramName);
        }

        return key;
    }

    #endregion

    #region Numeric Extension Methods

    /// <summary>
    /// Ensures the value is greater than zero.
    /// </summary>
    public static int EnsurePositive(
        this int value,
        string paramName,
        string? message = null)
    {
        if (value <= 0)
        {
            var errorMessage = message ?? $"{paramName} must be greater than zero";
            throw new ArgumentException(errorMessage, paramName);
        }
        return value;
    }

    /// <summary>
    /// Ensures the value is greater than or equal to zero.
    /// </summary>
    public static int EnsureNonNegative(
        this int value,
        string paramName,
        string? message = null)
    {
        if (value < 0)
        {
            var errorMessage = message ?? $"{paramName} cannot be negative";
            throw new ArgumentException(errorMessage, paramName);
        }
        return value;
    }

    /// <summary>
    /// Ensures the value is within the specified range (inclusive).
    /// </summary>
    public static int EnsureInRange(
        this int value,
        int min,
        int max,
        string paramName,
        string? message = null)
    {
        if (value < min || value > max)
        {
            var errorMessage = message ?? $"{paramName} must be between {min} and {max}";
            throw new ArgumentException(errorMessage, paramName);
        }
        return value;
    }

    #endregion
}
