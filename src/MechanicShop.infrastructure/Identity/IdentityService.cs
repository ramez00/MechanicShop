using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace MechanicShop.infrastructure.Identity;

public class IdentityService(
    UserManager<AppUser> userManager,
    IUserClaimsPrincipalFactory<AppUser> userClaimsPrincipalFactory,
    IAuthorizationService authorizationService
) : IIdentityService
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly IUserClaimsPrincipalFactory<AppUser> _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService = authorizationService;


    private async Task<IList<Claim>> getUserClaims(AppUser user) 
        => await _userManager.GetClaimsAsync(user);

    private async Task<IList<string>> getUserRoles(AppUser user)
        => await _userManager.GetRolesAsync(user);    
    
    public async Task<Result<AppUserDto>> AuthenticateAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if(user is null)
            return ApplicationErrors.UserNotFound;

        if(!await _userManager.CheckPasswordAsync(user,password))
            return Error.Conflict("Invalid_Login_Attempt", "Email / Password are incorrect");

        var userRoles = await getUserRoles(user);

        // Checked after the password so the confirmation state isn't revealed to someone guessing.
        // Only self-registered customers must confirm; staff accounts are created by the shop.
        if(!user.EmailConfirmed && userRoles.Contains(RoleNames.Customer))
            return ApplicationErrors.EmailNotConfirmed;

        var claims = await getUserClaims(user);

        return new AppUserDto(user.Id, user.Email!, userRoles,claims);    
    }

    public async Task<bool> AuthorizeAsync(string userId, string? policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if(user is null)
            return false;

        var principles = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principles,policyName!);

        return result.Succeeded;
    }

    public async Task<Result<AppUserDto>> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if(user is null)
            return ApplicationErrors.UserNotFound;

        var userRoles = await getUserRoles(user);

        var claims = await getUserClaims(user);

        return new AppUserDto(user.Id, user.Email!, userRoles,claims);       
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if(user is null)
            return ApplicationErrors.UserNotFound.Description.ToString();

        return user.UserName;    
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user is not null && await _userManager.IsInRoleAsync(user,role);
    }

    public async Task<Result<string>> CreateUserAsync(string userName, string email, string password, string role)
    {
        var user = new AppUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = false
        };

        var createResult = await _userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
            return ToErrors(createResult.Errors);

        var roleResult = await _userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
            return ToErrors(roleResult.Errors);

        return user.Id;
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User '{userId}' not found while generating a confirmation token.");

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        // Raw tokens contain '+', '/' and '=', which break inside URLs.
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
    }

    public async Task<Result<Success>> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null || !TryDecodeToken(token, out var rawToken))
            return ApplicationErrors.InvalidEmailConfirmationLink;

        // Verify even when already confirmed, so a reused link still succeeds but a tampered one doesn't.
        var isValid = await _userManager.VerifyUserTokenAsync(
            user,
            _userManager.Options.Tokens.EmailConfirmationTokenProvider,
            UserManager<AppUser>.ConfirmEmailTokenPurpose,
            rawToken);

        if (!isValid)
            return ApplicationErrors.InvalidEmailConfirmationLink;

        if (user.EmailConfirmed)
            return Result.success;

        var confirmResult = await _userManager.ConfirmEmailAsync(user, rawToken);

        if (!confirmResult.Succeeded)
            return ApplicationErrors.InvalidEmailConfirmationLink;

        return Result.success;
    }

    public async Task<string?> FindUnconfirmedUserIdByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        return user is { EmailConfirmed: false } ? user.Id : null;
    }

    private static bool TryDecodeToken(string token, out string rawToken)
    {
        try
        {
            rawToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            return true;
        }
        catch (FormatException)
        {
            rawToken = string.Empty;
            return false;
        }
    }

    private static List<Error> ToErrors(IEnumerable<IdentityError> identityErrors)
    {
        var errors = identityErrors.ToList();

        // Duplicates are conflicts (409); report username before email so the client gets one clear message.
        if (errors.Any(e => e.Code == nameof(IdentityErrorDescriber.DuplicateUserName)))
            return [ApplicationErrors.UserNameTaken];

        if (errors.Any(e => e.Code == nameof(IdentityErrorDescriber.DuplicateEmail)))
            return [ApplicationErrors.EmailTaken];

        return errors.ConvertAll(e => Error.Validation(e.Code, e.Description));
    }
}