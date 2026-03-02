using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.Result.SourceGenerators.Core.Interfaces;
using REslava.Result.SourceGenerators.Generators.Validate.Attributes;
using REslava.Result.SourceGenerators.Generators.Validate.CodeGeneration;
using System.Linq;

namespace REslava.Result.SourceGenerators.Generators.Validate.Orchestration
{
    internal class ValidateOrchestrator : IGeneratorOrchestrator
    {
        private const string AttributeShortName = "Validate";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Stage 1: Always emit [Validate] attribute (available immediately to user code)
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource("ValidateAttribute.g.cs", ValidateAttributeGenerator.GenerateAttribute());
            });

            // Stage 2: Find type declarations decorated with [Validate]
            var validatedTypes = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is TypeDeclarationSyntax typeDecl &&
                        typeDecl.AttributeLists.SelectMany(al => al.Attributes)
                            .Any(a => a.Name.ToString().Contains(AttributeShortName)),
                    transform: (ctx, _) => (TypeDeclarationSyntax)ctx.Node)
                .Where(t => t != null);

            var compilationAndTypes = context.CompilationProvider.Combine(validatedTypes.Collect());

            // Stage 3: Emit one Validate() extension file per decorated type
            context.RegisterSourceOutput(compilationAndTypes, (spc, source) =>
            {
                var compilation = source.Left;
                var types = source.Right;

                if (!types.Any()) return;

                foreach (var typeDecl in types)
                {
                    var semanticModel = compilation.GetSemanticModel(typeDecl.SyntaxTree);
                    var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl) as INamedTypeSymbol;
                    if (typeSymbol == null) continue;

                    var code = ValidateExtensionGenerator.GenerateForType(typeSymbol);
                    spc.AddSource($"{typeSymbol.Name}ValidationExtensions.g.cs", code);
                }
            });
        }
    }
}
