using Microsoft.CodeAnalysis;
using REslava.Result.SourceGenerators.Generators.OneOfToActionResult.Orchestration;
using REslava.Result.SourceGenerators.Core.Interfaces;

namespace REslava.Result.SourceGenerators.Generators.OneOfToActionResult
{
    [Generator]
    public class OneOf3ToActionResultGenerator : IIncrementalGenerator
    {
        private readonly IGeneratorOrchestrator _orchestrator = new OneOfToActionResultOrchestrator(arity: 3);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            _orchestrator.Initialize(context);
        }
    }
}
