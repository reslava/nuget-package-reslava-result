using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace REslava.Result.SourceGenerators.Core.Interfaces
{
    /// <summary>
    /// Interface for generating source code extensions.
    /// Implements Single Responsibility Principle - only generates code.
    /// Shared across all generators (ResultToIResult, OneOf2ToIResult, etc.).
    /// </summary>
    public interface ICodeGenerator
    {
        /// <summary>
        /// Generates source code based on compilation analysis.
        /// </summary>
        /// <param name="compilation">The compilation to analyze.</param>
        /// <param name="config">Configuration for code generation.</param>
        /// <returns>Source text containing the generated code.</returns>
        SourceText GenerateCode(Compilation compilation, object config);
    }
}
