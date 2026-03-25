using Microsoft.CodeAnalysis;
using REslava.ResultFlow.Generators.ResultFlow.Orchestration;

namespace REslava.ResultFlow.Generators.ResultFlow
{
    /// <summary>
    /// Always-on generator that emits <c>{ClassName}_PipelineRegistry.g.cs</c> for every source class
    /// where at least one method has a return type whose name contains "Result".
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
