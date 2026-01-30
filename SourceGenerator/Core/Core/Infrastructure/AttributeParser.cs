using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace REslava.Result.SourceGenerators.Core.Infrastructure
{
    /// <summary>
    /// Utility for parsing configuration from attributes.
    /// Provides type-safe extraction of attribute arguments.
    /// </summary>
    public static class AttributeParser
    {
        /// <summary>
        /// Safely gets a string value from attribute arguments.
        /// </summary>
        public static string? GetStringValue(this IEnumerable<KeyValuePair<string, TypedConstant>> arguments, string name, string? defaultValue = null)
        {
            var arg = arguments.FirstOrDefault(a => a.Key == name);
            return arg.Value.Value?.ToString() ?? defaultValue;
        }

        /// <summary>
        /// Safely gets a boolean value from attribute arguments.
        /// </summary>
        public static bool GetBoolValue(this IEnumerable<KeyValuePair<string, TypedConstant>> arguments, string name, bool defaultValue = false)
        {
            var arg = arguments.FirstOrDefault(a => a.Key == name);
            if (arg.Value.Value is bool boolValue)
                return boolValue;
            return defaultValue;
        }

        /// <summary>
        /// Safely gets an integer value from attribute arguments.
        /// </summary>
        public static int GetIntValue(this IEnumerable<KeyValuePair<string, TypedConstant>> arguments, string name, int defaultValue = 0)
        {
            var arg = arguments.FirstOrDefault(a => a.Key == name);
            if (arg.Value.Value is int intValue)
                return intValue;
            return defaultValue;
        }

        /// <summary>
        /// Safely gets a string array value from attribute arguments.
        /// </summary>
        public static string[] GetStringArrayValue(this IEnumerable<KeyValuePair<string, TypedConstant>> arguments, string name)
        {
            var arg = arguments.FirstOrDefault(a => a.Key == name);
            if (arg.Value.Kind == TypedConstantKind.Array && arg.Value.Values != null)
            {
                return arg.Value.Values
                    .Select(v => v.Value?.ToString())
                    .Where(v => v != null)
                    .ToArray()!;
            }
            return Array.Empty<string>();
        }

        /// <summary>
        /// Gets all named arguments from an attribute as a dictionary.
        /// </summary>
        public static Dictionary<string, object?> GetAllNamedArguments(AttributeData attribute)
        {
            var result = new Dictionary<string, object?>();
            foreach (var arg in attribute.NamedArguments)
            {
                result[arg.Key] = arg.Value.Value;
            }
            return result;
        }

        /// <summary>
        /// Checks if an attribute has a specific named argument.
        /// </summary>
        public static bool HasNamedArgument(AttributeData attribute, string name)
        {
            return attribute.NamedArguments.Any(a => a.Key == name);
        }
    }
}
