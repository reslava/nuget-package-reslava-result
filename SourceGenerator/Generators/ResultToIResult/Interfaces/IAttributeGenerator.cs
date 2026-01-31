using Microsoft.CodeAnalysis.Text;

namespace REslava.Result.SourceGenerators.Generators.ResultToIResult.Interfaces
{
    /// <summary>
    /// Interface for generating source code attributes.
    /// Implements Single Responsibility Principle - only generates attributes.
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
