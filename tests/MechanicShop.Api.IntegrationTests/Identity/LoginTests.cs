using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Common.Identity;
using MechanicShop.Application.Features.Identity;
using MechanicShop.Application.Features.Identity.Queries.GenerateTokens;
using MechanicShop.Application.Features.Identity.Queries.RefreshTokens;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Identity;
using MechanicShop.infrastructure.Data;
using MechanicShop.infrastructure.Identity;
using MechanicShop.Tests.Common.Fakes;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xunit;

namespace MechanicShop.Api.IntegrationTests.Identity;

[Collection(WebAppFactoryCollection.CollectionName)]
public class LoginTests(WebAppFactory webAppFactory)
{
    private const string LoginUrl = "/identity/token/generate";
    private const string RefreshUrl = "/identity/token/refresh-token";
    private const string Password = "Passw0rd1";

    private readonly HttpClient _client = webAppFactory.CreateClient();

    private sealed record ConfirmedCustomer(string UserId, string UserName, string Email, Guid CustomerId);

    // Creates a confirmed customer directly; registration and confirmation are covered by their own tests.
    private async Task<ConfirmedCustomer> CreateConfirmedCustomerAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var user = new AppUser { UserName = $"login_{suffix}", Email = $"login.{suffix}@test.local", EmailConfirmed = true };

        using var scope = webAppFactory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.True((await userManager.CreateAsync(user, Password)).Succeeded);
        Assert.True((await userManager.AddToRoleAsync(user, RoleNames.Customer)).Succeeded);

        var customer = Customer.CreateSelfRegistered(Guid.NewGuid(), user.Id, "Login Test", user.Email, "+201000000000").Value;
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        return new ConfirmedCustomer(user.Id, user.UserName, user.Email, customer.Id);
    }

    private static Task<HttpResponseMessage> LoginAsync(HttpClient client, string userName, string password)
        => client.PostAsJsonAsync(LoginUrl, new GenerateTokenQuery(userName, password));

    private async Task<TokenResponse> LoginOkAsync(string userName, string password)
    {
        var response = await LoginAsync(_client, userName, password);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return (await response.Content.ReadFromJsonAsync<TokenResponse>())!;
    }

    // Reads the claims as written to the token, without inbound claim-type mapping.
    private static JwtSecurityToken Decode(TokenResponse tokens)
        => new JwtSecurityTokenHandler().ReadJwtToken(tokens.AccessToken);

    private static async Task<string?> ProblemCodeAsync(HttpResponseMessage response)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        return json.RootElement.TryGetProperty("code", out var code) ? code.GetString() : null;
    }

    [Fact]
    public async Task Customer_login_by_username_returns_tokens_with_role_and_customer_id()
    {
        var customer = await CreateConfirmedCustomerAsync();

        var tokens = await LoginOkAsync(customer.UserName, Password);

        Assert.False(string.IsNullOrEmpty(tokens.RefreshToken));

        var jwt = Decode(tokens);
        Assert.Contains(jwt.Claims, c => c.Type == "role" && c.Value == RoleNames.Customer);
        Assert.Equal(customer.CustomerId.ToString(), jwt.Claims.Single(c => c.Type == AppClaimTypes.CustomerId).Value);
    }

    [Fact]
    public async Task Customer_cannot_log_in_with_email_instead_of_username()
    {
        var customer = await CreateConfirmedCustomerAsync();

        var response = await LoginAsync(_client, customer.Email, Password);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Seeded_manager_login_with_email_as_username_still_works()
    {
        var tokens = await LoginOkAsync("pm@localhost", "pm@localhost");

        Assert.Contains(Decode(tokens).Claims, c => c.Type == "role" && c.Value == "Manager");
    }

    [Fact]
    public async Task Staff_token_has_no_customer_id_claim()
    {
        var tokens = await LoginOkAsync("pm@localhost", "pm@localhost");

        Assert.DoesNotContain(Decode(tokens).Claims, c => c.Type == AppClaimTypes.CustomerId);
    }

    [Fact]
    public async Task Wrong_password_returns_401_generic()
    {
        var customer = await CreateConfirmedCustomerAsync();

        var wrongPassword = await LoginAsync(_client, customer.UserName, "WrongPassw0rd");
        var unknownUser = await LoginAsync(_client, $"nobody_{Guid.NewGuid():N}", Password);

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownUser.StatusCode);

        // An unknown username must be indistinguishable from a wrong password.
        Assert.Equal("Auth_InvalidCredentials", await ProblemCodeAsync(wrongPassword));
        Assert.Equal("Auth_InvalidCredentials", await ProblemCodeAsync(unknownUser));
    }

    [Fact]
    public async Task Five_failures_lock_account()
    {
        var clock = new FixedTimeProvider(DateTimeOffset.UtcNow);

        await using var clockFactory = webAppFactory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(clock);
            }));

        var client = clockFactory.CreateClient();
        var customer = await CreateConfirmedCustomerAsync();

        for (var i = 0; i < 5; i++)
            Assert.Equal(HttpStatusCode.Unauthorized, (await LoginAsync(client, customer.UserName, "WrongPassw0rd")).StatusCode);

        // Locked: the right password gets the same generic 401 as a wrong one.
        var whileLocked = await LoginAsync(client, customer.UserName, Password);
        Assert.Equal(HttpStatusCode.Unauthorized, whileLocked.StatusCode);
        Assert.Equal("Auth_InvalidCredentials", await ProblemCodeAsync(whileLocked));

        using (var scope = webAppFactory.Services.CreateScope())
        {
            var user = await scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>().FindByIdAsync(customer.UserId);
            Assert.Equal(clock.GetUtcNow().AddMinutes(15), user!.LockoutEnd!.Value, TimeSpan.FromSeconds(1));
        }

        clock.Advance(TimeSpan.FromMinutes(14));
        Assert.Equal(HttpStatusCode.Unauthorized, (await LoginAsync(client, customer.UserName, Password)).StatusCode);

        clock.Advance(TimeSpan.FromMinutes(1));
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(client, customer.UserName, Password)).StatusCode);
    }

    [Fact]
    public async Task Successful_login_resets_failure_count()
    {
        var customer = await CreateConfirmedCustomerAsync();

        for (var i = 0; i < 4; i++)
            await LoginAsync(_client, customer.UserName, "WrongPassw0rd");

        await LoginOkAsync(customer.UserName, Password);

        // Four more failures would lock the account only if the earlier ones still counted.
        for (var i = 0; i < 4; i++)
            await LoginAsync(_client, customer.UserName, "WrongPassw0rd");

        await LoginOkAsync(customer.UserName, Password);
    }

    [Fact]
    public async Task Refresh_token_keeps_customer_id_claim()
    {
        var customer = await CreateConfirmedCustomerAsync();
        var tokens = await LoginOkAsync(customer.UserName, Password);

        var response = await _client.PostAsJsonAsync(RefreshUrl, new RefreshTokenQuery(tokens.RefreshToken!, tokens.AccessToken!));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var refreshed = (await response.Content.ReadFromJsonAsync<TokenResponse>())!;
        Assert.Equal(customer.CustomerId.ToString(), Decode(refreshed).Claims.Single(c => c.Type == AppClaimTypes.CustomerId).Value);
    }

    [Fact]
    public async Task Refresh_for_customer_whose_customer_row_was_deleted_returns_401()
    {
        var customer = await CreateConfirmedCustomerAsync();
        var tokens = await LoginOkAsync(customer.UserName, Password);

        using (var scope = webAppFactory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Customers.Where(c => c.Id == customer.CustomerId).ExecuteDeleteAsync();
        }

        var response = await _client.PostAsJsonAsync(RefreshUrl, new RefreshTokenQuery(tokens.RefreshToken!, tokens.AccessToken!));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
