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
    /// Generates SmartEndpoint extension methods with OpenAPI metadata.
    /// Groups endpoints by controller using MapGroup, emits .WithName(),
    /// .WithSummary(), .WithTags(), and .Produces() for full Scalar/Swagger support.
    /// </summary>
    public class SmartEndpointExtensionGenerator : ICodeGenerator
    {
        public SourceText GenerateCode(Compilation compilation, object config)
        {
            // Accept List<ControllerMetadata> (new) or List<EndpointMetadata> (backward compat)
            var controllers = config as List<ControllerMetadata>;

            if (controllers == null && config is List<EndpointMetadata> flatEndpoints)
            {
                if (!flatEndpoints.Any())
                    return SourceText.From("// No SmartEndpoints detected", Encoding.UTF8);

                // Wrap flat list in a single controller for backward compatibility
                controllers = new List<ControllerMetadata>
                {
                    new ControllerMetadata
                    {
                        ClassName = "Legacy",
                        RoutePrefix = flatEndpoints.First().RoutePrefix,
                        Endpoints = flatEndpoints,
                        Tags = new List<string> { "SmartEndpoints" }
                    }
                };
            }

            if (controllers == null || !controllers.Any())
            {
                return SourceText.From("// No SmartEndpoints detected", Encoding.UTF8);
            }

            var builder = new StringBuilder();

            // Check if any endpoint uses Roles (needs AuthorizeAttribute)
            var anyRoles = controllers.Any(c => c.Endpoints.Any(e => e.Roles != null && e.Roles.Any()));

            // Using statements
            builder.AppendLine("using Microsoft.AspNetCore.Builder;");
            builder.AppendLine("using Microsoft.AspNetCore.Http;");
            builder.AppendLine("using Microsoft.AspNetCore.Routing;");
            if (anyRoles)
            {
                builder.AppendLine("using Microsoft.AspNetCore.Authorization;");
            }
            builder.AppendLine("using System;");
            builder.AppendLine("using System.Threading.Tasks;");
            builder.AppendLine("using Generated.ResultExtensions;");
            builder.AppendLine("using Generated.OneOfExtensions;");
            builder.AppendLine();

            // Namespace and class
            builder.AppendLine("namespace Generated.SmartEndpoints");
            builder.AppendLine("{");
            builder.AppendLine("    public static class SmartEndpointExtensions");
            builder.AppendLine("    {");
            builder.AppendLine("        public static IEndpointRouteBuilder MapSmartEndpoints(this IEndpointRouteBuilder endpoints)");
            builder.AppendLine("        {");

            // Generate a route group per controller
            foreach (var controller in controllers)
            {
                GenerateControllerGroup(builder, controller);
            }

            builder.AppendLine("            return endpoints;");
            builder.AppendLine("        }");
            builder.AppendLine("    }");
            builder.AppendLine("}");

            return SourceText.From(builder.ToString(), Encoding.UTF8);
        }

        private void GenerateControllerGroup(StringBuilder builder, ControllerMetadata controller)
        {
            // Generate a safe variable name: "SmartProductController" → "smartProductGroup"
            var baseName = controller.ClassName;
            foreach (var suffix in new[] { "Controller", "Service", "Endpoint", "Endpoints" })
            {
                if (baseName.EndsWith(suffix))
                {
                    baseName = baseName.Substring(0, baseName.Length - suffix.Length);
                    break;
                }
            }
            var varName = char.ToLower(baseName[0]) + baseName.Substring(1) + "Group";

            var tag = controller.Tags.Any() ? controller.Tags[0] : controller.ClassName;

            builder.AppendLine();
            builder.AppendLine($"            // === {controller.ClassName} ===");
            builder.AppendLine($"            var {varName} = endpoints.MapGroup(\"{controller.RoutePrefix}\")");
            builder.AppendLine($"                .WithTags(\"{EscapeString(tag)}\");");
            builder.AppendLine();

            foreach (var endpoint in controller.Endpoints)
            {
                GenerateEndpoint(builder, endpoint, varName, baseName);
            }
        }

        private void GenerateEndpoint(StringBuilder builder, EndpointMetadata endpoint, string groupVarName, string controllerBaseName)
        {
            var mapMethod = endpoint.HttpMethod.ToLowerInvariant() switch
            {
                "get" => "MapGet",
                "post" => "MapPost",
                "put" => "MapPut",
                "delete" => "MapDelete",
                "patch" => "MapPatch",
                _ => "MapGet"
            };

            // Compute relative route (strip prefix since the group handles it)
            var relativeRoute = ComputeRelativeRoute(endpoint.Route, endpoint.RoutePrefix);

            // Build parameter list (method params + DI service)
            var methodParams = string.Join(", ", endpoint.Parameters.Select(p => $"{p.Type} {p.Name}"));
            var serviceParam = $"{endpoint.Namespace}.{endpoint.ClassName} service";
            var fullParamList = string.IsNullOrEmpty(methodParams)
                ? serviceParam
                : $"{methodParams}, {serviceParam}";
            var argList = string.Join(", ", endpoint.Parameters.Select(p => p.Name));

            var asyncKeyword = endpoint.IsAsync ? "async " : "";
            var awaitKeyword = endpoint.IsAsync ? "await " : "";

            // Endpoint comment
            builder.AppendLine($"            // {endpoint.MethodName}: {endpoint.HttpMethod} {endpoint.Route}");

            // Map method + handler lambda
            builder.AppendLine($"            {groupVarName}.{mapMethod}(\"{relativeRoute}\", {asyncKeyword}({fullParamList}) =>");
            builder.AppendLine("            {");
            builder.AppendLine($"                var result = {awaitKeyword}service.{endpoint.MethodName}({argList});");
            builder.AppendLine("                return result.ToIResult();");
            builder.AppendLine("            })");

            // Build the fluent chain
            var chain = new List<string>();

            // .WithName() — prefixed with controller base name for global uniqueness
            chain.Add($".WithName(\"{controllerBaseName}_{endpoint.MethodName}\")");

            // .WithSummary()
            if (!string.IsNullOrEmpty(endpoint.Summary))
            {
                chain.Add($".WithSummary(\"{EscapeString(endpoint.Summary)}\")");
            }

            // .Produces<T>(statusCode) and .Produces(statusCode)
            foreach (var produces in endpoint.ProducesList)
            {
                if (produces.ResponseType != null)
                {
                    chain.Add($".Produces<{produces.ResponseType}>({produces.StatusCode})");
                }
                else
                {
                    chain.Add($".Produces({produces.StatusCode})");
                }
            }

            // Authorization chain
            if (endpoint.AllowAnonymous)
            {
                chain.Add(".AllowAnonymous()");
            }
            else if (endpoint.RequiresAuth)
            {
                if (endpoint.Roles != null && endpoint.Roles.Any())
                {
                    var rolesStr = string.Join(",", endpoint.Roles);
                    chain.Add($".RequireAuthorization(new AuthorizeAttribute {{ Roles = \"{rolesStr}\" }})");
                }
                else if (endpoint.Policies != null && endpoint.Policies.Any())
                {
                    var policiesStr = string.Join(", ", endpoint.Policies.Select(p => $"\"{p}\""));
                    chain.Add($".RequireAuthorization({policiesStr})");
                }
                else
                {
                    chain.Add(".RequireAuthorization()");
                }
            }

            // Emit chain — last item gets semicolon
            for (int i = 0; i < chain.Count; i++)
            {
                var terminator = (i == chain.Count - 1) ? ";" : "";
                builder.AppendLine($"                {chain[i]}{terminator}");
            }

            builder.AppendLine();
        }

        private static string ComputeRelativeRoute(string fullRoute, string routePrefix)
        {
            if (string.IsNullOrEmpty(routePrefix))
                return fullRoute;

            if (fullRoute.StartsWith(routePrefix) && fullRoute.Length > routePrefix.Length)
            {
                return fullRoute.Substring(routePrefix.Length);
            }

            // Route equals prefix — use root
            if (fullRoute == routePrefix)
                return "/";

            return fullRoute;
        }

        private static string EscapeString(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
        }
    }
}
