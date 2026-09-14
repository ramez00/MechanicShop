using System.Net;
using System.Net.Http.Json;

using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Features.Dashboard.Dtos;
using MechanicShop.Tests.Common.Common;

using Xunit;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class DashboardControllerTests(WebAppFactory webAppFactory)
{
    private readonly AppHttpClient _client = webAppFactory.CreateAppHttpClient();

    [Fact]
    public async Task GetTodayStats_WithAuth_ShouldReturnStats()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/api/v1.0/dashboard/stats");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TodayWorkOrderStatsDto>();

        Assert.NotNull(result);
        Assert.True(result!.Total >= 0);
    }

    [Fact]
    public async Task GetTodayStats_WithDate_ShouldReturnStatsForThatDate()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);

        _client.SetAuthorizationHeader(token);

        var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2).ToString("yyyy-MM-dd");

        var response = await _client.GetAsync($"/api/v1.0/dashboard/stats?date={date}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TodayWorkOrderStatsDto>();

        Assert.NotNull(result);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2), result!.Date);
    }

    [Fact]
    public async Task GetTodayStats_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1.0/dashboard/stats");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}