using Microsoft.CodeAnalysis.Text;
using REslava.Result.SourceGenerators.Core.Interfaces;
using System.Text;

namespace REslava.Result.SourceGenerators.Generators.SmartEndpoints.Attributes
{
    /// <summary>
    /// Generates the SmartAllowAnonymous attribute for method-level anonymous access override.
    /// Used in Convention strategy to exempt individual methods from class-level auth.
    /// </summary>
    public class SmartAllowAnonymousAttributeGenerator : IAttributeGenerator
    {
        public SourceText GenerateAttribute()
        {
            var source = @"
using System;

namespace REslava.Result.SourceGenerators.SmartEndpoints
{
    /// <summary>
    /// Marks a SmartEndpoint method as allowing anonymous access,
    /// overriding the class-level RequiresAuth setting.
    /// </summary>
    /// <example>
    /// <code>
    /// [AutoGenerateEndpoints(RoutePrefix = ""/api/orders"", RequiresAuth = true)]
    /// public class OrderController
    /// {
    ///     [SmartAllowAnonymous]  // Public read access
    ///     public Result&lt;List&lt;Order&gt;&gt; GetOrders() { }
    ///
    ///     // Requires authentication (inherited from class)
    ///     public Result&lt;Order&gt; CreateOrder(CreateOrderRequest request) { }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SmartAllowAnonymousAttribute : Attribute
    {
    }
}";

            return SourceText.From(source, Encoding.UTF8);
        }
    }
}
