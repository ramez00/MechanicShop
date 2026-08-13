using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Identity;

public sealed class RefreshToken : AuditableEntity
{
    public string? Token { get; private set; }
    public string? UserId { get; private set; }
    public DateTimeOffset Expiry { get; private set; }

    private RefreshToken() { } // For EF Core

    private RefreshToken(Guid id, string token, string userId, DateTimeOffset expiry) : base(id)
    {
        Token = token;
        UserId = userId;
        Expiry = expiry;
    }

    public static Result<RefreshToken> Create(Guid id, string token, string userId, DateTimeOffset expiry)
    {
        if (id == Guid.Empty)
            return RefreshTokenErrors.IdRequired;

        if (string.IsNullOrWhiteSpace(token))
            return RefreshTokenErrors.TokenRequired;

        if (string.IsNullOrEmpty(userId))
            return RefreshTokenErrors.UserIdRequired;

        if (expiry <= DateTimeOffset.UtcNow)
            return RefreshTokenErrors.ExpiryInvalid;

        return new RefreshToken(id, token, userId, expiry);
    }
}
