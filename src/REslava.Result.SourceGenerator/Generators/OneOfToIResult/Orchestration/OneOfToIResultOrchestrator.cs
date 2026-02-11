using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.Result.SourceGenerators.Generators.OneOfToIResult.Attributes;
using REslava.Result.SourceGenerators.Generators.OneOfToIResult.CodeGeneration;
using REslava.Result.SourceGenerators.Core.Interfaces;
using System;
using System.Linq;

namespace REslava.Result.SourceGenerators.Generators.OneOfToIResult.Orchestration
{
    /// <summary>
    /// Shared orchestrator for OneOf{N}ToIResult generation, parameterized by arity.
    /// Handles OneOf2, OneOf3, and OneOf4 via a single pipeline.
    /// </summary>
    public class OneOfToIResultOrchestrator : IGeneratorOrchestrator
    {
        private readonly int _arity;
        private readonly IAttributeGenerator _extensionsAttributeGenerator;
        private readonly IAttributeGenerator _mapToProblemDetailsAttributeGenerator;
        private readonly ICodeGenerator _extensionGenerator;

        public OneOfToIResultOrchestrator(int arity)
        {
            _arity = arity;
            _extensionsAttributeGenerator = new OneOfExtensionsAttributeGenerator(arity);
            _mapToProblemDetailsAttributeGenerator = new MapToProblemDetailsAttributeGenerator();
            _extensionGenerator = new OneOfToIResultExtensionGenerator(arity);
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

                spc.AddSource($"GenerateOneOf{_arity}ExtensionsAttribute.g.cs",
                    _extensionsAttributeGenerator.GenerateAttribute());

                // Only emit the shared MapToProblemDetailsAttribute once (from arity 2)
                // to avoid duplicate type definitions across OneOf2/3/4 generators
                if (_arity == 2)
                {
                    spc.AddSource($"MapToProblemDetailsAttribute.g.cs",
                        _mapToProblemDetailsAttributeGenerator.GenerateAttribute());
                }
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
                    spc.AddSource($"OneOf{_arity}ToIResultExtensions.g.cs", extensionCode);
                }
                catch (Exception ex)
                {
                    spc.AddSource($"OneOf{_arity}ToIResultExtensions.g.cs",
                        Microsoft.CodeAnalysis.Text.SourceText.From(
                            $"// OneOf{_arity}ToIResult generation error: {ex.Message}",
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
