using Microsoft.CodeAnalysis;
using REslava.Result.SourceGenerators.Generators.OneOfToIResult.Orchestration;
using REslava.Result.SourceGenerators.Core.Interfaces;

namespace REslava.Result.SourceGenerators.Generators.OneOfToIResult
{
    [Generator]
    public class OneOf5ToIResultGenerator : IIncrementalGenerator
    {
        private readonly IGeneratorOrchestrator _orchestrator = new OneOfToIResultOrchestrator(arity: 5);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            _orchestrator.Initialize(context);
        }
    }
}
