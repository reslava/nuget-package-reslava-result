using Microsoft.CodeAnalysis;
using REslava.Result.FluentValidation.Generators.FluentValidate.Orchestration;

namespace REslava.Result.FluentValidation.Generators.FluentValidate
{
    /// <summary>
    /// Roslyn source generator entry point for [FluentValidate].
    /// Thin wrapper — delegates all work to FluentValidateOrchestrator.
    /// </summary>
    [Generator]
    public class FluentValidateGenerator : IIncrementalGenerator
    {
        private readonly FluentValidateOrchestrator _orchestrator = new();

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            _orchestrator.Initialize(context);
        }
    }
}
