using Microsoft.CodeAnalysis;
using REslava.Result.Flow.Core.Interfaces;
using REslava.Result.Flow.Generators.ErrorTaxonomy.Orchestration;

namespace REslava.Result.Flow.Generators.ErrorTaxonomy
{
    /// <summary>
    /// Scans every class that contains at least one [ResultFlow] method and emits
    /// a <c>{Class}_ErrorTaxonomy.g.cs</c> file with an <c>_ErrorTaxonomy</c> markdown
    /// table listing each method's possible error types and confidence level.
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
