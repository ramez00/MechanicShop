using MechanicShop.Domain.Identity;

namespace MechanicShop.Tests.Common.Builders;

public sealed class RefreshTokenBuilder
{
    private Guid _id = Guid.Parse("88888888-8888-8888-8888-888888888888");
    private string _token = "test-refresh-token";
    private string _userId = "user-1";
    // RefreshToken.Create validates against the real DateTimeOffset.UtcNow, so keep this genuinely in the future.
    private DateTimeOffset _expiry = DateTimeOffset.UtcNow.AddDays(7);

    public RefreshTokenBuilder WithId(Guid id) { _id = id; return this; }
    public RefreshTokenBuilder WithToken(string token) { _token = token; return this; }
    public RefreshTokenBuilder WithUserId(string userId) { _userId = userId; return this; }
    public RefreshTokenBuilder WithExpiry(DateTimeOffset expiry) { _expiry = expiry; return this; }

    public RefreshToken Build()
    {
        var result = RefreshToken.Create(_id, _token, _userId, _expiry);
        if (result.IsError)
            throw new InvalidOperationException($"RefreshTokenBuilder produced an invalid RefreshToken: {result.TopError.Code}");
        return result.Value;
    }
}
