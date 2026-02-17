using Microsoft.CodeAnalysis;
using REslava.Result.SourceGenerators.Generators.ResultToActionResult.Orchestration;
using REslava.Result.SourceGenerators.Core.Interfaces;

namespace REslava.Result.SourceGenerators.Generators.ResultToActionResult
{
    /// <summary>
    /// Source generator for converting Result&lt;T&gt; to ASP.NET MVC IActionResult.
    /// Delegates all work to the orchestrator following SOLID principles.
    /// </summary>
    [Generator]
    public class ResultToActionResultGenerator : IIncrementalGenerator
    {
        private readonly IGeneratorOrchestrator _orchestrator;

        public ResultToActionResultGenerator()
        {
            _orchestrator = new ResultToActionResultOrchestrator();
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            _orchestrator.Initialize(context);
        }
    }
}
