using System;
using System.Linq;
using System.Text;

namespace REslava.Result.SourceGenerators.Core.Utilities
{
    /// <summary>
    /// String utilities for code generation.
    /// Provides helpers for common string operations in generators.
    /// </summary>
    public static class StringUtilities
    {
        /// <summary>
        /// Converts a string to PascalCase.
        /// </summary>
        public static string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var words = input.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();

            foreach (var word in words)
            {
                if (word.Length > 0)
                {
                    sb.Append(char.ToUpperInvariant(word[0]));
                    if (word.Length > 1)
                        sb.Append(word.Substring(1).ToLowerInvariant());
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts a string to camelCase.
        /// </summary>
        public static string ToCamelCase(string input)
        {
            var pascal = ToPascalCase(input);
            if (string.IsNullOrEmpty(pascal))
                return pascal;

            return char.ToLowerInvariant(pascal[0]) + pascal.Substring(1);
        }

        /// <summary>
        /// Converts PascalCase or camelCase to snake_case.
        /// </summary>
        public static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var sb = new StringBuilder();
            sb.Append(char.ToLowerInvariant(input[0]));

            for (int i = 1; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]))
                {
                    sb.Append('_');
                    sb.Append(char.ToLowerInvariant(input[i]));
                }
                else
                {
                    sb.Append(input[i]);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Pluralizes a word (simple English rules).
        /// </summary>
        public static string Pluralize(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;

            if (word.EndsWith("s") || word.EndsWith("x") || word.EndsWith("z") ||
                word.EndsWith("ch") || word.EndsWith("sh"))
                return word + "es";

            if (word.EndsWith("y") && word.Length > 1 && !IsVowel(word[word.Length - 2]))
                return word.Substring(0, word.Length - 1) + "ies";

            return word + "s";
        }

        /// <summary>
        /// Singularizes a word (simple English rules).
        /// </summary>
        public static string Singularize(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;

            if (word.EndsWith("ies") && word.Length > 3)
                return word.Substring(0, word.Length - 3) + "y";

            if (word.EndsWith("es") && word.Length > 2)
            {
                var stem = word.Substring(0, word.Length - 2);
                if (stem.EndsWith("s") || stem.EndsWith("x") || stem.EndsWith("z") ||
                    stem.EndsWith("ch") || stem.EndsWith("sh"))
                    return stem;
                return stem + "e";
            }

            if (word.EndsWith("s") && word.Length > 1)
                return word.Substring(0, word.Length - 1);

            return word;
        }

        /// <summary>
        /// Escapes a string for use in C# code (adds quotes and escapes special characters).
        /// </summary>
        public static string EscapeForCSharp(string input)
        {
            if (input == null)
                return "null";

            return "\"" + input
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t")
                + "\"";
        }

        /// <summary>
        /// Indents each line of a multiline string.
        /// </summary>
        public static string Indent(string input, int spaces)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var indent = new string(' ', spaces);
            var lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
            return string.Join("\r\n", lines.Select(l => indent + l));
        }

        /// <summary>
        /// Truncates a string to a maximum length and adds ellipsis if truncated.
        /// </summary>
        public static string Truncate(string input, int maxLength, string ellipsis = "...")
        {
            if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
                return input;

            return input.Substring(0, maxLength - ellipsis.Length) + ellipsis;
        }

        /// <summary>
        /// Checks if a character is a vowel.
        /// </summary>
        private static bool IsVowel(char c)
        {
            c = char.ToLowerInvariant(c);
            return c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u';
        }

        /// <summary>
        /// Converts a namespace string to a valid directory path.
        /// </summary>
        public static string NamespaceToPath(string namespaceName)
        {
            return namespaceName.Replace('.', System.IO.Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Generates a valid C# identifier from a string (removes invalid characters).
        /// </summary>
        public static string ToValidIdentifier(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "_";

            var sb = new StringBuilder();

            // First character must be letter or underscore
            if (char.IsLetter(input[0]) || input[0] == '_')
                sb.Append(input[0]);
            else
                sb.Append('_');

            // Subsequent characters can be letter, digit, or underscore
            for (int i = 1; i < input.Length; i++)
            {
                if (char.IsLetterOrDigit(input[i]) || input[i] == '_')
                    sb.Append(input[i]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Joins strings with a separator, skipping null or empty values.
        /// </summary>
        public static string JoinNonEmpty(string separator, params string[] values)
        {
            return string.Join(separator, values.Where(v => !string.IsNullOrEmpty(v)));
        }
    }
}
