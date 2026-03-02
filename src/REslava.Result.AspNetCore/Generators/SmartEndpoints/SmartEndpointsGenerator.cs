using Microsoft.CodeAnalysis;
using REslava.Result.SourceGenerators.SmartEndpoints.Orchestration;
using REslava.Result.SourceGenerators.Core.Interfaces;

namespace REslava.Result.SourceGenerators.Generators.SmartEndpoints
{
    /// <summary>
    /// SmartEndpoints generator - generates ASP.NET Core Minimal API endpoints.
    /// Single Responsibility: Only delegates to the orchestrator.
    /// Following the same pattern as OneOf2ToIResultGenerator.
    /// </summary>
    [Generator]
    public class SmartEndpointsGenerator : IIncrementalGenerator
    {
        private readonly IGeneratorOrchestrator _orchestrator;

        public SmartEndpointsGenerator()
        {
            _orchestrator = new SmartEndpointsOrchestrator();
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            _orchestrator.Initialize(context);
        }
    }
}
