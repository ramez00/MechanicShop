using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string? policyName);

    Task<Result<AppUserDto>> AuthenticateAsync(string email, string password);

    Task<Result<AppUserDto>> GetUserByIdAsync(string userId);

    Task<string?> GetUserNameAsync(string userId);

    // Creates an unconfirmed user in the given role and returns the new user's id.
    Task<Result<string>> CreateUserAsync(string userName, string email, string password, string role);

    // Returns a URL-safe (Base64Url) email confirmation token.
    Task<string> GenerateEmailConfirmationTokenAsync(string userId);

    // Accepts the Base64Url token from GenerateEmailConfirmationTokenAsync. Succeeds again for an
    // already-confirmed user as long as the token is still valid.
    Task<Result<Success>> ConfirmEmailAsync(string userId, string token);

    // Returns the id of the user with this email only if they still need to confirm it; otherwise null.
    Task<string?> FindUnconfirmedUserIdByEmailAsync(string email);
}
