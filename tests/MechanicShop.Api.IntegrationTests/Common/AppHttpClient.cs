using System.Net.Http.Headers;
using System.Net.Http.Json;
using MechanicShop.Application.Features.Identity;
using MechanicShop.Application.Features.Identity.Queries.GenerateTokens;
using MechanicShop.infrastructure.Identity;

namespace MechanicShop.Api.IntegrationTests.Common;

public class AppHttpClient(HttpClient client)
{
    private readonly HttpClient _client = client;

    public async Task<string> GenerateTokenAsync(AppUser user)
    {
        var generateTokenQuery = new GenerateTokenQuery(user.Email!, user.Email!);

        var response = await _client.PostAsJsonAsync("/identity/token/generate", generateTokenQuery);

        if(!response.IsSuccessStatusCode)
            throw new Exception($"Failed to generate token for user {user.Email}. Status code: {response.StatusCode}");

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

        if(tokenResponse == null)
            throw new Exception($"Failed to deserialize token response for user {user.Email}.");

        return tokenResponse.AccessToken!;    
    }

    public void SetAuthorizationHeader(string token)
        => _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public void ClearAuthorizationHeader()
        => _client.DefaultRequestHeaders.Authorization = null;

    public async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
        => await _client.GetAsync(requestUri, cancellationToken);

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        => await _client.SendAsync(request, cancellationToken);
        
    public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T content, CancellationToken cancellationToken = default)
        => await _client.PostAsJsonAsync(requestUri, content, cancellationToken);

    public async Task<HttpResponseMessage> PutAsJsonAsync<T>(string requestUri, T value, CancellationToken cancellationToken = default)
        => await _client.PutAsJsonAsync(requestUri, value, cancellationToken);
    
    public async Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
        => await _client.DeleteAsync(requestUri, cancellationToken);

    public async Task<HttpResponseMessage> PatchAsJsonAsync<T>(string requestUri, T value, CancellationToken cancellationToken = default)
        => await _client.PatchAsJsonAsync(requestUri, value, cancellationToken);

    public async Task<T?> GetFromJsonAsync<T>(string requestUri, CancellationToken cancellationToken = default)
        => await _client.GetFromJsonAsync<T>(requestUri, cancellationToken);

    public async Task<T?> PostAndGetFromJsonAsync<TRequest, T>(string requestUri, TRequest value, CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsJsonAsync(requestUri, value, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Request to {requestUri} failed with status code {response.StatusCode}");

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }

    public void Dispose()
        =>  _client.Dispose();
      
        
}