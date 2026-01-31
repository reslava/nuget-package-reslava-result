using Microsoft.CodeAnalysis;
using REslava.Result.SourceGenerators.Generators.ResultToIResult.Orchestration;
using REslava.Result.SourceGenerators.Generators.ResultToIResult.Interfaces;

namespace REslava.Result.SourceGenerators.Generators.ResultToIResult
{
    /// <summary>
    /// Refactored main generator using SOLID principles.
    /// Single Responsibility: Only delegates to the orchestrator.
    /// This replaces the problematic dual-generator approach.
    /// </summary>
    [Generator]
    public class ResultToIResultRefactoredGenerator : IIncrementalGenerator
    {
        private readonly IOrchestrator _orchestrator;

        public ResultToIResultRefactoredGenerator()
        {
            _orchestrator = new ResultToIResultOrchestrator();
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
