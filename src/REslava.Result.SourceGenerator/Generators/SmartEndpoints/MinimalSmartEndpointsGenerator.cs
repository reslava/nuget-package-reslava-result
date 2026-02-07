using Microsoft.CodeAnalysis;

namespace REslava.Result.SourceGenerators.Generators.OneOf2ToIResult
{
    /// <summary>
    /// Minimal SmartEndpoints generator - simple test without orchestrator
    /// Testing if generator discovery works in this namespace
    /// </summary>
    [Generator]
    public class MinimalSmartEndpointsGenerator : IIncrementalGenerator
    {
        public MinimalSmartEndpointsGenerator()
        {
            System.Diagnostics.Debug.WriteLine("🚀 MinimalSmartEndpointsGenerator CONSTRUCTOR called!");
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            System.Diagnostics.Debug.WriteLine("🚀 MinimalSmartEndpointsGenerator.Initialize called!");
            
            // Only generate our test file - no orchestrator to avoid conflicts
            context.RegisterSourceOutput(context.CompilationProvider, (spc, compilation) =>
            {
                spc.AddSource("MinimalSmartEndpointsTest.g.cs", 
                    $"// MinimalSmartEndpointsGenerator worked! Generated at {DateTime.UtcNow:O}");
            });
        }
    }
}
