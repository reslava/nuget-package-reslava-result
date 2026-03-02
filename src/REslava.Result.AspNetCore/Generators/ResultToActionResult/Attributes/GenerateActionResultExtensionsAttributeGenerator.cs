using Microsoft.CodeAnalysis.Text;
using REslava.Result.SourceGenerators.Core.Interfaces;
using System.Text;

namespace REslava.Result.SourceGenerators.Generators.ResultToActionResult.Attributes
{
    /// <summary>
    /// Generates the GenerateActionResultExtensions assembly-level attribute.
    /// </summary>
    public class GenerateActionResultExtensionsAttributeGenerator : IAttributeGenerator
    {
        public SourceText GenerateAttribute()
        {
            var source = @"
using System;

namespace REslava.Result.SourceGenerators
{
    /// <summary>
    /// Enables automatic generation of extension methods for converting Result&lt;T&gt; to Microsoft.AspNetCore.Mvc.IActionResult.
    /// Apply this assembly-level attribute to MVC projects that want to use the source generator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class GenerateActionResultExtensionsAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the namespace for the generated extension methods.
        /// Default is ""Generated.ActionResultExtensions"".
        /// </summary>
        public string Namespace { get; set; } = ""Generated.ActionResultExtensions"";

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
