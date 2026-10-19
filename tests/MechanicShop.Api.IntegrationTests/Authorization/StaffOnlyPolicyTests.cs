using System.Net;
using System.Reflection;

using MechanicShop.Api.Controllers;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Identity;
using MechanicShop.infrastructure.Data;
using MechanicShop.infrastructure.Identity;
using MechanicShop.infrastructure.RealTime;
using MechanicShop.Tests.Common.Common;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace MechanicShop.Api.IntegrationTests.Authorization;

[Collection(WebAppFactoryCollection.CollectionName)]
public class StaffOnlyPolicyTests(WebAppFactory webAppFactory) : IAsyncLifetime
{
    private const string NegotiateUrl = "/hubs/workorders/negotiate?negotiateVersion=1";

    // Controllers that are meant to be reachable by non-staff users.
    private static readonly Type[] NonStaffControllers =
    [
        typeof(IdentityController),
        typeof(SettingsController),
        typeof(MyVehiclesController),
        typeof(MyServicesController)
    ];

    private static readonly string[] StaffPolicies = ["StaffOnly", "ManagerOnly"];

    private readonly AppHttpClient _client = webAppFactory.CreateAppHttpClient();

    // Authenticated user in the Customer role — the main audience these policies must keep out.
    private AppUser _nonStaffUser = null!;

    public async Task InitializeAsync()
    {
        var email = $"nonstaff.{Guid.NewGuid():N}@localhost";

        _nonStaffUser = new AppUser { Email = email, UserName = email, EmailConfirmed = true };

        using var scope = webAppFactory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var result = await userManager.CreateAsync(_nonStaffUser, email);

        Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(e => e.Description)));

        var roleResult = await userManager.AddToRoleAsync(_nonStaffUser, RoleNames.Customer);

        Assert.True(roleResult.Succeeded, string.Join(", ", roleResult.Errors.Select(e => e.Description)));

        // Customers can only obtain a token when a Customer profile is linked to the account.
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Customers.Add(Customer.CreateSelfRegistered(Guid.NewGuid(), _nonStaffUser.Id, "Non Staff", email, "+201000000000").Value);
        await context.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public static TheoryData<string, string> StaffEndpoints => new()
    {
        { "GET", "/api/v1.0/customers/GetCustomers" },
        { "GET", $"/api/v1.0/customers/{Guid.NewGuid()}" },
        { "GET", "/api/v1.0/workorders?page=1&pageSize=10" },
        { "GET", $"/api/v1.0/workorders/{Guid.NewGuid()}" },
        { "GET", $"/api/v1.0/workorders/schedule/{DateTime.UtcNow:yyyy-MM-dd}" },
        { "GET", "/api/v1.0/labors" },
        { "GET", "/api/v1.0/repair-tasks" },
        { "GET", $"/api/v1.0/repair-tasks/{Guid.NewGuid()}" },
        { "GET", "/api/v1.0/dashboard/stats" },
        { "GET", $"/api/v1.0/invoices/{Guid.NewGuid()}" },
        { "POST", "/api/v1.0/customers" },
        { "POST", "/api/v1.0/workorders" },
        { "DELETE", $"/api/v1.0/workorders/{Guid.NewGuid()}" }
    };

    public static TheoryData<string> StaffReadEndpoints => new()
    {
        "/api/v1.0/customers/GetCustomers",
        "/api/v1.0/workorders?page=1&pageSize=10",
        "/api/v1.0/labors",
        "/api/v1.0/repair-tasks",
        "/api/v1.0/dashboard/stats"
    };

    [Fact]
    public void Every_staff_controller_requires_a_staff_policy()
    {
        var controllers = typeof(ApiController).Assembly.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract && t != typeof(ApiController))
            .Except(NonStaffControllers)
            .ToList();

        Assert.NotEmpty(controllers);

        Assert.All(controllers, controller =>
        {
            var policies = controller.GetCustomAttributes<AuthorizeAttribute>(inherit: true).Select(a => a.Policy);

            Assert.True(
                policies.Any(p => StaffPolicies.Contains(p)),
                $"{controller.Name} must have [Authorize(Policy = \"StaffOnly\")] or a stricter staff policy.");

            var anonymousActions = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.GetCustomAttribute<AllowAnonymousAttribute>() is not null)
                .Select(m => m.Name);

            Assert.Empty(anonymousActions);
        });
    }

    [Fact]
    public void WorkOrderHub_requires_StaffOnly()
    {
        var policies = typeof(WorkOrderHub).GetCustomAttributes<AuthorizeAttribute>().Select(a => a.Policy);

        Assert.Contains("StaffOnly", policies);
    }

    [Theory]
    [MemberData(nameof(StaffEndpoints))]
    public async Task NonStaff_user_gets_403_on_staff_endpoints(string method, string url)
    {
        var token = await _client.GenerateTokenAsync(_nonStaffUser);

        _client.SetAuthorizationHeader(token);

        var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(StaffEndpoints))]
    public async Task Anonymous_gets_401_on_staff_endpoints(string method, string url)
    {
        _client.ClearAuthorizationHeader();

        var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(StaffReadEndpoints))]
    public async Task Manager_still_gets_200_on_staff_endpoints(string url)
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Mechanic_still_gets_200_on_labors()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);

        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/api/v1.0/labors");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Hub_negotiate_without_token_returns_401()
    {
        _client.ClearAuthorizationHeader();

        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Post, NegotiateUrl));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Hub_negotiate_as_nonStaff_returns_403()
    {
        var token = await _client.GenerateTokenAsync(_nonStaffUser);

        _client.SetAuthorizationHeader(token);

        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Post, NegotiateUrl));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Hub_negotiate_as_manager_with_token_in_query_string_returns_200()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.ClearAuthorizationHeader();

        var response = await _client.SendAsync(
            new HttpRequestMessage(HttpMethod.Post, $"{NegotiateUrl}&access_token={token}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
