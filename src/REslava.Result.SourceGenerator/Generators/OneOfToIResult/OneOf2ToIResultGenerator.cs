using Microsoft.CodeAnalysis;
using REslava.Result.SourceGenerators.Generators.OneOfToIResult.Orchestration;
using REslava.Result.SourceGenerators.Core.Interfaces;

namespace REslava.Result.SourceGenerators.Generators.OneOfToIResult
{
    [Generator]
    public class OneOf2ToIResultGenerator : IIncrementalGenerator
    {
        private readonly IGeneratorOrchestrator _orchestrator = new OneOfToIResultOrchestrator(arity: 2);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            _orchestrator.Initialize(context);
        }
    }
}
