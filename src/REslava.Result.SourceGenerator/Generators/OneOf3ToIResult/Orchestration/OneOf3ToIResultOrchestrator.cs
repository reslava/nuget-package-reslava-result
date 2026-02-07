using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.Result.SourceGenerators.Generators.OneOf3ToIResult.Attributes;
using REslava.Result.SourceGenerators.Generators.OneOf3ToIResult.CodeGeneration;
using REslava.Result.SourceGenerators.Core.Interfaces;
using System.Text;

namespace REslava.Result.SourceGenerators.Generators.OneOf3ToIResult.Orchestration
{
    /// <summary>
    /// Orchestrates the generation pipeline for OneOf3ToIResult.
    /// Single Responsibility: Only coordinates OneOf<T1,T2,T3> to IResult generators.
    /// Implements Dependency Inversion - depends on abstractions.
    /// </summary>
    public class OneOf3ToIResultOrchestrator : IGeneratorOrchestrator
    {
        private readonly IAttributeGenerator _generateOneOf3ExtensionsAttributeGenerator;
        private readonly IAttributeGenerator _mapToProblemDetailsAttributeGenerator;
        private readonly ICodeGenerator _oneOf3ToIResultExtensionGenerator;

        public OneOf3ToIResultOrchestrator()
        {
            // Constructor injection - can be replaced with DI container
            _generateOneOf3ExtensionsAttributeGenerator = new GenerateOneOf3ExtensionsAttributeGenerator();
            _mapToProblemDetailsAttributeGenerator = new MapToProblemDetailsAttributeGenerator();
            _oneOf3ToIResultExtensionGenerator = new OneOf3ToIResultExtensionGenerator();
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            System.Diagnostics.Debug.WriteLine("🔥🔥🔥 OneOf3ToIResultOrchestrator.Initialize called!");
            
            // Step 1: Register attributes for immediate availability
            // Force generation for testing - always return compilation
            var attributePipeline = context.CompilationProvider.Select((compilation, _) =>
            {
                // Debug: Check what OneOf types are available
                var oneOf2Type = compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns.OneOf`2");
                var oneOf3Type = compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns.OneOf`3");
                var oneOfType = compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns.OneOf");
                var advancedPatternsNamespace = compilation.References.Any(r => r.Display?.Contains("REslava.Result") == true);
                
                System.Diagnostics.Debug.WriteLine($"🔍 OneOf2Type: {oneOf2Type != null}, OneOf3Type: {oneOf3Type != null}, OneOfType: {oneOfType != null}, AdvancedPatterns: {advancedPatternsNamespace != null}");
                
                // Force generation for testing - always return compilation
                return compilation;
            });

            context.RegisterSourceOutput(attributePipeline, (spc, compilation) =>
            {
                if (compilation == null) return;
                
                // Check if we have OneOf3 types before generating attributes
                var allGenericNames = compilation.SyntaxTrees
                    .SelectMany(st => st.GetRoot().DescendantNodes())
                    .OfType<GenericNameSyntax>()
                    .ToList();
                
                var hasOneOf3Types = allGenericNames
                    .Any(gns => 
                        gns.Identifier.ValueText == "OneOf" && 
                        gns.TypeArgumentList?.Arguments.Count == 3);
                
                if (!hasOneOf3Types)
                {
                    // Don't generate attributes when no OneOf3 types are found
                    return;
                }
                
                // Generate GenerateOneOf3Extensions attribute
                spc.AddSource("GenerateOneOf3ExtensionsAttribute.g.cs",
                    _generateOneOf3ExtensionsAttributeGenerator.GenerateAttribute());

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
                
                var hasOneOf3Types = allGenericNames
                    .Any(gns => 
                        gns.Identifier.ValueText == "OneOf" && 
                        gns.TypeArgumentList?.Arguments.Count == 3);
                
                if (!hasOneOf3Types)
                {
                    // Don't generate anything when no OneOf3 types are found
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine("🔍 OneOf3 types detected, generating extensions");
                
                try
                {
                    // Generate extension methods
                    var extensionCode = _oneOf3ToIResultExtensionGenerator.GenerateCode(compilation, null);
                    spc.AddSource("OneOf3ToIResultExtensions.g.cs", extensionCode);
                    System.Diagnostics.Debug.WriteLine("🔥 OneOf3ToIResultExtensions.g.cs generated successfully!");
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the build
                    System.Diagnostics.Debug.WriteLine($"🔥 OneOf3ToIResult: Error generating extensions: {ex.Message}");
                    // Generate a simple fallback
                    spc.AddSource("OneOf3ToIResultExtensions.g.cs", "// Fallback extension file");
                }
            });
        }
    }
}
