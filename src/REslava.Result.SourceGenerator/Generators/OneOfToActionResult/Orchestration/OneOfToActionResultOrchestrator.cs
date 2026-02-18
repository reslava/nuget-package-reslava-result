using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.Result.SourceGenerators.Generators.OneOfToActionResult.Attributes;
using REslava.Result.SourceGenerators.Generators.OneOfToActionResult.CodeGeneration;
using REslava.Result.SourceGenerators.Core.Interfaces;
using System;
using System.Linq;

namespace REslava.Result.SourceGenerators.Generators.OneOfToActionResult.Orchestration
{
    /// <summary>
    /// Shared orchestrator for OneOf{N}ToActionResult generation, parameterized by arity.
    /// Handles OneOf2, OneOf3, and OneOf4 via a single pipeline.
    /// Mirrors OneOfToIResultOrchestrator but targets MVC IActionResult types.
    /// </summary>
    public class OneOfToActionResultOrchestrator : IGeneratorOrchestrator
    {
        private readonly int _arity;
        private readonly IAttributeGenerator _extensionsAttributeGenerator;
        private readonly ICodeGenerator _extensionGenerator;

        public OneOfToActionResultOrchestrator(int arity)
        {
            _arity = arity;
            _extensionsAttributeGenerator = new OneOfActionResultExtensionsAttributeGenerator(arity);
            _extensionGenerator = new OneOfToActionResultExtensionGenerator(arity);
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Stage 1: Attribute generation
            var attributePipeline = context.CompilationProvider.Select((compilation, _) => compilation);

            context.RegisterSourceOutput(attributePipeline, (spc, compilation) =>
            {
                if (compilation == null) return;

                if (!HasOneOfUsageWithArity(compilation))
                    return;

                spc.AddSource($"GenerateOneOf{_arity}ActionResultExtensionsAttribute.g.cs",
                    _extensionsAttributeGenerator.GenerateAttribute());
            });

            // Stage 2: Extension method generation
            var codePipeline = context.CompilationProvider.Select((compilation, _) => compilation);

            context.RegisterSourceOutput(codePipeline, (spc, compilation) =>
            {
                if (compilation == null) return;

                if (!HasOneOfUsageWithArity(compilation))
                    return;

                try
                {
                    var extensionCode = _extensionGenerator.GenerateCode(compilation, null);
                    spc.AddSource($"OneOf{_arity}ToActionResultExtensions.g.cs", extensionCode);
                }
                catch (Exception ex)
                {
                    spc.AddSource($"OneOf{_arity}ToActionResultExtensions.g.cs",
                        Microsoft.CodeAnalysis.Text.SourceText.From(
                            $"// OneOf{_arity}ToActionResult generation error: {ex.Message}",
                            System.Text.Encoding.UTF8));
                }
            });
        }

        private bool HasOneOfUsageWithArity(Compilation compilation)
        {
            return compilation.SyntaxTrees
                .SelectMany(st => st.GetRoot().DescendantNodes())
                .OfType<GenericNameSyntax>()
                .Any(gns =>
                    gns.Identifier.ValueText == "OneOf" &&
                    gns.TypeArgumentList?.Arguments.Count == _arity);
        }
    }
}
