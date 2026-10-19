using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Features.Identity.Queries.GenerateTokens;
using MechanicShop.Contracts.Requests.Identity;
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
public partial class ConfirmEmailTests(WebAppFactory webAppFactory)
{
    private const string RegisterUrl = "/identity/register";
    private const string ConfirmUrl = "/identity/confirm-email";
    private const string ResendUrl = "/identity/resend-confirmation";
    private const string LoginUrl = "/identity/token/generate";
    private const string ValidPassword = "Passw0rd1";

    private readonly HttpClient _client = webAppFactory.CreateClient();

    private sealed record Registration(string UserId, string Email, string Token);

    [GeneratedRegex(@"token=([A-Za-z0-9_-]+)")]
    private static partial Regex TokenInLink();

    // Registers through the API and returns the token from the confirmation email that was actually sent.
    private async Task<Registration> RegisterAsync(HttpClient? client = null)
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var email = $"confirm.{suffix}@test.local";

        var response = await (client ?? _client).PostAsJsonAsync(RegisterUrl, new RegisterCustomerRequest
        {
            UserName = $"confirm_{suffix}",
            Password = ValidPassword,
            ConfirmPassword = ValidPassword,
            Email = email,
            FullName = "Confirm Test",
            PhoneNumber = "+201000000000"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var userId = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("userId").GetString()!;
        var token = await WaitForTokenAsync(email, expectedCount: 1);

        return new Registration(userId, email, token);
    }

    private async Task<string> WaitForTokenAsync(string email, int expectedCount)
    {
        var transport = webAppFactory.EmailTransport;

        await transport.WaitForAsync(_ => transport.Sent.Count(m => m.To == email) >= expectedCount);

        var message = transport.Sent.Where(m => m.To == email).ElementAt(expectedCount - 1);

        return TokenInLink().Match(message.HtmlBody).Groups[1].Value;
    }

    private Task<HttpResponseMessage> ConfirmAsync(string userId, string token, HttpClient? client = null)
        => (client ?? _client).PostAsJsonAsync(ConfirmUrl, new ConfirmEmailRequest { UserId = userId, Token = token });

    private Task<HttpResponseMessage> LoginAsync(string email, string password)
        => _client.PostAsJsonAsync(LoginUrl, new GenerateTokenQuery(email, password));

    private async Task<bool> IsEmailConfirmedAsync(string userId)
    {
        using var scope = webAppFactory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        return (await userManager.FindByIdAsync(userId))!.EmailConfirmed;
    }

    private static async Task<JsonElement> BodyAsync(HttpResponseMessage response)
        => await response.Content.ReadFromJsonAsync<JsonElement>();

    private static string Tamper(string token)
    {
        // Change a middle character: the last Base64 character can carry only padding bits.
        var chars = token.ToCharArray();
        var i = chars.Length / 2;
        chars[i] = chars[i] == 'A' ? 'B' : 'A';

        return new string(chars);
    }

    [Fact]
    public async Task Confirm_with_valid_token_sets_EmailConfirmed_and_login_succeeds()
    {
        var registration = await RegisterAsync();

        var response = await ConfirmAsync(registration.UserId, registration.Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(await IsEmailConfirmedAsync(registration.UserId));

        var login = await LoginAsync(registration.Email, ValidPassword);

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.False(string.IsNullOrEmpty((await BodyAsync(login)).GetProperty("accessToken").GetString()));
    }

    [Fact]
    public async Task Confirm_twice_is_idempotent_200()
    {
        var registration = await RegisterAsync();

        var first = await ConfirmAsync(registration.UserId, registration.Token);
        var second = await ConfirmAsync(registration.UserId, registration.Token);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.True(await IsEmailConfirmedAsync(registration.UserId));
    }

    [Fact]
    public async Task Confirm_with_tampered_token_returns_400()
    {
        var registration = await RegisterAsync();

        var response = await ConfirmAsync(registration.UserId, Tamper(registration.Token));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(await IsEmailConfirmedAsync(registration.UserId));
    }

    [Fact]
    public async Task Confirm_with_tampered_token_returns_400_even_after_confirming()
    {
        var registration = await RegisterAsync();
        Assert.Equal(HttpStatusCode.OK, (await ConfirmAsync(registration.UserId, registration.Token)).StatusCode);

        var response = await ConfirmAsync(registration.UserId, Tamper(registration.Token));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Confirm_with_expired_token_returns_400_and_resend_issues_a_working_link()
    {
        var clock = new FixedTimeProvider(DateTimeOffset.UtcNow);

        await using var clockFactory = webAppFactory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(clock);
            }));

        var client = clockFactory.CreateClient();
        var registration = await RegisterAsync(client);

        clock.Advance(TimeSpan.FromHours(25));

        var expired = await ConfirmAsync(registration.UserId, registration.Token, client);

        Assert.Equal(HttpStatusCode.BadRequest, expired.StatusCode);
        Assert.False(await IsEmailConfirmedAsync(registration.UserId));

        var resend = await client.PostAsJsonAsync(ResendUrl, new ResendEmailConfirmationRequest { Email = registration.Email });
        Assert.Equal(HttpStatusCode.Accepted, resend.StatusCode);

        var newToken = await WaitForTokenAsync(registration.Email, expectedCount: 2);

        var confirmed = await ConfirmAsync(registration.UserId, newToken, client);

        Assert.Equal(HttpStatusCode.OK, confirmed.StatusCode);
        Assert.True(await IsEmailConfirmedAsync(registration.UserId));
    }

    [Fact]
    public async Task Confirm_just_before_24h_still_succeeds()
    {
        var clock = new FixedTimeProvider(DateTimeOffset.UtcNow);

        await using var clockFactory = webAppFactory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(clock);
            }));

        var client = clockFactory.CreateClient();
        var registration = await RegisterAsync(client);

        clock.Advance(TimeSpan.FromHours(23));

        Assert.Equal(HttpStatusCode.OK, (await ConfirmAsync(registration.UserId, registration.Token, client)).StatusCode);
    }

    [Fact]
    public async Task Confirm_with_unknown_userId_returns_400_generic_message()
    {
        var registration = await RegisterAsync();

        var unknownUser = await ConfirmAsync(Guid.NewGuid().ToString(), registration.Token);
        var tampered = await ConfirmAsync(registration.UserId, Tamper(registration.Token));

        Assert.Equal(HttpStatusCode.BadRequest, unknownUser.StatusCode);

        // An unknown user must be indistinguishable from a bad token.
        var unknownErrors = (await BodyAsync(unknownUser)).GetProperty("errors").GetRawText();
        var tamperedErrors = (await BodyAsync(tampered)).GetProperty("errors").GetRawText();

        Assert.Equal(tamperedErrors, unknownErrors);
        Assert.Contains("User_InvalidConfirmationLink", unknownErrors);
    }

    [Fact]
    public async Task Unconfirmed_customer_login_returns_403_User_EmailNotConfirmed()
    {
        var registration = await RegisterAsync();

        var response = await LoginAsync(registration.Email, ValidPassword);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("User_EmailNotConfirmed", (await BodyAsync(response)).GetProperty("code").GetString());
    }

    [Fact]
    public async Task Unconfirmed_customer_login_with_wrong_password_does_not_reveal_confirmation_state()
    {
        var registration = await RegisterAsync();

        var response = await LoginAsync(registration.Email, "WrongPassw0rd");

        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Resend_returns_202_for_unknown_confirmed_and_unconfirmed_email()
    {
        var transport = webAppFactory.EmailTransport;

        var confirmed = await RegisterAsync();
        Assert.Equal(HttpStatusCode.OK, (await ConfirmAsync(confirmed.UserId, confirmed.Token)).StatusCode);

        var unconfirmed = await RegisterAsync();
        var unknownEmail = $"nobody.{Guid.NewGuid():N}@test.local";

        // Queue the no-op requests first; the dispatcher delivers in order, so once the
        // unconfirmed user's email arrives, anything the others had queued would have arrived too.
        foreach (var email in new[] { unknownEmail, confirmed.Email, unconfirmed.Email })
        {
            var response = await _client.PostAsJsonAsync(ResendUrl, new ResendEmailConfirmationRequest { Email = email });
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }

        await WaitForTokenAsync(unconfirmed.Email, expectedCount: 2);

        Assert.DoesNotContain(transport.Sent, m => m.To == unknownEmail);
        Assert.Single(transport.Sent, m => m.To == confirmed.Email);
    }

    [Fact]
    public async Task Resend_returns_429_after_rate_limit()
    {
        await using var limitedFactory = webAppFactory.WithWebHostBuilder(builder =>
            builder.UseSetting("RateLimiting:ResendConfirmation:PermitLimit", "2"));

        var client = limitedFactory.CreateClient();
        var request = new ResendEmailConfirmationRequest { Email = $"nobody.{Guid.NewGuid():N}@test.local" };

        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 3; i++)
            statuses.Add((await client.PostAsJsonAsync(ResendUrl, request)).StatusCode);

        Assert.Equal([HttpStatusCode.Accepted, HttpStatusCode.Accepted, HttpStatusCode.TooManyRequests], statuses);
    }

    [Fact]
    public async Task Seeded_staff_login_unaffected()
    {
        // Seeded staff use their email as password (see ApplicationDbContextInitialiser).
        var response = await LoginAsync("pm@localhost", "pm@localhost");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Link_issued_before_restart_still_works_and_keys_are_stored_in_db()
    {
        Registration registration;

        // A separate host stands in for the API before the restart; it shares only the database.
        await using (var beforeRestart = webAppFactory.WithWebHostBuilder(_ => { }))
            registration = await RegisterAsync(beforeRestart.CreateClient());

        await using var afterRestart = webAppFactory.WithWebHostBuilder(_ => { });

        var response = await ConfirmAsync(registration.UserId, registration.Token, afterRestart.CreateClient());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = webAppFactory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.True(await context.DataProtectionKeys.AnyAsync());
    }
}
