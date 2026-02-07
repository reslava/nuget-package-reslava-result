using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.Result.SourceGenerators.Generators.OneOf4ToIResult.Attributes;
using REslava.Result.SourceGenerators.Generators.OneOf4ToIResult.CodeGeneration;
using REslava.Result.SourceGenerators.Core.Interfaces;
using System.Linq;

namespace REslava.Result.SourceGenerators.Generators.OneOf4ToIResult.Orchestration
{
    /// <summary>
    /// Orchestrates the generation pipeline for OneOf4ToIResult.
    /// Single Responsibility: Only coordinates OneOf&lt;T1,T2,T3,T4&gt; to IResult generators.
    /// Implements Dependency Inversion - depends on abstractions.
    /// </summary>
    public class OneOf4ToIResultOrchestrator : IGeneratorOrchestrator
    {
        private readonly IAttributeGenerator _generateOneOf4ExtensionsAttributeGenerator;
        private readonly IAttributeGenerator _mapToProblemDetailsAttributeGenerator;
        private readonly ICodeGenerator _oneOf4ToIResultExtensionGenerator;

        public OneOf4ToIResultOrchestrator()
        {
            // Constructor injection - can be replaced with DI container
            _generateOneOf4ExtensionsAttributeGenerator = new GenerateOneOf4ExtensionsAttributeGenerator();
            _mapToProblemDetailsAttributeGenerator = new MapToProblemDetailsAttributeGenerator();
            _oneOf4ToIResultExtensionGenerator = new OneOf4ToIResultExtensionGenerator();
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            System.Diagnostics.Debug.WriteLine("🔥🔥🔥🔥 OneOf4ToIResultOrchestrator.Initialize called!");
            
            // Step 1: Register attributes for immediate availability
            var attributePipeline = context.CompilationProvider.Select((compilation, _) =>
            {
                // Debug: Check what OneOf types are available
                var oneOf2Type = compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns.OneOf`2");
                var oneOf3Type = compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns.OneOf`3");
                var oneOf4Type = compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns.OneOf`4");
                var advancedPatternsExists = compilation.References.Any(r => r.Display?.Contains("REslava.Result") == true);
                
                System.Diagnostics.Debug.WriteLine($"🔍 OneOf2Type: {oneOf2Type != null}, OneOf3Type: {oneOf3Type != null}, OneOf4Type: {oneOf4Type != null}, AdvancedPatterns: {advancedPatternsExists}");
                
                return compilation;
            });

            context.RegisterSourceOutput(attributePipeline, (spc, compilation) =>
            {
                if (compilation == null) return;
                
                // Check if we have OneOf4 types before generating attributes
                var allGenericNames = compilation.SyntaxTrees
                    .SelectMany(st => st.GetRoot().DescendantNodes())
                    .OfType<GenericNameSyntax>()
                    .ToList();
                
                var hasOneOf4Types = allGenericNames
                    .Any(gns => 
                        gns.Identifier.ValueText == "OneOf" && 
                        gns.TypeArgumentList?.Arguments.Count == 4);
                
                if (!hasOneOf4Types)
                {
                    System.Diagnostics.Debug.WriteLine("🔍 No OneOf4 types detected, skipping attribute generation");
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine("🔍 OneOf4 types detected, generating attributes");
                
                // Generate GenerateOneOf4Extensions attribute
                spc.AddSource("GenerateOneOf4ExtensionsAttribute.g.cs",
                    _generateOneOf4ExtensionsAttributeGenerator.GenerateAttribute());

                // Generate MapToProblemDetails attribute
                spc.AddSource("OneOf4.MapToProblemDetailsAttribute.g.cs",
                    _mapToProblemDetailsAttributeGenerator.GenerateAttribute());
            });

            // Step 2: Register code generation pipeline
            var codePipeline = context.CompilationProvider.Select((compilation, _) =>
            {
                // Debug: Check what OneOf types are available
                var oneOf4Type = compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns.OneOf`4");
                System.Diagnostics.Debug.WriteLine($"🔍 Code Pipeline - OneOf4Type: {oneOf4Type != null}");
                
                return compilation;
            });

            context.RegisterSourceOutput(codePipeline, (spc, compilation) =>
            {
                if (compilation == null) return;
                
                // Only generate if we have OneOf4 types detected
                var allGenericNames = compilation.SyntaxTrees
                    .SelectMany(st => st.GetRoot().DescendantNodes())
                    .OfType<GenericNameSyntax>()
                    .ToList();
                
                var hasOneOf4Types = allGenericNames
                    .Any(gns => 
                        gns.Identifier.ValueText == "OneOf" && 
                        gns.TypeArgumentList?.Arguments.Count == 4);
                
                if (!hasOneOf4Types)
                {
                    System.Diagnostics.Debug.WriteLine("🔍 No OneOf4 types detected, skipping code generation");
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine("🔍 OneOf4 types detected, generating extensions");
                
                try
                {
                    // Generate extension methods
                    var extensionCode = _oneOf4ToIResultExtensionGenerator.GenerateCode(compilation, null);
                    spc.AddSource("OneOf4ToIResultExtensions.g.cs", extensionCode);
                    System.Diagnostics.Debug.WriteLine("🔥 OneOf4ToIResultExtensions.g.cs generated successfully!");
                }
                catch (System.Exception ex)
                {
                    // Log error but don't fail the build
                    System.Diagnostics.Debug.WriteLine($"🔥 OneOf4ToIResult: Error generating extensions: {ex.Message}");
                    // Generate a simple fallback
                    spc.AddSource("OneOf4ToIResultExtensions.g.cs", "// Fallback extension file");
                }
            });
        }
    }
}
