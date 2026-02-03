using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using REslava.Result.SourceGenerators.Generators.ResultToIResult.Attributes;
using REslava.Result.SourceGenerators.Generators.ResultToIResult.CodeGeneration;
using REslava.Result.SourceGenerators.Core.Interfaces;
using System.Text;

namespace REslava.Result.SourceGenerators.Generators.ResultToIResult.Orchestration
{
    /// <summary>
    /// Orchestrates the generation pipeline for ResultToIResult.
    /// Single Responsibility: Only coordinates attribute and code generators.
    /// Implements Dependency Inversion - depends on abstractions.
    /// </summary>
    public class ResultToIResultOrchestrator : IGeneratorOrchestrator
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
            // Force generation for testing - always return compilation
            var attributePipeline = context.CompilationProvider.Select((compilation, _) =>
            {
                // Debug: Check what Result types are available
                var resultType = compilation.GetTypeByMetadataName("REslava.Result.Result`1");
                var nonGenericResultType = compilation.GetTypeByMetadataName("REslava.Result.Result");
                System.Diagnostics.Debug.WriteLine($"🔍 ResultType: {resultType != null}, NonGenericResultType: {nonGenericResultType != null}");
                
                // Force generation for testing - always return compilation
                return compilation;
            });

            context.RegisterSourceOutput(attributePipeline, (spc, compilation) =>
            {
                if (compilation == null) return;
                
                // Check if we have Result types before generating attributes
                var allGenericNames = compilation.SyntaxTrees
                    .SelectMany(st => st.GetRoot().DescendantNodes())
                    .OfType<GenericNameSyntax>()
                    .ToList();
                
                var hasResultTypes = allGenericNames
                    .Any(gns => 
                        gns.Identifier.ValueText == "Result" && 
                        gns.TypeArgumentList?.Arguments.Count >= 1);
                
                // Also check for non-generic Result usage
                var allIdentifierNames = compilation.SyntaxTrees
                    .SelectMany(st => st.GetRoot().DescendantNodes())
                    .OfType<IdentifierNameSyntax>()
                    .ToList();
                
                var hasNonGenericResult = allIdentifierNames
                    .Any(ins => 
                        ins.Identifier.ValueText == "Result" &&
                        ins.Parent is not GenericNameSyntax);
                
                if (!hasResultTypes && !hasNonGenericResult)
                {
                    // Don't generate attributes when no Result types are found
                    return;
                }
                
                // Generate GenerateResultExtensions attribute
                spc.AddSource("GenerateResultExtensionsAttribute.g.cs",
                    _generateResultExtensionsAttributeGenerator.GenerateAttribute());

                // Generate MapToProblemDetails attribute
                spc.AddSource("MapToProblemDetailsAttribute.g.cs",
                    _mapToProblemDetailsAttributeGenerator.GenerateAttribute());
            });

            // Step 2: Register code generation pipeline with auto-detection
            // Force generation for testing - always return compilation
            var codePipeline = context.CompilationProvider.Select((compilation, _) =>
            {
                // Debug: Check what Result types are available
                var resultType = compilation.GetTypeByMetadataName("REslava.Result.Result`1");
                var nonGenericResultType = compilation.GetTypeByMetadataName("REslava.Result.Result");
                System.Diagnostics.Debug.WriteLine($"🔍 CodePipeline - ResultType: {resultType != null}, NonGenericResultType: {nonGenericResultType != null}");
                
                // Force generation for testing - always return compilation
                return compilation;
            });

            context.RegisterSourceOutput(codePipeline, (spc, compilation) =>
            {
                if (compilation == null) return;
                
                // Check if we have Result types before generating extensions
                var allGenericNames = compilation.SyntaxTrees
                    .SelectMany(st => st.GetRoot().DescendantNodes())
                    .OfType<GenericNameSyntax>()
                    .ToList();
                
                var hasResultTypes = allGenericNames
                    .Any(gns => 
                        gns.Identifier.ValueText == "Result" && 
                        gns.TypeArgumentList?.Arguments.Count >= 1);
                
                // Also check for non-generic Result usage
                var allIdentifierNames = compilation.SyntaxTrees
                    .SelectMany(st => st.GetRoot().DescendantNodes())
                    .OfType<IdentifierNameSyntax>()
                    .ToList();
                
                var hasNonGenericResult = allIdentifierNames
                    .Any(ins => 
                        ins.Identifier.ValueText == "Result" &&
                        ins.Parent is not GenericNameSyntax);
                
                if (!hasResultTypes && !hasNonGenericResult)
                {
                    // Don't generate extensions when no Result types are found
                    return;
                }
                
                // Generate extension methods
                var extensionCode = _resultToIResultExtensionGenerator.GenerateCode(compilation, null);
                spc.AddSource("ResultToIResultExtensions.g.cs", extensionCode);
            });
        }
    }
}
