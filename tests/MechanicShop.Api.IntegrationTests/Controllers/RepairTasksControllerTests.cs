using System.Net;
using System.Net.Http.Json;

using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Commands.Create;
using MechanicShop.Application.Features.RepairTasks.Commands.Update;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Tests.Common.Builders;
using MechanicShop.Tests.Common.Common;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RepairTasksControllerTests(WebAppFactory webAppFactory)
{
    private readonly AppHttpClient _client = webAppFactory.CreateAppHttpClient();
    private readonly IAppDbContext _context = webAppFactory.CreateAppDbContext();

    private async Task<RepairTask> CreateRepairTaskAsync(string? name = null)
    {
        var repairTask = new RepairTaskBuilder()
            .WithId(Guid.NewGuid())
            .WithName(name ?? $"Test Task {Guid.NewGuid():N}")
            .WithParts(new PartBuilder().WithId(Guid.NewGuid()).Build())
            .Build();

        _context.RepairTasks.Add(repairTask);

        await _context.SaveChangesAsync(default);

        return repairTask;
    }

    [Fact]
    public async Task GetRepairTasks_WithValidAuth_ShouldReturnList()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/api/v1.0/repair-tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<RepairTaskDto>>();

        Assert.NotNull(result);
        Assert.NotEmpty(result!);
    }

    [Fact]
    public async Task GetRepairTasks_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1.0/repair-tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetRepairTaskById_WithValidId_ShouldReturnRepairTask()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var repairTask = await _context.RepairTasks.FirstAsync();

        var response = await _client.GetAsync($"/api/v1.0/repair-tasks/{repairTask.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RepairTaskDto>();

        Assert.NotNull(result);
        Assert.Equal(repairTask.Id, result!.RepairTaskId);
    }

    [Fact]
    public async Task GetRepairTaskById_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync($"/api/v1.0/repair-tasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetRepairTaskById_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync($"/api/v1.0/repair-tasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateRepairTask_WithValidRequest_ShouldReturnCreated()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var request = new CreateRepairTaskCommand(
            $"Brake Fluid Flush {Guid.NewGuid():N}",
            120m,
            RepairDurationInMinutes.Min60,
            [new CreateRepairTaskPartCommand("Brake Fluid", 25m, 1)]);

        Guid? createdId = null;

        try
        {
            var response = await _client.PostAsJsonAsync("/api/v1.0/repair-tasks", request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<RepairTaskDto>();

            Assert.NotNull(result);
            createdId = result!.RepairTaskId;
        }
        finally
        {
            if (createdId.HasValue)
                await _context.RepairTasks.Where(rt => rt.Id == createdId.Value).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task CreateRepairTask_WithDuplicateName_ShouldReturnConflict()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var duplicateName = $"Duplicate Task {Guid.NewGuid():N}";

        var existing = await CreateRepairTaskAsync(duplicateName);

        try
        {
            var request = new CreateRepairTaskCommand(
                duplicateName,
                100m,
                RepairDurationInMinutes.Min30,
                [new CreateRepairTaskPartCommand("Some Part", 10m, 1)]);

            var response = await _client.PostAsJsonAsync("/api/v1.0/repair-tasks", request);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
        finally
        {
            await _context.RepairTasks.Where(rt => rt.Id == existing.Id).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task CreateRepairTask_WithInvalidRequest_ShouldReturnBadRequest()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var request = new CreateRepairTaskCommand(
            $"Invalid Task {Guid.NewGuid():N}",
            0m,
            RepairDurationInMinutes.Min30,
            [new CreateRepairTaskPartCommand("Some Part", 10m, 1)]);

        var response = await _client.PostAsJsonAsync("/api/v1.0/repair-tasks", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateRepairTask_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);

        _client.SetAuthorizationHeader(token);

        var request = new CreateRepairTaskCommand(
            $"Forbidden Task {Guid.NewGuid():N}",
            100m,
            RepairDurationInMinutes.Min30,
            [new CreateRepairTaskPartCommand("Some Part", 10m, 1)]);

        var response = await _client.PostAsJsonAsync("/api/v1.0/repair-tasks", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateRepairTask_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var request = new CreateRepairTaskCommand(
            $"Anonymous Task {Guid.NewGuid():N}",
            100m,
            RepairDurationInMinutes.Min30,
            [new CreateRepairTaskPartCommand("Some Part", 10m, 1)]);

        var response = await _client.PostAsJsonAsync("/api/v1.0/repair-tasks", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRepairTask_WithValidRequest_ShouldReturnOk()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var repairTask = await CreateRepairTaskAsync();

        try
        {
            var request = new UpdateRepairTaskCommand(
                repairTask.Id,
                repairTask.Name,
                150m,
                RepairDurationInMinutes.Min45,
                [new UpdateRepairTaskPartCommand(Guid.NewGuid(), "Updated Part", 12m, 2)]);

            var response = await _client.PutAsJsonAsync($"/api/v1.0/repair-tasks/{repairTask.Id}", request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        finally
        {
            await _context.RepairTasks.Where(rt => rt.Id == repairTask.Id).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task UpdateRepairTask_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var request = new UpdateRepairTaskCommand(
            Guid.NewGuid(),
            "Non Existent Task",
            150m,
            RepairDurationInMinutes.Min45,
            [new UpdateRepairTaskPartCommand(null, "Part", 12m, 1)]);

        var response = await _client.PutAsJsonAsync($"/api/v1.0/repair-tasks/{request.RepairTaskId}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRepairTask_WithInvalidRequest_ShouldReturnBadRequest()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var repairTask = await CreateRepairTaskAsync();

        try
        {
            var request = new UpdateRepairTaskCommand(
                repairTask.Id,
                "",
                150m,
                RepairDurationInMinutes.Min45,
                [new UpdateRepairTaskPartCommand(Guid.NewGuid(), "Part", 12m, 1)]);

            var response = await _client.PutAsJsonAsync($"/api/v1.0/repair-tasks/{repairTask.Id}", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        finally
        {
            await _context.RepairTasks.Where(rt => rt.Id == repairTask.Id).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task UpdateRepairTask_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);

        _client.SetAuthorizationHeader(token);

        var request = new UpdateRepairTaskCommand(
            Guid.NewGuid(),
            "Forbidden Update",
            150m,
            RepairDurationInMinutes.Min45,
            [new UpdateRepairTaskPartCommand(null, "Part", 12m, 1)]);

        var response = await _client.PutAsJsonAsync($"/api/v1.0/repair-tasks/{request.RepairTaskId}", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RemoveRepairTask_WithValidId_ShouldReturnNoContent()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var repairTask = await CreateRepairTaskAsync();

        try
        {
            var response = await _client.DeleteAsync($"/api/v1.0/repair-tasks/{repairTask.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        finally
        {
            await _context.RepairTasks.Where(rt => rt.Id == repairTask.Id).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task RemoveRepairTask_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var response = await _client.DeleteAsync($"/api/v1.0/repair-tasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveRepairTask_WhenInUse_ShouldReturnConflict()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var repairTask = await _context.RepairTasks.FirstAsync();

        var workOrder = WorkOrderTestDataBuilder.Create()
            .InProgress()
            .WithVehicle((await _context.Vehicles.FirstAsync()).Id)
            .WithRepairTasks(repairTask)
            .WithLabor(Guid.Parse(TestUsers.Labor01.Id))
            .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(default);

        try
        {
            var response = await _client.DeleteAsync($"/api/v1.0/repair-tasks/{repairTask.Id}");

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
        finally
        {
            await _context.WorkOrders.Where(w => w.Id == workOrder.Id).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task RemoveRepairTask_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);

        _client.SetAuthorizationHeader(token);

        var response = await _client.DeleteAsync($"/api/v1.0/repair-tasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RemoveRepairTask_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await _client.DeleteAsync($"/api/v1.0/repair-tasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}