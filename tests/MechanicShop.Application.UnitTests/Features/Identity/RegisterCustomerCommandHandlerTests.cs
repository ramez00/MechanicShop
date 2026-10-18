using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Commands.RegisterCustomer;
using MechanicShop.Domain.Identity;

namespace MechanicShop.Application.UnitTests.Features.Identity;

public class RegisterCustomerCommandHandlerTests : HandlerTestBase
{
    private const string UserId = "c0ffee00-0000-4000-8000-000000000001";
    private const string Token = "encoded-token";
    private const string Link = "https://shop.test/portal/confirm-email?userId=1&token=encoded-token";

    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IAppLinkBuilder _linkBuilder = Substitute.For<IAppLinkBuilder>();

    public RegisterCustomerCommandHandlerTests()
    {
        _identity.CreateUserAsync(default!, default!, default!, default!).ReturnsForAnyArgs(UserId);
        _identity.GenerateEmailConfirmationTokenAsync(UserId).Returns(Token);
        _linkBuilder.EmailConfirmationLink(UserId, Token).Returns(Link);
    }

    private RegisterCustomerCommandHandler CreateSut() =>
        new(Context, _identity, _emailSender, _linkBuilder, Logger<RegisterCustomerCommandHandler>());

    private static RegisterCustomerCommand Command() =>
        new("  ramez01 ", "Passw0rd1", "Passw0rd1", " ramez01@test.local ", " Ramez Test ", " +201000000000 ");

    [Fact]
    public async Task Valid_registration_creates_linked_customer_and_returns_user_id()
    {
        var result = await CreateSut().Handle(Command(), CancellationToken.None);

        Assert.Equal(UserId, result.ShouldBeSuccess().UserId);

        var customer = await Context.Customers.AsNoTracking().SingleAsync();
        Assert.Equal(UserId, customer.UserId);
        Assert.Equal("Ramez Test", customer.Name);
        Assert.Equal("ramez01@test.local", customer.Email);
        Assert.Equal("+201000000000", customer.PhoneNumber);
    }

    [Fact]
    public async Task Valid_registration_creates_user_in_customer_role_with_trimmed_values()
    {
        await CreateSut().Handle(Command(), CancellationToken.None);

        await _identity.Received(1).CreateUserAsync("ramez01", "ramez01@test.local", "Passw0rd1", RoleNames.Customer);
    }

    [Fact]
    public async Task Valid_registration_sends_one_confirmation_email_with_link()
    {
        await CreateSut().Handle(Command(), CancellationToken.None);

        await _emailSender.Received(1).SendAsync(
            "ramez01@test.local",
            Arg.Any<string>(),
            Arg.Is<string>(body => body.Contains("https://shop.test/portal/confirm-email?userId=1&amp;token=encoded-token")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Identity_failure_returns_error_without_customer_or_email()
    {
        _identity.CreateUserAsync(default!, default!, default!, default!).ReturnsForAnyArgs(ApplicationErrors.UserNameTaken);

        var result = await CreateSut().Handle(Command(), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.UserNameTaken);
        Assert.False(await Context.Customers.AnyAsync());
        await _emailSender.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default!, default);
    }
}
