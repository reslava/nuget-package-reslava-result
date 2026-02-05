using Microsoft.CodeAnalysis;
using REslava.Result.SourceGenerators.Generators.OneOf3ToIResult.Orchestration;
using REslava.Result.SourceGenerators.Core.Interfaces;

namespace REslava.Result.SourceGenerators.Generators.OneOf3ToIResult
{
    /// <summary>
    /// OneOf3ToIResult generator - handles OneOf<T1,T2,T3> to IResult conversion.
    /// Single Responsibility: Only delegates to the orchestrator.
    /// Following OneOf2ToIResult pattern exactly.
    /// </summary>
    [Generator]
    public class OneOf3ToIResultGenerator : IIncrementalGenerator
    {
        private readonly IGeneratorOrchestrator _orchestrator;

        public OneOf3ToIResultGenerator()
        {
            System.Diagnostics.Debug.WriteLine("🔥🔥🔥 OneOf3ToIResultGenerator constructor called!");
            // TEMP: Test OneOf4 generation from OneOf3 generator
            _orchestrator = new OneOf3ToIResultOrchestrator();
        }

        /// <summary>
        /// Initializes the generator pipeline using the orchestrator.
        /// </summary>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            System.Diagnostics.Debug.WriteLine("🔥🔥🔥 OneOf3ToIResultGenerator.Initialize called!");
            _orchestrator.Initialize(context);
        }
    }
}
