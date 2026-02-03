using Microsoft.CodeAnalysis.Text;

namespace REslava.Result.SourceGenerators.Core.Interfaces
{
    /// <summary>
    /// Interface for generating source code attributes.
    /// Implements Single Responsibility Principle - only generates attributes.
    /// Shared across all generators (ResultToIResult, OneOf2ToIResult, etc.).
    /// </summary>
    public interface IAttributeGenerator
    {
        /// <summary>
        /// Generates the source code for the attribute.
        /// </summary>
        /// <returns>Source text containing the attribute code.</returns>
        SourceText GenerateAttribute();
    }
}
