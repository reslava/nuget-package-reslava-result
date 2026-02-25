using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.Result.FluentValidation.Generators.FluentValidate.Attributes;
using REslava.Result.FluentValidation.Generators.FluentValidate.CodeGeneration;
using System.Linq;

namespace REslava.Result.FluentValidation.Generators.FluentValidate.Orchestration
{
    internal class FluentValidateOrchestrator
    {
        private const string AttributeShortName = "FluentValidate";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Stage 1: Always emit [FluentValidate] attribute (available immediately to user code)
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource("FluentValidateAttribute.g.cs", FluentValidateAttributeGenerator.GenerateAttribute());
            });

            // Stage 2: Find type declarations decorated with [FluentValidate]
            var validatedTypes = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is TypeDeclarationSyntax typeDecl &&
                        typeDecl.AttributeLists.SelectMany(al => al.Attributes)
                            .Any(a => a.Name.ToString().Contains(AttributeShortName)),
                    transform: (ctx, _) => (TypeDeclarationSyntax)ctx.Node)
                .Where(t => t != null);

            var compilationAndTypes = context.CompilationProvider.Combine(validatedTypes.Collect());

            // Stage 3: Emit one .Validate(IValidator<T>) extension file per decorated type
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

                    var code = FluentValidateExtensionGenerator.GenerateForType(typeSymbol);
                    spc.AddSource($"{typeSymbol.Name}FluentValidationExtensions.g.cs", code);
                }
            });
        }
    }
}
