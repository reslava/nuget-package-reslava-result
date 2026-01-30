using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using REslava.Result.SourceGenerators.Core.Infrastructure;
using REslava.Result.SourceGenerators.Core.CodeGeneration;
using REslava.Result.SourceGenerators.Core.Utilities;
using REslava.Result.SourceGenerators.Generators.ResultToIResult;

namespace REslava.Result.SourceGenerators.Generators.ResultToIResult;

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
public class ResultToIResultGenerator : IncrementalGeneratorBase<ResultToIResultConfig>
{
    private const string MapToProblemDetailsAttributeName = "REslava.Result.SourceGenerators.MapToProblemDetailsAttribute";

    protected override string AttributeFullName =>
        "REslava.Result.SourceGenerators.GenerateResultExtensionsAttribute";

    protected override string AttributeShortName => "GenerateResultExtensions";
    protected override string GeneratedFileName => "ResultToIResultExtensions";
    protected override string AttributeSourceCode => GetGenerateResultExtensionsAttributeSource();

    protected override ResultToIResultConfig ParseConfiguration(AttributeData attribute)
    {
        var config = new ResultToIResultConfig();
        var args = attribute.NamedArguments;

        config.Namespace = args.GetStringValue("Namespace", "Generated");
        config.IncludeDiagnostics = args.GetBoolValue("IncludeDiagnostics", false);
        config.IncludeDocumentation = args.GetBoolValue("IncludeDocumentation", true);
        config.IncludeErrorTags = args.GetBoolValue("IncludeErrorTags", true);
        config.CustomErrorMappings = args.GetStringArrayValue("CustomErrorMappings") ?? Array.Empty<string>();
        config.GenerateHttpMethodExtensions = args.GetBoolValue("GenerateHttpMethodExtensions", true);
        config.DefaultErrorStatusCode = args.GetIntValue("DefaultErrorStatusCode", 400);
        config.IncludeDetailedErrors = args.GetBoolValue("IncludeDetailedErrors", false);
        config.GenerateAsyncMethods = args.GetBoolValue("GenerateAsyncMethods", true);

        return config;
    }

    protected override string GenerateCode(Compilation compilation, ResultToIResultConfig config)
    {
        var builder = new CodeBuilder();
        var mapper = new HttpStatusCodeMapper(config.DefaultErrorStatusCode);
        
        // Add custom mappings if provided
        if (config.CustomErrorMappings.Length > 0)
        {
            mapper.AddMappings(config.CustomErrorMappings);
        }

        // Generate file header and usings
        builder.AppendFileHeader("ResultToIResultGenerator")
            .AppendUsings(
                "System",
                "System.Collections.Generic",
                "System.Linq",
                "Microsoft.AspNetCore.Http",
                "Microsoft.AspNetCore.Mvc",
                "REslava.Result")
            .AppendNamespace(config.Namespace);

        // Generate main extension class
        GenerateMainExtensionClass(builder, config, mapper);

        // Generate HTTP method extensions if enabled
        if (config.GenerateHttpMethodExtensions)
        {
            builder.BlankLine();
            GenerateHttpMethodExtensions(builder, config);
        }

        // Generate async methods if enabled
        if (config.GenerateAsyncMethods)
        {
            builder.BlankLine();
            GenerateAsyncExtensions(builder, config);
        }

        builder.CloseNamespace();
        return builder.ToString();
    }

    private void GenerateMainExtensionClass(
        CodeBuilder builder,
        ResultToIResultConfig config,
        HttpStatusCodeMapper mapper)
    {
        builder.AppendXmlSummary("Extension methods for converting Result<T> to IResult.")
            .AppendClassDeclaration("ResultToIResultExtensions", "public", "static");

        // Generate ToIResult method
        builder.AppendXmlSummary("Converts Result<T> to IResult.")
            .AppendMethodDeclaration(
                "ToIResult",
                "IResult",
                "this Result<T> result",
                "T",
                "public", "static")
            .AppendLine("if (result.IsSuccess)")
            .OpenBrace()
            .AppendLine("return Results.Ok(result.Value);")
            .CloseBrace()
            .BlankLine()
            .AppendLine("var statusCode = DetermineStatusCode(result.Errors);")
            .AppendLine("var problemDetails = CreateProblemDetails(result.Errors, statusCode);")
            .AppendLine("return Results.Problem(problemDetails);")
            .CloseBrace(); // method

        // Generate helper methods inline
        builder.BlankLine()
            .AppendXmlSummary("Determines the appropriate HTTP status code based on error types.")
            .AppendMethodDeclaration(
                "DetermineStatusCode",
                "int",
                "System.Collections.Generic.IReadOnlyList<IError> errors",
                null,
                "private", "static")
            .AppendLine("if (!errors.Any())")
            .OpenBrace()
            .AppendLine($"return {config.DefaultErrorStatusCode};")
            .CloseBrace()
            .BlankLine()
            .AppendLine("// Check for explicit mappings first")
            .AppendLine("foreach (var error in errors)")
            .OpenBrace()
            .AppendLine("var errorType = error.GetType();")
            .AppendLine("var statusCode = DetermineStatusCodeFromType(errorType.Name);")
            .AppendLine("if (statusCode != 0)")
            .OpenBrace()
            .AppendLine("return statusCode;")
            .CloseBrace()
            .CloseBrace()
            .BlankLine()
            .AppendLine($"return {config.DefaultErrorStatusCode};")
            .CloseBrace(); // method

        builder.BlankLine()
            .AppendXmlSummary("Determines HTTP status code from error type name using conventions.")
            .AppendMethodDeclaration(
                "DetermineStatusCodeFromType",
                "int",
                "string errorTypeName",
                null,
                "private", "static")
            .AppendLine("// Convention-based mapping")
            .AppendLine("if (errorTypeName.Contains(\"NotFound\", StringComparison.OrdinalIgnoreCase))")
            .OpenBrace()
            .AppendLine("return 404;")
            .CloseBrace()
            .AppendLine("if (errorTypeName.Contains(\"Validation\", StringComparison.OrdinalIgnoreCase))")
            .OpenBrace()
            .AppendLine("return 422;")
            .CloseBrace()
            .AppendLine("if (errorTypeName.Contains(\"Unauthorized\", StringComparison.OrdinalIgnoreCase))")
            .OpenBrace()
            .AppendLine("return 401;")
            .CloseBrace()
            .AppendLine("if (errorTypeName.Contains(\"Forbidden\", StringComparison.OrdinalIgnoreCase))")
            .OpenBrace()
            .AppendLine("return 403;")
            .CloseBrace()
            .AppendLine("if (errorTypeName.Contains(\"Conflict\", StringComparison.OrdinalIgnoreCase))")
            .OpenBrace()
            .AppendLine("return 409;")
            .CloseBrace()
            .AppendLine("return 0; // Not found by convention")
            .CloseBrace(); // method

        builder.BlankLine()
            .AppendXmlSummary("Creates RFC 7807 ProblemDetails from errors.")
            .AppendMethodDeclaration(
                "CreateProblemDetails",
                "ProblemDetails",
                "System.Collections.Generic.IReadOnlyList<IError> errors, int statusCode",
                null,
                "private", "static")
            .AppendLine("var problemDetails = new ProblemDetails")
            .OpenBrace()
            .AppendLine("Status = statusCode,")
            .AppendLine("Title = GetDefaultTitle(statusCode),")
            .AppendLine("Type = $\"https://httpstatuses.com/{statusCode}\"")
            .CloseBrace()
            .AppendLine(";")
            .BlankLine()
            .AppendLine("if (errors.Count == 1)")
            .OpenBrace()
            .AppendLine("problemDetails.Detail = errors[0].Message;")
            .CloseBrace()
            .AppendLine("else")
            .OpenBrace()
            .AppendLine("problemDetails.Detail = errors.Count + \" errors occurred.\";")
            .CloseBrace()
            .BlankLine()
            .AppendLine("return problemDetails;")
            .CloseBrace(); // method

        builder.BlankLine()
            .AppendXmlSummary("Gets default title for HTTP status code.")
            .AppendMethodDeclaration(
                "GetDefaultTitle",
                "string",
                "int statusCode",
                null,
                "private", "static")
            .AppendLine("return statusCode switch")
            .OpenBrace()
            .AppendLine("400 => \"Bad Request\",")
            .AppendLine("401 => \"Unauthorized\",")
            .AppendLine("403 => \"Forbidden\",")
            .AppendLine("404 => \"Not Found\",")
            .AppendLine("409 => \"Conflict\",")
            .AppendLine("422 => \"Unprocessable Entity\",")
            .AppendLine("500 => \"Internal Server Error\",")
            .AppendLine("_ => \"Error\"")
            .CloseBrace()
            .AppendLine(";")
            .CloseBrace(); // method

        builder.CloseBrace(); // class
    }

    private void GenerateHttpMethodExtensions(CodeBuilder builder, ResultToIResultConfig config)
    {
        builder.AppendXmlSummary("HTTP method-specific extensions for Result<T>.")
            .AppendClassDeclaration("HttpMethodExtensions", "public", "static");

        var httpMethods = new[]
        {
            ("Get", 200),
            ("Post", 201),
            ("Put", 200),
            ("Delete", 204),
            ("Patch", 200)
        };

        foreach (var (method, successCode) in httpMethods)
        {
            builder.BlankLine()
                .AppendXmlSummary($"Converts Result<T> to IResult for HTTP {method} requests.")
                .AppendMethodDeclaration(
                    $"To{method}Result",
                    "IResult",
                    "this Result<T> result",
                    "T",
                    "public", "static")
                .AppendLine("if (result.IsSuccess)")
                .OpenBrace()
                .AppendLine(successCode == 204 
                    ? "return Results.NoContent();" 
                    : $"return Results.Ok(result.Value);")
                .CloseBrace()
                .BlankLine()
                .AppendLine("var statusCode = DetermineStatusCode(result.Errors);")
                .AppendLine("var problemDetails = CreateProblemDetails(result.Errors, statusCode);")
                .AppendLine("return Results.Problem(problemDetails);")
                .CloseBrace(); // method
        }

        // Add helper methods to HttpMethodExtensions class
        builder.BlankLine()
            .AppendXmlSummary("Determines the appropriate HTTP status code based on error types.")
            .AppendMethodDeclaration(
                "DetermineStatusCode",
                "int",
                "System.Collections.Generic.IReadOnlyList<IError> errors",
                null,
                "private", "static")
            .AppendLine("if (!errors.Any())")
            .OpenBrace()
            .AppendLine("return 400;")
            .CloseBrace()
            .BlankLine()
            .AppendLine("foreach (var error in errors)")
            .OpenBrace()
            .AppendLine("var errorType = error.GetType().Name;")
            .AppendLine("if (errorType.Contains(\"NotFound\")) return 404;")
            .AppendLine("if (errorType.Contains(\"Validation\")) return 422;")
            .AppendLine("if (errorType.Contains(\"Unauthorized\")) return 401;")
            .AppendLine("if (errorType.Contains(\"Forbidden\")) return 403;")
            .AppendLine("if (errorType.Contains(\"Conflict\")) return 409;")
            .CloseBrace()
            .BlankLine()
            .AppendLine("return 400;")
            .CloseBrace(); // method

        builder.BlankLine()
            .AppendXmlSummary("Creates RFC 7807 ProblemDetails from errors.")
            .AppendMethodDeclaration(
                "CreateProblemDetails",
                "ProblemDetails",
                "System.Collections.Generic.IReadOnlyList<IError> errors, int statusCode",
                null,
                "private", "static")
            .AppendLine("var problemDetails = new ProblemDetails")
            .OpenBrace()
            .AppendLine("Status = statusCode,")
            .AppendLine("Title = statusCode switch { 400 => \"Bad Request\", 401 => \"Unauthorized\", 403 => \"Forbidden\", 404 => \"Not Found\", 409 => \"Conflict\", 422 => \"Unprocessable Entity\", 500 => \"Internal Server Error\", _ => \"Error\" },")
            .AppendLine("Type = $\"https://httpstatuses.com/{statusCode}\"")
            .CloseBrace()
            .AppendLine(";")
            .BlankLine()
            .AppendLine("if (errors.Count == 1)")
            .OpenBrace()
            .AppendLine("problemDetails.Detail = errors[0].Message;")
            .CloseBrace()
            .AppendLine("else")
            .OpenBrace()
            .AppendLine("problemDetails.Detail = errors.Count + \" errors occurred.\";")
            .CloseBrace()
            .BlankLine()
            .AppendLine("return problemDetails;")
            .CloseBrace(); // method

        builder.CloseBrace(); // class
    }

    private void GenerateAsyncExtensions(CodeBuilder builder, ResultToIResultConfig config)
    {
        builder.AppendXmlSummary("Async extensions for Result<T>.")
            .AppendClassDeclaration("AsyncExtensions", "public", "static");

        builder.BlankLine()
            .AppendXmlSummary("Asynchronously converts Result<T> to IResult.")
            .AppendMethodDeclaration(
                "ToIResultAsync",
                "Task<IResult>",
                "this Result<T> result",
                "T",
                "public", "static")
            .AppendLine("return Task.FromResult(result.ToIResult());")
            .CloseBrace(); // method

        builder.CloseBrace(); // class
    }

    private static string GetGenerateResultExtensionsAttributeSource()
    {
        return @"using System;

namespace REslava.Result.SourceGenerators
{
    /// <summary>
    /// Attribute to generate Result<T> to IResult extension methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class GenerateResultExtensionsAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the namespace for generated extensions.
        /// </summary>
        public string Namespace { get; set; } = ""Generated"";

        /// <summary>
        /// Gets or sets whether to include diagnostic information.
        /// </summary>
        public bool IncludeDiagnostics { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to include XML documentation.
        /// </summary>
        public bool IncludeDocumentation { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to include error tags.
        /// </summary>
        public bool IncludeErrorTags { get; set; } = true;

        /// <summary>
        /// Gets or sets custom error mappings in format ""ErrorType:StatusCode"".
        /// </summary>
        public string[] CustomErrorMappings { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets whether to generate HTTP method extensions.
        /// </summary>
        public bool GenerateHttpMethodExtensions { get; set; } = true;

        /// <summary>
        /// Gets or sets the default HTTP status code for errors.
        /// </summary>
        public int DefaultErrorStatusCode { get; set; } = 400;

        /// <summary>
        /// Gets or sets whether to include detailed error information.
        /// </summary>
        public bool IncludeDetailedErrors { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to generate async extension methods.
        /// </summary>
        public bool GenerateAsyncMethods { get; set; } = true;
    }
}";
    }
}
