using System.Net.Http.Json;
using System.Text.Json;

namespace REslava.Result.Http;

/// <summary>
/// Extension methods on <see cref="HttpClient"/> that return <see cref="Result{T}"/>
/// (or <see cref="Result"/>) instead of throwing on HTTP errors or network failures.
/// </summary>
/// <remarks>
/// <para>HTTP 4xx/5xx responses are mapped to typed domain errors via the default
/// <see cref="HttpStatusCodeMapper"/> or a custom <see cref="HttpResultOptions.StatusCodeMapper"/>.</para>
/// <para>Network-level failures (<see cref="System.Net.Http.HttpRequestException"/>) and
/// timeouts (<see cref="TaskCanceledException"/>) are wrapped in <see cref="ExceptionError"/>.</para>
/// </remarks>
public static class HttpClientResultExtensions
{
    // Default JSON options reused across calls — avoids repeated allocation.
    // JsonSerializerOptions.Web is .NET 9+ only; use the constructor overload for net8 compat.
    private static readonly JsonSerializerOptions s_defaultJsonOptions =
        new(JsonSerializerDefaults.Web);

    // ── GET ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Sends a GET request and deserializes the response body as <typeparamref name="T"/>.
    /// </summary>
    public static Task<Result<T>> GetResult<T>(
        this HttpClient client,
        string requestUri,
        HttpResultOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        return SendAndReadAsync<T>(client, request, options, cancellationToken);
    }

    /// <summary>
    /// Sends a GET request and deserializes the response body as <typeparamref name="T"/>.
    /// </summary>
    public static Task<Result<T>> GetResult<T>(
        this HttpClient client,
        Uri requestUri,
        HttpResultOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        return SendAndReadAsync<T>(client, request, options, cancellationToken);
    }

    // ── POST ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Sends a POST request with a JSON-serialized <paramref name="body"/> and
    /// deserializes the response body as <typeparamref name="TResponse"/>.
    /// </summary>
    public static Task<Result<TResponse>> PostResult<TBody, TResponse>(
        this HttpClient client,
        string requestUri,
        TBody body,
        HttpResultOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var jsonOptions = options?.JsonOptions ?? s_defaultJsonOptions;
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(body, options: jsonOptions)
        };
        return SendAndReadAsync<TResponse>(client, request, options, cancellationToken);
    }

    // ── PUT ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Sends a PUT request with a JSON-serialized <paramref name="body"/> and
    /// deserializes the response body as <typeparamref name="TResponse"/>.
    /// </summary>
    public static Task<Result<TResponse>> PutResult<TBody, TResponse>(
        this HttpClient client,
        string requestUri,
        TBody body,
        HttpResultOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var jsonOptions = options?.JsonOptions ?? s_defaultJsonOptions;
        var request = new HttpRequestMessage(HttpMethod.Put, requestUri)
        {
            Content = JsonContent.Create(body, options: jsonOptions)
        };
        return SendAndReadAsync<TResponse>(client, request, options, cancellationToken);
    }

    // ── DELETE ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Sends a DELETE request. Returns <see cref="Result.Ok()"/> on success (2xx),
    /// or a typed error result on failure.
    /// </summary>
    public static Task<Result> DeleteResult(
        this HttpClient client,
        string requestUri,
        HttpResultOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
        return SendAsync(client, request, options, cancellationToken);
    }

    /// <summary>
    /// Sends a DELETE request and deserializes the response body as <typeparamref name="T"/>.
    /// </summary>
    public static Task<Result<T>> DeleteResult<T>(
        this HttpClient client,
        string requestUri,
        HttpResultOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
        return SendAndReadAsync<T>(client, request, options, cancellationToken);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static async Task<Result<T>> SendAndReadAsync<T>(
        HttpClient client,
        HttpRequestMessage request,
        HttpResultOptions? options,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await client.SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var jsonOptions = options?.JsonOptions ?? s_defaultJsonOptions;
                var value = await response.Content
                    .ReadFromJsonAsync<T>(jsonOptions, cancellationToken)
                    .ConfigureAwait(false);
                return Result<T>.Ok(value!);
            }

            return Result<T>.Fail(MapStatusCode(response, options));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            return Result<T>.Fail(new ExceptionError(ex));
        }
        catch (OperationCanceledException ex) // HttpClient timeout
        {
            return Result<T>.Fail(new ExceptionError(ex));
        }
    }

    private static async Task<Result> SendAsync(
        HttpClient client,
        HttpRequestMessage request,
        HttpResultOptions? options,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await client.SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return Result.Ok();

            return Result.Fail(MapStatusCode(response, options));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            return Result.Fail(new ExceptionError(ex));
        }
        catch (OperationCanceledException ex) // HttpClient timeout
        {
            return Result.Fail(new ExceptionError(ex));
        }
    }

    private static IError MapStatusCode(HttpResponseMessage response, HttpResultOptions? options)
    {
        if (options?.StatusCodeMapper is { } mapper)
            return mapper(response.StatusCode, response.ReasonPhrase);

        return HttpStatusCodeMapper.Map(response.StatusCode, response.ReasonPhrase);
    }
}
