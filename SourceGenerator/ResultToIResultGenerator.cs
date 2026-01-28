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
    /// Source generator that creates extension methods for converting Result&lt;T&gt; to IResult.
    /// This generator implements IIncrementalGenerator for optimal performance and caching.
    /// </summary>
    [Generator]
    public class ResultToIResultGenerator : IIncrementalGenerator
    {
        private const string AttributeName = "REslava.Result.SourceGenerators.GenerateResultExtensionsAttribute";
        private const string AttributeShortName = "GenerateResultExtensions";

        /// <summary>
        /// Initializes the incremental generator pipeline.
        /// </summary>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Debug: Log that generator is being initialized
            System.Diagnostics.Debug.WriteLine("🚀 ResultToIResultGenerator.Initialize() called!");
            
            // Find assemblies with [GenerateResultExtensions] attribute
            var assemblyAttributes = context.CompilationProvider
                .SelectMany((compilation, cancellationToken) =>
                {
                    try
                    {
                        // Debug: Log compilation info
                        System.Diagnostics.Debug.WriteLine($"🔍 Checking assembly: {compilation.AssemblyName}");
                        
                        var attributes = compilation.Assembly.GetAttributes();
                        System.Diagnostics.Debug.WriteLine($"📋 Found {attributes.Length} attributes in assembly");
                        
                        var targetAttribute = attributes.FirstOrDefault(a =>
                            a.AttributeClass?.ToDisplayString() == AttributeName ||
                            a.AttributeClass?.Name == AttributeShortName);

                        if (targetAttribute == null)
                        {
                            // Debug: Log that no attribute was found
                            System.Diagnostics.Debug.WriteLine($"❌ No {AttributeName} attribute found in assembly {compilation.AssemblyName}");
                            return ImmutableArray<(Compilation, GeneratorConfiguration)>.Empty;
                        }

                        // Debug: Log that attribute was found
                        System.Diagnostics.Debug.WriteLine($"✅ Found {AttributeName} attribute in assembly {compilation.AssemblyName}");

                        // Parse configuration from attribute
                        var config = ParseConfiguration(targetAttribute);
                        return ImmutableArray.Create((compilation, config));
                    }
                    catch (Exception ex)
                    {
                        // Debug: Log the exception
                        System.Diagnostics.Debug.WriteLine($"💥 Exception in attribute detection: {ex.Message}");
                        return ImmutableArray<(Compilation, GeneratorConfiguration)>.Empty;
                    }
                });

            // Generate the extension methods
            context.RegisterSourceOutput(assemblyAttributes, (spc, data) =>
            {
                try
                {
                    var compilation = data.Item1;
                    var config = data.Item2;
                    var source = GenerateExtensionMethods(config);
                    
                    // Debug: Log that source is being generated
                    System.Diagnostics.Debug.WriteLine($"Generating source code with {config.GenerateHttpMethodExtensions} extensions");
                    
                    spc.AddSource("ResultToIResultExtensions.g.cs", 
                        SourceText.From(source, Encoding.UTF8));
                }
                catch (Exception ex)
                {
                    // Debug: Log the exception
                    System.Diagnostics.Debug.WriteLine($"Exception in source generation: {ex.Message}");
                    // Don't fail the build
                }
            });
        }

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
                                .Select(v => v.Value?.ToString() ?? string.Empty)
                                .Where(v => !string.IsNullOrEmpty(v))
                                .ToArray();
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

        /// <summary>
        /// Generates the extension methods source code.
        /// </summary>
        private string GenerateExtensionMethods(GeneratorConfiguration config)
        {
            var sb = new StringBuilder();

            // File header
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("// This file was automatically generated by REslava.Result.SourceGenerators");
            sb.AppendLine($"// Configuration: Namespace={config.Namespace}, IncludeErrorTags={config.IncludeErrorTags}");
            sb.AppendLine();
            sb.AppendLine("#nullable enable");
            sb.AppendLine();

            // Usings
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

            // Namespace
            sb.AppendLine($"namespace {config.Namespace}");
            sb.AppendLine("{");

            // Main extension class
            GenerateMainExtensionClass(sb, config);

            sb.AppendLine("}"); // Close namespace

            return sb.ToString();
        }

        /// <summary>
        /// Generates the main ResultToIResultExtensions class.
        /// </summary>
        private void GenerateMainExtensionClass(StringBuilder sb, GeneratorConfiguration config)
        {
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Extension methods for converting Result&lt;T&gt; to Microsoft.AspNetCore.Http.IResult.");
            sb.AppendLine("    /// Auto-generated by REslava.Result.SourceGenerators.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static class ResultToIResultExtensions");
            sb.AppendLine("    {");

            // ToIResult<T> method
            GenerateToIResultMethod(sb, config);

            // ToIResult (non-generic) method
            sb.AppendLine();
            GenerateToIResultNonGenericMethod(sb, config);

            // CreateProblemDetails helper
            sb.AppendLine();
            GenerateCreateProblemDetailsMethod(sb, config);

            // DetermineStatusCode helper
            sb.AppendLine();
            GenerateDetermineStatusCodeMethod(sb, config);

            // HTTP method-specific extensions
            if (config.GenerateHttpMethodExtensions)
            {
                sb.AppendLine();
                GenerateHttpMethodExtensions(sb, config);
            }

            sb.AppendLine("    }"); // Close class
        }

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
            sb.AppendLine("        /// <remarks>");
            sb.AppendLine("        /// This method automatically:");
            sb.AppendLine("        /// <list type=\"bullet\">");
            sb.AppendLine("        /// <item>Determines appropriate HTTP status codes based on error types</item>");
            sb.AppendLine("        /// <item>Creates RFC 7807 ProblemDetails for error responses</item>");
            sb.AppendLine("        /// <item>Includes error tags in ProblemDetails.Extensions</item>");
            sb.AppendLine("        /// </list>");
            sb.AppendLine("        /// </remarks>");
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

        /// <summary>
        /// Generates the CreateProblemDetails helper method.
        /// </summary>
        private void GenerateCreateProblemDetailsMethod(StringBuilder sb, GeneratorConfiguration config)
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Creates an RFC 7807 compliant ProblemDetails object from errors.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        private static ProblemDetails CreateProblemDetails(");
            sb.AppendLine("            IReadOnlyCollection<IError> errors,");
            sb.AppendLine("            int statusCode,");
            sb.AppendLine("            string? instance)");
            sb.AppendLine("        {");
            sb.AppendLine("            var title = statusCode switch");
            sb.AppendLine("            {");
            sb.AppendLine("                400 => \"Bad Request\",");
            sb.AppendLine("                401 => \"Unauthorized\",");
            sb.AppendLine("                403 => \"Forbidden\",");
            sb.AppendLine("                404 => \"Not Found\",");
            sb.AppendLine("                409 => \"Conflict\",");
            sb.AppendLine("                422 => \"Unprocessable Entity\",");
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
            sb.AppendLine("                problemDetails.Detail = errors.First().Message;");
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
        /// Generates the DetermineStatusCode helper method.
        /// </summary>
        private void GenerateDetermineStatusCodeMethod(StringBuilder sb, GeneratorConfiguration config)
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Determines the appropriate HTTP status code based on error types and messages.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        private static int DetermineStatusCode(IReadOnlyCollection<IError> errors)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (!errors.Any())");
            sb.AppendLine($"                return {config.DefaultErrorStatusCode};");
            sb.AppendLine();
            sb.AppendLine("            var firstError = errors.First();");
            sb.AppendLine("            var errorType = firstError.GetType().Name;");
            sb.AppendLine("            var message = firstError.Message.ToLowerInvariant();");
            sb.AppendLine();

            // Custom error mappings
            if (config.CustomErrorMappings?.Length > 0)
            {
                sb.AppendLine("            // Custom error type mappings");
                sb.AppendLine("            switch (errorType)");
                sb.AppendLine("            {");
                foreach (var mapping in config.CustomErrorMappings)
                {
                    var parts = mapping.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[1], out var statusCode))
                    {
                        sb.AppendLine($"                case \"{parts[0]}\":");
                        sb.AppendLine($"                    return {statusCode};");
                    }
                }
                sb.AppendLine("            }");
                sb.AppendLine();
            }

            sb.AppendLine("            // Pattern-based detection");
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

        /// <summary>
        /// Generates HTTP method-specific extension methods.
        /// </summary>
        private void GenerateHttpMethodExtensions(StringBuilder sb, GeneratorConfiguration config)
        {
            // GET
            GenerateGetResultMethod(sb);
            sb.AppendLine();

            // POST
            GeneratePostResultMethod(sb);
            sb.AppendLine();

            // PUT
            GeneratePutResultMethod(sb);
            sb.AppendLine();

            // DELETE
            GenerateDeleteResultMethod(sb);
            sb.AppendLine();

            // PATCH
            GeneratePatchResultMethod(sb);
        }

        private void GenerateGetResultMethod(StringBuilder sb)
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Converts Result&lt;T&gt; to Microsoft.AspNetCore.Http.IResult optimized for GET operations.");
            sb.AppendLine("        /// Returns 200 OK on success, 404 Not Found for not found errors, 400 Bad Request otherwise.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static Microsoft.AspNetCore.Http.IResult ToGetResult<T>(this Result<T> result)");
            sb.AppendLine("        {");
            sb.AppendLine("            // Same as ToIResult for GET");
            sb.AppendLine("            return result.ToIResult();");
            sb.AppendLine("        }");
        }

        private void GeneratePostResultMethod(StringBuilder sb)
        {
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
            sb.AppendLine("                var location = locationFactory?.Invoke(result.Value ?? default(T)!);");
            sb.AppendLine("                return location != null");
            sb.AppendLine("                    ? Results.Created(location, result.Value)");
            sb.AppendLine("                    : Results.Ok(result.Value);");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            return result.ToIResult();");
            sb.AppendLine("        }");
        }

        private void GeneratePutResultMethod(StringBuilder sb)
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Converts Result&lt;T&gt; to Microsoft.AspNetCore.Http.IResult optimized for PUT operations.");
            sb.AppendLine("        /// Returns 200 OK on success, 404 Not Found if resource doesn't exist.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static Microsoft.AspNetCore.Http.IResult ToPutResult<T>(this Result<T> result)");
            sb.AppendLine("        {");
            sb.AppendLine("            return result.ToIResult();");
            sb.AppendLine("        }");
        }

        private void GenerateDeleteResultMethod(StringBuilder sb)
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Converts non-generic Result to Microsoft.AspNetCore.Http.IResult optimized for DELETE operations.");
            sb.AppendLine("        /// Returns 204 No Content on success, 404 Not Found if resource doesn't exist.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static Microsoft.AspNetCore.Http.IResult ToDeleteResult(this Result result)");
            sb.AppendLine("        {");
            sb.AppendLine("            return result.ToIResult();");
            sb.AppendLine("        }");
        }

        private void GeneratePatchResultMethod(StringBuilder sb)
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Converts Result&lt;T&gt; to Microsoft.AspNetCore.Http.IResult optimized for PATCH operations.");
            sb.AppendLine("        /// Returns 200 OK on success, 404 Not Found if resource doesn't exist, 422 for validation errors.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static Microsoft.AspNetCore.Http.IResult ToPatchResult<T>(this Result<T> result)");
            sb.AppendLine("        {");
            sb.AppendLine("            return result.ToIResult();");
            sb.AppendLine("        }");
        }

        /// <summary>
        /// Generates implicit conversion operators.
        /// </summary>
        private void GenerateImplicitConversions(StringBuilder sb, GeneratorConfiguration config)
        {
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Implicit conversion operators for seamless Result to IResult conversion.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static class ResultImplicitConversions");
            sb.AppendLine("    {");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Implicitly converts a non-generic Result to IResult.");
            sb.AppendLine("        /// Enables: return result; (instead of return result.ToIResult();)");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static IResult ToIResult(Result result) => result.ToIResult();");
            sb.AppendLine("    }");
        }

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
        /// Generates a helper class for performance optimizations.
        /// </summary>
        private void GenerateHelperClass(StringBuilder sb, GeneratorConfiguration config)
        {
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Helper class for optimized error processing and caching.");
            sb.AppendLine("    /// Auto-generated by REslava.Result.SourceGenerators.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    internal static class ResultHelper");
            sb.AppendLine("    {");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Cache for frequently used status codes to avoid reflection.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        private static readonly Dictionary<Type, int> StatusCodeCache = new()");
            sb.AppendLine();
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Determines the appropriate HTTP status code for the given errors.");
            sb.AppendLine("        /// Uses caching for performance optimization.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"errors\">The errors to analyze.</param>");
            sb.AppendLine("        /// <returns>The appropriate HTTP status code.</returns>");
            sb.AppendLine("        public static int DetermineStatusCode(IEnumerable<IError> errors)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (!errors.Any()) return 400;");
            sb.AppendLine();
            sb.AppendLine("            var firstError = errors.First();");
            sb.AppendLine("            var errorType = firstError.GetType();");
            sb.AppendLine();
            sb.AppendLine("            // Use cache for performance");
            sb.AppendLine("            if (StatusCodeCache.TryGetValue(errorType, out var cachedCode))");
            sb.AppendLine("                return cachedCode;");
            sb.AppendLine();
            sb.AppendLine("            // Determine status code based on error type");
            sb.AppendLine("            var statusCode = errorType.Name switch");
            sb.AppendLine("            {");
            sb.AppendLine("                var name when name.Contains(\"NotFound\") => 404,");
            sb.AppendLine("                var name when name.Contains(\"Validation\") => 422,");
            sb.AppendLine("                var name when name.Contains(\"Unauthorized\") => 401,");
            sb.AppendLine("                var name when name.Contains(\"Forbidden\") => 403,");
            sb.AppendLine("                var name when name.Contains(\"Conflict\") => 409,");
            sb.AppendLine("                _ => 400");
            sb.AppendLine("            };");
            sb.AppendLine();
            sb.AppendLine("            // Cache the result");
            sb.AppendLine("            StatusCodeCache[errorType] = statusCode;");
            sb.AppendLine("            return statusCode;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
        }
    }
}
