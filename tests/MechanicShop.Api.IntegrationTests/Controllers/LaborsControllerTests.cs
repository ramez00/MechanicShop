using System.Net;
using System.Net.Http.Json;

using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Features.Labors.Dtos;
using MechanicShop.Tests.Common.Common;

using Xunit;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class LaborsControllerTests(WebAppFactory webAppFactory)
{
    private readonly AppHttpClient _client = webAppFactory.CreateAppHttpClient();

    [Fact]
    public async Task GetLabors_WithValidAuth_ShouldReturnList()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/api/v1.0/labors");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<LaborDto>>();

        Assert.NotNull(result);
        Assert.NotEmpty(result!);
    }

    [Fact]
    public async Task GetLabors_AsMechanic_ShouldReturnList()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);

        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/api/v1.0/labors");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<LaborDto>>();

        Assert.NotNull(result);
        Assert.All(result!, labor => Assert.False(string.IsNullOrWhiteSpace(labor.Name)));
    }

    [Fact]
    public async Task GetLabors_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1.0/labors");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}