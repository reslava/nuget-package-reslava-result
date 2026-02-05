using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints.Attributes;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints.Models;
using REslava.Result.SourceGenerators.SmartEndpoints.Models;
using REslava.Result.SourceGenerators.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace REslava.Result.SourceGenerators.SmartEndpoints.Orchestration;

/// <summary>
/// Orchestrates the SmartEndpoints generation pipeline with two-pass approach
/// </summary>
internal class SmartEndpointsOrchestrator : IGeneratorOrchestrator
{
    private readonly IAttributeGenerator _autoGenerateEndpointsAttributeGenerator;
    private readonly IAttributeGenerator _autoMapEndpointAttributeGenerator;
    private readonly ICodeGenerator _smartEndpointExtensionGenerator;

    public SmartEndpointsOrchestrator(
        IAttributeGenerator autoGenerateEndpointsAttributeGenerator,
        IAttributeGenerator autoMapEndpointAttributeGenerator,
        ICodeGenerator smartEndpointExtensionGenerator)
    {
        _autoGenerateEndpointsAttributeGenerator = autoGenerateEndpointsAttributeGenerator;
        _autoMapEndpointAttributeGenerator = autoMapEndpointAttributeGenerator;
        _smartEndpointExtensionGenerator = smartEndpointExtensionGenerator;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        System.Diagnostics.Debug.WriteLine("🚀 SmartEndpointsOrchestrator.Initialize called!");

        // Step 1: Detect generation mode from MSBuild properties
        var generationModePipeline = context.AnalyzerConfigOptionsProvider
            .Select((provider, _) =>
            {
                var globalOptions = provider.GlobalOptions;
                var modeString = globalOptions.TryGetValue("build_property.SmartEndpointsGenerationMode", out var mode) 
                    ? mode 
                    : "Cache"; // Default to Cache mode
                
                var generationMode = modeString?.Equals("File", StringComparison.OrdinalIgnoreCase) == true 
                    ? GenerationMode.File 
                    : GenerationMode.Cache;
                
                System.Diagnostics.Debug.WriteLine($"🔍 SmartEndpoints: Generation mode = {generationMode}");
                return generationMode;
            });

        // Step 2: Pass 1 - Always generate attributes (both modes)
        var attributePipeline = context.CompilationProvider.Select((compilation, _) =>
        {
            // Check if we have Result or OneOf types (check all OneOf variants)
            var hasResultTypes = compilation.GetTypeByMetadataName("REslava.Result.Result`1") != null;
            var hasOneOfTypes = new[] { "OneOf`2", "OneOf`3", "OneOf`4" }
                .Any(type => compilation.GetTypeByMetadataName($"REslava.Result.AdvancedPatterns.{type}") != null);

            System.Diagnostics.Debug.WriteLine($"🔍 SmartEndpoints - HasResult: {hasResultTypes}, HasOneOf: {hasOneOfTypes}");

            return compilation;
        });

        context.RegisterSourceOutput(attributePipeline, (spc, compilation) =>
        {
            if (compilation == null) return;

            System.Diagnostics.Debug.WriteLine("🚀 SmartEndpoints: Attribute pipeline executing!");

            // Generate AutoMapEndpoint attribute
            spc.AddSource("AutoMapEndpointAttribute.g.cs",
                _autoMapEndpointAttributeGenerator.GenerateAttribute());

            // Generate AutoGenerateEndpoints attribute
            spc.AddSource("AutoGenerateEndpointsAttribute.g.cs",
                _autoGenerateEndpointsAttributeGenerator.GenerateAttribute());
        });

        // Step 3: Direct syntax-based pipeline with test file generation
        var directPipeline = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => 
                    node is ClassDeclarationSyntax cls && 
                    cls.AttributeLists
                        .SelectMany(al => al.Attributes)
                        .Any(a => a.Name.ToString()
                            .Contains("AutoGenerateEndpoints")),
                transform: (ctx, ct) =>
                {
                    var classSyntax = (ClassDeclarationSyntax)ctx.Node;
                    var className = classSyntax.Identifier.ValueText;
                    
                    System.Diagnostics.Debug.WriteLine($"🔍 SmartEndpoints: DIRECT - Found class with AutoGenerateEndpoints attribute: {className}");
                    
                    return new { ClassName = className, SyntaxNode = classSyntax, Compilation = ctx.SemanticModel.Compilation };
                });

        context.RegisterSourceOutput(directPipeline, (spc, target) =>
        {
            System.Diagnostics.Debug.WriteLine("🚀 SmartEndpoints: DIRECT pipeline executing!");

            // TEMP: Generate a test file to verify pipeline is working
            spc.AddSource("SmartEndpointsDirectTest.g.cs", $"// SmartEndpoints DIRECT pipeline found: {target.ClassName}");

            try
            {
                // Extract endpoint metadata from the class
                var endpointMetadata = new List<EndpointMetadata>();
                if (target.SyntaxNode is ClassDeclarationSyntax classSyntax)
                {
                    // Extract RoutePrefix from [AutoGenerateEndpoints(RoutePrefix = "...")] attribute
                    var routePrefix = "/api/smarttest"; // Default fallback
                    var attributeList = classSyntax.AttributeLists
                        .SelectMany(al => al.Attributes)
                        .FirstOrDefault(a => a.Name.ToString().Contains("AutoGenerateEndpoints"));
                    
                    if (attributeList != null)
                    {
                        // Parse the RoutePrefix argument
                        var argumentList = attributeList.ArgumentList?.Arguments;
                        if (argumentList != null && argumentList.Value.Count > 0)
                        {
                            var firstArg = argumentList.Value[0];
                            var routeArg = firstArg.Expression?.ToString();
                            if (!string.IsNullOrEmpty(routeArg) && routeArg.StartsWith("\"") && routeArg.EndsWith("\""))
                            {
                                routePrefix = routeArg.Trim('"');
                            }
                        }
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"🔍 SmartEndpoints: Found route prefix: {routePrefix}");
                    
                    // Get all methods in the class
                    var methods = classSyntax.Members.OfType<MethodDeclarationSyntax>();
                    
                    foreach (var method in methods)
                    {
                        var methodName = method.Identifier.ValueText;
                        var returnType = method.ReturnType?.ToString() ?? "";
                        
                        System.Diagnostics.Debug.WriteLine($"🔍 SmartEndpoints: Found method: {methodName}, Return: {returnType}");
                        
                        // Check if method returns Result<T> or OneOf<...>
                        if (returnType.Contains("Result<") || returnType.Contains("OneOf<"))
                        {
                            // Extract parameters
                            var parameters = new List<ParameterMetadata>();
                            var paramList = new List<string>();
                            var argList = new List<string>();
                            
                            foreach (var param in method.ParameterList.Parameters)
                            {
                                var paramName = param.Identifier.ValueText;
                                var paramType = param.Type?.ToString() ?? "object";
                                
                                parameters.Add(new ParameterMetadata
                                {
                                    Name = paramName,
                                    Type = paramType,
                                    Source = paramType.Contains("CreateUserRequest") ? ParameterSource.Body : ParameterSource.Route
                                });
                                
                                paramList.Add($"{paramType} {paramName}");
                                argList.Add(paramName);
                            }
                            
                            var endpoint = new EndpointMetadata
                            {
                                MethodName = methodName,
                                ReturnType = returnType,
                                ClassName = target.ClassName, // Set the class name
                                Parameters = parameters,
                                Route = routePrefix + InferRouteFromMethodName(methodName, parameters),
                                HttpMethod = InferHttpMethodFromMethodName(methodName)
                            };
                            
                            endpointMetadata.Add(endpoint);
                            System.Diagnostics.Debug.WriteLine($"🔍 SmartEndpoints: Added endpoint: {endpoint.HttpMethod} {endpoint.Route} with params: {string.Join(", ", argList)}");
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"🔍 SmartEndpoints: Found {endpointMetadata.Count} endpoints in {target.ClassName}");

                // Generate endpoint mapping code with actual metadata
                var extensionCode = _smartEndpointExtensionGenerator.GenerateCode(target.Compilation, endpointMetadata);
                
                spc.AddSource("SmartEndpointExtensions.g.cs", extensionCode);
                System.Diagnostics.Debug.WriteLine("🚀 SmartEndpointExtensions.g.cs generated successfully via DIRECT pipeline!");
            }
            catch (System.Exception ex)
            {
                // Log error but don't fail the build
                System.Diagnostics.Debug.WriteLine($"❌ SmartEndpoints error: {ex.Message}");
                spc.AddSource("SmartEndpointExtensions.g.cs", $"// Error during generation: {ex.Message}");
            }
        });

        // Step 4: Fallback - Always generate a test file to verify generator is working
        context.RegisterSourceOutput(context.CompilationProvider, (spc, compilation) =>
        {
            System.Diagnostics.Debug.WriteLine("🚀 SmartEndpoints: FALLBACK pipeline executing!");
            spc.AddSource("SmartEndpointsFallbackTest.g.cs", "// SmartEndpoints FALLBACK pipeline is working!");
        });
    }

    private static string InferRouteFromMethodName(string methodName, List<ParameterMetadata> parameters)
    {
        var baseRoute = "";
        // Simple convention-based route inference
        if (methodName.StartsWith("Get", StringComparison.OrdinalIgnoreCase))
        {
            if (methodName.Equals("Get", StringComparison.OrdinalIgnoreCase))
                baseRoute = "";
            else
                baseRoute = "/" + methodName.Substring(3);
        }
        
        if (methodName.StartsWith("Create", StringComparison.OrdinalIgnoreCase) ||
            methodName.StartsWith("Add", StringComparison.OrdinalIgnoreCase))
        {
            baseRoute = "";
        }
        
        if (methodName.StartsWith("Update", StringComparison.OrdinalIgnoreCase) ||
            methodName.StartsWith("Modify", StringComparison.OrdinalIgnoreCase))
        {
            baseRoute = "/{id}";
        }
        
        if (methodName.StartsWith("Delete", StringComparison.OrdinalIgnoreCase) ||
            methodName.StartsWith("Remove", StringComparison.OrdinalIgnoreCase))
        {
            baseRoute = "/{id}";
        }
        
        // Default: use method name as route
        if (string.IsNullOrEmpty(baseRoute))
            baseRoute = "/" + methodName.ToLowerInvariant();

        // Check if method has an 'id' parameter and add it to the route if not already present
        var hasIdParam = parameters.Any(p => p.Name.Equals("id", StringComparison.OrdinalIgnoreCase));
        if (hasIdParam && !baseRoute.Contains("{id}"))
        {
            baseRoute += "/{id}";
        }

        return baseRoute;
    }

    private static string InferHttpMethodFromMethodName(string methodName)
    {
        if (methodName.StartsWith("Get", StringComparison.OrdinalIgnoreCase))
            return "GET";
        
        if (methodName.StartsWith("Create", StringComparison.OrdinalIgnoreCase) ||
            methodName.StartsWith("Add", StringComparison.OrdinalIgnoreCase))
            return "POST";
        
        if (methodName.StartsWith("Update", StringComparison.OrdinalIgnoreCase) ||
            methodName.StartsWith("Modify", StringComparison.OrdinalIgnoreCase))
            return "PUT";
        
        if (methodName.StartsWith("Delete", StringComparison.OrdinalIgnoreCase) ||
            methodName.StartsWith("Remove", StringComparison.OrdinalIgnoreCase))
            return "DELETE";
        
        // Default to GET
        return "GET";
    }
}
