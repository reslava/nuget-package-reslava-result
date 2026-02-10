using Microsoft.CodeAnalysis.Text;
using REslava.Result.SourceGenerators.Core.Interfaces;
using System.Text;

namespace REslava.Result.SourceGenerators.Generators.SmartEndpoints.Attributes
{
    /// <summary>
    /// Generates the AutoGenerateEndpoints attribute for class-level automatic mapping.
    /// Single Responsibility: Only generates this specific attribute.
    /// </summary>
    public class AutoGenerateEndpointsAttributeGenerator : IAttributeGenerator
    {
        public SourceText GenerateAttribute()
        {
            var source = @"
using System;

namespace REslava.Result.SourceGenerators.SmartEndpoints
{
    /// <summary>
    /// Automatically generates Minimal API endpoints for all public methods
    /// in this class that return Result&lt;T&gt; or OneOf&lt;...&gt; types.
    /// </summary>
    /// <example>
    /// <code>
    /// [AutoGenerateEndpoints(RoutePrefix = ""/api/users"")]
    /// public class UserController
    /// {
    ///     // Generates: GET /api/users/{id}
    ///     public OneOf&lt;ValidationError, UserNotFoundError, User&gt; GetUser(int id) { }
    ///     
    ///     // Generates: POST /api/users
    ///     public OneOf&lt;ValidationError, ConflictError, User&gt; CreateUser(CreateUserRequest request) { }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class AutoGenerateEndpointsAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the route prefix for all endpoints in this class.
        /// If not specified, uses the class name (e.g., UserController → /user).
        /// </summary>
        public string? RoutePrefix { get; set; }

        /// <summary>
        /// Gets or sets the OpenAPI tags for all endpoints in this class.
        /// </summary>
        public string[]? Tags { get; set; }

        /// <summary>
        /// Gets or sets whether to include the class name in the route.
        /// Default is true (UserController → /user).
        /// </summary>
        public bool IncludeClassNameInRoute { get; set; } = true;

        /// <summary>
        /// Gets or sets whether all endpoints require authentication by default.
        /// </summary>
        public bool RequiresAuth { get; set; }

        /// <summary>
        /// Gets or sets the default authorization policies.
        /// </summary>
        public string[]? Policies { get; set; }

        /// <summary>
        /// Gets or sets the default required roles.
        /// Generates .RequireAuthorization(new AuthorizeAttribute { Roles = ""..."" }).
        /// </summary>
        public string[]? Roles { get; set; }

        /// <summary>
        /// Gets or sets the HTTP method mapping strategy.
        /// </summary>
        public EndpointMappingStrategy Strategy { get; set; } = EndpointMappingStrategy.Convention;
    }

    /// <summary>
    /// Strategy for mapping methods to HTTP verbs.
    /// </summary>
    public enum EndpointMappingStrategy
    {
        /// <summary>
        /// Use naming conventions (Get*, Create*, Update*, Delete*, etc.).
        /// </summary>
        Convention,

        /// <summary>
        /// Explicitly require [AutoMapEndpoint] on each method.
        /// </summary>
        Explicit,

        /// <summary>
        /// Map all public methods returning Result/OneOf.
        /// </summary>
        All
    }
}";

            return SourceText.From(source, Encoding.UTF8);
        }
    }
}
