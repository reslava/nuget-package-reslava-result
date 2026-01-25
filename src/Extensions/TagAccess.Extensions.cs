using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// Extension methods for safe tag access on Reason and Error types.
/// </summary>
public static class TagAccessExtensions
{
    /// <summary>
    /// Safely gets a tag value by key, returning default if not found.
    /// </summary>
    public static T? GetTag<T>(this IReason reason, string key, T? defaultValue = default)
    {
        if (reason?.Tags?.TryGetValue(key, out var value) == true)
        {
            if (value is T typedValue)
                return typedValue;
            
            // Try to convert if types don't match exactly
            try
            {
                return (T?)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
        
        return defaultValue;
    }

    /// <summary>
    /// Safely gets a tag value by key, throwing if not found.
    /// </summary>
    public static T RequireTag<T>(this IReason reason, string key)
    {
        if (reason?.Tags?.TryGetValue(key, out var value) == true)
        {
            if (value is T typedValue)
                return typedValue;
            
            // Try to convert if types don't match exactly
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Tag '{key}' exists but cannot be converted to type {typeof(T).Name}. " +
                    $"Actual type: {value?.GetType().Name ?? "null"}, Value: {value}", ex);
            }
        }
        
        throw new KeyNotFoundException($"Required tag '{key}' not found on {reason?.GetType().Name ?? "null"}");
    }

    /// <summary>
    /// Checks if a tag exists.
    /// </summary>
    public static bool HasTag(this IReason reason, string key)
    {
        return reason?.Tags?.ContainsKey(key) == true;
    }

    /// <summary>
    /// Gets all tag keys.
    /// </summary>
    public static IEnumerable<string> GetTagKeys(this IReason reason)
    {
        return reason?.Tags?.Keys ?? Enumerable.Empty<string>();
    }

    /// <summary>
    /// Gets all tag key-value pairs.
    /// </summary>
    public static IEnumerable<KeyValuePair<string, object>> GetTags(this IReason reason)
    {
        return reason?.Tags ?? Enumerable.Empty<KeyValuePair<string, object>>();
    }

    /// <summary>
    /// Formats tags for display.
    /// </summary>
    public static string FormatTags(this IReason reason, string separator = ", ")
    {
        if (reason?.Tags == null || !reason.Tags.Any())
            return string.Empty;

        return string.Join(separator, reason.Tags.Select(t => $"{t.Key}={t.Value}"));
    }

    /// <summary>
    /// Gets a string tag value safely.
    /// </summary>
    public static string GetTagString(this IReason reason, string key, string defaultValue = "")
    {
        return reason.GetTag<string>(key, defaultValue) ?? defaultValue;
    }

    /// <summary>
    /// Gets an integer tag value safely.
    /// </summary>
    public static int GetTagInt(this IReason reason, string key, int defaultValue = 0)
    {
        return reason.GetTag<int>(key, defaultValue);
    }

    /// <summary>
    /// Gets a boolean tag value safely.
    /// </summary>
    public static bool GetTagBool(this IReason reason, string key, bool defaultValue = false)
    {
        return reason.GetTag<bool>(key, defaultValue);
    }
}
