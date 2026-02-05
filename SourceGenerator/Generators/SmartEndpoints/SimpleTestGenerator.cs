using Microsoft.CodeAnalysis;

namespace REslava.Result.SourceGenerators.Generators.OneOf2ToIResult
{
    /// <summary>
    /// Simple test generator to verify registration mechanism works
    /// Moved to OneOf2ToIResult namespace to test namespace hypothesis
    /// </summary>
    [Generator]
    public class SimpleTestGenerator : IIncrementalGenerator
    {
        public SimpleTestGenerator()
        {
            System.Diagnostics.Debug.WriteLine("🚀 SimpleTestGenerator CONSTRUCTOR called!");
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            System.Diagnostics.Debug.WriteLine("🚀 SimpleTestGenerator.Initialize called!");
            
            // Force generate a test file
            context.RegisterSourceOutput(context.CompilationProvider, (spc, compilation) =>
            {
                spc.AddSource("SimpleTest.g.cs", 
                    $"// SimpleTestGenerator worked! Generated at {DateTime.UtcNow:O}");
            });
        }
    }
}
