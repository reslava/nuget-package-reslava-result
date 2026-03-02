using Microsoft.CodeAnalysis;
using REslava.Result.SourceGenerators.Core.Interfaces;
using REslava.Result.SourceGenerators.Generators.Validate.Orchestration;

namespace REslava.Result.SourceGenerators.Generators.Validate
{
    [Generator]
    public class ValidateGenerator : IIncrementalGenerator
    {
        private readonly IGeneratorOrchestrator _orchestrator = new ValidateOrchestrator();

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            _orchestrator.Initialize(context);
        }
    }
}
