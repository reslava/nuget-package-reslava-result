using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using REslava.Result.SourceGenerators.Core.Interfaces;
using System.Text;

namespace REslava.Result.SourceGenerators.Generators.OneOfToActionResult.CodeGeneration
{
    /// <summary>
    /// Generates OneOf{N}ToActionResult extension methods for ASP.NET MVC IActionResult,
    /// parameterized by arity. Mirrors OneOfToIResultExtensionGenerator but targets MVC types.
    /// </summary>
    public class OneOfToActionResultExtensionGenerator : ICodeGenerator
    {
        private readonly int _arity;

        public OneOfToActionResultExtensionGenerator(int arity)
        {
            _arity = arity;
        }

        public SourceText GenerateCode(Compilation compilation, object config)
        {
            var builder = new StringBuilder();

            // Nullable enable
            builder.AppendLine("#nullable enable");
            builder.AppendLine();

            // Usings
            builder.AppendLine("using Microsoft.AspNetCore.Mvc;");
            builder.AppendLine("using REslava.Result;");
            builder.AppendLine("using REslava.Result.AdvancedPatterns;");
            builder.AppendLine("using System;");
            builder.AppendLine("using System.Linq;");
            builder.AppendLine();

            // Class
            builder.AppendLine("namespace Generated.OneOfActionResultExtensions");
            builder.AppendLine("{");
            builder.AppendLine("    /// <summary>");
            builder.AppendLine($"    /// Extension methods for converting OneOf with {_arity} types to IActionResult.");
            builder.AppendLine("    /// </summary>");
            builder.AppendLine($"    public static class OneOf{_arity}ActionResultExtensions");
            builder.AppendLine("    {");

            // ToActionResult method
            var typeParams = string.Join(", ", GenerateSequence("T", _arity));
            builder.AppendLine($"        public static IActionResult ToActionResult<{typeParams}>(this OneOf<{typeParams}> oneOf)");
            builder.AppendLine("        {");
            builder.AppendLine("            return oneOf.Match(");

            for (int i = 1; i <= _arity; i++)
            {
                var comma = i < _arity ? "," : "";
                builder.AppendLine($"                t{i} => IsErrorType(typeof(T{i})) ? MapErrorToActionResult(t{i}, typeof(T{i})) : (IActionResult)new OkObjectResult(t{i}){comma}");
            }

            builder.AppendLine("            );");
            builder.AppendLine("        }");
            builder.AppendLine();

            // MapErrorToActionResult helper — checks IError.Tags first, falls back to type-name heuristic
            builder.AppendLine("        private static IActionResult MapErrorToActionResult(object error, Type errorType)");
            builder.AppendLine("        {");
            builder.AppendLine("            if (error == null)");
            builder.AppendLine("                return new ObjectResult(new { Detail = \"Unknown error\" }) { StatusCode = 500 };");
            builder.AppendLine();
            builder.AppendLine("            var errorMessage = error.ToString() ?? \"Unknown error\";");
            builder.AppendLine();
            builder.AppendLine("            // Phase 1: Check IError.Tags[\"HttpStatusCode\"] for tag-based mapping");
            builder.AppendLine("            if (error is IError iError && iError.Tags != null)");
            builder.AppendLine("            {");
            builder.AppendLine("                var statusCode = -1;");
            builder.AppendLine("                if (iError.Tags.TryGetValue(\"HttpStatusCode\", out var code) && code is int sc)");
            builder.AppendLine("                    statusCode = sc;");
            builder.AppendLine("                else if (iError.Tags.TryGetValue(\"StatusCode\", out var code2) && code2 is int sc2)");
            builder.AppendLine("                    statusCode = sc2;");
            builder.AppendLine();
            builder.AppendLine("                if (statusCode > 0)");
            builder.AppendLine("                {");
            builder.AppendLine("                    return statusCode switch");
            builder.AppendLine("                    {");
            builder.AppendLine("                        401 => new UnauthorizedResult(),");
            builder.AppendLine("                        403 => new ForbidResult(),");
            builder.AppendLine("                        404 => new NotFoundObjectResult(errorMessage),");
            builder.AppendLine("                        409 => new ConflictObjectResult(errorMessage),");
            builder.AppendLine("                        _ => new ObjectResult(new { Detail = errorMessage }) { StatusCode = statusCode }");
            builder.AppendLine("                    };");
            builder.AppendLine("                }");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("            // Phase 2: Fall back to type-name heuristic for non-IError types");
            builder.AppendLine("            var typeName = errorType.Name;");
            builder.AppendLine();
            builder.AppendLine("            if (typeName.Contains(\"ValidationError\") || typeName.Contains(\"Invalid\"))");
            builder.AppendLine("                return new ObjectResult(new { Detail = errorMessage }) { StatusCode = 422 };");
            builder.AppendLine();
            builder.AppendLine("            if (typeName.Contains(\"NotFound\") || typeName.Contains(\"Missing\"))");
            builder.AppendLine("                return new NotFoundObjectResult(errorMessage);");
            builder.AppendLine();
            builder.AppendLine("            if (typeName.Contains(\"Conflict\") || typeName.Contains(\"Duplicate\"))");
            builder.AppendLine("                return new ConflictObjectResult(errorMessage);");
            builder.AppendLine();
            builder.AppendLine("            if (typeName.Contains(\"Unauthorized\") || typeName.Contains(\"Authentication\"))");
            builder.AppendLine("                return new UnauthorizedResult();");
            builder.AppendLine();
            builder.AppendLine("            if (typeName.Contains(\"Forbidden\") || typeName.Contains(\"Permission\"))");
            builder.AppendLine("                return new ForbidResult();");
            builder.AppendLine();
            builder.AppendLine("            if (typeName.Contains(\"Database\") || typeName.Contains(\"System\") || typeName.Contains(\"Infrastructure\"))");
            builder.AppendLine("                return new ObjectResult(new { Detail = errorMessage }) { StatusCode = 500 };");
            builder.AppendLine();
            builder.AppendLine("            return new ObjectResult(new { Detail = errorMessage }) { StatusCode = 400 };");
            builder.AppendLine("        }");
            builder.AppendLine();

            // IsErrorType helper
            builder.AppendLine("        private static bool IsErrorType(Type type)");
            builder.AppendLine("        {");
            builder.AppendLine("            if (type == null) return false;");
            builder.AppendLine("            ");
            builder.AppendLine("            try");
            builder.AppendLine("            {");
            builder.AppendLine("                if (type.BaseType != null && type.BaseType.Name == \"Error\")");
            builder.AppendLine("                    return true;");
            builder.AppendLine("            }");
            builder.AppendLine("            catch");
            builder.AppendLine("            {");
            builder.AppendLine("            }");
            builder.AppendLine("            ");
            builder.AppendLine("            var typeName = type.Name.ToLowerInvariant();");
            builder.AppendLine("            return typeName.Contains(\"error\") || ");
            builder.AppendLine("                   typeName.Contains(\"exception\") || ");
            builder.AppendLine("                   typeName.Contains(\"fault\") ||");
            builder.AppendLine("                   typeName.Contains(\"failure\");");
            builder.AppendLine("        }");
            builder.AppendLine("    }");
            builder.AppendLine("}");

            return SourceText.From(builder.ToString(), Encoding.UTF8);
        }

        private static string[] GenerateSequence(string prefix, int count)
        {
            var result = new string[count];
            for (int i = 0; i < count; i++)
                result[i] = $"{prefix}{i + 1}";
            return result;
        }
    }
}
