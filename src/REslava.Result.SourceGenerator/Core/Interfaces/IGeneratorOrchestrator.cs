using Microsoft.CodeAnalysis;

namespace REslava.Result.SourceGenerators.Core.Interfaces
{
    /// <summary>
    /// Generic interface for orchestrating generator pipelines.
    /// Implements Single Responsibility Principle - only coordinates generators.
    /// Shared across all generators (ResultToIResult, OneOf2ToIResult, etc.).
    /// </summary>
    public interface IGeneratorOrchestrator
    {
        /// <summary>
        /// Coordinates the generation pipeline.
        /// </summary>
        /// <param name="context">The generator initialization context.</param>
        void Initialize(IncrementalGeneratorInitializationContext context);
    }
}
