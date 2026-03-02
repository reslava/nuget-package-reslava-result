using Microsoft.CodeAnalysis;
using REslava.ResultFlow.Core.Interfaces;
using REslava.ResultFlow.Generators.ResultFlow.Orchestration;

namespace REslava.ResultFlow.Generators.ResultFlow
{
    [Generator]
    public class ResultFlowGenerator : IIncrementalGenerator
    {
        private readonly IGeneratorOrchestrator _orchestrator = new ResultFlowOrchestrator();

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            _orchestrator.Initialize(context);
        }
    }
}
