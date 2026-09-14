using System.Net;
using System.Net.Http.Json;

using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Features.Identity;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Features.Identity.Queries.GenerateTokens;
using MechanicShop.Application.Features.Identity.Queries.RefreshTokens;
using MechanicShop.Tests.Common.Common;

using Xunit;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class IdentityControllerTests(WebAppFactory webAppFactory)
{
    private readonly AppHttpClient _client = webAppFactory.CreateAppHttpClient();

    [Fact]
    public async Task GenerateToken_WithValidCredentials_ShouldReturnToken()
    {
        var request = new GenerateTokenQuery(TestUsers.Manager.Email!, TestUsers.Manager.Email!);

        var response = await _client.PostAsJsonAsync("/identity/token/generate", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
    }

    [Fact]
    public async Task GenerateToken_WithInvalidPassword_ShouldReturnConflict()
    {
        var request = new GenerateTokenQuery(TestUsers.Labor01.Email!, "wrong-password");

        var response = await _client.PostAsJsonAsync("/identity/token/generate", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GenerateToken_WithUnknownUser_ShouldReturnNotFound()
    {
        var request = new GenerateTokenQuery("unknown.user@localhost", "whatever");

        var response = await _client.PostAsJsonAsync("/identity/token/generate", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GenerateToken_WithEmptyCredentials_ShouldReturnBadRequest()
    {
        var request = new GenerateTokenQuery("", "");

        var response = await _client.PostAsJsonAsync("/identity/token/generate", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_WithValidTokens_ShouldReturnNewTokens()
    {
        var tokenResponse = await _client.PostAndGetFromJsonAsync<GenerateTokenQuery, TokenResponse>(
            "/identity/token/generate",
            new GenerateTokenQuery(TestUsers.Manager.Email!, TestUsers.Manager.Email!));

        Assert.NotNull(tokenResponse);

        var request = new RefreshTokenQuery(tokenResponse!.RefreshToken!, tokenResponse.AccessToken!);

        var response = await _client.PostAsJsonAsync("/identity/token/refresh-token", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
    }

    [Fact]
    public async Task RefreshToken_WithInvalidRefreshToken_ShouldReturnConflict()
    {
        var tokenResponse = await _client.PostAndGetFromJsonAsync<GenerateTokenQuery, TokenResponse>(
            "/identity/token/generate",
            new GenerateTokenQuery(TestUsers.Manager.Email!, TestUsers.Manager.Email!));

        Assert.NotNull(tokenResponse);

        var request = new RefreshTokenQuery("invalid-refresh-token", tokenResponse!.AccessToken!);

        var response = await _client.PostAsJsonAsync("/identity/token/refresh-token", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUserInfo_WithAuth_ShouldReturnUser()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/identity/current-user/claims");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<AppUserDto>();

        Assert.NotNull(result);
        Assert.Equal(TestUsers.Manager.Id, result!.UserId);
        Assert.Equal(TestUsers.Manager.Email, result.Email);
        Assert.Contains("Manager", result.Roles);
    }

    [Fact]
    public async Task GetCurrentUserInfo_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("/identity/current-user/claims");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}