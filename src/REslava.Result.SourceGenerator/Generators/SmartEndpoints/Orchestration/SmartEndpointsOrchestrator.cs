using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints.Attributes;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints.Models;
using REslava.Result.SourceGenerators.Core.Interfaces;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints.CodeGeneration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace REslava.Result.SourceGenerators.SmartEndpoints.Orchestration
{
    /// <summary>
    /// Orchestrator for SmartEndpoints — discovers attributed classes, extracts metadata,
    /// and delegates code generation. Now produces ControllerMetadata with OpenAPI info.
    /// </summary>
    internal class SmartEndpointsOrchestrator : IGeneratorOrchestrator
    {
        private readonly IAttributeGenerator _autoGenerateAttributeGenerator;
        private readonly IAttributeGenerator _autoMapAttributeGenerator;
        private readonly IAttributeGenerator _smartAllowAnonymousAttributeGenerator;

        public SmartEndpointsOrchestrator()
        {
            _autoGenerateAttributeGenerator = new AutoGenerateEndpointsAttributeGenerator();
            _autoMapAttributeGenerator = new AutoMapEndpointAttributeGenerator();
            _smartAllowAnonymousAttributeGenerator = new SmartAllowAnonymousAttributeGenerator();
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Step 1: Always generate attributes
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource("AutoGenerateEndpointsAttribute.g.cs",
                    _autoGenerateAttributeGenerator.GenerateAttribute());
                ctx.AddSource("AutoMapEndpointAttribute.g.cs",
                    _autoMapAttributeGenerator.GenerateAttribute());
                ctx.AddSource("SmartAllowAnonymousAttribute.g.cs",
                    _smartAllowAnonymousAttributeGenerator.GenerateAttribute());
            });

            // Step 2: Detect classes with [AutoGenerateEndpoints]
            var classesWithAttribute = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is ClassDeclarationSyntax cls &&
                        cls.AttributeLists.SelectMany(al => al.Attributes)
                            .Any(a => a.Name.ToString().Contains("AutoGenerateEndpoints")),
                    transform: (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
                .Where(cls => cls != null);

            // Step 3: Generate endpoints grouped by controller
            var compilationAndClasses = context.CompilationProvider.Combine(classesWithAttribute.Collect());

            context.RegisterSourceOutput(compilationAndClasses, (spc, source) =>
            {
                var compilation = source.Left;
                var classes = source.Right;

                if (!classes.Any())
                    return;

                var controllers = new List<ControllerMetadata>();

                foreach (var classDecl in classes)
                {
                    var semanticModel = compilation.GetSemanticModel(classDecl.SyntaxTree);
                    var classSymbol = semanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
                    if (classSymbol == null) continue;

                    var controller = BuildControllerMetadata(classDecl, classSymbol, semanticModel);
                    if (controller != null && controller.Endpoints.Any())
                    {
                        controllers.Add(controller);
                    }
                }

                if (controllers.Any())
                {
                    var generator = new SmartEndpointExtensionGenerator();
                    var code = generator.GenerateCode(compilation, controllers);
                    spc.AddSource("SmartEndpointExtensions.g.cs", code);
                }
            });
        }

        private ControllerMetadata BuildControllerMetadata(
            ClassDeclarationSyntax classDecl,
            INamedTypeSymbol classSymbol,
            SemanticModel semanticModel)
        {
            // Extract attribute data
            var attr = classSymbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "AutoGenerateEndpointsAttribute");

            // Derive default route from class name (e.g., "UserService" → "/api/users")
            var className = classSymbol.Name;
            var defaultRoute = "/api/" + className
                .Replace("Service", "")
                .Replace("Controller", "")
                .Replace("Endpoints", "")
                .Replace("Endpoint", "")
                .ToLowerInvariant() + "s";

            var routePrefix = defaultRoute;
            if (attr != null)
            {
                var routeArg = attr.NamedArguments.FirstOrDefault(kv => kv.Key == "RoutePrefix");
                if (routeArg.Value.Value != null)
                {
                    routePrefix = routeArg.Value.Value.ToString();
                }
            }

            // Extract Tags from attribute, or auto-generate from class name
            var tags = ExtractTagsFromAttribute(attr);
            if (!tags.Any())
            {
                tags.Add(GenerateTagFromClassName(classSymbol.Name));
            }

            // Extract auth properties from class attribute
            var requiresAuth = false;
            var policies = new List<string>();
            var roles = new List<string>();
            if (attr != null)
            {
                var authArg = attr.NamedArguments.FirstOrDefault(kv => kv.Key == "RequiresAuth");
                if (authArg.Key == "RequiresAuth" && authArg.Value.Value is bool authVal)
                    requiresAuth = authVal;

                policies = ExtractStringArrayFromAttribute(attr, "Policies");
                roles = ExtractStringArrayFromAttribute(attr, "Roles");

                // If policies or roles are set, auth is implicitly required
                if (policies.Any() || roles.Any())
                    requiresAuth = true;
            }

            var controller = new ControllerMetadata
            {
                ClassName = classSymbol.Name,
                Namespace = classSymbol.ContainingNamespace?.ToDisplayString() ?? "Global",
                RoutePrefix = routePrefix,
                Tags = tags,
                HasAutoGenerateAttribute = true,
                RequiresAuth = requiresAuth,
                Policies = policies,
                Roles = roles
            };

            // Process public methods
            foreach (var member in classDecl.Members.OfType<MethodDeclarationSyntax>())
            {
                var methodSymbol = semanticModel.GetDeclaredSymbol(member) as IMethodSymbol;
                if (methodSymbol == null || !methodSymbol.DeclaredAccessibility.HasFlag(Accessibility.Public))
                    continue;

                var endpoint = BuildEndpointMetadata(methodSymbol, classSymbol, routePrefix, controller);
                if (endpoint != null)
                {
                    controller.Endpoints.Add(endpoint);
                }
            }

            return controller;
        }

        private EndpointMetadata BuildEndpointMetadata(
            IMethodSymbol methodSymbol,
            INamedTypeSymbol classSymbol,
            string routePrefix,
            ControllerMetadata controller)
        {
            // Unwrap Task<T> if async
            var actualReturnType = methodSymbol.ReturnType;
            var isAsync = false;

            if (actualReturnType is INamedTypeSymbol taskType &&
                taskType.OriginalDefinition.ToDisplayString().StartsWith("System.Threading.Tasks.Task<"))
            {
                isAsync = true;
                actualReturnType = taskType.TypeArguments[0];
            }

            // Check if return type is Result<T> or OneOf<...>
            var returnTypeStr = actualReturnType.ToDisplayString();
            var isResult = returnTypeStr.Contains("Result<");
            var isOneOf = returnTypeStr.Contains("OneOf<");

            if (!isResult && !isOneOf)
                return null;

            // Extract generic type arguments via Roslyn symbols
            var genericArgs = new List<string>();
            if (actualReturnType is INamedTypeSymbol namedReturnType && namedReturnType.IsGenericType)
            {
                foreach (var typeArg in namedReturnType.TypeArguments)
                {
                    genericArgs.Add(typeArg.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                }
            }

            var httpMethod = InferHttpMethod(methodSymbol.Name);
            var route = InferRoute(methodSymbol.Name, routePrefix, methodSymbol.Parameters);

            var endpoint = new EndpointMetadata
            {
                MethodName = methodSymbol.Name,
                ClassName = classSymbol.Name,
                Namespace = classSymbol.ContainingNamespace?.ToDisplayString() ?? "Global",
                ReturnType = returnTypeStr,
                IsAsync = isAsync,
                IsResult = isResult,
                IsOneOf = isOneOf,
                IsOneOf4 = isOneOf && genericArgs.Count == 4,
                Route = route,
                RoutePrefix = routePrefix,
                HttpMethod = httpMethod,
                GenericTypeArguments = genericArgs,
                Parameters = methodSymbol.Parameters.Select(p => new ParameterMetadata
                {
                    Name = p.Name,
                    Type = p.Type.ToDisplayString(),
                    Source = InferParameterSource(p.Name, httpMethod)
                }).ToList()
            };

            // Extract method-level auth attributes
            ExtractMethodAuthMetadata(methodSymbol, endpoint);

            // Apply class-level auth inheritance (method overrides take priority)
            ApplyAuthInheritance(endpoint, controller);

            // Build OpenAPI metadata (after auth, so Produces(401) can be added)
            endpoint.Summary = ExtractSummary(methodSymbol);
            endpoint.ProducesList = BuildProducesList(endpoint);

            return endpoint;
        }

        private void ExtractMethodAuthMetadata(IMethodSymbol methodSymbol, EndpointMetadata endpoint)
        {
            var attrs = methodSymbol.GetAttributes();

            // Check for [SmartAllowAnonymous]
            if (attrs.Any(a => a.AttributeClass?.Name == "SmartAllowAnonymousAttribute"))
            {
                endpoint.AllowAnonymous = true;
                return; // AllowAnonymous wins — skip other auth extraction
            }

            // Check for [AutoMapEndpoint] with auth properties
            var mapAttr = attrs.FirstOrDefault(a => a.AttributeClass?.Name == "AutoMapEndpointAttribute");
            if (mapAttr != null)
            {
                var allowAnonArg = mapAttr.NamedArguments.FirstOrDefault(kv => kv.Key == "AllowAnonymous");
                if (allowAnonArg.Key == "AllowAnonymous" && allowAnonArg.Value.Value is bool anonVal && anonVal)
                {
                    endpoint.AllowAnonymous = true;
                    return;
                }

                var authArg = mapAttr.NamedArguments.FirstOrDefault(kv => kv.Key == "RequiresAuth");
                if (authArg.Key == "RequiresAuth" && authArg.Value.Value is bool authVal)
                    endpoint.RequiresAuth = authVal;

                var methodPolicies = ExtractStringArrayFromAttribute(mapAttr, "Policies");
                if (methodPolicies.Any())
                    endpoint.Policies = methodPolicies;

                var methodRoles = ExtractStringArrayFromAttribute(mapAttr, "Roles");
                if (methodRoles.Any())
                    endpoint.Roles = methodRoles;

                // If policies or roles set, auth is implicitly required
                if (endpoint.Policies.Any() || endpoint.Roles.Any())
                    endpoint.RequiresAuth = true;
            }
        }

        private void ApplyAuthInheritance(EndpointMetadata endpoint, ControllerMetadata controller)
        {
            // If method explicitly allows anonymous, skip inheritance
            if (endpoint.AllowAnonymous)
                return;

            // If method already has its own auth config, skip inheritance
            if (endpoint.RequiresAuth)
                return;

            // Inherit class-level auth defaults
            if (controller.RequiresAuth)
            {
                endpoint.RequiresAuth = true;

                if (!endpoint.Policies.Any() && controller.Policies.Any())
                    endpoint.Policies = new List<string>(controller.Policies);

                if (!endpoint.Roles.Any() && controller.Roles.Any())
                    endpoint.Roles = new List<string>(controller.Roles);
            }
        }

        #region OpenAPI Metadata Helpers

        private string ExtractSummary(IMethodSymbol methodSymbol)
        {
            // Try XML doc <summary> first
            var xmlDoc = methodSymbol.GetDocumentationCommentXml();
            if (!string.IsNullOrEmpty(xmlDoc))
            {
                var summary = ParseXmlSummary(xmlDoc);
                if (!string.IsNullOrEmpty(summary))
                    return summary;
            }

            // Fall back to PascalCase → human words
            return GenerateSummaryFromMethodName(methodSymbol.Name);
        }

        private string ParseXmlSummary(string xmlDoc)
        {
            var startTag = "<summary>";
            var endTag = "</summary>";
            var startIdx = xmlDoc.IndexOf(startTag);
            var endIdx = xmlDoc.IndexOf(endTag);
            if (startIdx >= 0 && endIdx > startIdx)
            {
                var content = xmlDoc.Substring(startIdx + startTag.Length, endIdx - startIdx - startTag.Length);
                // Clean up whitespace and newlines
                var cleaned = content.Replace("\r", "").Replace("\n", " ").Trim();
                // Collapse multiple spaces
                while (cleaned.Contains("  "))
                    cleaned = cleaned.Replace("  ", " ");
                // Escape quotes for generated string literal
                cleaned = cleaned.Replace("\"", "\\\"");
                if (!string.IsNullOrEmpty(cleaned))
                    return cleaned;
            }
            return null;
        }

        private string GenerateSummaryFromMethodName(string methodName)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < methodName.Length; i++)
            {
                if (i > 0 && char.IsUpper(methodName[i]) &&
                    (char.IsLower(methodName[i - 1]) ||
                     (i + 1 < methodName.Length && char.IsLower(methodName[i + 1]))))
                {
                    sb.Append(' ');
                    sb.Append(char.ToLower(methodName[i]));
                }
                else if (i == 0)
                {
                    sb.Append(methodName[i]);
                }
                else
                {
                    sb.Append(methodName[i]);
                }
            }
            return sb.ToString();
        }

        private List<ProducesMetadata> BuildProducesList(EndpointMetadata endpoint)
        {
            var produces = new List<ProducesMetadata>();
            var seenStatusCodes = new HashSet<int>();

            if (endpoint.IsResult)
            {
                // Result<T>: T is success type, errors default to 400
                if (endpoint.GenericTypeArguments.Count > 0)
                {
                    produces.Add(new ProducesMetadata
                    {
                        StatusCode = 200,
                        ResponseType = endpoint.GenericTypeArguments[0]
                    });
                    seenStatusCodes.Add(200);
                }
                if (seenStatusCodes.Add(400))
                {
                    produces.Add(new ProducesMetadata { StatusCode = 400 });
                }
            }
            else if (endpoint.IsOneOf)
            {
                // OneOf<T1, T2, ...>: error types → status codes, non-error → success (200)
                foreach (var typeArg in endpoint.GenericTypeArguments)
                {
                    var simpleName = GetSimpleTypeName(typeArg);
                    if (IsErrorTypeName(simpleName))
                    {
                        var statusCode = DetermineOpenApiStatusCode(simpleName);
                        if (seenStatusCodes.Add(statusCode))
                        {
                            produces.Add(new ProducesMetadata { StatusCode = statusCode });
                        }
                    }
                    else
                    {
                        // Success type
                        if (seenStatusCodes.Add(200))
                        {
                            produces.Add(new ProducesMetadata
                            {
                                StatusCode = 200,
                                ResponseType = typeArg
                            });
                        }
                    }
                }
            }

            // Auto-add 401 for auth-required endpoints
            if (endpoint.RequiresAuth && !endpoint.AllowAnonymous && seenStatusCodes.Add(401))
            {
                produces.Add(new ProducesMetadata { StatusCode = 401 });
            }

            // Sort: 200 first, then errors ascending
            produces.Sort((a, b) => a.StatusCode.CompareTo(b.StatusCode));
            return produces;
        }

        private List<string> ExtractStringArrayFromAttribute(AttributeData attr, string propertyName)
        {
            var result = new List<string>();
            if (attr == null) return result;

            var arg = attr.NamedArguments.FirstOrDefault(kv => kv.Key == propertyName);
            if (arg.Key == propertyName && arg.Value.Kind == TypedConstantKind.Array)
            {
                foreach (var val in arg.Value.Values)
                {
                    if (val.Value != null)
                        result.Add(val.Value.ToString());
                }
            }
            return result;
        }

        private List<string> ExtractTagsFromAttribute(AttributeData attr)
        {
            var tags = new List<string>();
            if (attr == null) return tags;

            var tagsArg = attr.NamedArguments.FirstOrDefault(kv => kv.Key == "Tags");
            if (tagsArg.Key == "Tags" && tagsArg.Value.Kind == TypedConstantKind.Array)
            {
                foreach (var val in tagsArg.Value.Values)
                {
                    if (val.Value != null)
                        tags.Add(val.Value.ToString());
                }
            }
            return tags;
        }

        private string GenerateTagFromClassName(string className)
        {
            var name = className;
            foreach (var suffix in new[] { "Controller", "Service", "Endpoint", "Endpoints" })
            {
                if (name.EndsWith(suffix))
                {
                    name = name.Substring(0, name.Length - suffix.Length);
                    break;
                }
            }

            // Split PascalCase: "SmartProduct" → "Smart Product"
            var sb = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                if (i > 0 && char.IsUpper(name[i]) &&
                    (char.IsLower(name[i - 1]) ||
                     (i + 1 < name.Length && char.IsLower(name[i + 1]))))
                {
                    sb.Append(' ');
                }
                sb.Append(name[i]);
            }
            return sb.ToString();
        }

        private static string GetSimpleTypeName(string fullyQualifiedName)
        {
            // "global::Namespace.TypeName" → "TypeName"
            var lastDot = fullyQualifiedName.LastIndexOf('.');
            return lastDot >= 0 ? fullyQualifiedName.Substring(lastDot + 1) : fullyQualifiedName;
        }

        private static bool IsErrorTypeName(string typeName)
        {
            var lower = typeName.ToLowerInvariant();
            return lower.Contains("error") ||
                   lower.Contains("exception") ||
                   lower.Contains("fault") ||
                   lower.Contains("failure");
        }

        /// <summary>
        /// Maps error type names to HTTP status codes, matching the runtime behavior
        /// in the OneOf ToIResult extension generators.
        /// </summary>
        private static int DetermineOpenApiStatusCode(string errorTypeName)
        {
            var name = errorTypeName.ToLowerInvariant();

            if (name.Contains("notfound") || name.Contains("missing"))
                return 404;
            if (name.Contains("conflict") || name.Contains("duplicate") || name.Contains("insufficient"))
                return 409;
            if (name.Contains("unauthorized") || name.Contains("authentication"))
                return 401;
            if (name.Contains("forbidden") || name.Contains("permission") || name.Contains("inactive"))
                return 403;
            if (name.Contains("database") || name.Contains("system") || name.Contains("infrastructure"))
                return 500;

            // Validation, Invalid, and default
            return 400;
        }

        #endregion

        #region Route & HTTP Method Inference

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

        #endregion
    }
}
