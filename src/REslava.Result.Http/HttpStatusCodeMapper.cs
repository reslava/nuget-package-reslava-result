using System.Net;

namespace REslava.Result.Http;

/// <summary>
/// Default HTTP status code → <see cref="IError"/> mapping used when no custom
/// <see cref="HttpResultOptions.StatusCodeMapper"/> is configured.
/// </summary>
internal static class HttpStatusCodeMapper
{
    internal static IError Map(HttpStatusCode statusCode, string? reasonPhrase)
        => statusCode switch
        {
            HttpStatusCode.NotFound            => new NotFoundError("Resource not found"),
            HttpStatusCode.Unauthorized        => new UnauthorizedError(),
            HttpStatusCode.Forbidden           => new ForbiddenError(),
            HttpStatusCode.Conflict            => new ConflictError("A conflict occurred"),
            HttpStatusCode.UnprocessableEntity => new ValidationError("Validation failed"),
            _                                  => new Error(
                                                    $"HTTP {(int)statusCode}: {reasonPhrase ?? statusCode.ToString()}")
        };
}
