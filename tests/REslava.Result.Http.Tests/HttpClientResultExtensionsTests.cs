using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using REslava.Result;
using REslava.Result.Http;

namespace REslava.Result.Http.Tests;

// Simple DTO used throughout all tests
file record SampleDto(int Id, string Name);

[TestClass]
public sealed class HttpClientResultExtensionsTests
{
    private static HttpClient MakeClient(HttpResponseMessage response)
        => new(new StubHttpMessageHandler(response)) { BaseAddress = new Uri("http://test/") };

    private static HttpClient MakeThrowingClient()
        => new(new ThrowingHttpMessageHandler()) { BaseAddress = new Uri("http://test/") };

    private static HttpResponseMessage JsonResponse(HttpStatusCode status, object body)
        => new(status) { Content = JsonContent.Create(body) };

    // ── GetResult<T> ─────────────────────────────────────────────────────────

    [TestMethod]
    public async Task GetResult_200_ReturnsOkWithDeserializedBody()
    {
        var client = MakeClient(JsonResponse(HttpStatusCode.OK, new SampleDto(1, "Alice")));

        var result = await client.GetResult<SampleDto>("/items/1");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Value!.Id);
        Assert.AreEqual("Alice", result.Value.Name);
    }

    [TestMethod]
    public async Task GetResult_404_ReturnsNotFoundError()
    {
        var client = MakeClient(new HttpResponseMessage(HttpStatusCode.NotFound));

        var result = await client.GetResult<SampleDto>("/items/99");

        Assert.IsTrue(result.IsFailure);
        Assert.IsInstanceOfType<NotFoundError>(result.Errors[0]);
    }

    [TestMethod]
    public async Task GetResult_401_ReturnsUnauthorizedError()
    {
        var client = MakeClient(new HttpResponseMessage(HttpStatusCode.Unauthorized));

        var result = await client.GetResult<SampleDto>("/items/1");

        Assert.IsTrue(result.IsFailure);
        Assert.IsInstanceOfType<UnauthorizedError>(result.Errors[0]);
    }

    [TestMethod]
    public async Task GetResult_403_ReturnsForbiddenError()
    {
        var client = MakeClient(new HttpResponseMessage(HttpStatusCode.Forbidden));

        var result = await client.GetResult<SampleDto>("/items/1");

        Assert.IsTrue(result.IsFailure);
        Assert.IsInstanceOfType<ForbiddenError>(result.Errors[0]);
    }

    [TestMethod]
    public async Task GetResult_409_ReturnsConflictError()
    {
        var client = MakeClient(new HttpResponseMessage(HttpStatusCode.Conflict));

        var result = await client.GetResult<SampleDto>("/items/1");

        Assert.IsTrue(result.IsFailure);
        Assert.IsInstanceOfType<ConflictError>(result.Errors[0]);
    }

    [TestMethod]
    public async Task GetResult_422_ReturnsValidationError()
    {
        var client = MakeClient(new HttpResponseMessage(HttpStatusCode.UnprocessableEntity));

        var result = await client.GetResult<SampleDto>("/items/1");

        Assert.IsTrue(result.IsFailure);
        Assert.IsInstanceOfType<ValidationError>(result.Errors[0]);
    }

    [TestMethod]
    public async Task GetResult_500_ReturnsGenericErrorWithStatusCode()
    {
        var client = MakeClient(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var result = await client.GetResult<SampleDto>("/items/1");

        Assert.IsTrue(result.IsFailure);
        Assert.IsInstanceOfType<Error>(result.Errors[0]);
        StringAssert.Contains(result.Errors[0].Message, "500");
    }

    [TestMethod]
    public async Task GetResult_NetworkException_ReturnsExceptionError()
    {
        var client = MakeThrowingClient();

        var result = await client.GetResult<SampleDto>("/items/1");

        Assert.IsTrue(result.IsFailure);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
        StringAssert.Contains(result.Errors[0].Message, "Connection refused");
    }

    [TestMethod]
    public async Task GetResult_CustomStatusCodeMapper_OverridesDefault()
    {
        var client = MakeClient(new HttpResponseMessage(HttpStatusCode.NotFound));
        var options = new HttpResultOptions
        {
            StatusCodeMapper = (_, _) => new Error("Custom not found message")
        };

        var result = await client.GetResult<SampleDto>("/items/99", options);

        Assert.IsTrue(result.IsFailure);
        Assert.IsInstanceOfType<Error>(result.Errors[0]);
        Assert.AreEqual("Custom not found message", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task GetResult_UriOverload_200_ReturnsOkWithDeserializedBody()
    {
        var client = MakeClient(JsonResponse(HttpStatusCode.OK, new SampleDto(7, "Bob")));

        var result = await client.GetResult<SampleDto>(new Uri("http://test/items/7"));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(7, result.Value!.Id);
    }

    [TestMethod]
    public async Task GetResult_CustomJsonOptions_SucceedsWithValidJson()
    {
        var client = MakeClient(JsonResponse(HttpStatusCode.OK, new SampleDto(3, "Carol")));
        var options = new HttpResultOptions
        {
            JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        };

        var result = await client.GetResult<SampleDto>("/items/3", options);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(3, result.Value!.Id);
    }

    // ── PostResult<TBody, TResponse> ─────────────────────────────────────────

    [TestMethod]
    public async Task PostResult_201_ReturnsOkWithDeserializedBody()
    {
        var client = MakeClient(JsonResponse(HttpStatusCode.Created, new SampleDto(10, "Dave")));

        var result = await client.PostResult<SampleDto, SampleDto>(
            "/items", new SampleDto(0, "Dave"));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(10, result.Value!.Id);
    }

    [TestMethod]
    public async Task PostResult_409_ReturnsConflictError()
    {
        var client = MakeClient(new HttpResponseMessage(HttpStatusCode.Conflict));

        var result = await client.PostResult<SampleDto, SampleDto>(
            "/items", new SampleDto(0, "Dave"));

        Assert.IsTrue(result.IsFailure);
        Assert.IsInstanceOfType<ConflictError>(result.Errors[0]);
    }

    [TestMethod]
    public async Task PostResult_NetworkException_ReturnsExceptionError()
    {
        var client = MakeThrowingClient();

        var result = await client.PostResult<SampleDto, SampleDto>(
            "/items", new SampleDto(0, "Dave"));

        Assert.IsTrue(result.IsFailure);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
    }

    // ── PutResult<TBody, TResponse> ──────────────────────────────────────────

    [TestMethod]
    public async Task PutResult_200_ReturnsOkWithDeserializedBody()
    {
        var client = MakeClient(JsonResponse(HttpStatusCode.OK, new SampleDto(5, "Updated")));

        var result = await client.PutResult<SampleDto, SampleDto>(
            "/items/5", new SampleDto(5, "Updated"));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Updated", result.Value!.Name);
    }

    [TestMethod]
    public async Task PutResult_404_ReturnsNotFoundError()
    {
        var client = MakeClient(new HttpResponseMessage(HttpStatusCode.NotFound));

        var result = await client.PutResult<SampleDto, SampleDto>(
            "/items/99", new SampleDto(99, "Ghost"));

        Assert.IsTrue(result.IsFailure);
        Assert.IsInstanceOfType<NotFoundError>(result.Errors[0]);
    }

    // ── DeleteResult (non-generic) ────────────────────────────────────────────

    [TestMethod]
    public async Task DeleteResult_204_ReturnsOk()
    {
        var client = MakeClient(new HttpResponseMessage(HttpStatusCode.NoContent));

        var result = await client.DeleteResult("/items/1");

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task DeleteResult_404_ReturnsNotFoundError()
    {
        var client = MakeClient(new HttpResponseMessage(HttpStatusCode.NotFound));

        var result = await client.DeleteResult("/items/99");

        Assert.IsTrue(result.IsFailure);
        Assert.IsInstanceOfType<NotFoundError>(result.Errors[0]);
    }

    [TestMethod]
    public async Task DeleteResult_NetworkException_ReturnsExceptionError()
    {
        var client = MakeThrowingClient();

        var result = await client.DeleteResult("/items/1");

        Assert.IsTrue(result.IsFailure);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
    }

    // ── DeleteResult<T> ──────────────────────────────────────────────────────

    [TestMethod]
    public async Task DeleteResult_Generic_200_ReturnsOkWithDeserializedBody()
    {
        var client = MakeClient(JsonResponse(HttpStatusCode.OK, new SampleDto(9, "Eve")));

        var result = await client.DeleteResult<SampleDto>("/items/9");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(9, result.Value!.Id);
    }
}
