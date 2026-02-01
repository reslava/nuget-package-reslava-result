using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace REslava.Result.SourceGenerators.Generators.OneOfToIResult
{
    /// <summary>
    /// Simple test generator to verify generator registration is working.
    /// </summary>
    [Generator]
    public class TestGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register a simple post-initialization output
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource("TestGenerator.g.cs", SourceText.From("// TestGenerator is working!", Encoding.UTF8));
            });
        }
    }
}
