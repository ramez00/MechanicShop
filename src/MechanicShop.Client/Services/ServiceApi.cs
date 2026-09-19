using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace MechanicShop.Client.Services;

public sealed class ServiceApi(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient = httpClient;

    public Task<ApiResult<TResponse>> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken = default) =>
        SendAsync<TResponse>(() => new HttpRequestMessage(HttpMethod.Get, requestUri), cancellationToken);

    public Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken cancellationToken = default) =>
        SendAsync<TResponse>(() => CreateJsonRequest(HttpMethod.Post, requestUri, body), cancellationToken);

    public Task<ApiResult> PostAsync<TRequest>(string requestUri, TRequest body, CancellationToken cancellationToken = default) =>
        AsVoidResult(SendAsync<object?>(() => CreateJsonRequest(HttpMethod.Post, requestUri, body), cancellationToken));

    public Task<ApiResult> PostAsync(string requestUri, CancellationToken cancellationToken = default) =>
        AsVoidResult(SendAsync<object?>(() => new HttpRequestMessage(HttpMethod.Post, requestUri), cancellationToken));

    public Task<ApiResult<TResponse>> PutAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken cancellationToken = default) =>
        SendAsync<TResponse>(() => CreateJsonRequest(HttpMethod.Put, requestUri, body), cancellationToken);

    public Task<ApiResult> PutAsync<TRequest>(string requestUri, TRequest body, CancellationToken cancellationToken = default) =>
        AsVoidResult(SendAsync<object?>(() => CreateJsonRequest(HttpMethod.Put, requestUri, body), cancellationToken));

    public Task<ApiResult> DeleteAsync(string requestUri, CancellationToken cancellationToken = default) =>
        AsVoidResult(SendAsync<object?>(() => new HttpRequestMessage(HttpMethod.Delete, requestUri), cancellationToken));

    private async Task<ApiResult<TResponse>> SendAsync<TResponse>(Func<HttpRequestMessage> requestFactory, CancellationToken cancellationToken)
    {
        try
        {
            using var request = requestFactory();
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ToFailureAsync<TResponse>(response, cancellationToken);
            }

            if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
            {
                return ApiResult<TResponse>.Success(default!);
            }

            var data = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);

            return ApiResult<TResponse>.Success(data!);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ApiResult<TResponse>.Failure(ex.Message);
        }
    }

    private static async Task<ApiResult> AsVoidResult(Task<ApiResult<object?>> resultTask)
    {
        var result = await resultTask;

        return result.IsSuccess
            ? ApiResult.Success()
            : ApiResult.Failure(result.ErrorMessage, result.ErrorDetail, result.StatusCode, result.ValidationErrors);
    }

    private static async Task<ApiResult<TResponse>> ToFailureAsync<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var statusCode = (int)response.StatusCode;

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var validationProblem = await TryReadAsync<ValidationProblemDetailsDto>(response, cancellationToken);

            if (validationProblem?.Errors is { Count: > 0 })
            {
                return ApiResult<TResponse>.Failure(
                    validationProblem.Title ?? "Validation failed.",
                    validationProblem.Detail,
                    statusCode,
                    validationProblem.Errors);
            }
        }

        var problem = await TryReadAsync<ProblemDetailsDto>(response, cancellationToken);

        return ApiResult<TResponse>.Failure(
            problem?.Title ?? response.ReasonPhrase ?? "Request failed.",
            problem?.Detail,
            statusCode);
    }

    private static async Task<T?> TryReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken) where T : class
    {
        if (response.Content.Headers.ContentLength is null or 0)
        {
            return null;
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static HttpRequestMessage CreateJsonRequest<TRequest>(HttpMethod method, string requestUri, TRequest body) =>
        new(method, requestUri) { Content = JsonContent.Create(body, options: JsonOptions) };

    private class ProblemDetailsDto
    {
        public string? Title { get; set; }

        public string? Detail { get; set; }

        public int? Status { get; set; }
    }

    private sealed class ValidationProblemDetailsDto : ProblemDetailsDto
    {
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
