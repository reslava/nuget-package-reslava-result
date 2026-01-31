using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using REslava.Result.SourceGenerators.Generators.ResultToIResult.Attributes;
using REslava.Result.SourceGenerators.Generators.ResultToIResult.CodeGeneration;
using REslava.Result.SourceGenerators.Generators.ResultToIResult.Interfaces;
using System.Text;

namespace REslava.Result.SourceGenerators.Generators.ResultToIResult.Orchestration
{
    /// <summary>
    /// Orchestrates the generation pipeline for ResultToIResult.
    /// Single Responsibility: Only coordinates attribute and code generators.
    /// Implements Dependency Inversion - depends on abstractions.
    /// </summary>
    public class ResultToIResultOrchestrator : IOrchestrator
    {
        private readonly IAttributeGenerator _generateResultExtensionsAttributeGenerator;
        private readonly IAttributeGenerator _mapToProblemDetailsAttributeGenerator;
        private readonly ICodeGenerator _resultToIResultExtensionGenerator;

        public ResultToIResultOrchestrator()
        {
            // Constructor injection - can be replaced with DI container
            _generateResultExtensionsAttributeGenerator = new GenerateResultExtensionsAttributeGenerator();
            _mapToProblemDetailsAttributeGenerator = new MapToProblemDetailsAttributeGenerator();
            _resultToIResultExtensionGenerator = new ResultToIResultExtensionGenerator();
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Step 1: Register attributes for immediate availability
            context.RegisterPostInitializationOutput(ctx =>
            {
                // Generate GenerateResultExtensions attribute
                ctx.AddSource("GenerateResultExtensionsAttribute.g.cs",
                    _generateResultExtensionsAttributeGenerator.GenerateAttribute());

                // Generate MapToProblemDetails attribute
                ctx.AddSource("MapToProblemDetailsAttribute.g.cs",
                    _mapToProblemDetailsAttributeGenerator.GenerateAttribute());
            });

            // Step 2: Register code generation pipeline
            // This will be expanded with the full compilation analysis logic
            var pipeline = context.CompilationProvider.Select((compilation, _) => compilation);

            context.RegisterSourceOutput(pipeline, (spc, compilation) =>
            {
                // Generate extension methods
                var extensionCode = _resultToIResultExtensionGenerator.GenerateCode(compilation, null);
                spc.AddSource("ResultToIResultExtensions.g.cs", extensionCode);
            });
        }
    }
}
