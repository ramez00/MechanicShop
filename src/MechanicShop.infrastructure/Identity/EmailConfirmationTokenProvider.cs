using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MechanicShop.infrastructure.Identity;

public sealed class EmailConfirmationTokenProviderOptions : DataProtectionTokenProviderOptions
{
    public const string ProviderName = "EmailConfirmation";

    public EmailConfirmationTokenProviderOptions()
    {
        Name = ProviderName;
        TokenLifespan = TimeSpan.FromHours(24);
    }
}

// Same token format as Identity's DataProtectorTokenProvider, which reads DateTimeOffset.UtcNow
// directly. Using TimeProvider lets tests prove that links really expire after TokenLifespan.
public sealed class EmailConfirmationTokenProvider<TUser>(
    IDataProtectionProvider dataProtectionProvider,
    IOptions<EmailConfirmationTokenProviderOptions> options,
    ILogger<DataProtectorTokenProvider<TUser>> logger,
    TimeProvider timeProvider)
    : DataProtectorTokenProvider<TUser>(dataProtectionProvider, options, logger)
    where TUser : class
{
    private static readonly Encoding TokenEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    public override async Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
    {
        using var stream = new MemoryStream();

        using (var writer = new BinaryWriter(stream, TokenEncoding, leaveOpen: true))
        {
            writer.Write(timeProvider.GetUtcNow().UtcTicks);
            writer.Write(await manager.GetUserIdAsync(user));
            writer.Write(purpose ?? string.Empty);
            writer.Write(manager.SupportsUserSecurityStamp ? await manager.GetSecurityStampAsync(user) : string.Empty);
        }

        return Convert.ToBase64String(Protector.Protect(stream.ToArray()));
    }

    public override async Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
    {
        try
        {
            using var stream = new MemoryStream(Protector.Unprotect(Convert.FromBase64String(token)));
            using var reader = new BinaryReader(stream, TokenEncoding, leaveOpen: true);

            var createdAt = new DateTimeOffset(reader.ReadInt64(), TimeSpan.Zero);

            if (createdAt + Options.TokenLifespan < timeProvider.GetUtcNow())
                return false;

            if (reader.ReadString() != await manager.GetUserIdAsync(user))
                return false;

            if (!string.Equals(reader.ReadString(), purpose, StringComparison.Ordinal))
                return false;

            var stamp = reader.ReadString();

            if (reader.PeekChar() != -1)
                return false;

            return manager.SupportsUserSecurityStamp
                ? stamp == await manager.GetSecurityStampAsync(user)
                : stamp.Length == 0;
        }
        catch
        {
            // Tampered, truncated or foreign-key tokens all fail to unprotect or parse.
            return false;
        }
    }
}
