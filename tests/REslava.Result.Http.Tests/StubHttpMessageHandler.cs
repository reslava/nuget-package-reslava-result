namespace REslava.Result.Http.Tests;

/// <summary>
/// Stub message handler that returns a pre-configured response without making
/// any real network calls. Used to unit-test <see cref="HttpClientResultExtensions"/>.
/// </summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;

    public StubHttpMessageHandler(HttpResponseMessage response) => _response = response;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(_response);
}

/// <summary>
/// Message handler that always throws <see cref="HttpRequestException"/>,
/// simulating a network failure (DNS, connection refused, etc.).
/// </summary>
internal sealed class ThrowingHttpMessageHandler : HttpMessageHandler
{
    private readonly string _message;

    public ThrowingHttpMessageHandler(string message = "Connection refused")
        => _message = message;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        => throw new HttpRequestException(_message);
}
