using System.Net;
using System.Net.Http.Json;

using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Commands;
using MechanicShop.Application.Features.Customers.Commands.Update;
using MechanicShop.Application.Features.Customers.Create.Commands;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Customers;
using MechanicShop.Tests.Common.Builders;
using MechanicShop.Tests.Common.Common;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class CustomersControllerTests(WebAppFactory webAppFactory)
{
    private readonly AppHttpClient _client = webAppFactory.CreateAppHttpClient();
    private readonly IAppDbContext _context = webAppFactory.CreateAppDbContext();

    private async Task<Customer> CreateCustomerAsync()
    {
        var customer = new CustomerBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail($"customer.{Guid.NewGuid():N}@example.com")
            .WithCars(new CarBuilder().WithId(Guid.NewGuid()).Build())
            .Build();

        _context.Customers.Add(customer);

        await _context.SaveChangesAsync(default);

        return customer;
    }

    [Fact]
    public async Task GetCustomers_WithValidAuth_ShouldReturnList()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/api/v1.0/customers/GetCustomers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<CustomerDto>>();

        Assert.NotNull(result);
        Assert.NotEmpty(result!);
    }

    [Fact]
    public async Task GetCustomers_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1.0/customers/GetCustomers");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerById_WithValidId_ShouldReturnCustomer()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var customer = await _context.Customers
            .Include(c => c.Cars)
            .FirstAsync();

        var response = await _client.GetAsync($"/api/v1.0/customers/{customer.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CustomerDto>();

        Assert.NotNull(result);
        Assert.Equal(customer.Id, result!.Id);
    }

    [Fact]
    public async Task GetCustomerById_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync($"/api/v1.0/customers/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerById_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync($"/api/v1.0/customers/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_WithValidRequest_ShouldReturnCreated()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var request = new CreateCustomerCommand(
            "Catherine Test",
            "+12025550123",
            $"customer.create.{Guid.NewGuid():N}@example.com",
            [new CreateCarCommand("Toyota", "Camry", 2022, "PLT-001")]);

        Guid? createdId = null;

        try
        {
            var response = await _client.PostAsJsonAsync("/api/v1.0/customers", request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var location = response.Headers.Location?.ToString();

            Assert.NotNull(location);

            createdId = Guid.Parse(location!.Split('/')[^1]);
        }
        finally
        {
            if (createdId.HasValue)
                await _context.Customers.Where(c => c.Id == createdId.Value).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task CreateCustomer_WithDuplicateEmail_ShouldReturnConflict()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var duplicateEmail = $"customer.duplicate.{Guid.NewGuid():N}@example.com";

        var existingCustomer = new CustomerBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail(duplicateEmail)
            .WithCars(new CarBuilder().WithId(Guid.NewGuid()).Build())
            .Build();

        _context.Customers.Add(existingCustomer);

        await _context.SaveChangesAsync(default);

        try
        {
            var request = new CreateCustomerCommand(
                "Duplicate Customer",
                "+12025550123",
                duplicateEmail,
                [new CreateCarCommand("Honda", "Civic", 2021, "PLT-002")]);

            var response = await _client.PostAsJsonAsync("/api/v1.0/customers", request);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
        finally
        {
            await _context.Customers.Where(c => c.Id == existingCustomer.Id).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task CreateCustomer_WithInvalidRequest_ShouldReturnBadRequest()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var request = new CreateCustomerCommand(
            "",
            "+12025550123",
            $"customer.invalid.{Guid.NewGuid():N}@example.com",
            [new CreateCarCommand("Toyota", "Camry", 2022, "PLT-003")]);

        var response = await _client.PostAsJsonAsync("/api/v1.0/customers", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);

        _client.SetAuthorizationHeader(token);

        var request = new CreateCustomerCommand(
            "Forbidden Customer",
            "+12025550123",
            $"customer.forbidden.{Guid.NewGuid():N}@example.com",
            [new CreateCarCommand("Ford", "Focus", 2020, "PLT-004")]);

        var response = await _client.PostAsJsonAsync("/api/v1.0/customers", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var request = new CreateCustomerCommand(
            "Anonymous Customer",
            "+12025550123",
            $"customer.anon.{Guid.NewGuid():N}@example.com",
            [new CreateCarCommand("Ford", "Focus", 2020, "PLT-005")]);

        var response = await _client.PostAsJsonAsync("/api/v1.0/customers", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_WithValidRequest_ShouldReturnOk()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var customer = await CreateCustomerAsync();

        try
        {
            var request = new UpdateCustomerCommand(
                customer.Id,
                "Updated Customer",
                "+12025550123",
                customer.Email!,
                [new UpdateCarCommand(customer.Cars.First().Id, "Honda", "Civic", 2019, "PLT-006")]);

            var response = await _client.PutAsJsonAsync($"/api/v1.0/customers/{customer.Id}", request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        finally
        {
            await _context.Customers.Where(c => c.Id == customer.Id).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task UpdateCustomer_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var request = new UpdateCustomerCommand(
            Guid.NewGuid(),
            "Updated Customer",
            "+12025550123",
            $"customer.notfound.{Guid.NewGuid():N}@example.com",
            [new UpdateCarCommand(null, "Honda", "Civic", 2019, "PLT-007")]);

        var response = await _client.PutAsJsonAsync($"/api/v1.0/customers/{request.CustomerId}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_WithInvalidRequest_ShouldReturnBadRequest()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var customer = await CreateCustomerAsync();

        try
        {
            var request = new UpdateCustomerCommand(
                customer.Id,
                "",
                "+12025550123",
                customer.Email!,
                [new UpdateCarCommand(customer.Cars.First().Id, "Honda", "Civic", 2019, "PLT-008")]);

            var response = await _client.PutAsJsonAsync($"/api/v1.0/customers/{customer.Id}", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        finally
        {
            await _context.Customers.Where(c => c.Id == customer.Id).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task UpdateCustomer_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);

        _client.SetAuthorizationHeader(token);

        var request = new UpdateCustomerCommand(
            Guid.NewGuid(),
            "Updated Customer",
            "+12025550123",
            $"customer.forbidden.{Guid.NewGuid():N}@example.com",
            [new UpdateCarCommand(null, "Honda", "Civic", 2019, "PLT-009")]);

        var response = await _client.PutAsJsonAsync($"/api/v1.0/customers/{request.CustomerId}", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCustomer_WithValidId_ShouldReturnNoContent()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var customer = await CreateCustomerAsync();

        try
        {
            var response = await _client.DeleteAsync($"/api/v1.0/customers/{customer.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        finally
        {
            await _context.Customers.Where(c => c.Id == customer.Id).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task DeleteCustomer_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var response = await _client.DeleteAsync($"/api/v1.0/customers/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCustomer_WithActiveWorkOrders_ShouldReturnConflict()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var customer = new CustomerBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail($"customer.conflict.{Guid.NewGuid():N}@example.com")
            .WithCars(new CarBuilder().WithId(Guid.NewGuid()).Build())
            .Build();

        _context.Customers.Add(customer);

        await _context.SaveChangesAsync(default);

        var workOrder = WorkOrderTestDataBuilder.Create()
            .InProgress()
            .WithVehicle(customer.Cars.First().Id)
            .WithRepairTasks(await _context.RepairTasks.Take(1).ToListAsync())
            .WithLabor(Guid.Parse(TestUsers.Labor01.Id))
            .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(default);

        try
        {
            var response = await _client.DeleteAsync($"/api/v1.0/customers/{customer.Id}");

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
        finally
        {
            await _context.WorkOrders.Where(w => w.Id == workOrder.Id).ExecuteDeleteAsync();
            await _context.Customers.Where(c => c.Id == customer.Id).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task DeleteCustomer_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);

        _client.SetAuthorizationHeader(token);

        var response = await _client.DeleteAsync($"/api/v1.0/customers/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}