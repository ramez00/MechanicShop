using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Contracts.Requests.Identity;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Identity;
using MechanicShop.infrastructure.Data;
using MechanicShop.infrastructure.Identity;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace MechanicShop.Api.IntegrationTests.Identity;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RegisterCustomerTests(WebAppFactory webAppFactory)
{
    private const string RegisterUrl = "/identity/register";
    private const string ValidPassword = "Passw0rd1";

    private readonly AppHttpClient _client = webAppFactory.CreateAppHttpClient();

    private static RegisterCustomerRequest NewRequest(string? userName = null, string? email = null)
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];

        return new RegisterCustomerRequest
        {
            UserName = userName ?? $"user_{suffix}",
            Password = ValidPassword,
            ConfirmPassword = ValidPassword,
            Email = email ?? $"user.{suffix}@test.local",
            FullName = "Ramez Test",
            PhoneNumber = "+201000000000"
        };
    }

    private static async Task<string?> ProblemCodeAsync(HttpResponseMessage response)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        return json.RootElement.TryGetProperty("code", out var code) ? code.GetString() : null;
    }

    [Fact]
    public async Task Register_valid_returns_201_and_creates_unconfirmed_customer_user()
    {
        var request = NewRequest();

        var response = await _client.PostAsJsonAsync(RegisterUrl, request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var userId = body.GetProperty("userId").GetString()!;

        using var scope = webAppFactory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await userManager.FindByIdAsync(userId);
        Assert.NotNull(user);
        Assert.Equal(request.UserName, user!.UserName);
        Assert.False(user.EmailConfirmed);
        Assert.True(await userManager.IsInRoleAsync(user, RoleNames.Customer));

        var customer = await context.Customers.AsNoTracking().SingleAsync(c => c.UserId == userId);
        Assert.Equal(request.Email, customer.Email);
        Assert.Equal(request.FullName, customer.Name);
    }

    [Fact]
    public async Task Register_response_contains_no_tokens()
    {
        var response = await _client.PostAsJsonAsync(RegisterUrl, NewRequest());

        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("accessToken", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("refreshToken", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_queues_exactly_one_confirmation_email_with_link()
    {
        var request = NewRequest();

        var response = await _client.PostAsJsonAsync(RegisterUrl, request);
        var userId = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("userId").GetString()!;

        var email = await webAppFactory.EmailTransport.WaitForAsync(m => m.To == request.Email);

        Assert.Contains($"/portal/confirm-email?userId={userId}&amp;token=", email.HtmlBody);
        Assert.Single(webAppFactory.EmailTransport.Sent, m => m.To == request.Email);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Register_duplicate_username_returns_409_UserNameTaken(bool differentCase)
    {
        var first = NewRequest();
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync(RegisterUrl, first)).StatusCode);

        var userName = differentCase ? first.UserName.ToUpperInvariant() : first.UserName;
        var response = await _client.PostAsJsonAsync(RegisterUrl, NewRequest(userName: userName));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("User_UserNameTaken", await ProblemCodeAsync(response));
    }

    [Fact]
    public async Task Register_duplicate_email_returns_409_EmailTaken()
    {
        var first = NewRequest();
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync(RegisterUrl, first)).StatusCode);

        var response = await _client.PostAsJsonAsync(RegisterUrl, NewRequest(email: first.Email.ToUpperInvariant()));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("User_EmailTaken", await ProblemCodeAsync(response));
    }

    [Fact]
    public async Task Register_weak_password_returns_400_listing_password_errors()
    {
        var request = NewRequest();
        request.Password = request.ConfirmPassword = "password";

        var response = await _client.PostAsJsonAsync(RegisterUrl, request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var passwordErrors = body.GetProperty("errors").GetProperty("Password");
        Assert.True(passwordErrors.GetArrayLength() >= 2, "Expected every broken password rule to be reported.");
    }

    [Fact]
    public async Task Register_rolls_back_user_when_customer_insert_fails()
    {
        await using var failingFactory = webAppFactory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.AddSingleton<ISaveChangesInterceptor, FailCustomerInsertInterceptor>()));

        var request = NewRequest();

        var response = await failingFactory.CreateClient().PostAsJsonAsync(RegisterUrl, request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        using var scope = webAppFactory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        Assert.Null(await userManager.FindByNameAsync(request.UserName));
    }

    [Fact]
    public async Task Register_returns_429_after_rate_limit()
    {
        await using var limitedFactory = webAppFactory.WithWebHostBuilder(builder =>
            builder.UseSetting("RateLimiting:Registration:PermitLimit", "2"));

        var client = limitedFactory.CreateClient();

        // Invalid bodies still count against the limit, so these don't create users.
        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 3; i++)
            statuses.Add((await client.PostAsJsonAsync(RegisterUrl, new RegisterCustomerRequest())).StatusCode);

        Assert.Equal([HttpStatusCode.BadRequest, HttpStatusCode.BadRequest, HttpStatusCode.TooManyRequests], statuses);
    }

    // Simulates a database failure on the customer insert, after Identity has already written the user.
    private sealed class FailCustomerInsertInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context!.ChangeTracker.Entries<Customer>().Any(e => e.State == EntityState.Added))
                throw new InvalidOperationException("Simulated customer insert failure.");

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
