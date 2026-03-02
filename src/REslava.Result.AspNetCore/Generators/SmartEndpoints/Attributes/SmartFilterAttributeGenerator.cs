using Microsoft.CodeAnalysis.Text;
using REslava.Result.SourceGenerators.Core.Interfaces;
using System.Text;

namespace REslava.Result.SourceGenerators.Generators.SmartEndpoints.Attributes
{
    /// <summary>
    /// Generates the SmartFilter attribute for method-level endpoint filter registration.
    /// AllowMultiple = true so multiple filters can be stacked on one method.
    /// </summary>
    public class SmartFilterAttributeGenerator : IAttributeGenerator
    {
        public SourceText GenerateAttribute()
        {
            var source = @"
using System;

namespace REslava.Result.SourceGenerators.SmartEndpoints
{
    /// <summary>
    /// Adds an endpoint filter to a SmartEndpoint method.
    /// The filter type must implement <see cref=""Microsoft.AspNetCore.Http.IEndpointFilter""/>.
    /// Multiple filters are applied in declaration order.
    /// </summary>
    /// <remarks>
    /// For open generic filters, specify the closed type:
    /// <c>[SmartFilter(typeof(ValidationFilter&lt;CreateOrderRequest&gt;))]</c>
    /// </remarks>
    /// <example>
    /// <code>
    /// [AutoGenerateEndpoints(RoutePrefix = ""/api/orders"")]
    /// public class OrderController
    /// {
    ///     [SmartFilter(typeof(LoggingFilter))]
    ///     [SmartFilter(typeof(ValidationFilter&lt;CreateOrderRequest&gt;))]
    ///     public Result&lt;Order&gt; CreateOrder(CreateOrderRequest request) { }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class SmartFilterAttribute : Attribute
    {
        /// <summary>
        /// Gets the filter type to register with .AddEndpointFilter&lt;T&gt;().
        /// </summary>
        public Type FilterType { get; }

        public SmartFilterAttribute(Type filterType)
        {
            FilterType = filterType ?? throw new ArgumentNullException(nameof(filterType));
        }
    }
}";

            return SourceText.From(source, Encoding.UTF8);
        }
    }
}
