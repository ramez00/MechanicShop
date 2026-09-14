using System.Net;
using System.Net.Http.Json;

using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Contracts.Responses;

using Xunit;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class SettingsControllerTests(WebAppFactory webAppFactory)
{
    private readonly AppHttpClient _client = webAppFactory.CreateAppHttpClient();

    [Fact]
    public async Task GetOperatingHours_ShouldReturnOperatingHours()
    {
        var response = await _client.GetAsync("/api/settings/GetOperatingHours");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<OperatingHoursResponse>();

        Assert.NotNull(result);
        Assert.True(result!.OpeningTime < result.ClosingTime);
    }
}