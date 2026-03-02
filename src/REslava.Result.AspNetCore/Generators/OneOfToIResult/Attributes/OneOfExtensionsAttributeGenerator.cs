using Microsoft.CodeAnalysis.Text;
using REslava.Result.SourceGenerators.Core.Interfaces;
using System;
using System.Text;

namespace REslava.Result.SourceGenerators.Generators.OneOfToIResult.Attributes
{
    /// <summary>
    /// Generates the [GenerateOneOf{N}Extensions] attribute, parameterized by arity.
    /// </summary>
    public class OneOfExtensionsAttributeGenerator : IAttributeGenerator
    {
        private readonly int _arity;

        public OneOfExtensionsAttributeGenerator(int arity)
        {
            _arity = arity;
        }

        public SourceText GenerateAttribute()
        {
            var source = $@"
using System;

namespace REslava.Result.SourceGenerators.OneOf{_arity}
{{
    /// <summary>
    /// Marks an assembly for automatic generation of OneOf{_arity} to IResult extension methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class GenerateOneOf{_arity}ExtensionsAttribute : Attribute
    {{
        /// <summary>Target namespace for generated extensions.</summary>
        public string Namespace {{ get; set; }} = ""Generated.ResultExtensions"";

        /// <summary>Whether to include error tags in generated ProblemDetails.</summary>
        public bool IncludeErrorTags {{ get; set; }} = true;

        /// <summary>Whether to log errors during generation.</summary>
        public bool LogErrors {{ get; set; }} = false;

        /// <summary>Custom error type to HTTP status code mappings.</summary>
        public string[] CustomErrorMappings {{ get; set; }} = Array.Empty<string>();

        /// <summary>Whether to generate HTTP method-specific extensions.</summary>
        public bool GenerateHttpMethodExtensions {{ get; set; }} = true;

        /// <summary>Default HTTP status code for unmapped error types.</summary>
        public int DefaultErrorStatusCode {{ get; set; }} = 400;
    }}
}}";
            return SourceText.From(source, Encoding.UTF8);
        }
    }
}
