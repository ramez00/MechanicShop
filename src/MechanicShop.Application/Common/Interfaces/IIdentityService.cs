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
}
