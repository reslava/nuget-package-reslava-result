using Microsoft.CodeAnalysis;
using REslava.Result.Flow.Generators.ResultFlow.Orchestration;

namespace REslava.Result.Flow.Generators.ResultFlow
{
    /// <summary>
    /// Always-on generator that emits <c>{ClassName}_PipelineRegistry.g.cs</c> for every source class
    /// containing at least one method returning <c>IResultBase</c>.
    /// Opt-out: set <c>&lt;ResultFlowRegistry&gt;false&lt;/ResultFlowRegistry&gt;</c> in your project file.
    /// </summary>
    [Generator]
    public class ResultFlowRegistryGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            new ResultFlowRegistryOrchestrator().Initialize(context);
        }
    }
}
