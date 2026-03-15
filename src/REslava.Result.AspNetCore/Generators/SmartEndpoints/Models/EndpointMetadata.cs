using System;
using System.Collections.Generic;
using System.Linq;

namespace REslava.Result.SourceGenerators.Generators.SmartEndpoints.Models
{
    /// <summary>
    /// Metadata for a single endpoint method.
    /// </summary>
    public class EndpointMetadata
    {
        public string MethodName { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = "GET";
        public string ReturnType { get; set; } = string.Empty;
        public bool IsOneOf { get; set; }

        /// <summary>
        /// Number of generic type arguments when IsOneOf is true (0 otherwise).
        /// Replaces the old bool IsOneOf4 flag to support arities beyond 4.
        /// </summary>
        public int OneOfArity { get; set; }

        /// <summary>Backward-compatible alias — true when IsOneOf and arity is exactly 4.</summary>
        public bool IsOneOf4 => IsOneOf && OneOfArity == 4;
        public bool IsResult { get; set; }
        public bool IsAsync { get; set; }
        public List<ParameterMetadata> Parameters { get; set; } = new();
        public string ClassName { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        
        /// <summary>
        /// Generic type arguments for OneOf or Result.
        /// For OneOf&lt;Error, User&gt;, this would be ["Error", "User"]
        /// For Result&lt;User&gt;, this would be ["User"]
        /// </summary>
        public List<string> GenericTypeArguments { get; set; } = new();

        /// <summary>
        /// For <c>Result&lt;T, ErrorsOf&lt;T1..Tn&gt;&gt;</c> — the individual error type names
        /// extracted from the ErrorsOf union. Empty for plain <c>Result&lt;T&gt;</c>.
        /// When non-empty, <c>BuildProducesList</c> emits typed .Produces&lt;Ti&gt;(status)
        /// per error type instead of the generic catch-all status codes.
        /// </summary>
        public List<string> ErrorsOfTypeArguments { get; set; } = new();

        /// <summary>
        /// Tags for OpenAPI/Swagger grouping.
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Summary for OpenAPI documentation.
        /// </summary>
        public string? Summary { get; set; }

        /// <summary>
        /// Whether this endpoint requires authentication.
        /// </summary>
        public bool RequiresAuth { get; set; }

        /// <summary>
        /// Authorization policies.
        /// </summary>
        public List<string> Policies { get; set; } = new();

        /// <summary>
        /// Required roles for this endpoint.
        /// </summary>
        public List<string> Roles { get; set; } = new();

        /// <summary>
        /// Whether this endpoint explicitly allows anonymous access,
        /// overriding class-level RequiresAuth.
        /// </summary>
        public bool AllowAnonymous { get; set; }

        /// <summary>
        /// Route prefix from the class-level attribute (e.g., "/api/smart/products").
        /// Used to compute relative routes when generating MapGroup.
        /// </summary>
        public string RoutePrefix { get; set; } = string.Empty;

        /// <summary>
        /// Pre-computed .Produces() metadata for OpenAPI documentation.
        /// </summary>
        public List<ProducesMetadata> ProducesList { get; set; } = new();

        /// <summary>
        /// Fully-qualified filter type names to register with .AddEndpointFilter&lt;T&gt;().
        /// Populated from [SmartFilter(typeof(T))] attributes on the method.
        /// </summary>
        public List<string> FilterTypes { get; set; } = new();

        /// <summary>
        /// Output cache duration in seconds for GET endpoints.
        /// 0 = no cache (default). -1 = explicitly disabled (overrides class default).
        /// </summary>
        public int CacheSeconds { get; set; } = 0;

        /// <summary>
        /// Rate limit policy name. Null or empty = no rate limiting.
        /// "none" = explicitly disabled (overrides class default).
        /// </summary>
        public string? RateLimitPolicy { get; set; }
    }

    /// <summary>
    /// Metadata for method parameters.
    /// </summary>
    public class ParameterMetadata
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public ParameterSource Source { get; set; } = ParameterSource.Query;
        public bool IsOptional { get; set; }
        public string? DefaultValue { get; set; }

        /// <summary>
        /// True when the parameter's type is decorated with [Validate],
        /// causing SmartEndpoints to inject a .Validate() call in the generated lambda.
        /// </summary>
        public bool HasValidateAttribute { get; set; }

        /// <summary>
        /// True when the parameter's type is decorated with [FluentValidate]
        /// (from REslava.Result.FluentValidation), causing SmartEndpoints to inject
        /// IValidator&lt;T&gt; as a lambda parameter and emit a .Validate(validator) call.
        /// </summary>
        public bool HasFluentValidateAttribute { get; set; }
    }

    /// <summary>
    /// Parameter binding source.
    /// </summary>
    public enum ParameterSource
    {
        Query,
        Route,
        Body,
        Header,
        Service,
        CancellationToken
    }

    /// <summary>
    /// Metadata for a .Produces() or .Produces&lt;T&gt;() call on an endpoint.
    /// </summary>
    public class ProducesMetadata
    {
        /// <summary>HTTP status code (e.g., 200, 400, 404).</summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Fully qualified type name for .Produces&lt;T&gt;(statusCode).
        /// Null means untyped .Produces(statusCode).
        /// </summary>
        public string ResponseType { get; set; }
    }

    /// <summary>
    /// Metadata for a controller/class containing endpoints.
    /// </summary>
    public class ControllerMetadata
    {
        public string ClassName { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public string RoutePrefix { get; set; } = string.Empty;
        public List<EndpointMetadata> Endpoints { get; set; } = new();
        public bool HasAutoGenerateAttribute { get; set; }
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Whether all endpoints in this controller require authentication by default.
        /// </summary>
        public bool RequiresAuth { get; set; }

        /// <summary>
        /// Class-level authorization policies.
        /// </summary>
        public List<string> Policies { get; set; } = new();

        /// <summary>
        /// Class-level required roles.
        /// </summary>
        public List<string> Roles { get; set; } = new();

        /// <summary>
        /// Default output cache duration in seconds for GET endpoints in this controller.
        /// 0 = no cache. Overridden per-method via [AutoMapEndpoint(CacheSeconds = N)].
        /// </summary>
        public int CacheSeconds { get; set; } = 0;

        /// <summary>
        /// Default rate limit policy for all endpoints in this controller.
        /// Null = no rate limiting. Overridden per-method via [AutoMapEndpoint(RateLimitPolicy = "...")].
        /// </summary>
        public string? RateLimitPolicy { get; set; }
    }
}
