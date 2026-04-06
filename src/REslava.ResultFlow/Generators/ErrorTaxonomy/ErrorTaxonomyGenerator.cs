using Microsoft.CodeAnalysis;
using REslava.ResultFlow.Core.Interfaces;
using REslava.ResultFlow.Generators.ErrorTaxonomy.Orchestration;

namespace REslava.ResultFlow.Generators.ErrorTaxonomy
{
    /// <summary>
    /// Scans every class that contains at least one [ResultFlow] method and emits
    /// a <c>{Class}_ErrorTaxonomy.g.cs</c> file with an <c>_ErrorTaxonomy</c> markdown
    /// table listing each method's possible error types and confidence level.
    /// Syntax-only (library-agnostic) — no dependency on REslava.Result types.
    /// </summary>
    [Generator]
    public class ErrorTaxonomyGenerator : IIncrementalGenerator
    {
        private readonly IGeneratorOrchestrator _orchestrator = new ErrorTaxonomyOrchestrator();

        /// <inheritdoc/>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            _orchestrator.Initialize(context);
        }
    }
}
