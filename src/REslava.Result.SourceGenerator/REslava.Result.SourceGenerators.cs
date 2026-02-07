using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using REslava.Result.SourceGenerators.Generators.ResultToIResult;
using REslava.Result.SourceGenerators.Generators.OneOf2ToIResult;
using REslava.Result.SourceGenerators.Generators.OneOf3ToIResult;
using REslava.Result.SourceGenerators.Generators.OneOf4ToIResult;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints;

namespace REslava.Result.SourceGenerators
{
    /// <summary>
    /// Main entry point for REslava.Result source generators.
    /// Registers all individual generators with the compiler.
    /// </summary>
    [Generator]
    public class REslavaResultSourceGenerators : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register all individual generators
            var resultToIResultGenerator = new ResultToIResultRefactoredGenerator();
            context.RegisterSourceOutput(resultToIResultGenerator.AsSourceGenerator());
            
            var oneOf2ToIResultGenerator = new OneOf2ToIResultGenerator();
            context.RegisterSourceOutput(oneOf2ToIResultGenerator.AsSourceGenerator());
            
            var oneOf3ToIResultGenerator = new OneOf3ToIResultGenerator();
            context.RegisterSourceOutput(oneOf3ToIResultGenerator.AsSourceGenerator());
            
            var oneOf4ToIResultGenerator = new OneOf4ToIResultGenerator();
            context.RegisterSourceOutput(oneOf4ToIResultGenerator.AsSourceGenerator());
            
            var smartEndpointsGenerator = new SmartEndpointsGenerator();
            context.RegisterSourceOutput(smartEndpointsGenerator.AsSourceGenerator());
        }
    }
}
