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
        public bool IsResult { get; set; }
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
        Service
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
    }
}
