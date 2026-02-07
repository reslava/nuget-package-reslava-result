using Microsoft.CodeAnalysis;
using REslava.Result.SourceGenerators.Generators.OneOf2ToIResult.Orchestration;
using REslava.Result.SourceGenerators.Core.Interfaces;

namespace REslava.Result.SourceGenerators.Generators.OneOf2ToIResult
{
    /// <summary>
    /// OneOf2ToIResult generator - handles OneOf<T1,T2> to IResult conversion.
    /// Single Responsibility: Only delegates to the orchestrator.
    /// Following ResultToIResult pattern exactly.
    /// </summary>
    [Generator]
    public class OneOf2ToIResultGenerator : IIncrementalGenerator
    {
        private readonly IGeneratorOrchestrator _orchestrator;

        public OneOf2ToIResultGenerator()
        {
            _orchestrator = new OneOf2ToIResultOrchestrator();
        }

        /// <summary>
        /// Initializes the generator pipeline using the orchestrator.
        /// </summary>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            System.Diagnostics.Debug.WriteLine("🔥 OneOf2ToIResultGenerator.Initialize called!");
            
            // Add test file generation to confirm this generator is working
            context.RegisterSourceOutput(context.CompilationProvider, (spc, compilation) =>
            {
                spc.AddSource("OneOf2ToIResultTest.g.cs", 
                    $"// OneOf2ToIResultGenerator is working! Generated at {DateTime.UtcNow:O}");
            });
            
            _orchestrator.Initialize(context);
        }
    }
}
