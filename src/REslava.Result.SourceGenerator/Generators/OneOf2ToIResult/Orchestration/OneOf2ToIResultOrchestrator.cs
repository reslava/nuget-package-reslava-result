using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.Result.SourceGenerators.Generators.OneOf2ToIResult.Attributes;
using REslava.Result.SourceGenerators.Generators.OneOf2ToIResult.CodeGeneration;
using REslava.Result.SourceGenerators.Core.Interfaces;
using System.Text;

namespace REslava.Result.SourceGenerators.Generators.OneOf2ToIResult.Orchestration
{
    /// <summary>
    /// Orchestrates the generation pipeline for OneOf2ToIResult.
    /// Single Responsibility: Only coordinates OneOf<T1,T2> to IResult generators.
    /// Implements Dependency Inversion - depends on abstractions.
    /// </summary>
    public class OneOf2ToIResultOrchestrator : IGeneratorOrchestrator
    {
        private readonly IAttributeGenerator _generateOneOf2ExtensionsAttributeGenerator;
        private readonly IAttributeGenerator _mapToProblemDetailsAttributeGenerator;
        private readonly ICodeGenerator _oneOf2ToIResultExtensionGenerator;

        public OneOf2ToIResultOrchestrator()
        {
            // Constructor injection - can be replaced with DI container
            _generateOneOf2ExtensionsAttributeGenerator = new GenerateOneOf2ExtensionsAttributeGenerator();
            _mapToProblemDetailsAttributeGenerator = new MapToProblemDetailsAttributeGenerator();
            _oneOf2ToIResultExtensionGenerator = new OneOf2ToIResultExtensionGenerator();
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Step 1: Register attributes for immediate availability
            // Force generation for testing - always return compilation
            var attributePipeline = context.CompilationProvider.Select((compilation, _) =>
            {
                // Debug: Check what OneOf types are available
                var oneOf2Type = compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns.OneOf`2");
                var oneOfType = compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns.OneOf");
                var advancedPatternsNamespace = compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns");
                System.Diagnostics.Debug.WriteLine($"🔍 OneOf2Type: {oneOf2Type != null}, OneOfType: {oneOfType != null}, AdvancedPatterns: {advancedPatternsNamespace != null}");
                
                // Force generation for testing - always return compilation
                return compilation;
            });

            context.RegisterSourceOutput(attributePipeline, (spc, compilation) =>
            {
                if (compilation == null) return;
                
                // Check if we have OneOf2 types before generating attributes
                var allGenericNames = compilation.SyntaxTrees
                    .SelectMany(st => st.GetRoot().DescendantNodes())
                    .OfType<GenericNameSyntax>()
                    .ToList();
                
                var hasOneOf2Types = allGenericNames
                    .Any(gns => 
                        gns.Identifier.ValueText == "OneOf" && 
                        gns.TypeArgumentList?.Arguments.Count == 2);
                
                if (!hasOneOf2Types)
                {
                    // Don't generate attributes when no OneOf2 types are found
                    return;
                }
                
                // Generate GenerateOneOf2Extensions attribute
                spc.AddSource("GenerateOneOf2ExtensionsAttribute.g.cs",
                    _generateOneOf2ExtensionsAttributeGenerator.GenerateAttribute());

                // Generate MapToProblemDetails attribute
                spc.AddSource("MapToProblemDetailsAttribute.g.cs",
                    _mapToProblemDetailsAttributeGenerator.GenerateAttribute());
            });

            // Step 2: Register code generation pipeline
            // Force generation for testing - always return compilation
            var codePipeline = context.CompilationProvider.Select((compilation, _) =>
            {
                // Debug: Check what OneOf types are available
                var oneOf2Type = compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns.OneOf`2");
                var oneOfType = compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns.OneOf");
                var advancedPatternsNamespace = compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns");
                System.Diagnostics.Debug.WriteLine($"🔍 Code Pipeline - OneOf2Type: {oneOf2Type != null}, OneOfType: {oneOfType != null}, AdvancedPatterns: {advancedPatternsNamespace != null}");
                
                // Force generation for testing - always return compilation
                return compilation;
            });

            context.RegisterSourceOutput(codePipeline, (spc, compilation) =>
            {
                if (compilation == null) return;
                
                // Only generate if we have OneOf2 types detected
                var allGenericNames = compilation.SyntaxTrees
                    .SelectMany(st => st.GetRoot().DescendantNodes())
                    .OfType<GenericNameSyntax>()
                    .ToList();
                
                var hasOneOf2Types = allGenericNames
                    .Any(gns => 
                        gns.Identifier.ValueText == "OneOf" && 
                        gns.TypeArgumentList?.Arguments.Count == 2);
                
                if (!hasOneOf2Types)
                {
                    // Don't generate anything when no OneOf2 types are found
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine("🔍 OneOf2 types detected, generating extensions");
                
                try
                {
                    // Generate extension methods
                    var extensionCode = _oneOf2ToIResultExtensionGenerator.GenerateCode(compilation, null);
                    spc.AddSource("OneOf2ToIResultExtensions.g.cs", extensionCode);
                    System.Diagnostics.Debug.WriteLine("🔥 OneOf2ToIResultExtensions.g.cs generated successfully!");
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the build
                    System.Diagnostics.Debug.WriteLine($"🔥 OneOf2ToIResult: Error generating extensions: {ex.Message}");
                    // Generate a simple fallback
                    spc.AddSource("OneOf2ToIResultExtensions.g.cs", "// Fallback extension file");
                }
            });
        }
    }
}
