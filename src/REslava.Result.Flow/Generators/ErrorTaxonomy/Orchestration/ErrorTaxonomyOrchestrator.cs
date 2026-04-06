using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.Result.Flow.Core.Interfaces;
using System.Linq;

namespace REslava.Result.Flow.Generators.ErrorTaxonomy.Orchestration
{
    internal class ErrorTaxonomyOrchestrator : IGeneratorOrchestrator
    {
        private const string AttributeShortName = "ResultFlow";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Find classes that contain at least one [ResultFlow]-decorated method
            var classesWithResultFlow = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is ClassDeclarationSyntax cls &&
                        cls.Members.OfType<MethodDeclarationSyntax>().Any(m =>
                            m.AttributeLists.SelectMany(al => al.Attributes)
                                .Any(a => a.Name.ToString().Contains(AttributeShortName))),
                    transform: (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
                .Where(cls => cls != null);

            // Resolve IResultBase and IError once per compilation
            var compilationProvider = context.CompilationProvider.Select((compilation, _) =>
            {
                var resultBase = compilation.GetTypeByMetadataName("REslava.Result.IResultBase");
                var iError    = compilation.GetTypeByMetadataName("REslava.Result.IError");
                return (Compilation: compilation, ResultBase: resultBase, IError: iError);
            });

            var combined = compilationProvider.Combine(classesWithResultFlow.Collect());

            context.RegisterSourceOutput(combined, (spc, source) =>
            {
                var (compSymbols, classes) = source;
                if (!classes.Any()) return;

                var compilation   = compSymbols.Compilation;
                var resultBase    = compSymbols.ResultBase;
                var iError        = compSymbols.IError;

                foreach (var classDecl in classes)
                {
                    var semanticModel = compilation.GetSemanticModel(classDecl.SyntaxTree);
                    var className     = classDecl.Identifier.ValueText;

                    var rows = ErrorTaxonomyScanner.Scan(
                        classDecl, semanticModel, compilation, resultBase, iError);

                    // Skip classes where no errors were detected — no constant to emit
                    if (rows.Count == 0) continue;

                    var table      = ErrorTaxonomyRenderer.Render(rows);
                    var sourceText = ErrorTaxonomyCodeGenerator.Generate(className, table);
                    spc.AddSource($"{className}_ErrorTaxonomy.g.cs", sourceText);
                }
            });
        }
    }
}
