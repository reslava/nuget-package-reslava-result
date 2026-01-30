using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace REslava.Result.SourceGenerators
{
    /// <summary>
    /// Enhanced source generator that creates extension methods for converting Result&lt;T&gt; to IResult.
    /// Now includes metadata discovery for error types to enable intelligent HTTP status code mapping.
    /// 
    /// Features:
    /// - Automatic error type discovery at compile-time
    /// - Convention-based HTTP status mapping (NotFoundError → 404)
    /// - Attribute-based overrides ([MapToProblemDetails])
    /// - RFC 7807 ProblemDetails generation
    /// - HTTP method-specific extensions (ToPostResult, ToDeleteResult, etc.)
    /// </summary>
    [Generator]
    public class ResultToIResultGenerator : IIncrementalGenerator
    {
        private const string AttributeName = "REslava.Result.SourceGenerators.GenerateResultExtensionsAttribute";
        private const string AttributeShortName = "GenerateResultExtensions";
        private const string MapToProblemDetailsAttributeName = "REslava.Result.SourceGenerators.MapToProblemDetailsAttribute";

        /// <summary>
        /// Initializes the incremental generator pipeline with enhanced metadata discovery.
        /// This is the entry point that sets up all the compilation analysis and code generation.
        /// </summary>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // ═══════════════════════════════════════════════════════════════════════════
            // STEP 1: Register attributes so consumers can use them in their code
            // ═══════════════════════════════════════════════════════════════════════════
            // This runs FIRST during compilation, making attributes available immediately
            context.RegisterPostInitializationOutput(ctx =>
            {
                // Generate [GenerateResultExtensions] attribute
                ctx.AddSource("GenerateResultExtensionsAttribute.g.cs",
                    SourceText.From(GetGenerateResultExtensionsAttributeSource(), Encoding.UTF8));

                // Generate [MapToProblemDetails] attribute (NEW!)
                // This allows users to explicitly map error types to HTTP status codes
                ctx.AddSource("MapToProblemDetailsAttribute.g.cs",
                    SourceText.From(GetMapToProblemDetailsAttributeSource(), Encoding.UTF8));
            });

            // ═══════════════════════════════════════════════════════════════════════════
            // STEP 2: Find assemblies with [GenerateResultExtensions] attribute
            // ═══════════════════════════════════════════════════════════════════════════
            // This determines if this assembly wants the generator to run
            var assemblyAttributes = context.CompilationProvider
                .Select((compilation, cancellationToken) =>
                {
                    var attributes = compilation.Assembly.GetAttributes();
                    var targetAttribute = attributes.FirstOrDefault(a =>
                        a.AttributeClass?.ToDisplayString() == AttributeName ||
                        a.AttributeClass?.Name == AttributeShortName);

                    if (targetAttribute == null)
                        return ImmutableArray<(Compilation, GeneratorConfiguration)>.Empty;

                    // Parse configuration from attribute (CustomErrorMappings, etc.)
                    var config = ParseConfiguration(targetAttribute);
                    return ImmutableArray.Create((compilation, config));
                })
                .WithTrackingName("GenerateResultExtensions");

            // ═══════════════════════════════════════════════════════════════════════════
            // STEP 3: Discover error types with [MapToProblemDetails] attribute (NEW!)
            // ═══════════════════════════════════════════════════════════════════════════
            // This finds all IError types that have explicit attribute-based mappings
            var errorTypesWithAttributes = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    MapToProblemDetailsAttributeName,
                    // Predicate: Only look at class declarations
                    predicate: (node, _) => node is ClassDeclarationSyntax,
                    // Transform: Extract metadata from the attributed class
                    transform: TransformToErrorMetadata)
                .Where(m => m != null);

            // ═══════════════════════════════════════════════════════════════════════════
            // STEP 4: Discover ALL IError types for convention-based mapping (NEW!)
            // ═══════════════════════════════════════════════════════════════════════════
            // This finds all error types even without attributes, enabling convention-based mapping
            var allErrorTypes = context.SyntaxProvider
                .CreateSyntaxProvider(
                    // Predicate: Find potential error class declarations
                    predicate: (node, _) => IsErrorClassDeclaration(node),
                    // Transform: Get the symbol and verify it implements IError
                    transform: GetErrorTypeSymbol)
                .Where(s => s != null);

            // ═══════════════════════════════════════════════════════════════════════════
            // STEP 5: Combine all discovered information
            // ═══════════════════════════════════════════════════════════════════════════
            // We need: configuration + explicit mappings + convention types
            var combined = assemblyAttributes
                .Combine(errorTypesWithAttributes.Collect())  // Collect explicit mappings
                .Combine(allErrorTypes.Collect());            // Collect convention-based types

            // ═══════════════════════════════════════════════════════════════════════════
            // STEP 6: Generate the enhanced extension methods
            // ═══════════════════════════════════════════════════════════════════════════
            context.RegisterSourceOutput(combined, (spc, data) =>
            {
                if (data.Left.Left.IsEmpty) return;

                var (compilation, config) = data.Left.Left[0];
                var explicitMappings = data.Left.Right;  // From attributes
                var conventionTypes = data.Right;         // For convention matching

                var source = GenerateEnhancedExtensions(config, explicitMappings, conventionTypes);

                spc.AddSource("ResultToIResultExtensions.g.cs",
                    SourceText.From(source, Encoding.UTF8));
            });
        }

        #region Metadata Discovery Methods (NEW!)

        /// <summary>
        /// Transforms a class with [MapToProblemDetails] into error metadata.
        /// This captures explicit developer intent for HTTP status mapping.
        /// </summary>
        private static ErrorMetadata? TransformToErrorMetadata(GeneratorAttributeSyntaxContext context, CancellationToken ct)
        {
            var classSymbol = (INamedTypeSymbol)context.TargetSymbol;
            var attribute = context.Attributes[0];

            // Verify this class actually implements IError
            if (!ImplementsIError(classSymbol))
                return null;

            // Extract attribute parameters with defaults
            var statusCode = 400;      // Default: Bad Request
            string? type = null;       // Default: https://httpstatuses.io/{statusCode}
            string? title = null;      // Default: Status code text
            var includeTags = true;    // Default: Include error tags in response

            // Parse named arguments from attribute
            foreach (var namedArg in attribute.NamedArguments)
            {
                switch (namedArg.Key)
                {
                    case "StatusCode":
                        statusCode = (int)namedArg.Value.Value!;
                        break;
                    case "Type":
                        type = namedArg.Value.Value?.ToString();
                        break;
                    case "Title":
                        title = namedArg.Value.Value?.ToString();
                        break;
                    case "IncludeTags":
                        includeTags = (bool)namedArg.Value.Value!;
                        break;
                }
            }

            return new ErrorMetadata
            {
                TypeName = classSymbol.Name,
                FullName = classSymbol.ToDisplayString(),
                StatusCode = statusCode,
                Type = type ?? $"https://httpstatuses.io/{statusCode}",
                Title = title ?? GetDefaultTitle(statusCode),
                IncludeTags = includeTags,
                Source = "Attribute"  // Mark as explicitly defined
            };
        }

        /// <summary>
        /// Quick check if a syntax node might be an error class declaration.
        /// This is a fast filter before doing expensive semantic analysis.
        /// </summary>
        private static bool IsErrorClassDeclaration(SyntaxNode node)
        {
            // Must be a class declaration
            if (node is not ClassDeclarationSyntax classDecl)
                return false;

            // Quick heuristic: class name contains "Error" (fast filter)
            // This reduces the number of nodes we need to do full semantic analysis on
            return classDecl.Identifier.Text.Contains("Error", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the error type symbol if it implements IError.
        /// This does the full semantic analysis to verify it's actually an error type.
        /// </summary>
        private static INamedTypeSymbol? GetErrorTypeSymbol(GeneratorSyntaxContext context, CancellationToken ct)
        {
            var classDecl = (ClassDeclarationSyntax)context.Node;
            var symbol = context.SemanticModel.GetDeclaredSymbol(classDecl, ct);

            if (symbol == null)
                return null;

            // Verify it implements IError interface
            return ImplementsIError(symbol) ? symbol : null;
        }

        /// <summary>
        /// Checks if a type symbol implements the IError interface.
        /// </summary>
        private static bool ImplementsIError(INamedTypeSymbol symbol)
        {
            return symbol.AllInterfaces.Any(i =>
                i.ToDisplayString() == "REslava.Result.IError");
        }

        #endregion

        #region Configuration Parsing

        /// <summary>
        /// Parses the configuration from the GenerateResultExtensions attribute.
        /// </summary>
        private GeneratorConfiguration ParseConfiguration(AttributeData attribute)
        {
            var config = new GeneratorConfiguration();

            foreach (var namedArg in attribute.NamedArguments)
            {
                switch (namedArg.Key)
                {
                    case "Namespace":
                        config.Namespace = namedArg.Value.Value?.ToString() ?? config.Namespace;
                        break;
                    case "IncludeSuccessReasons":
                        config.IncludeSuccessReasons = (bool)(namedArg.Value.Value ?? false);
                        break;
                    case "UseImplicitConversions":
                        config.UseImplicitConversions = (bool)(namedArg.Value.Value ?? true);
                        break;
                    case "IncludeErrorTags":
                        config.IncludeErrorTags = (bool)(namedArg.Value.Value ?? true);
                        break;
                    case "LogErrors":
                        config.LogErrors = (bool)(namedArg.Value.Value ?? false);
                        break;
                    case "CustomErrorMappings":
                        if (namedArg.Value.Values != null)
                        {
                            config.CustomErrorMappings = namedArg.Value.Values
                                .Select(v => v.Value?.ToString())
                                .Where(v => v != null)
                                .ToArray()!;
                        }
                        break;
                    case "GenerateHttpMethodExtensions":
                        config.GenerateHttpMethodExtensions = (bool)(namedArg.Value.Value ?? true);
                        break;
                    case "DefaultErrorStatusCode":
                        config.DefaultErrorStatusCode = (int)(namedArg.Value.Value ?? 400);
                        break;
                }
            }

            return config;
        }

        #endregion

        #region Enhanced Code Generation

        /// <summary>
        /// Generates the complete extension methods source code with enhanced error mapping.
        /// This is the main code generation method that produces the final output.
        /// </summary>
        private string GenerateEnhancedExtensions(
            GeneratorConfiguration config,
            ImmutableArray<ErrorMetadata?> explicitMappings,
            ImmutableArray<INamedTypeSymbol?> conventionTypes)
        {
            var sb = new StringBuilder();

            // ═══════════════════════════════════════════════════════════════════════════
            // File Header
            // ═══════════════════════════════════════════════════════════════════════════
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("// This file was automatically generated by REslava.Result.SourceGenerators");
            sb.AppendLine($"// Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine("//");
            sb.AppendLine("// Features enabled:");
            sb.AppendLine($"//   - Explicit mappings: {explicitMappings.Count(m => m != null)} error types");
            sb.AppendLine($"//   - Convention-based: {conventionTypes.Count(t => t != null)} error types");
            sb.AppendLine($"//   - HTTP method extensions: {config.GenerateHttpMethodExtensions}");
            sb.AppendLine($"//   - Error tags included: {config.IncludeErrorTags}");
            sb.AppendLine();
            sb.AppendLine("#nullable enable");
            sb.AppendLine();

            // ═══════════════════════════════════════════════════════════════════════════
            // Usings
            // ═══════════════════════════════════════════════════════════════════════════
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using Microsoft.AspNetCore.Http;");
            sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
            sb.AppendLine("using REslava.Result;");
            if (config.LogErrors)
            {
                sb.AppendLine("using Microsoft.Extensions.Logging;");
            }
            sb.AppendLine();

            // ═══════════════════════════════════════════════════════════════════════════
            // Namespace
            // ═══════════════════════════════════════════════════════════════════════════
            sb.AppendLine($"namespace {config.Namespace}");
            sb.AppendLine("{");

            // ═══════════════════════════════════════════════════════════════════════════
            // Build error mappings (merge explicit + convention)
            // ═══════════════════════════════════════════════════════════════════════════
            var errorMappings = BuildErrorMappings(explicitMappings, conventionTypes, config);

            // ═══════════════════════════════════════════════════════════════════════════
            // Main extension class
            // ═══════════════════════════════════════════════════════════════════════════
            GenerateMainExtensionClass(sb, config, errorMappings);

            sb.AppendLine("}"); // Close namespace

            return sb.ToString();
        }

        /// <summary>
        /// Builds the complete error mappings by merging explicit attributes and conventions.
        /// Priority: Explicit attributes > Custom mappings > Conventions > Default
        /// </summary>
        private Dictionary<string, ErrorMetadata> BuildErrorMappings(
            ImmutableArray<ErrorMetadata?> explicitMappings,
            ImmutableArray<INamedTypeSymbol?> conventionTypes,
            GeneratorConfiguration config)
        {
            var mappings = new Dictionary<string, ErrorMetadata>(StringComparer.OrdinalIgnoreCase);

            // ═══════════════════════════════════════════════════════════════════════════
            // Priority 1: Explicit [MapToProblemDetails] attributes (HIGHEST PRIORITY)
            // ═══════════════════════════════════════════════════════════════════════════
            // These are explicitly defined by the developer and take precedence
            foreach (var mapping in explicitMappings.Where(m => m != null))
            {
                mappings[mapping!.TypeName] = mapping;
            }

            // ═══════════════════════════════════════════════════════════════════════════
            // Priority 2: Custom error mappings from configuration
            // ═══════════════════════════════════════════════════════════════════════════
            // Example: CustomErrorMappings = new[] { "PaymentRequiredError:402" }
            if (config.CustomErrorMappings != null)
            {
                foreach (var customMapping in config.CustomErrorMappings)
                {
                    var parts = customMapping.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[1], out var statusCode))
                    {
                        var errorName = parts[0];
                        if (!mappings.ContainsKey(errorName))
                        {
                            mappings[errorName] = new ErrorMetadata
                            {
                                TypeName = errorName,
                                StatusCode = statusCode,
                                Type = $"https://httpstatuses.io/{statusCode}",
                                Title = GetDefaultTitle(statusCode),
                                Source = "CustomMapping"
                            };
                        }
                    }
                }
            }

            // ═══════════════════════════════════════════════════════════════════════════
            // Priority 3: Convention-based mapping (LOWEST PRIORITY)
            // ═══════════════════════════════════════════════════════════════════════════
            // Use naming conventions to guess the appropriate HTTP status code
            // Example: NotFoundError → 404, ValidationError → 422
            foreach (var errorType in conventionTypes.Where(t => t != null))
            {
                if (!mappings.ContainsKey(errorType!.Name))
                {
                    mappings[errorType.Name] = ConventionMatcher.Match(errorType);
                }
            }

            return mappings;
        }

        /// <summary>
        /// Generates the main ResultToIResultExtensions class with all extension methods.
        /// </summary>
        private void GenerateMainExtensionClass(
            StringBuilder sb,
            GeneratorConfiguration config,
            Dictionary<string, ErrorMetadata> errorMappings)
        {
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Extension methods for converting Result&lt;T&gt; to Microsoft.AspNetCore.Http.IResult.");
            sb.AppendLine("    /// Auto-generated by REslava.Result.SourceGenerators with enhanced error mapping.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static class ResultToIResultExtensions");
            sb.AppendLine("    {");

            // ═══════════════════════════════════════════════════════════════════════════
            // Core ToIResult methods
            // ═══════════════════════════════════════════════════════════════════════════
            GenerateToIResultMethod(sb, config);
            sb.AppendLine();
            GenerateToIResultNonGenericMethod(sb, config);

            // ═══════════════════════════════════════════════════════════════════════════
            // Helper methods (with enhanced mapping)
            // ═══════════════════════════════════════════════════════════════════════════
            sb.AppendLine();
            GenerateEnhancedCreateProblemDetailsMethod(sb, config);
            sb.AppendLine();
            GenerateEnhancedDetermineStatusCodeMethod(sb, config, errorMappings);

            // ═══════════════════════════════════════════════════════════════════════════
            // HTTP method-specific extensions
            // ═══════════════════════════════════════════════════════════════════════════
            if (config.GenerateHttpMethodExtensions)
            {
                sb.AppendLine();
                GenerateHttpMethodExtensions(sb, config);
            }

            sb.AppendLine("    }"); // Close class
        }

        #endregion

        #region Core Extension Methods Generation

        /// <summary>
        /// Generates the ToIResult&lt;T&gt; extension method.
        /// </summary>
        private void GenerateToIResultMethod(StringBuilder sb, GeneratorConfiguration config)
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Converts a Result&lt;T&gt; to Microsoft.AspNetCore.Http.IResult for use in Minimal API endpoints.");
            sb.AppendLine("        /// Success returns 200 OK with the value, failure returns appropriate error response.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <typeparam name=\"T\">The type of the result value.</typeparam>");
            sb.AppendLine("        /// <param name=\"result\">The result to convert.</param>");
            sb.AppendLine("        /// <returns>A Microsoft.AspNetCore.Http.IResult representing the operation outcome.</returns>");
            sb.AppendLine("        public static Microsoft.AspNetCore.Http.IResult ToIResult<T>(this Result<T> result)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (result.IsSuccess)");
            sb.AppendLine("            {");
            sb.AppendLine("                return Results.Ok(result.Value);");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            var statusCode = DetermineStatusCode(result.Errors);");
            sb.AppendLine("            var problemDetails = CreateProblemDetails(result.Errors, statusCode, null);");
            sb.AppendLine();
            sb.AppendLine("            return Results.Problem(");
            sb.AppendLine("                detail: problemDetails.Detail,");
            sb.AppendLine("                instance: problemDetails.Instance,");
            sb.AppendLine("                statusCode: problemDetails.Status,");
            sb.AppendLine("                title: problemDetails.Title,");
            sb.AppendLine("                type: problemDetails.Type,");
            sb.AppendLine("                extensions: problemDetails.Extensions);");
            sb.AppendLine("        }");
        }

        /// <summary>
        /// Generates the non-generic ToIResult method.
        /// </summary>
        private void GenerateToIResultNonGenericMethod(StringBuilder sb, GeneratorConfiguration config)
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Converts a non-generic Result to Microsoft.AspNetCore.Http.IResult for operations without return values.");
            sb.AppendLine("        /// Success returns 204 No Content, failure returns appropriate error response.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"result\">The result to convert.</param>");
            sb.AppendLine("        /// <returns>A Microsoft.AspNetCore.Http.IResult representing the operation outcome.</returns>");
            sb.AppendLine("        public static Microsoft.AspNetCore.Http.IResult ToIResult(this Result result)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (result.IsSuccess)");
            sb.AppendLine("            {");
            sb.AppendLine("                return Results.NoContent();");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            var statusCode = DetermineStatusCode(result.Errors);");
            sb.AppendLine("            var problemDetails = CreateProblemDetails(result.Errors, statusCode, null);");
            sb.AppendLine();
            sb.AppendLine("            return Results.Problem(");
            sb.AppendLine("                detail: problemDetails.Detail,");
            sb.AppendLine("                instance: problemDetails.Instance,");
            sb.AppendLine("                statusCode: problemDetails.Status,");
            sb.AppendLine("                title: problemDetails.Title,");
            sb.AppendLine("                type: problemDetails.Type,");
            sb.AppendLine("                extensions: problemDetails.Extensions);");
            sb.AppendLine("        }");
        }

        #endregion

        #region Enhanced Helper Methods

        /// <summary>
        /// Generates the enhanced CreateProblemDetails helper method.
        /// </summary>
        private void GenerateEnhancedCreateProblemDetailsMethod(StringBuilder sb, GeneratorConfiguration config)
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Creates an RFC 7807 compliant ProblemDetails object from errors.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        private static ProblemDetails CreateProblemDetails(");
            sb.AppendLine("            System.Collections.Immutable.ImmutableList<IError> errors,");
            sb.AppendLine("            int statusCode,");
            sb.AppendLine("            string? instance)");
            sb.AppendLine("        {");
            sb.AppendLine("            var title = statusCode switch");
            sb.AppendLine("            {");
            sb.AppendLine("                400 => \"Bad Request\",");
            sb.AppendLine("                401 => \"Unauthorized\",");
            sb.AppendLine("                403 => \"Forbidden\",");
            sb.AppendLine("                404 => \"Not Found\",");
            sb.AppendLine("                408 => \"Request Timeout\",");
            sb.AppendLine("                409 => \"Conflict\",");
            sb.AppendLine("                422 => \"Unprocessable Entity\",");
            sb.AppendLine("                429 => \"Too Many Requests\",");
            sb.AppendLine("                500 => \"Internal Server Error\",");
            sb.AppendLine("                503 => \"Service Unavailable\",");
            sb.AppendLine("                _ => \"An error occurred\"");
            sb.AppendLine("            };");
            sb.AppendLine();
            sb.AppendLine("            var problemDetails = new ProblemDetails");
            sb.AppendLine("            {");
            sb.AppendLine("                Status = statusCode,");
            sb.AppendLine("                Title = title,");
            sb.AppendLine("                Type = $\"https://httpstatuses.io/{statusCode}\",");
            sb.AppendLine("                Instance = instance");
            sb.AppendLine("            };");
            sb.AppendLine();
            sb.AppendLine("            // Set detail from errors");
            sb.AppendLine("            if (errors.Count == 1)");
            sb.AppendLine("            {");
            sb.AppendLine("                problemDetails.Detail = errors[0].Message;");
            sb.AppendLine("            }");
            sb.AppendLine("            else if (errors.Count > 1)");
            sb.AppendLine("            {");
            sb.AppendLine("                problemDetails.Detail = $\"{errors.Count} errors occurred\";");
            sb.AppendLine("                problemDetails.Extensions[\"errors\"] = errors.Select(e => e.Message).ToArray();");
            sb.AppendLine("            }");
            sb.AppendLine();

            if (config.IncludeErrorTags)
            {
                sb.AppendLine("            // Include error tags for rich context");
                sb.AppendLine("            var allTags = errors");
                sb.AppendLine("                .Where(e => e.Tags?.Any() == true)");
                sb.AppendLine("                .SelectMany(e => e.Tags)");
                sb.AppendLine("                .GroupBy(t => t.Key)");
                sb.AppendLine("                .ToDictionary(g => g.Key, g => g.First().Value);");
                sb.AppendLine();
                sb.AppendLine("            if (allTags.Any())");
                sb.AppendLine("            {");
                sb.AppendLine("                problemDetails.Extensions[\"context\"] = allTags;");
                sb.AppendLine("            }");
                sb.AppendLine();
            }

            sb.AppendLine("            return problemDetails;");
            sb.AppendLine("        }");
        }

        /// <summary>
        /// Generates the ENHANCED DetermineStatusCode method with discovered error mappings.
        /// This is where the magic happens - it uses compile-time discovered metadata!
        /// </summary>
        private void GenerateEnhancedDetermineStatusCodeMethod(
            StringBuilder sb,
            GeneratorConfiguration config,
            Dictionary<string, ErrorMetadata> errorMappings)
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Determines the appropriate HTTP status code based on error types.");
            sb.AppendLine("        /// Uses compile-time discovered error mappings for intelligent status code selection.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        private static int DetermineStatusCode(System.Collections.Immutable.ImmutableList<IError> errors)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (!errors.Any())");
            sb.AppendLine($"                return {config.DefaultErrorStatusCode};");
            sb.AppendLine();
            sb.AppendLine("            var firstError = errors[0];");
            sb.AppendLine("            var errorType = firstError.GetType().Name;");
            sb.AppendLine();

            // ═══════════════════════════════════════════════════════════════════════════
            // Generate switch statement with discovered error mappings
            // ═══════════════════════════════════════════════════════════════════════════
            if (errorMappings.Any())
            {
                sb.AppendLine("            // Compile-time discovered error mappings");
                sb.AppendLine("            switch (errorType)");
                sb.AppendLine("            {");

                // Sort by source priority: Attribute > CustomMapping > Convention
                var sortedMappings = errorMappings
                    .OrderBy(m => m.Value.Source == "Attribute" ? 0 :
                                  m.Value.Source == "CustomMapping" ? 1 : 2)
                    .ThenBy(m => m.Key);

                foreach (var mapping in sortedMappings)
                {
                    sb.AppendLine($"                case \"{mapping.Key}\":");
                    sb.AppendLine($"                    return {mapping.Value.StatusCode}; // {mapping.Value.Source}: {mapping.Value.Title}");
                }

                sb.AppendLine("            }");
                sb.AppendLine();
            }

            // ═══════════════════════════════════════════════════════════════════════════
            // Fallback: Pattern-based detection for unmapped errors
            // ═══════════════════════════════════════════════════════════════════════════
            sb.AppendLine("            // Fallback: Pattern-based detection for unmapped errors");
            sb.AppendLine("            var message = firstError.Message.ToLowerInvariant();");
            sb.AppendLine();
            sb.AppendLine("            if (message.Contains(\"not found\") || message.Contains(\"does not exist\"))");
            sb.AppendLine("                return 404;");
            sb.AppendLine();
            sb.AppendLine("            if (message.Contains(\"already exists\") || message.Contains(\"duplicate\"))");
            sb.AppendLine("                return 409;");
            sb.AppendLine();
            sb.AppendLine("            if (message.Contains(\"validation\") || message.Contains(\"invalid\"))");
            sb.AppendLine("                return 422;");
            sb.AppendLine();
            sb.AppendLine("            if (message.Contains(\"unauthorized\") || message.Contains(\"not authorized\"))");
            sb.AppendLine("                return 401;");
            sb.AppendLine();
            sb.AppendLine("            if (message.Contains(\"forbidden\") || message.Contains(\"access denied\"))");
            sb.AppendLine("                return 403;");
            sb.AppendLine();
            sb.AppendLine($"            return {config.DefaultErrorStatusCode};");
            sb.AppendLine("        }");
        }

        #endregion

        #region HTTP Method Extensions

        /// <summary>
        /// Generates HTTP method-specific extension methods.
        /// </summary>
        private void GenerateHttpMethodExtensions(StringBuilder sb, GeneratorConfiguration config)
        {
            // GET
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Converts Result&lt;T&gt; to Microsoft.AspNetCore.Http.IResult optimized for GET operations.");
            sb.AppendLine("        /// Returns 200 OK on success, 404 Not Found for not found errors, 400 Bad Request otherwise.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static Microsoft.AspNetCore.Http.IResult ToGetResult<T>(this Result<T> result)");
            sb.AppendLine("        {");
            sb.AppendLine("            return result.ToIResult();");
            sb.AppendLine("        }");
            sb.AppendLine();

            // POST
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Converts Result&lt;T&gt; to Microsoft.AspNetCore.Http.IResult optimized for POST operations.");
            sb.AppendLine("        /// Returns 201 Created with location on success, appropriate error codes on failure.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"result\">The result to convert.</param>");
            sb.AppendLine("        /// <param name=\"locationFactory\">Factory to create location URI from the created resource.</param>");
            sb.AppendLine("        public static Microsoft.AspNetCore.Http.IResult ToPostResult<T>(this Result<T> result, Func<T, string>? locationFactory = null)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (result.IsSuccess)");
            sb.AppendLine("            {");
            sb.AppendLine("                var location = locationFactory?.Invoke(result.Value!);");
            sb.AppendLine("                return location != null");
            sb.AppendLine("                    ? Results.Created(location, result.Value)");
            sb.AppendLine("                    : Results.Ok(result.Value);");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            return result.ToIResult();");
            sb.AppendLine("        }");
            sb.AppendLine();

            // PUT
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Converts Result&lt;T&gt; to Microsoft.AspNetCore.Http.IResult optimized for PUT operations.");
            sb.AppendLine("        /// Returns 200 OK on success, 404 Not Found if resource doesn't exist.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static Microsoft.AspNetCore.Http.IResult ToPutResult<T>(this Result<T> result)");
            sb.AppendLine("        {");
            sb.AppendLine("            return result.ToIResult();");
            sb.AppendLine("        }");
            sb.AppendLine();

            // DELETE
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Converts non-generic Result to Microsoft.AspNetCore.Http.IResult optimized for DELETE operations.");
            sb.AppendLine("        /// Returns 204 No Content on success, 404 Not Found if resource doesn't exist.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static Microsoft.AspNetCore.Http.IResult ToDeleteResult(this Result result)");
            sb.AppendLine("        {");
            sb.AppendLine("            return result.ToIResult();");
            sb.AppendLine("        }");
            sb.AppendLine();

            // PATCH
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Converts Result&lt;T&gt; to Microsoft.AspNetCore.Http.IResult optimized for PATCH operations.");
            sb.AppendLine("        /// Returns 200 OK on success, 404 Not Found if resource doesn't exist, 422 for validation errors.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static Microsoft.AspNetCore.Http.IResult ToPatchResult<T>(this Result<T> result)");
            sb.AppendLine("        {");
            sb.AppendLine("            return result.ToIResult();");
            sb.AppendLine("        }");
        }

        #endregion

        #region Attribute Source Code

        /// <summary>
        /// Returns the source code for the GenerateResultExtensions attribute.
        /// </summary>
        private string GetGenerateResultExtensionsAttributeSource()
        {
            return @"
using System;

namespace REslava.Result.SourceGenerators
{
    /// <summary>
    /// Enables automatic generation of extension methods for converting Result&lt;T&gt; to Microsoft.AspNetCore.Http.IResult.
    /// Apply this assembly-level attribute to projects that want to use the source generator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class GenerateResultExtensionsAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the namespace for the generated extension methods.
        /// Default is ""Generated.ResultExtensions"".
        /// </summary>
        public string Namespace { get; set; } = ""Generated.ResultExtensions"";

        /// <summary>
        /// Gets or sets a value indicating whether to include error tags in ProblemDetails.
        /// Default is true.
        /// </summary>
        public bool IncludeErrorTags { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to log errors during conversion.
        /// Default is false.
        /// </summary>
        public bool LogErrors { get; set; } = false;

        /// <summary>
        /// Gets or sets custom error type to HTTP status code mappings.
        /// Format: ""ErrorType:StatusCode"" (e.g., ""PaymentRequiredError:402"")
        /// </summary>
        public string[] CustomErrorMappings { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets a value indicating whether to generate HTTP method-specific extension methods.
        /// Default is true.
        /// </summary>
        public bool GenerateHttpMethodExtensions { get; set; } = true;

        /// <summary>
        /// Gets or sets the default HTTP status code for errors that don't have a specific mapping.
        /// Default is 400 (Bad Request).
        /// </summary>
        public int DefaultErrorStatusCode { get; set; } = 400;
    }
}
";
        }

        /// <summary>
        /// Returns the source code for the MapToProblemDetails attribute (NEW!).
        /// This allows users to explicitly map error types to HTTP status codes.
        /// </summary>
        private string GetMapToProblemDetailsAttributeSource()
        {
            return @"
using System;

namespace REslava.Result.SourceGenerators
{
    /// <summary>
    /// Maps an IError type to RFC 7807 ProblemDetails metadata for HTTP responses.
    /// Use this attribute to explicitly control how an error type is converted to HTTP status codes.
    /// </summary>
    /// <example>
    /// <code>
    /// [MapToProblemDetails(
    ///     StatusCode = 404,
    ///     Type = ""https://api.example.com/errors/user-not-found"",
    ///     Title = ""User Not Found"")]
    /// public class UserNotFoundError : Error
    /// {
    ///     public UserNotFoundError(int userId) 
    ///         : base($""User {userId} not found"")
    ///     {
    ///         this.WithTag(""UserId"", userId);
    ///     }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class MapToProblemDetailsAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the HTTP status code (e.g., 404, 422, 500).
        /// Default is 400 (Bad Request).
        /// </summary>
        public int StatusCode { get; set; } = 400;

        /// <summary>
        /// Gets or sets the RFC 7807 type URI (e.g., ""https://api.example.com/errors/not-found"").
        /// If not specified, defaults to ""https://httpstatuses.io/{StatusCode}"".
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the human-readable title for this error type.
        /// If not specified, uses HTTP status text (e.g., ""Not Found"" for 404).
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets whether to include error tags in ProblemDetails.Extensions.
        /// Default is true.
        /// </summary>
        public bool IncludeTags { get; set; } = true;
    }
}
";
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets the default title text for an HTTP status code.
        /// </summary>
        private static string GetDefaultTitle(int statusCode)
        {
            return statusCode switch
            {
                400 => "Bad Request",
                401 => "Unauthorized",
                402 => "Payment Required",
                403 => "Forbidden",
                404 => "Not Found",
                408 => "Request Timeout",
                409 => "Conflict",
                422 => "Unprocessable Entity",
                429 => "Too Many Requests",
                500 => "Internal Server Error",
                503 => "Service Unavailable",
                _ => "Error"
            };
        }

        #endregion

        #region Supporting Classes

        /// <summary>
        /// Configuration parsed from the GenerateResultExtensions attribute.
        /// </summary>
        private class GeneratorConfiguration
        {
            public string Namespace { get; set; } = "Generated.ResultExtensions";
            public bool IncludeSuccessReasons { get; set; } = false;
            public bool UseImplicitConversions { get; set; } = true;
            public bool IncludeErrorTags { get; set; } = true;
            public bool LogErrors { get; set; } = false;
            public string[] CustomErrorMappings { get; set; } = Array.Empty<string>();
            public bool GenerateHttpMethodExtensions { get; set; } = true;
            public int DefaultErrorStatusCode { get; set; } = 400;
        }

        /// <summary>
        /// Metadata about an error type discovered at compile-time.
        /// This represents how an error should be mapped to HTTP responses.
        /// </summary>
        private class ErrorMetadata
        {
            /// <summary>Simple type name (e.g., "NotFoundError")</summary>
            public string TypeName { get; set; } = string.Empty;

            /// <summary>Fully qualified name (e.g., "MyApp.Errors.NotFoundError")</summary>
            public string FullName { get; set; } = string.Empty;

            /// <summary>HTTP status code (e.g., 404, 422, 500)</summary>
            public int StatusCode { get; set; } = 400;

            /// <summary>RFC 7807 type URI</summary>
            public string Type { get; set; } = "https://httpstatuses.io/400";

            /// <summary>Human-readable title</summary>
            public string Title { get; set; } = "Bad Request";

            /// <summary>Whether to include error tags in response</summary>
            public bool IncludeTags { get; set; } = true;

            /// <summary>How this mapping was determined: "Attribute", "CustomMapping", or "Convention"</summary>
            public string Source { get; set; } = "Unknown";
        }

        /// <summary>
        /// Matches error types to HTTP status codes using naming conventions.
        /// This provides sensible defaults without requiring attributes.
        /// </summary>
        private static class ConventionMatcher
        {
            /// <summary>
            /// Determines the appropriate HTTP status code based on error type name.
            /// Uses naming patterns to intelligently guess the right status code.
            /// </summary>
            public static ErrorMetadata Match(INamedTypeSymbol symbol)
            {
                var name = symbol.Name.ToLowerInvariant();
                var statusCode = DetermineStatusCode(name);

                return new ErrorMetadata
                {
                    TypeName = symbol.Name,
                    FullName = symbol.ToDisplayString(),
                    StatusCode = statusCode,
                    Type = $"https://httpstatuses.io/{statusCode}",
                    Title = GetDefaultTitle(statusCode),
                    Source = "Convention"
                };
            }

            /// <summary>
            /// Determines HTTP status code from error type name using pattern matching.
            /// Priority order: Specific patterns → Generic patterns → Default
            /// </summary>
            private static int DetermineStatusCode(string errorTypeName)
            {
                // ═══════════════════════════════════════════════════════════════════════
                // NOT FOUND (404) - Resource doesn't exist
                // ═══════════════════════════════════════════════════════════════════════
                if (ContainsAny(errorTypeName,
                    "notfound", "doesnotexist", "missing", "nosuch", "notexist"))
                    return 404;

                // ═══════════════════════════════════════════════════════════════════════
                // CONFLICT (409) - Resource already exists or state conflict
                // ═══════════════════════════════════════════════════════════════════════
                if (ContainsAny(errorTypeName,
                    "conflict", "duplicate", "alreadyexists", "exists", "duplicated"))
                    return 409;

                // ═══════════════════════════════════════════════════════════════════════
                // VALIDATION (422) - Invalid input that violates business rules
                // ═══════════════════════════════════════════════════════════════════════
                if (ContainsAny(errorTypeName,
                    "validation", "invalid", "malformed", "badformat", "invalidformat"))
                    return 422;

                // ═══════════════════════════════════════════════════════════════════════
                // UNAUTHORIZED (401) - Authentication required or failed
                // ═══════════════════════════════════════════════════════════════════════
                if (ContainsAny(errorTypeName,
                    "unauthorized", "unauthenticated", "notauthenticated", "authfailed"))
                    return 401;

                // ═══════════════════════════════════════════════════════════════════════
                // FORBIDDEN (403) - Authenticated but not authorized
                // ═══════════════════════════════════════════════════════════════════════
                if (ContainsAny(errorTypeName,
                    "forbidden", "accessdenied", "notauthorized", "denied", "nopermission"))
                    return 403;

                // ═══════════════════════════════════════════════════════════════════════
                // RATE LIMIT (429) - Too many requests
                // ═══════════════════════════════════════════════════════════════════════
                if (ContainsAny(errorTypeName,
                    "ratelimit", "throttle", "toomanyrequests", "limited", "throttled"))
                    return 429;

                // ═══════════════════════════════════════════════════════════════════════
                // TIMEOUT (408) - Request took too long
                // ═══════════════════════════════════════════════════════════════════════
                if (ContainsAny(errorTypeName,
                    "timeout", "timedout", "expired", "deadline"))
                    return 408;

                // ═══════════════════════════════════════════════════════════════════════
                // SERVER ERROR (500) - Internal server issues
                // ═══════════════════════════════════════════════════════════════════════
                if (ContainsAny(errorTypeName,
                    "servererror", "internalerror", "systemerror", "critical", "fatal"))
                    return 500;

                // ═══════════════════════════════════════════════════════════════════════
                // SERVICE UNAVAILABLE (503) - Service temporarily down
                // ═══════════════════════════════════════════════════════════════════════
                if (ContainsAny(errorTypeName,
                    "unavailable", "serviceunavailable", "maintenance", "down"))
                    return 503;

                // ═══════════════════════════════════════════════════════════════════════
                // Default: Bad Request (400) - Generic client error
                // ═══════════════════════════════════════════════════════════════════════
                return 400;
            }

            /// <summary>
            /// Helper method to check if a string contains any of the given keywords.
            /// </summary>
            private static bool ContainsAny(string text, params string[] keywords)
            {
                foreach (var keyword in keywords)
                {
                    if (text.Contains(keyword))
                        return true;
                }
                return false;
            }
        }

        #endregion
    }
}
