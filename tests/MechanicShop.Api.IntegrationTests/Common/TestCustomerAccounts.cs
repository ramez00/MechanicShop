using System.Net.Http.Json;

using MechanicShop.Application.Features.Identity;
using MechanicShop.Application.Features.Identity.Queries.GenerateTokens;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Identity;
using MechanicShop.infrastructure.Data;
using MechanicShop.infrastructure.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace MechanicShop.Api.IntegrationTests.Common;

public sealed record TestCustomerAccount(string UserId, string UserName, string Email, Guid CustomerId);

// Creates confirmed customer accounts directly in the database; registration and
// confirmation through the API are covered by their own tests.
public static class TestCustomerAccounts
{
    public const string Password = "Passw0rd1";

    public static async Task<TestCustomerAccount> CreateAsync(WebAppFactory factory)
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var user = new AppUser { UserName = $"cust_{suffix}", Email = $"cust.{suffix}@test.local", EmailConfirmed = true };

        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.True((await userManager.CreateAsync(user, Password)).Succeeded);
        Assert.True((await userManager.AddToRoleAsync(user, RoleNames.Customer)).Succeeded);

        var customer = Customer.CreateSelfRegistered(Guid.NewGuid(), user.Id, "Test Customer", user.Email, "+201000000000").Value;
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        return new TestCustomerAccount(user.Id, user.UserName, user.Email, customer.Id);
    }

    public static async Task<string> LoginAsync(HttpClient client, TestCustomerAccount account)
    {
        var response = await client.PostAsJsonAsync("/identity/token/generate", new GenerateTokenQuery(account.UserName, Password));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<TokenResponse>())!.AccessToken!;
    }
}
