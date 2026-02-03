using Microsoft.CodeAnalysis.Text;
using REslava.Result.SourceGenerators.Core.Interfaces;
using System.Text;

namespace REslava.Result.SourceGenerators.Generators.ResultToIResult.Attributes
{
    /// <summary>
    /// Generates the MapToProblemDetails attribute.
    /// Single Responsibility: Only generates this specific attribute.
    /// </summary>
    public class MapToProblemDetailsAttributeGenerator : IAttributeGenerator
    {
        public SourceText GenerateAttribute()
        {
            var source = @"
using System;

namespace REslava.Result.SourceGenerators
{
    /// <summary>
    /// Maps error types to specific HTTP status codes for ProblemDetails generation.
    /// Apply to error classes to override the default HTTP status code mapping.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MapToProblemDetailsAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the HTTP status code to return for this error type.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the title for the ProblemDetails.
        /// If not provided, will use the error message.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the type URI for the ProblemDetails.
        /// If not provided, will use a default type.
        /// </summary>
        public string? Type { get; set; }
    }
}";

            return SourceText.From(source, Encoding.UTF8);
        }
    }
}
