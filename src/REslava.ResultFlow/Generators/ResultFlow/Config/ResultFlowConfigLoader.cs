using REslava.ResultFlow.Generators.ResultFlow.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace REslava.ResultFlow.Generators.ResultFlow.Config
{
    /// <summary>
    /// Parses <c>resultflow.json</c> <see cref="Microsoft.CodeAnalysis.AdditionalText"/> into a
    /// method-name → <see cref="NodeKind"/> dictionary.
    /// <para>
    /// Config entries <b>override</b> the built-in convention dictionary — allowing full
    /// substitution of any built-in classification (e.g. reclassify <c>Bind</c> for a custom library).
    /// </para>
    /// </summary>
    internal static class ResultFlowConfigLoader
    {
        /// <summary>
        /// Recognised JSON keys and their corresponding <see cref="NodeKind"/> values.
        /// </summary>
        private static readonly Dictionary<string, NodeKind> KeyToKind = new Dictionary<string, NodeKind>
        {
            { "gatekeeper",   NodeKind.Gatekeeper },
            { "bind",         NodeKind.TransformWithRisk },
            { "map",          NodeKind.PureTransform },
            { "tap",          NodeKind.SideEffectSuccess },
            { "tapOnFailure", NodeKind.SideEffectFailure },
            { "terminal",     NodeKind.Terminal },
        };

        /// <summary>
        /// Parses <paramref name="json"/> and returns a method-name → <see cref="NodeKind"/> lookup,
        /// or <c>null</c> if the JSON is malformed (sets <paramref name="error"/>).
        /// </summary>
        /// <remarks>
        /// Namespace fields in the config are stored for future semantic-model resolution but are
        /// not yet used for filtering — all entries are registered globally (pure-syntax mode).
        /// </remarks>
        public static Dictionary<string, NodeKind>? TryLoad(string json, out string? error)
        {
            error = null;
            try
            {
                var result = new Dictionary<string, NodeKind>(StringComparer.Ordinal);

                // Locate the "mappings" array
                var arrayMatch = Regex.Match(json, @"""mappings""\s*:\s*\[");
                if (!arrayMatch.Success)
                {
                    error = "Missing required \"mappings\" key";
                    return null;
                }

                // Scan each top-level {...} object inside the array
                int arrayStart = arrayMatch.Index + arrayMatch.Length - 1; // index of '['
                var objects = ExtractObjects(json, arrayStart);

                foreach (var obj in objects)
                {
                    foreach (var kv in KeyToKind)
                    {
                        foreach (var name in ParseStringArray(obj, kv.Key))
                        {
                            if (!string.IsNullOrWhiteSpace(name))
                                result[name] = kv.Value; // later definition wins across multiple mapping blocks
                        }
                    }
                }

                return result;
            }
            catch (System.Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        // ── helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// Scans <paramref name="json"/> from <paramref name="arrayOpenBracketPos"/> and returns
        /// each top-level <c>{…}</c> object found before the matching <c>]</c>.
        /// </summary>
        private static List<string> ExtractObjects(string json, int arrayOpenBracketPos)
        {
            var objects = new List<string>();
            int depth = 0;
            int start = -1;

            for (int i = arrayOpenBracketPos; i < json.Length; i++)
            {
                char c = json[i];
                if (c == ']' && depth == 0) break; // end of mappings array
                if (c == '{') { if (depth++ == 0) start = i; }
                else if (c == '}')
                {
                    if (--depth == 0 && start >= 0)
                    {
                        objects.Add(json.Substring(start, i - start + 1));
                        start = -1;
                    }
                }
            }

            return objects;
        }

        /// <summary>
        /// Extracts all string values from <c>"key": ["v1", "v2"]</c> within <paramref name="json"/>.
        /// Returns an empty list when the key is absent.
        /// </summary>
        private static IReadOnlyList<string> ParseStringArray(string json, string key)
        {
            var match = Regex.Match(json, $@"""{Regex.Escape(key)}""\s*:\s*\[([^\]]*)\]");
            if (!match.Success) return System.Array.Empty<string>();

            var values = Regex.Matches(match.Groups[1].Value, @"""([^""\\]*)""");
            return values.Cast<Match>().Select(m => m.Groups[1].Value).ToList();
        }
    }
}
