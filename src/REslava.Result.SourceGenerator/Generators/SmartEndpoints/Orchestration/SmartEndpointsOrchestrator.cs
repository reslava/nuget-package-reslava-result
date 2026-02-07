using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints.Attributes;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints.Models;
using REslava.Result.SourceGenerators.Core.Interfaces;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints.CodeGeneration;
using System.Collections.Generic;
using System.Linq;

namespace REslava.Result.SourceGenerators.SmartEndpoints.Orchestration
{
    /// <summary>
    /// Simplified orchestrator for SmartEndpoints - FIXED VERSION v2
    /// </summary>
    internal class SmartEndpointsOrchestrator : IGeneratorOrchestrator
    {
        private readonly IAttributeGenerator _autoGenerateAttributeGenerator;
        private readonly IAttributeGenerator _autoMapAttributeGenerator;

        public SmartEndpointsOrchestrator()
        {
            _autoGenerateAttributeGenerator = new AutoGenerateEndpointsAttributeGenerator();
            _autoMapAttributeGenerator = new AutoMapEndpointAttributeGenerator();
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            System.Diagnostics.Debug.WriteLine("🔥 SmartEndpointsOrchestrator.Initialize");

            // Step 1: Always generate attributes
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource("AutoGenerateEndpointsAttribute.g.cs",
                    _autoGenerateAttributeGenerator.GenerateAttribute());
                ctx.AddSource("AutoMapEndpointAttribute.g.cs",
                    _autoMapAttributeGenerator.GenerateAttribute());
            });

            // Step 2: Detect classes with [AutoGenerateEndpoints]
            var classesWithAttribute = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is ClassDeclarationSyntax cls &&
                        cls.AttributeLists.SelectMany(al => al.Attributes)
                            .Any(a => a.Name.ToString().Contains("AutoGenerateEndpoints")),
                    transform: (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
                .Where(cls => cls != null);

            // Step 3: Generate endpoints
            var compilationAndClasses = context.CompilationProvider.Combine(classesWithAttribute.Collect());

            context.RegisterSourceOutput(compilationAndClasses, (spc, source) =>
            {
                var compilation = source.Left;
                var classes = source.Right;

                if (!classes.Any())
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ No SmartEndpoints classes found");
                    return;
                }

                var endpoints = new List<EndpointMetadata>();

                foreach (var classDecl in classes)
                {
                    var semanticModel = compilation.GetSemanticModel(classDecl.SyntaxTree);
                    var classSymbol = semanticModel.GetDeclaredSymbol(classDecl);

                    if (classSymbol == null) continue;

                    // Extract RoutePrefix
                    var attr = classSymbol.GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass?.Name == "AutoGenerateEndpointsAttribute");

                    var routePrefix = "/api/test";
                    if (attr != null)
                    {
                        var routeArg = attr.NamedArguments.FirstOrDefault(kv => kv.Key == "RoutePrefix");
                        if (routeArg.Value.Value != null)
                        {
                            routePrefix = routeArg.Value.Value.ToString();
                        }
                    }

                    // Process methods
                    foreach (var member in classDecl.Members.OfType<MethodDeclarationSyntax>())
                    {
                        var symbol = semanticModel.GetDeclaredSymbol(member);
                        
                        // FIX: Cast to IMethodSymbol
                        var methodSymbol = symbol as IMethodSymbol;
                        if (methodSymbol == null || !methodSymbol.DeclaredAccessibility.HasFlag(Accessibility.Public))
                            continue;

                        var returnType = methodSymbol.ReturnType.ToDisplayString();
                        if (!returnType.Contains("Result<") && !returnType.Contains("OneOf<"))
                            continue;

                        var endpoint = new EndpointMetadata
                        {
                            MethodName = methodSymbol.Name,
                            ClassName = classSymbol.Name,
                            Namespace = classSymbol.ContainingNamespace?.ToDisplayString() ?? "Global",
                            ReturnType = returnType,
                            Route = InferRoute(methodSymbol.Name, routePrefix, methodSymbol.Parameters),
                            HttpMethod = InferHttpMethod(methodSymbol.Name),
                            Parameters = methodSymbol.Parameters.Select(p => new ParameterMetadata
                            {
                                Name = p.Name,
                                Type = p.Type.ToDisplayString(),
                                Source = InferParameterSource(p.Name, InferHttpMethod(methodSymbol.Name))
                            }).ToList()
                        };

                        endpoints.Add(endpoint);
                        System.Diagnostics.Debug.WriteLine($"✅ Added endpoint: {endpoint.HttpMethod} {endpoint.Route}");
                    }
                }

                if (endpoints.Any())
                {
                    var generator = new SmartEndpointExtensionGenerator();
                    var code = generator.GenerateCode(compilation, endpoints);
                    spc.AddSource("SmartEndpointExtensions.g.cs", code);
                    System.Diagnostics.Debug.WriteLine($"✅ Generated {endpoints.Count} SmartEndpoints");
                }
            });
        }

        private string InferRoute(string methodName, string prefix, System.Collections.Immutable.ImmutableArray<IParameterSymbol> parameters)
        {
            var hasIdParam = parameters.Any(p => p.Name.Equals("id", System.StringComparison.OrdinalIgnoreCase));

            if (methodName.StartsWith("Get") && hasIdParam)
                return $"{prefix}/{{id}}";
            if (methodName.StartsWith("Update") || methodName.StartsWith("Delete"))
                return $"{prefix}/{{id}}";

            return prefix;
        }

        private string InferHttpMethod(string methodName)
        {
            if (methodName.StartsWith("Get")) return "GET";
            if (methodName.StartsWith("Create") || methodName.StartsWith("Add")) return "POST";
            if (methodName.StartsWith("Update")) return "PUT";
            if (methodName.StartsWith("Delete")) return "DELETE";
            return "GET";
        }

        private ParameterSource InferParameterSource(string paramName, string httpMethod)
        {
            if (paramName.Equals("id", System.StringComparison.OrdinalIgnoreCase))
                return ParameterSource.Route;
            if (httpMethod == "POST" || httpMethod == "PUT")
                return ParameterSource.Body;
            return ParameterSource.Query;
        }
    }
}
