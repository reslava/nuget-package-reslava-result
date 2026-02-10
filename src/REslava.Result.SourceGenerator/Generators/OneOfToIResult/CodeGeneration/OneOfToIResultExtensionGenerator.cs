using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using REslava.Result.SourceGenerators.Core.Interfaces;
using System.Text;

namespace REslava.Result.SourceGenerators.Generators.OneOfToIResult.CodeGeneration
{
    /// <summary>
    /// Generates OneOf{N}ToIResult extension methods, parameterized by arity.
    /// Single class handles OneOf2, OneOf3, and OneOf4 extension generation.
    /// </summary>
    public class OneOfToIResultExtensionGenerator : ICodeGenerator
    {
        private readonly int _arity;

        public OneOfToIResultExtensionGenerator(int arity)
        {
            _arity = arity;
        }

        public SourceText GenerateCode(Compilation compilation, object config)
        {
            var builder = new StringBuilder();

            // Usings
            builder.AppendLine("using Microsoft.AspNetCore.Http;");
            builder.AppendLine("using REslava.Result.AdvancedPatterns;");
            builder.AppendLine("using System;");
            builder.AppendLine();

            // Class
            builder.AppendLine("namespace Generated.OneOfExtensions");
            builder.AppendLine("{");
            builder.AppendLine("    /// <summary>");
            builder.AppendLine($"    /// Extension methods for converting OneOf with {_arity} types to IResult.");
            builder.AppendLine("    /// </summary>");
            builder.AppendLine($"    public static class OneOf{_arity}Extensions");
            builder.AppendLine("    {");

            // ToIResult method
            var typeParams = string.Join(", ", GenerateSequence("T", _arity));
            builder.AppendLine($"        public static IResult ToIResult<{typeParams}>(this OneOf<{typeParams}> oneOf)");
            builder.AppendLine("        {");
            builder.AppendLine("            return oneOf.Match(");

            for (int i = 1; i <= _arity; i++)
            {
                var comma = i < _arity ? "," : "";
                builder.AppendLine($"                t{i} => IsErrorType(typeof(T{i})) ? MapErrorToHttpResult(t{i}, typeof(T{i})) : Results.Ok(t{i}){comma}");
            }

            builder.AppendLine("            );");
            builder.AppendLine("        }");
            builder.AppendLine();

            // MapErrorToHttpResult helper
            builder.AppendLine("        private static IResult MapErrorToHttpResult(object error, Type errorType)");
            builder.AppendLine("        {");
            builder.AppendLine("            if (error == null)");
            builder.AppendLine("                return Results.Problem(\"Unknown error\", statusCode: 500);");
            builder.AppendLine("            ");
            builder.AppendLine("            var typeName = errorType.Name;");
            builder.AppendLine("            ");
            builder.AppendLine("            if (typeName.Contains(\"ValidationError\") || typeName.Contains(\"Invalid\"))");
            builder.AppendLine("                return Results.BadRequest(error?.ToString() ?? \"Validation error\");");
            builder.AppendLine("                ");
            builder.AppendLine("            if (typeName.Contains(\"NotFound\") || typeName.Contains(\"Missing\"))");
            builder.AppendLine("                return Results.NotFound(error?.ToString() ?? \"Resource not found\");");
            builder.AppendLine("                ");
            builder.AppendLine("            if (typeName.Contains(\"Conflict\") || typeName.Contains(\"Duplicate\"))");
            builder.AppendLine("                return Results.Conflict(error?.ToString() ?? \"Resource conflict\");");
            builder.AppendLine("                ");
            builder.AppendLine("            if (typeName.Contains(\"Unauthorized\") || typeName.Contains(\"Authentication\"))");
            builder.AppendLine("                return Results.Unauthorized();");
            builder.AppendLine("                ");
            builder.AppendLine("            if (typeName.Contains(\"Forbidden\") || typeName.Contains(\"Permission\"))");
            builder.AppendLine("                return Results.Forbid();");
            builder.AppendLine("                ");
            builder.AppendLine("            if (typeName.Contains(\"Database\") || typeName.Contains(\"System\") || typeName.Contains(\"Infrastructure\"))");
            builder.AppendLine("                return Results.Problem(detail: error?.ToString() ?? \"Server error\", statusCode: 500);");
            builder.AppendLine("                ");
            builder.AppendLine("            return Results.BadRequest(error?.ToString() ?? \"Error\");");
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
