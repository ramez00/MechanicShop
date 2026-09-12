using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Features.Identity.Queries.GenerateTokens;
using Microsoft.Extensions.Logging.Abstractions;

namespace MechanicShop.Application.UnitTests.Features.Identity;

public class GenerateTokenQueryHandlerTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly ITokenProvider _tokens = Substitute.For<ITokenProvider>();

    private GenerateTokenQueryHandler CreateSut() =>
        new(NullLogger<GenerateTokenQueryHandler>.Instance, _identity, _tokens);

    private static AppUserDto User() => new("user-1", "user@example.com", [], []);

    [Fact]
    public async Task Handle_WhenAuthenticatedAndTokenGenerated_ReturnsToken()
    {
        _identity.AuthenticateAsync("user@example.com", "pw").Returns((Result<AppUserDto>)User());
        var token = new TokenResponse { AccessToken = "access", RefreshToken = "refresh" };
        _tokens.GenerateJwtTokenAsync(Arg.Any<AppUserDto>(), Arg.Any<CancellationToken>())
               .Returns((Result<TokenResponse>)token);
        var sut = CreateSut();

        var result = await sut.Handle(new GenerateTokenQuery("user@example.com", "pw"), CancellationToken.None);

        var dto = result.ShouldBeSuccess();
        Assert.Equal("access", dto.AccessToken);
    }

    [Fact]
    public async Task Handle_WhenAuthenticationFails_ReturnsError()
    {
        _identity.AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>())
                 .Returns((Result<AppUserDto>)ApplicationErrors.UserNotFound);
        var sut = CreateSut();

        var result = await sut.Handle(new GenerateTokenQuery("user@example.com", "pw"), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.UserNotFound);
        await _tokens.DidNotReceive().GenerateJwtTokenAsync(Arg.Any<AppUserDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTokenGenerationFails_ReturnsError()
    {
        _identity.AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>()).Returns((Result<AppUserDto>)User());
        _tokens.GenerateJwtTokenAsync(Arg.Any<AppUserDto>(), Arg.Any<CancellationToken>())
               .Returns((Result<TokenResponse>)ApplicationErrors.TokenGenerationFailed);
        var sut = CreateSut();

        var result = await sut.Handle(new GenerateTokenQuery("user@example.com", "pw"), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.TokenGenerationFailed);
    }
}
