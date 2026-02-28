using System.Net;
using System.Text.Json;

namespace REslava.Result.Http;

/// <summary>
/// Options that control how HTTP responses are mapped to <see cref="Result{T}"/>.
/// Pass an instance to any <c>HttpClientResultExtensions</c> method via the optional
/// <c>options</c> parameter.
/// </summary>
public sealed class HttpResultOptions
{
    /// <summary>
    /// JSON serializer options used when deserializing the response body.
    /// If <see langword="null"/>, <see cref="JsonSerializerDefaults.Web"/> is used
    /// (camelCase property names, case-insensitive matching).
    /// </summary>
    public JsonSerializerOptions? JsonOptions { get; init; }

    /// <summary>
    /// Custom status code to error mapper. Receives the HTTP status code and the
    /// response reason phrase. When set, this completely replaces the built-in
    /// default mapping — return the <see cref="IError"/> you want.
    /// </summary>
    public Func<HttpStatusCode, string?, IError>? StatusCodeMapper { get; init; }
}
