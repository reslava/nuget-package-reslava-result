using Microsoft.CodeAnalysis.Text;
using REslava.Result.SourceGenerators.Core.Interfaces;
using System.Text;

namespace REslava.Result.SourceGenerators.Generators.SmartEndpoints.Attributes
{
    /// <summary>
    /// Generates the AutoMapEndpoint attribute for individual method mapping.
    /// Single Responsibility: Only generates this specific attribute.
    /// </summary>
    public class AutoMapEndpointAttributeGenerator : IAttributeGenerator
    {
        public SourceText GenerateAttribute()
        {
            var source = @"
using System;

namespace REslava.Result.SourceGenerators.SmartEndpoints
{
    /// <summary>
    /// Maps a method to a Minimal API endpoint.
    /// The method must return Result&lt;T&gt; or OneOf&lt;...&gt; for automatic HTTP mapping.
    /// </summary>
    /// <example>
    /// <code>
    /// [AutoMapEndpoint(""/users/{id}"", HttpMethod = ""GET"")]
    /// public OneOf&lt;ValidationError, UserNotFoundError, User&gt; GetUser(int id)
    /// {
    ///     // Your logic here
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class AutoMapEndpointAttribute : Attribute
    {
        /// <summary>
        /// Gets the route pattern for this endpoint.
        /// </summary>
        public string Route { get; }

        /// <summary>
        /// Gets or sets the HTTP method (GET, POST, PUT, PATCH, DELETE).
        /// Default is GET.
        /// </summary>
        public string HttpMethod { get; set; } = ""GET"";

        /// <summary>
        /// Gets or sets the endpoint name for URL generation.
        /// If not specified, uses the method name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the OpenAPI tags for this endpoint.
        /// </summary>
        public string[]? Tags { get; set; }

        /// <summary>
        /// Gets or sets the OpenAPI summary.
        /// </summary>
        public string? Summary { get; set; }

        /// <summary>
        /// Gets or sets whether this endpoint requires authentication.
        /// </summary>
        public bool RequiresAuth { get; set; }

        /// <summary>
        /// Gets or sets the authorization policies required.
        /// </summary>
        public string[]? Policies { get; set; }

        public AutoMapEndpointAttribute(string route)
        {
            Route = route ?? throw new ArgumentNullException(nameof(route));
        }
    }
}";

            return SourceText.From(source, Encoding.UTF8);
        }
    }
}
