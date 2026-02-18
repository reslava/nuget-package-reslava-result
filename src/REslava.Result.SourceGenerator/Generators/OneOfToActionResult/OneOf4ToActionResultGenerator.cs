using Microsoft.CodeAnalysis;
using REslava.Result.SourceGenerators.Generators.OneOfToActionResult.Orchestration;
using REslava.Result.SourceGenerators.Core.Interfaces;

namespace REslava.Result.SourceGenerators.Generators.OneOfToActionResult
{
    [Generator]
    public class OneOf4ToActionResultGenerator : IIncrementalGenerator
    {
        private readonly IGeneratorOrchestrator _orchestrator = new OneOfToActionResultOrchestrator(arity: 4);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            _orchestrator.Initialize(context);
        }
    }
}
