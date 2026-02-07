using Microsoft.CodeAnalysis;
using REslava.Result.SourceGenerators.Generators.OneOf4ToIResult.Orchestration;
using REslava.Result.SourceGenerators.Core.Interfaces;

namespace REslava.Result.SourceGenerators.Generators.OneOf4ToIResult
{
    /// <summary>
    /// OneOf4ToIResult generator - handles OneOf&lt;T1,T2,T3,T4&gt; to IResult conversion.
    /// Single Responsibility: Only delegates to the orchestrator.
    /// Following OneOf2ToIResult and OneOf3ToIResult patterns exactly.
    /// </summary>
    [Generator]
    public class OneOf4ToIResultGenerator : IIncrementalGenerator
    {
        private readonly IGeneratorOrchestrator _orchestrator;

        public OneOf4ToIResultGenerator()
        {
            _orchestrator = new OneOf4ToIResultOrchestrator();
        }

        /// <summary>
        /// Initializes the generator pipeline using the orchestrator.
        /// </summary>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            System.Diagnostics.Debug.WriteLine("🔥🔥🔥🔥 OneOf4ToIResultGenerator.Initialize called!");
            _orchestrator.Initialize(context);
        }
    }
}
