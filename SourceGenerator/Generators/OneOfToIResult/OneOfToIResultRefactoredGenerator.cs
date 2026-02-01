using Microsoft.CodeAnalysis;
using REslava.Result.SourceGenerators.Generators.OneOfToIResult.Orchestration;
using REslava.Result.SourceGenerators.Generators.OneOfToIResult.Orchestration.Interfaces;

namespace REslava.Result.SourceGenerators.Generators.OneOfToIResult
{
    /// <summary>
    /// Refactored main generator using SOLID principles.
    /// Single Responsibility: Only delegates to the orchestrator.
    /// Following the exact same pattern as ResultToIResultRefactoredGenerator.
    /// </summary>
    [Generator]
    public class OneOfToIResultRefactoredGenerator : IIncrementalGenerator
    {
        private readonly IOneOfToIResultOrchestrator _orchestrator;

        public OneOfToIResultRefactoredGenerator()
        {
            _orchestrator = new OneOfToIResultOrchestrator();
        }

        /// <summary>
        /// Initializes the generator pipeline using the orchestrator.
        /// </summary>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            _orchestrator.Initialize(context);
        }
    }
}
