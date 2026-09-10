using MechanicShop.Domain.Identity;

namespace MechanicShop.Domain.UnitTests.Identity;

public class RefreshTokenTests
{
    private static readonly Guid Id = Guid.Parse("88888888-8888-8888-8888-888888888888");
    private static DateTimeOffset FutureExpiry => DateTimeOffset.UtcNow.AddDays(7);

    // ---------- Create ----------

    [Fact]
    public void Create_WithValidInputs_ReturnsSuccess()
    {
        var result = RefreshToken.Create(Id, "token-value", "user-1", FutureExpiry);

        var token = result.ShouldBeSuccess();
        Assert.Equal("token-value", token.Token);
        Assert.Equal("user-1", token.UserId);
    }

    [Fact]
    public void Create_WithEmptyId_ReturnsIdRequired()
    {
        var result = RefreshToken.Create(Guid.Empty, "token-value", "user-1", FutureExpiry);

        result.ShouldBeError(RefreshTokenErrors.IdRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithBlankToken_ReturnsTokenRequired(string token)
    {
        var result = RefreshToken.Create(Id, token, "user-1", FutureExpiry);

        result.ShouldBeError(RefreshTokenErrors.TokenRequired);
    }

    [Fact]
    public void Create_WithBlankUserId_ReturnsUserIdRequired()
    {
        var result = RefreshToken.Create(Id, "token-value", "", FutureExpiry);

        result.ShouldBeError(RefreshTokenErrors.UserIdRequired);
    }

    [Fact]
    public void Create_WithPastExpiry_ReturnsExpiryInvalid()
    {
        var result = RefreshToken.Create(Id, "token-value", "user-1", DateTimeOffset.UtcNow.AddDays(-1));

        result.ShouldBeError(RefreshTokenErrors.ExpiryInvalid);
    }
}
