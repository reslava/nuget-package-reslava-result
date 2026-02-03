using Microsoft.CodeAnalysis.Text;
using REslava.Result.SourceGenerators.Core.Interfaces;
using System.Text;

namespace REslava.Result.SourceGenerators.Generators.ResultToIResult.Attributes
{
    /// <summary>
    /// Generates the GenerateResultExtensions attribute.
    /// Single Responsibility: Only generates this specific attribute.
    /// </summary>
    public class GenerateResultExtensionsAttributeGenerator : IAttributeGenerator
    {
        public SourceText GenerateAttribute()
        {
            var source = @"
using System;

namespace REslava.Result.SourceGenerators
{
    /// <summary>
    /// Enables automatic generation of extension methods for converting Result&lt;T&gt; to Microsoft.AspNetCore.Http.IResult.
    /// Apply this assembly-level attribute to projects that want to use the source generator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class GenerateResultExtensionsAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the namespace for the generated extension methods.
        /// Default is ""Generated.ResultExtensions"".
        /// </summary>
        public string Namespace { get; set; } = ""Generated.ResultExtensions"";

        /// <summary>
        /// Gets or sets a value indicating whether to include error tags in ProblemDetails.
        /// Default is true.
        /// </summary>
        public bool IncludeErrorTags { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to log errors during conversion.
        /// Default is false.
        /// </summary>
        public bool LogErrors { get; set; } = false;

        /// <summary>
        /// Gets or sets custom error type to HTTP status code mappings.
        /// Format: ""ErrorType:StatusCode"" (e.g., ""PaymentRequiredError:402"")
        /// </summary>
        public string[] CustomErrorMappings { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets a value indicating whether to generate HTTP method-specific extension methods.
        /// Default is true.
        /// </summary>
        public bool GenerateHttpMethodExtensions { get; set; } = true;

        /// <summary>
        /// Gets or sets the default HTTP status code for errors that don't have a specific mapping.
        /// Default is 400 (Bad Request).
        /// </summary>
        public int DefaultErrorStatusCode { get; set; } = 400;
    }
}";

            return SourceText.From(source, Encoding.UTF8);
        }
    }
}
