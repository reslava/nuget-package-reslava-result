using Microsoft.CodeAnalysis;

namespace REslava.Result.SourceGenerators.Generators.ResultToIResult.Interfaces
{
    /// <summary>
    /// Interface for orchestrating the generation pipeline.
    /// Implements Single Responsibility Principle - only coordinates generators.
    /// </summary>
    public interface IOrchestrator
    {
        /// <summary>
        /// Coordinates the generation of attributes and code.
        /// </summary>
        /// <param name="context">The generator initialization context.</param>
        void Initialize(IncrementalGeneratorInitializationContext context);
    }
}
