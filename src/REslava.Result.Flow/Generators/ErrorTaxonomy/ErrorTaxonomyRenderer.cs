using System.Collections.Generic;
using System.Text;

namespace REslava.Result.Flow.Generators.ErrorTaxonomy
{
    /// <summary>
    /// Renders a list of <see cref="ErrorTaxonomyScanner.TaxonomyRow"/> entries
    /// into a markdown table string for the <c>_ErrorTaxonomy</c> constant.
    /// </summary>
    internal static class ErrorTaxonomyRenderer
    {
        public static string Render(IReadOnlyList<ErrorTaxonomyScanner.TaxonomyRow> rows)
        {
            var sb = new StringBuilder();
            sb.AppendLine("| Method | Error Type | Confidence |");
            sb.AppendLine("|---|---|---|");

            foreach (var row in rows)
                sb.AppendLine($"| {row.MethodName} | {row.ErrorType} | {row.Confidence} |");

            return sb.ToString().TrimEnd();
        }
    }
}
