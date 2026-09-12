using MechanicShop.Application.Features.Identity.Queries.RefreshTokens;

namespace MechanicShop.Application.UnitTests.Features.Identity;

public class RefreshTokenQueryValidatorTests
{
    private readonly RefreshTokenQueryValidator _validator = new();

    private static RefreshTokenQuery Valid() => new("refresh-token", "expired-access-token");

    [Fact]
    public void Validate_WithValidQuery_IsValid()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
    }

    [Fact]
    public void Validate_WithEmptyRefreshToken_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { RefreshToken = "" });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RefreshTokenQuery.RefreshToken));
    }

    [Fact]
    public void Validate_WithEmptyExpiredAccessToken_IsInvalid()
    {
        var result = _validator.Validate(Valid() with { ExpiredAccessToken = "" });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RefreshTokenQuery.ExpiredAccessToken));
    }
}
