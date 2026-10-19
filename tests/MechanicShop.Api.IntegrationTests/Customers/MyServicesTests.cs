using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Features.BookableServices.Dtos;
using MechanicShop.Application.Features.RepairTasks.Commands.Update;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.infrastructure.Data;
using MechanicShop.Tests.Common.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace MechanicShop.Api.IntegrationTests.Customers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class MyServicesTests(WebAppFactory webAppFactory)
{
    private const string Url = "/api/v1/me/services";

    private async Task<HttpClient> SignedInCustomerAsync()
    {
        var account = await TestCustomerAccounts.CreateAsync(webAppFactory);
        var client = webAppFactory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", await TestCustomerAccounts.LoginAsync(client, account));

        return client;
    }

    [Fact]
    public async Task Customer_gets_all_repair_tasks()
    {
        var client = await SignedInCustomerAsync();

        var services = await client.GetFromJsonAsync<List<BookableServiceDto>>(Url);

        using var scope = webAppFactory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tasks = await context.RepairTasks.AsNoTracking().Include(r => r.Parts).ToListAsync();

        Assert.NotEmpty(services!);
        Assert.Equal(tasks.Count, services!.Count);

        foreach (var task in tasks)
        {
            var service = Assert.Single(services, s => s.Id == task.Id);
            Assert.Equal((int)task.EstimatedDurationInMins, service.EstimatedDurationInMins);
            Assert.Equal(task.LaborCost, service.LaborCost);
            Assert.Equal(task.totalCost, service.EstimatedTotal);
        }
    }

    [Fact]
    public async Task Response_has_no_part_details()
    {
        var client = await SignedInCustomerAsync();

        using var json = JsonDocument.Parse(await client.GetStringAsync(Url));

        Assert.NotEqual(0, json.RootElement.GetArrayLength());

        foreach (var service in json.RootElement.EnumerateArray())
        {
            var properties = service.EnumerateObject().Select(p => p.Name).ToHashSet();
            Assert.Equal(["id", "name", "estimatedDurationInMins", "laborCost", "estimatedTotal"], properties);
        }
    }

    [Fact]
    public async Task Manager_update_invalidates_services_cache()
    {
        var client = await SignedInCustomerAsync();

        // Prime the cache.
        var before = (await client.GetFromJsonAsync<List<BookableServiceDto>>(Url))!.First();

        var newDuration = before.EstimatedDurationInMins == (int)RepairDurationInMinutes.Min180
            ? RepairDurationInMinutes.Min15
            : RepairDurationInMinutes.Min180;

        var manager = webAppFactory.CreateAppHttpClient();
        manager.SetAuthorizationHeader(await manager.GenerateTokenAsync(TestUsers.Manager));

        var parts = await PartsOfAsync(before.Id);

        try
        {
            var update = await manager.PutAsJsonAsync($"/api/v1.0/repair-tasks/{before.Id}",
                new UpdateRepairTaskCommand(before.Id, before.Name + " (edited)", before.LaborCost, newDuration, parts));

            Assert.Equal(HttpStatusCode.OK, update.StatusCode);

            var after = (await client.GetFromJsonAsync<List<BookableServiceDto>>(Url))!.Single(s => s.Id == before.Id);

            Assert.Equal((int)newDuration, after.EstimatedDurationInMins);
            Assert.Equal(before.Name + " (edited)", after.Name);
        }
        finally
        {
            // Restore the seeded task so other tests see the original data.
            await manager.PutAsJsonAsync($"/api/v1.0/repair-tasks/{before.Id}",
                new UpdateRepairTaskCommand(before.Id, before.Name, before.LaborCost, (RepairDurationInMinutes)before.EstimatedDurationInMins, parts));
        }
    }

    [Fact]
    public async Task Staff_token_returns_403()
    {
        var client = webAppFactory.CreateAppHttpClient();
        client.SetAuthorizationHeader(await client.GenerateTokenAsync(TestUsers.Manager));

        var response = await client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_gets_401()
    {
        var response = await webAppFactory.CreateClient().GetAsync(Url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // The update endpoint replaces the part list, so the current parts are sent back unchanged.
    private async Task<List<UpdateRepairTaskPartCommand>> PartsOfAsync(Guid repairTaskId)
    {
        using var scope = webAppFactory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var task = await context.RepairTasks.AsNoTracking().Include(r => r.Parts).SingleAsync(r => r.Id == repairTaskId);

        return [.. task.Parts.Select(p => new UpdateRepairTaskPartCommand(p.Id, p.Name, p.Price, p.Quantity))];
    }
}
