using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.Result.SourceGenerators.Generators.ResultToActionResult.Attributes;
using REslava.Result.SourceGenerators.Generators.ResultToActionResult.CodeGeneration;
using REslava.Result.SourceGenerators.Core.Interfaces;

namespace REslava.Result.SourceGenerators.Generators.ResultToActionResult.Orchestration
{
    /// <summary>
    /// Orchestrates the generation pipeline for ResultToActionResult.
    /// Mirrors ResultToIResultOrchestrator — same gate logic, same two-stage pipeline.
    /// </summary>
    public class ResultToActionResultOrchestrator : IGeneratorOrchestrator
    {
        private readonly IAttributeGenerator _attributeGenerator;
        private readonly ICodeGenerator _codeGenerator;

        public ResultToActionResultOrchestrator()
        {
            _attributeGenerator = new GenerateActionResultExtensionsAttributeGenerator();
            _codeGenerator = new ResultToActionResultExtensionGenerator();
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Stage 1: Attribute generation pipeline
            var attributePipeline = context.CompilationProvider.Select((compilation, _) => compilation);

            context.RegisterSourceOutput(attributePipeline, (spc, compilation) =>
            {
                if (compilation == null) return;

                if (!HasResultUsage(compilation))
                    return;

                spc.AddSource("GenerateActionResultExtensionsAttribute.g.cs",
                    _attributeGenerator.GenerateAttribute());
            });

            // Stage 2: Code generation pipeline
            var codePipeline = context.CompilationProvider.Select((compilation, _) => compilation);

            context.RegisterSourceOutput(codePipeline, (spc, compilation) =>
            {
                if (compilation == null) return;

                if (!HasResultUsage(compilation))
                    return;

                var extensionCode = _codeGenerator.GenerateCode(compilation, null);
                spc.AddSource("ResultToActionResultExtensions.g.cs", extensionCode);
            });
        }

        /// <summary>
        /// Checks if the compilation contains Result&lt;T&gt; or Result usage in syntax trees.
        /// Same gate logic as ResultToIResultOrchestrator.
        /// </summary>
        private static bool HasResultUsage(Compilation compilation)
        {
            var hasGenericResult = compilation.SyntaxTrees
                .SelectMany(st => st.GetRoot().DescendantNodes())
                .OfType<GenericNameSyntax>()
                .Any(gns =>
                    gns.Identifier.ValueText == "Result" &&
                    gns.TypeArgumentList?.Arguments.Count >= 1);

            if (hasGenericResult)
                return true;

            var hasNonGenericResult = compilation.SyntaxTrees
                .SelectMany(st => st.GetRoot().DescendantNodes())
                .OfType<IdentifierNameSyntax>()
                .Any(ins =>
                    ins.Identifier.ValueText == "Result" &&
                    ins.Parent is not GenericNameSyntax);

            return hasNonGenericResult;
        }
    }
}
