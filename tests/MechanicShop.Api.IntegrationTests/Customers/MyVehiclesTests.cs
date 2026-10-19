using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Contracts.Requests.Customers;
using MechanicShop.Domain.workOrders.Enums;
using MechanicShop.infrastructure.Data;
using MechanicShop.Tests.Common.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace MechanicShop.Api.IntegrationTests.Customers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class MyVehiclesTests(WebAppFactory webAppFactory)
{
    private const string Url = "/api/v1/me/vehicles";

    // Plates are unique across the whole shop, so every test car gets its own.
    private static string NewPlate() => $"T{Random.Shared.Next(0, 1_000_000_000):D9}";

    private static CreateVehicleRequest NewVehicle(string? plate = null, int year = 2020) => new()
    {
        Make = "Toyota",
        Model = "Corolla",
        Year = year,
        LicensePlate = plate ?? NewPlate()
    };

    private async Task<(HttpClient Client, TestCustomerAccount Account)> SignedInCustomerAsync()
    {
        var account = await TestCustomerAccounts.CreateAsync(webAppFactory);
        var client = webAppFactory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", await TestCustomerAccounts.LoginAsync(client, account));

        return (client, account);
    }

    private static async Task<CarDto> AddAsync(HttpClient client, CreateVehicleRequest request)
    {
        var response = await client.PostAsJsonAsync(Url, request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return (await response.Content.ReadFromJsonAsync<CarDto>())!;
    }

    private static async Task<string?> ProblemCodeAsync(HttpResponseMessage response)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        return json.RootElement.TryGetProperty("code", out var code) ? code.GetString() : null;
    }

    private async Task AddWorkOrderAsync(Guid carId, WorkOrderState state)
    {
        using var scope = webAppFactory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var builder = WorkOrderTestDataBuilder.Create()
            .WithVehicle(carId)
            .WithRepairTasks(await context.RepairTasks.Take(1).ToListAsync())
            .WithLabor(TestUsers.Labor01.Id)
            .WithTimeSlot(DateTimeOffset.UtcNow.AddDays(30), DateTimeOffset.UtcNow.AddDays(30).AddHours(1));

        builder = state == WorkOrderState.Completed ? builder.Completed() : builder.WithState(state);

        context.WorkOrders.Add(builder.Build());
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Get_my_vehicles_returns_only_own_cars()
    {
        var (clientA, _) = await SignedInCustomerAsync();
        var (clientB, _) = await SignedInCustomerAsync();

        var carA = await AddAsync(clientA, NewVehicle());
        var carB = await AddAsync(clientB, NewVehicle());

        var carsA = await clientA.GetFromJsonAsync<List<CarDto>>(Url);
        var carsB = await clientB.GetFromJsonAsync<List<CarDto>>(Url);

        Assert.Equal([carA.Id], carsA!.Select(c => c.Id));
        Assert.Equal([carB.Id], carsB!.Select(c => c.Id));
    }

    [Fact]
    public async Task Post_vehicle_normalizes_plate_and_returns_location()
    {
        var (client, _) = await SignedInCustomerAsync();
        var plate = NewPlate();

        var response = await client.PostAsJsonAsync(Url, NewVehicle(plate: $" {plate.ToLowerInvariant()[..4]} {plate[4..]} "));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var car = (await response.Content.ReadFromJsonAsync<CarDto>())!;
        Assert.Equal(plate, car.LicensePlate);

        var fetched = await client.GetFromJsonAsync<CarDto>(response.Headers.Location);
        Assert.Equal(car.Id, fetched!.Id);
    }

    [Fact]
    public async Task Post_vehicle_ignores_customerId_in_body()
    {
        var (client, account) = await SignedInCustomerAsync();
        var other = await TestCustomerAccounts.CreateAsync(webAppFactory);

        var response = await client.PostAsJsonAsync(Url, new
        {
            customerId = other.CustomerId,
            make = "Honda",
            model = "Civic",
            year = 2021,
            licensePlate = NewPlate()
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var carId = (await response.Content.ReadFromJsonAsync<CarDto>())!.Id;

        using var scope = webAppFactory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Equal(account.CustomerId, (await context.Vehicles.SingleAsync(v => v.Id == carId)).CustomerId);
    }

    [Fact]
    public async Task Post_vehicle_with_next_model_year_returns_201()
    {
        var (client, _) = await SignedInCustomerAsync();

        var response = await client.PostAsJsonAsync(Url, NewVehicle(year: DateTime.UtcNow.Year + 1));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Post_vehicle_two_years_ahead_returns_400()
    {
        var (client, _) = await SignedInCustomerAsync();

        var response = await client.PostAsJsonAsync(Url, NewVehicle(year: DateTime.UtcNow.Year + 2));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_or_delete_other_customers_car_returns_404()
    {
        var (clientA, _) = await SignedInCustomerAsync();
        var (clientB, _) = await SignedInCustomerAsync();

        var carB = await AddAsync(clientB, NewVehicle());

        var get = await clientA.GetAsync($"{Url}/{carB.Id}");
        var put = await clientA.PutAsJsonAsync($"{Url}/{carB.Id}", new UpdateVehicleRequest
        {
            Make = "Hijack", Model = "Attempt", Year = 2020, LicensePlate = NewPlate()
        });
        var delete = await clientA.DeleteAsync($"{Url}/{carB.Id}");

        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, put.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);

        // B's car is untouched.
        var carsB = await clientB.GetFromJsonAsync<List<CarDto>>(Url);
        Assert.Equal(carB, Assert.Single(carsB!));
    }

    [Fact]
    public async Task Put_updates_own_car()
    {
        var (client, _) = await SignedInCustomerAsync();
        var car = await AddAsync(client, NewVehicle());
        var newPlate = NewPlate();

        var response = await client.PutAsJsonAsync($"{Url}/{car.Id}", new UpdateVehicleRequest
        {
            Make = "Honda", Model = "Civic", Year = 2022, LicensePlate = newPlate.ToLowerInvariant()
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new CarDto(car.Id, "Honda", "Civic", 2022, newPlate), await response.Content.ReadFromJsonAsync<CarDto>());
    }

    [Fact]
    public async Task Put_keeping_own_plate_is_not_a_conflict()
    {
        var (client, _) = await SignedInCustomerAsync();
        var car = await AddAsync(client, NewVehicle());

        var response = await client.PutAsJsonAsync($"{Url}/{car.Id}", new UpdateVehicleRequest
        {
            Make = car.Make, Model = "Corolla Cross", Year = car.Year, LicensePlate = car.LicensePlate
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Duplicate_plate_returns_409_Car_PlateTaken()
    {
        var (clientA, _) = await SignedInCustomerAsync();
        var (clientB, _) = await SignedInCustomerAsync();
        var plate = NewPlate();
        var variant = $"{plate[..3].ToLowerInvariant()} {plate[3..]}";

        await AddAsync(clientA, NewVehicle(plate));

        var sameCustomer = await clientA.PostAsJsonAsync(Url, NewVehicle(variant));
        var otherCustomer = await clientB.PostAsJsonAsync(Url, NewVehicle(variant));

        Assert.Equal(HttpStatusCode.Conflict, sameCustomer.StatusCode);
        Assert.Equal("Car_PlateTaken", await ProblemCodeAsync(sameCustomer));
        Assert.Equal(HttpStatusCode.Conflict, otherCustomer.StatusCode);
        Assert.Equal("Car_PlateTaken", await ProblemCodeAsync(otherCustomer));

        // Changing another car to the taken plate is rejected the same way.
        var carB = await AddAsync(clientB, NewVehicle());
        var put = await clientB.PutAsJsonAsync($"{Url}/{carB.Id}", new UpdateVehicleRequest
        {
            Make = "Toyota", Model = "Corolla", Year = 2020, LicensePlate = variant
        });

        Assert.Equal(HttpStatusCode.Conflict, put.StatusCode);
        Assert.Equal("Car_PlateTaken", await ProblemCodeAsync(put));
    }

    [Fact]
    public async Task Delete_own_car_without_bookings_returns_204()
    {
        var (client, _) = await SignedInCustomerAsync();
        var car = await AddAsync(client, NewVehicle());

        var response = await client.DeleteAsync($"{Url}/{car.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<List<CarDto>>(Url))!);
    }

    [Theory]
    [InlineData(WorkOrderState.Scheduled)]
    [InlineData(WorkOrderState.InProgress)]
    public async Task Delete_car_with_active_booking_returns_409(WorkOrderState state)
    {
        var (client, _) = await SignedInCustomerAsync();
        var car = await AddAsync(client, NewVehicle());
        await AddWorkOrderAsync(car.Id, state);

        var response = await client.DeleteAsync($"{Url}/{car.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("Car_HasActiveBookings", await ProblemCodeAsync(response));
    }

    [Fact]
    public async Task Delete_car_with_service_history_returns_409_and_keeps_history()
    {
        var (client, _) = await SignedInCustomerAsync();
        var car = await AddAsync(client, NewVehicle());
        await AddWorkOrderAsync(car.Id, WorkOrderState.Completed);

        var response = await client.DeleteAsync($"{Url}/{car.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("Car_HasServiceHistory", await ProblemCodeAsync(response));

        using var scope = webAppFactory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.True(await context.WorkOrders.AnyAsync(w => w.CarId == car.Id));
    }

    [Fact]
    public async Task Staff_token_on_me_vehicles_returns_403()
    {
        var client = webAppFactory.CreateAppHttpClient();
        client.SetAuthorizationHeader(await client.GenerateTokenAsync(TestUsers.Manager));

        var response = await client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_on_me_vehicles_returns_401()
    {
        var response = await webAppFactory.CreateClient().GetAsync(Url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
