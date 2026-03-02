using Microsoft.CodeAnalysis.Text;
using REslava.Result.SourceGenerators.Core.Interfaces;
using System.Text;

namespace REslava.Result.SourceGenerators.Generators.OneOfToActionResult.Attributes
{
    /// <summary>
    /// Generates the [GenerateOneOf{N}ActionResultExtensions] attribute, parameterized by arity.
    /// </summary>
    public class OneOfActionResultExtensionsAttributeGenerator : IAttributeGenerator
    {
        private readonly int _arity;

        public OneOfActionResultExtensionsAttributeGenerator(int arity)
        {
            _arity = arity;
        }

        public SourceText GenerateAttribute()
        {
            var source = $@"
using System;

namespace REslava.Result.SourceGenerators.OneOf{_arity}ActionResult
{{
    /// <summary>
    /// Marks an assembly for automatic generation of OneOf{_arity} to IActionResult extension methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class GenerateOneOf{_arity}ActionResultExtensionsAttribute : Attribute
    {{
    }}
}}";
            return SourceText.From(source, Encoding.UTF8);
        }
    }
}
