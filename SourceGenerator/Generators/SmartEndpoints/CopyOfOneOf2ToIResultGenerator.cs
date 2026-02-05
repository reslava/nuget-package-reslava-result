using Microsoft.CodeAnalysis;
using REslava.Result.SourceGenerators.Generators.OneOf2ToIResult.Orchestration;
using REslava.Result.SourceGenerators.Core.Interfaces;

namespace REslava.Result.SourceGenerators.Generators.OneOf2ToIResult
{
    /// <summary>
    /// Copy of OneOf2ToIResult generator - exact copy to test if SmartEndpoints namespace is the issue
    /// </summary>
    [Generator]
    public class CopyOfOneOf2ToIResultGenerator : IIncrementalGenerator
    {
        private readonly IGeneratorOrchestrator _orchestrator;

        public CopyOfOneOf2ToIResultGenerator()
        {
            System.Diagnostics.Debug.WriteLine("🚀 CopyOfOneOf2ToIResultGenerator CONSTRUCTOR called!");
            _orchestrator = new OneOf2ToIResultOrchestrator();
        }

        /// <summary>
        /// Initializes the generator pipeline using the orchestrator.
        /// </summary>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            System.Diagnostics.Debug.WriteLine("🚀 CopyOfOneOf2ToIResultGenerator.Initialize called!");
            
            // Add test file generation
            context.RegisterSourceOutput(context.CompilationProvider, (spc, compilation) =>
            {
                spc.AddSource("CopyOfOneOf2ToIResultTest.g.cs", 
                    $"// CopyOfOneOf2ToIResultGenerator is working! Generated at {DateTime.UtcNow:O}");
            });
            
            _orchestrator.Initialize(context);
        }
    }
}
