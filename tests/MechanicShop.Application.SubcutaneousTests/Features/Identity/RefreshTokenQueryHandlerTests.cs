using System.Security.Claims;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Features.Identity.Queries.RefreshTokens;

namespace MechanicShop.Application.UnitTests.Features.Identity;

public class RefreshTokenQueryHandlerTests : HandlerTestBase
{
    private const string UserId = "user-1";
    private const string RefreshTokenValue = "test-refresh-token";

    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly ITokenProvider _tokens = Substitute.For<ITokenProvider>();

    private RefreshTokenQueryHandler CreateSut() =>
        new(Logger<RefreshTokenQueryHandler>(), _identity, Context, _tokens);

    private static ClaimsPrincipal PrincipalWith(string? userId)
    {
        var claims = userId is null ? new List<Claim>() : [new Claim(ClaimTypes.NameIdentifier, userId)];
        return new ClaimsPrincipal(new ClaimsIdentity(claims));
    }

    private static AppUserDto User() => new(UserId, "user@example.com", [], []);

    private static RefreshTokenQuery Query() => new(RefreshTokenValue, "expired-access-token");

    [Fact]
    public async Task Handle_WhenValid_ReturnsNewToken()
    {
        await SeedAsync(new RefreshTokenBuilder().WithToken(RefreshTokenValue).WithUserId(UserId).Build());
        _tokens.GetPrincipalFromExpiredToken(Arg.Any<string>()).Returns(PrincipalWith(UserId));
        _identity.GetUserByIdAsync(UserId).Returns((Result<AppUserDto>)User());
        _tokens.GenerateJwtTokenAsync(Arg.Any<AppUserDto>(), Arg.Any<CancellationToken>())
               .Returns((Result<TokenResponse>)new TokenResponse { AccessToken = "new-access" });
        var sut = CreateSut();

        var result = await sut.Handle(Query(), CancellationToken.None);

        var dto = result.ShouldBeSuccess();
        Assert.Equal("new-access", dto.AccessToken);
    }

    [Fact]
    public async Task Handle_WhenPrincipalNull_ReturnsExpiredAccessTokenInvalid()
    {
        _tokens.GetPrincipalFromExpiredToken(Arg.Any<string>()).Returns((ClaimsPrincipal?)null);
        var sut = CreateSut();

        var result = await sut.Handle(Query(), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.ExpiredAccessTokenInvalid);
    }

    [Fact]
    public async Task Handle_WhenUserIdClaimMissing_ReturnsUserIdClaimInvalid()
    {
        _tokens.GetPrincipalFromExpiredToken(Arg.Any<string>()).Returns(PrincipalWith(null));
        var sut = CreateSut();

        var result = await sut.Handle(Query(), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.UserIdClaimInvalid);
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenMissing_ReturnsRefreshTokenExpired()
    {
        // No refresh token seeded for this user.
        _tokens.GetPrincipalFromExpiredToken(Arg.Any<string>()).Returns(PrincipalWith(UserId));
        _identity.GetUserByIdAsync(UserId).Returns((Result<AppUserDto>)User());
        var sut = CreateSut();

        var result = await sut.Handle(Query(), CancellationToken.None);

        result.ShouldBeError(ApplicationErrors.RefreshTokenExpired);
    }
}
