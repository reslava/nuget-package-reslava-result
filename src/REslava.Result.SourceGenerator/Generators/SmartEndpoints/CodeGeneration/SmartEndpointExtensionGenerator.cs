using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using REslava.Result.SourceGenerators.Core.Interfaces;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints.Models;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace REslava.Result.SourceGenerators.Generators.SmartEndpoints.CodeGeneration
{
    /// <summary>
    /// Generates SmartEndpoint extension methods - FIXED VERSION
    /// </summary>
    public class SmartEndpointExtensionGenerator : ICodeGenerator
    {
        public SourceText GenerateCode(Compilation compilation, object config)
        {
            var endpoints = config as List<EndpointMetadata>;
            
            if (endpoints == null || !endpoints.Any())
            {
                return SourceText.From("// No SmartEndpoints detected", Encoding.UTF8);
            }

            var builder = new StringBuilder();

            // Using statements
            builder.AppendLine("using Microsoft.AspNetCore.Builder;");
            builder.AppendLine("using Microsoft.AspNetCore.Http;");
            builder.AppendLine("using Microsoft.AspNetCore.Routing;");
            builder.AppendLine("using System;");
            builder.AppendLine("using System.Threading.Tasks;");
            builder.AppendLine("using Generated.ResultExtensions;");
            builder.AppendLine("using Generated.OneOfExtensions;");
            builder.AppendLine();

            // Namespace
            builder.AppendLine("namespace Generated.SmartEndpoints");
            builder.AppendLine("{");
            builder.AppendLine("    public static class SmartEndpointExtensions");
            builder.AppendLine("    {");
            builder.AppendLine("        public static IEndpointRouteBuilder MapSmartEndpoints(this IEndpointRouteBuilder endpoints)");
            builder.AppendLine("        {");

            // Generate each endpoint
            foreach (var endpoint in endpoints)
            {
                GenerateEndpoint(builder, endpoint);
            }

            builder.AppendLine("            return endpoints;");
            builder.AppendLine("        }");
            builder.AppendLine("    }");
            builder.AppendLine("}");

            return SourceText.From(builder.ToString(), Encoding.UTF8);
        }

        private void GenerateEndpoint(StringBuilder builder, EndpointMetadata endpoint)
        {
            var httpMethod = endpoint.HttpMethod.ToLowerInvariant();
            var mapMethod = httpMethod switch
            {
                "get" => "MapGet",
                "post" => "MapPost",
                "put" => "MapPut",
                "delete" => "MapDelete",
                "patch" => "MapPatch",
                _ => "MapGet"
            };

            // Build parameter list (method params + DI service)
            var methodParams = string.Join(", ", endpoint.Parameters.Select(p => $"{p.Type} {p.Name}"));
            var serviceParam = $"{endpoint.Namespace}.{endpoint.ClassName} service";
            var fullParamList = string.IsNullOrEmpty(methodParams)
                ? serviceParam
                : $"{methodParams}, {serviceParam}";
            var argList = string.Join(", ", endpoint.Parameters.Select(p => p.Name));

            var asyncKeyword = endpoint.IsAsync ? "async " : "";
            var awaitKeyword = endpoint.IsAsync ? "await " : "";

            builder.AppendLine($"            // {endpoint.MethodName}: {endpoint.HttpMethod} {endpoint.Route}");
            builder.AppendLine($"            endpoints.{mapMethod}(\"{endpoint.Route}\", {asyncKeyword}({fullParamList}) =>");
            builder.AppendLine("            {");
            builder.AppendLine($"                var result = {awaitKeyword}service.{endpoint.MethodName}({argList});");
            builder.AppendLine("                return result.ToIResult();");
            builder.AppendLine("            });");
            builder.AppendLine();
        }
    }
}
